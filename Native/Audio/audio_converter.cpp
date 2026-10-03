
#define _CRT_SECURE_NO_WARNINGS
#define WIN32_LEAN_AND_MEAN

#include <Windows.h>
#include <commdlg.h>
#include <shlobj.h>

#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <cwchar>
#include <string>
#include <vector>
#include <filesystem>
#include <fstream>
#include <sstream>
#include <algorithm>
#include <atomic>
#include <cwctype>
#include <mutex>
#include <thread>

#include "audio_converter.h"

#pragma comment(lib, "Comdlg32.lib")
#pragma comment(lib, "Shell32.lib")
#pragma comment(lib, "Ole32.lib")

namespace fs = std::filesystem;

static std::atomic_bool g_audio_cancel_requested = false;
static std::mutex g_audio_process_mutex;
static HANDLE g_audio_process = nullptr;
static char g_audio_last_error[4096] = {};

static void set_audio_last_error(const std::string& message)
{
    strncpy_s(g_audio_last_error, message.c_str(), _TRUNCATE);
}

// ============================================================
// WINDOWS WCHAR -> UTF-8
// ============================================================

static std::string wide_to_utf8(const wchar_t* text)
{
    if (!text)
        return {};

    int size = WideCharToMultiByte(
        CP_UTF8,
        0,
        text,
        -1,
        nullptr,
        0,
        nullptr,
        nullptr
    );

    if (size <= 0)
        return {};

    std::string result(size, '\0');

    WideCharToMultiByte(
        CP_UTF8,
        0,
        text,
        -1,
        result.data(),
        size,
        nullptr,
        nullptr
    );

    if (!result.empty() && result.back() == '\0')
        result.pop_back();

    return result;
}

// ============================================================
// WINDOWS UTF-8 -> WCHAR
// ============================================================

static std::wstring utf8_to_wide(const std::string& text)
{
    if (text.empty())
        return {};

    int size = MultiByteToWideChar(
        CP_UTF8,
        0,
        text.c_str(),
        -1,
        nullptr,
        0
    );

    if (size <= 0)
        return {};

    std::wstring result(size, L'\0');

    MultiByteToWideChar(
        CP_UTF8,
        0,
        text.c_str(),
        -1,
        result.data(),
        size
    );

    if (!result.empty() && result.back() == L'\0')
        result.pop_back();

    return result;
}

// ============================================================
// QUOTE WINDOWS ARGUMENT
// ============================================================

static std::wstring quote_windows_argument(const std::wstring& value)
{
    std::wstring result;
    result.reserve(value.size() + 2);

    result += L'"';

    size_t backslashes = 0;

    for (wchar_t c : value)
    {
        if (c == L'\\')
        {
            ++backslashes;
        }
        else if (c == L'"')
        {
            // Escape backslashes before a quote
            result.append(backslashes * 2 + 1, L'\\');
            result += L'"';
            backslashes = 0;
        }
        else
        {
            result.append(backslashes, L'\\');
            backslashes = 0;
            result += c;
        }
    }

    // Escape trailing backslashes before closing quote
    result.append(backslashes * 2, L'\\');

    result += L'"';

    return result;
}

// ============================================================
// RUN PROCESS WITH UNICODE PATHS
// ============================================================

struct ProcessResult
{
    int exit_code = -1;
    bool started = false;
    bool cancelled = false;
    std::string output;
};

static ProcessResult run_process(
    const std::wstring& executable,
    const std::wstring& arguments)
{
    ProcessResult result;
    std::wstring command_line =
        quote_windows_argument(executable);

    if (!arguments.empty())
    {
        command_line += L" ";
        command_line += arguments;
    }

    std::vector<wchar_t> command_buffer(
        command_line.begin(),
        command_line.end()
    );

    command_buffer.push_back(L'\0');

    SECURITY_ATTRIBUTES security_attributes = {};
    security_attributes.nLength = sizeof(security_attributes);
    security_attributes.bInheritHandle = TRUE;

    HANDLE read_pipe = nullptr;
    HANDLE write_pipe = nullptr;

    if (!CreatePipe(&read_pipe, &write_pipe, &security_attributes, 0))
    {
        set_audio_last_error("Could not create the FFmpeg output pipe.");
        return result;
    }

    if (!SetHandleInformation(read_pipe, HANDLE_FLAG_INHERIT, 0))
    {
        CloseHandle(read_pipe);
        CloseHandle(write_pipe);
        set_audio_last_error("Could not configure the FFmpeg output pipe.");
        return result;
    }

    STARTUPINFOW startup_info = {};
    startup_info.cb = sizeof(startup_info);
    startup_info.dwFlags = STARTF_USESTDHANDLES;
    startup_info.hStdOutput = write_pipe;
    startup_info.hStdError = write_pipe;
    startup_info.hStdInput = GetStdHandle(STD_INPUT_HANDLE);

    PROCESS_INFORMATION process_info = {};

    BOOL created = CreateProcessW(
        nullptr,
        command_buffer.data(),
        nullptr,
        nullptr,
        TRUE,
        CREATE_NO_WINDOW | CREATE_UNICODE_ENVIRONMENT,
        nullptr,
        nullptr,
        &startup_info,
        &process_info
    );

    CloseHandle(write_pipe);

    if (!created)
    {
        DWORD error = GetLastError();
        CloseHandle(read_pipe);
        char message[256] = {};
        sprintf_s(message, "Could not start FFmpeg. Windows error: %lu", static_cast<unsigned long>(error));
        set_audio_last_error(message);
        return result;
    }

    result.started = true;

    {
        std::lock_guard<std::mutex> lock(g_audio_process_mutex);
        g_audio_process = process_info.hProcess;
    }

    std::thread reader_thread(
        [&result, read_pipe]()
        {
            char buffer[4096] = {};
            DWORD bytes_read = 0;

            while (ReadFile(read_pipe, buffer, sizeof(buffer), &bytes_read, nullptr) && bytes_read > 0)
            {
                result.output.append(buffer, bytes_read);
            }

            CloseHandle(read_pipe);
        }
    );

    while (WaitForSingleObject(process_info.hProcess, 100) == WAIT_TIMEOUT)
    {
        if (g_audio_cancel_requested.load())
        {
            TerminateProcess(process_info.hProcess, ERROR_CANCELLED);
            result.cancelled = true;
            break;
        }
    }

    WaitForSingleObject(process_info.hProcess, INFINITE);
    reader_thread.join();

    DWORD exit_code = 0;

    {
        std::lock_guard<std::mutex> lock(g_audio_process_mutex);
        if (g_audio_process == process_info.hProcess)
        {
            g_audio_process = nullptr;
        }
    }

    if (!GetExitCodeProcess(
        process_info.hProcess,
        &exit_code))
    {
        CloseHandle(process_info.hThread);
        CloseHandle(process_info.hProcess);
        return result;
    }

    CloseHandle(process_info.hThread);
    CloseHandle(process_info.hProcess);

    result.exit_code = static_cast<int>(exit_code);
    return result;
}

// ============================================================
// FFMPEG MANAGER
// ============================================================

class FFmpegManager
{
private:

    std::wstring ffmpeg_path;
    std::wstring ffprobe_path;

    bool initialized = false;

public:

    FFmpegManager()
    {
        initialize();
    }

    // ========================================================
    // FIND EXECUTABLE IN PATH
    // ========================================================

    std::wstring find_in_path(const std::wstring& exe_name)
    {
        wchar_t path_buffer[32768] = {};

        DWORD length = GetEnvironmentVariableW(
            L"PATH",
            path_buffer,
            static_cast<DWORD>(
                sizeof(path_buffer) / sizeof(wchar_t)
                )
        );

        if (length == 0 || length >= sizeof(path_buffer) / sizeof(wchar_t))
            return L"";

        std::wstring path_string(path_buffer);

        size_t start = 0;

        while (start <= path_string.size())
        {
            size_t end = path_string.find(L';', start);

            if (end == std::wstring::npos)
                end = path_string.size();

            std::wstring directory =
                path_string.substr(start, end - start);

            if (!directory.empty())
            {
                fs::path full_path =
                    fs::path(directory) / exe_name;

                std::error_code ec;

                if (fs::exists(full_path, ec))
                    return full_path.wstring();
            }

            if (end == path_string.size())
                break;

            start = end + 1;
        }

        return L"";
    }

    // ========================================================
    // COMMON LOCATIONS
    // ========================================================

    std::vector<std::wstring> get_common_locations()
    {
        std::vector<std::wstring> locations;

        auto add_location =
            [&locations](const fs::path& path)
            {
                if (path.empty())
                    return;

                std::error_code ec;
                fs::path absolute_path = fs::absolute(path, ec);
                std::wstring value = ec ? path.wstring() : absolute_path.wstring();

                if (std::find(locations.begin(), locations.end(), value) == locations.end())
                {
                    locations.push_back(value);
                }
            };

        wchar_t environment_path[32768] = {};
        DWORD environment_length = GetEnvironmentVariableW(
            L"FFMPEG_BIN",
            environment_path,
            static_cast<DWORD>(sizeof(environment_path) / sizeof(wchar_t)));

        if (environment_length > 0 &&
            environment_length < sizeof(environment_path) / sizeof(wchar_t))
        {
            add_location(environment_path);
        }

        wchar_t module_path[32768] = {};
        DWORD module_length = GetModuleFileNameW(
            nullptr,
            module_path,
            static_cast<DWORD>(sizeof(module_path) / sizeof(wchar_t)));

        if (module_length > 0 &&
            module_length < sizeof(module_path) / sizeof(wchar_t))
        {
            fs::path current = fs::path(module_path).parent_path();

            for (int level = 0; level < 6 && !current.empty(); ++level)
            {
                add_location(current / L"FFmpeg" / L"bin");
                add_location(current / L"bin" / L"FFmpeg");
                add_location(current / L"bin");

                fs::path parent = current.parent_path();
                if (parent == current)
                    break;

                current = parent;
            }
        }

        std::error_code ec;
        fs::path working_directory = fs::current_path(ec);
        if (!ec)
        {
            add_location(working_directory / L"FFmpeg" / L"bin");
            add_location(working_directory / L"bin" / L"FFmpeg");
            add_location(working_directory / L"bin");
        }

        add_location(L"F:\\FormatForge\\FFmpeg\\bin");
        add_location(L"C:\\FFmpeg\\bin");
        add_location(L"C:\\ffmpeg\\bin");
        add_location(L"C:\\Program Files\\FFmpeg\\bin");
        add_location(L"C:\\Program Files (x86)\\FFmpeg\\bin");

        return locations;
    }

    // ========================================================
    // INITIALIZE
    // ========================================================

    bool initialize()
    {
        // Prefer the runtime shipped with FormatForge. PATH is a fallback.
        auto common_paths = get_common_locations();

        for (const auto& directory : common_paths)
        {
            fs::path ffmpeg_test = fs::path(directory) / L"ffmpeg.exe";
            fs::path ffprobe_test = fs::path(directory) / L"ffprobe.exe";
            std::error_code ec;

            if (ffmpeg_path.empty() && fs::is_regular_file(ffmpeg_test, ec))
            {
                ffmpeg_path = ffmpeg_test.wstring();
            }

            ec.clear();
            if (ffprobe_path.empty() && fs::is_regular_file(ffprobe_test, ec))
            {
                ffprobe_path = ffprobe_test.wstring();
            }

            if (!ffmpeg_path.empty() && !ffprobe_path.empty())
                break;
        }

        if (ffmpeg_path.empty())
            ffmpeg_path = find_in_path(L"ffmpeg.exe");

        if (ffprobe_path.empty())
            ffprobe_path = find_in_path(L"ffprobe.exe");

        initialized = !ffmpeg_path.empty();

        if (initialized)
        {
            printf("FFmpeg found:\n");
            printf(
                "  ffmpeg:  %s\n",
                wide_to_utf8(ffmpeg_path.c_str()).c_str()
            );

            printf("  ffprobe: %s\n\n",
                ffprobe_path.empty() ? "not found (optional)" : wide_to_utf8(ffprobe_path.c_str()).c_str());
        }

        return initialized;
    }

    // ========================================================
    // AVAILABILITY
    // ========================================================

    bool is_available() const
    {
        return initialized;
    }

    // ========================================================
    // GET FFMPEG PATH
    // ========================================================

    std::wstring get_ffmpeg_path() const
    {
        return ffmpeg_path;
    }

    // ========================================================
    // GET FFPROBE PATH
    // ========================================================

    std::wstring get_ffprobe_path() const
    {
        return ffprobe_path;
    }

    // ========================================================
    // GET FFMPEG VERSION
    // ========================================================

    std::string get_version()
    {
        if (!initialized)
            return "";

        std::wstring arguments =
            L"-version";

        // Pentru a captura output-ul ar trebui redirectare.
        // Folosim _wpopen doar aici pentru versiune.
        std::wstring command =
            quote_windows_argument(ffmpeg_path) +
            L" -version";

        FILE* pipe = _wpopen(command.c_str(), L"r");

        if (!pipe)
            return "";

        wchar_t buffer[512] = {};
        std::wstring result;

        while (fgetws(
            buffer,
            static_cast<int>(
                sizeof(buffer) / sizeof(wchar_t)
                ),
            pipe) != nullptr)
        {
            result += buffer;

            if (result.find(L"ffmpeg version") !=
                std::wstring::npos)
            {
                break;
            }
        }

        _pclose(pipe);

        return wide_to_utf8(result.c_str());
    }

    // ========================================================
    // CONVERT AUDIO
    // ========================================================

    int convert_audio(
        const std::wstring& input,
        const std::wstring& output,
        const std::wstring& format,
        const FF_AUDIO_OPTIONS& options)
    {
        if (!initialized)
        {
            printf("FFmpeg not initialized!\n");
            set_audio_last_error("FFmpeg was not found.");
            return -1;
        }

        std::error_code file_error;
        if (!fs::is_regular_file(fs::path(input), file_error))
        {
            set_audio_last_error("The input media file does not exist.");
            return -1;
        }

        fs::path output_path(output);
        if (!output_path.parent_path().empty())
        {
            fs::create_directories(output_path.parent_path(), file_error);
            if (file_error)
            {
                set_audio_last_error("The output directory could not be created.");
                return -1;
            }
        }

        std::wstring arguments;

        // ====================================================
        // GENERAL OPTIONS
        // ====================================================

        if (options.overwrite)
        {
            arguments += L"-y ";
        }
        else
        {
            arguments += L"-n ";
        }

        arguments += L"-nostdin -hide_banner -loglevel error ";

        // ====================================================
        // INPUT
        // ====================================================

        arguments += L"-i ";
        arguments += quote_windows_argument(input);
        arguments += L" ";

        arguments += L"-map 0:a:";
        arguments += std::to_wstring(options.audio_stream_index);
        arguments += L" -vn -sn -dn ";

        // ====================================================
        // CODEC
        // ====================================================

        if (format == L"mp3")
        {
            arguments += L"-c:a libmp3lame ";

            if (options.mp3_vbr)
            {
                arguments += L"-q:a ";
                arguments += std::to_wstring(options.quality);
                arguments += L" ";
            }
            else
            {
                arguments += L"-b:a ";
                arguments += std::to_wstring(options.bitrate_kbps);
                arguments += L"k ";
            }
        }
        else if (format == L"aac" ||
            format == L"m4a")
        {
            arguments += L"-c:a aac ";
            arguments += L"-b:a ";
            arguments += std::to_wstring(options.bitrate_kbps);
            arguments += L"k ";
        }
        else if (format == L"flac")
        {
            arguments += L"-c:a flac ";
            arguments += L"-compression_level ";
            arguments += std::to_wstring(options.flac_compression_level);
            arguments += L" ";
        }
        else if (format == L"wav")
        {
            arguments += L"-c:a pcm_s16le ";
        }
        else if (format == L"ogg")
        {
            arguments += L"-c:a libvorbis ";
            arguments += L"-q:a ";
            arguments += std::to_wstring(options.quality);
            arguments += L" ";
        }
        else if (format == L"opus")
        {
            arguments += L"-c:a libopus ";
            arguments += L"-b:a ";
            arguments += std::to_wstring(options.bitrate_kbps);
            arguments += L"k ";
        }
        else
        {
            printf(
                "Unsupported format: %s\n",
                wide_to_utf8(format.c_str()).c_str()
            );

            set_audio_last_error("The selected audio output format is not supported.");
            return -1;
        }

        // ====================================================
        // SAMPLE RATE
        // ====================================================

        if (options.sample_rate > 0)
        {
            arguments += L"-ar ";
            arguments += std::to_wstring(options.sample_rate);
            arguments += L" ";
        }

        // ====================================================
        // CHANNELS
        // ====================================================

        if (options.channels > 0)
        {
            arguments += L"-ac ";
            arguments += std::to_wstring(options.channels);
            arguments += L" ";
        }

        // ====================================================
        // METADATA
        // ====================================================

        if (options.preserve_metadata)
        {
            arguments += L"-map_metadata 0 ";
        }
        else
        {
            arguments += L"-map_metadata -1 ";
        }

        // ====================================================
        // OUTPUT
        // ====================================================

        arguments += quote_windows_argument(output);

        // ====================================================
        // DEBUG
        // ====================================================

        printf("\nFFmpeg command:\n");

        printf(
            "  %s %s\n\n",
            wide_to_utf8(
                quote_windows_argument(ffmpeg_path).c_str()
            ).c_str(),
            wide_to_utf8(arguments.c_str()).c_str()
        );

        // ====================================================
        // RUN FFMPEG
        // ====================================================

        ProcessResult process_result =
            run_process(
                ffmpeg_path,
                arguments
            );

        if (!process_result.started)
        {
            return -1;
        }

        if (process_result.cancelled)
        {
            set_audio_last_error("Audio conversion was cancelled.");
            std::error_code remove_error;
            fs::remove(output_path, remove_error);
            return -1;
        }

        if (process_result.exit_code != 0)
        {
            printf(
                "FFmpeg error code: %d\n",
                process_result.exit_code
            );

            std::string message = process_result.output;
            if (message.empty())
            {
                message = "FFmpeg could not extract or convert the selected audio stream.";
            }
            set_audio_last_error(message);
            return process_result.exit_code;
        }

        bool output_exists = fs::is_regular_file(output_path, file_error);
        std::uintmax_t output_size = 0;

        if (output_exists && !file_error)
        {
            output_size = fs::file_size(output_path, file_error);
        }

        if (!output_exists || file_error || output_size == 0)
        {
            set_audio_last_error("FFmpeg finished without creating a valid audio file.");
            return -1;
        }

        set_audio_last_error("");
        return 0;
    }

    // ========================================================
    // AUDIO INFO
    // ========================================================

    struct AudioInfo
    {
        std::string codec;
        int sample_rate = 0;
        int channels = 0;
        double duration = 0.0;
        int bitrate = 0;
    };

    // ========================================================
    // GET AUDIO INFO
    // ========================================================

    AudioInfo get_audio_info(
        const std::wstring& input)
    {
        AudioInfo info;

        if (!initialized)
            return info;

        std::wstring command =
            quote_windows_argument(ffprobe_path) +
            L" -v error "
            L"-select_streams a:0 "
            L"-show_entries "
            L"stream=codec_name,sample_rate,channels,bit_rate "
            L"-show_entries format=duration "
            L"-of default=noprint_wrappers=1 " +
            quote_windows_argument(input);

        FILE* pipe =
            _wpopen(command.c_str(), L"r");

        if (!pipe)
            return info;

        wchar_t buffer[512] = {};

        while (fgetws(
            buffer,
            static_cast<int>(
                sizeof(buffer) / sizeof(wchar_t)
                ),
            pipe) != nullptr)
        {
            std::wstring line(buffer);

            while (!line.empty() &&
                (line.back() == L'\n' ||
                    line.back() == L'\r'))
            {
                line.pop_back();
            }

            size_t pos = line.find(L'=');

            if (pos == std::wstring::npos)
                continue;

            std::wstring key =
                line.substr(0, pos);

            std::wstring value =
                line.substr(pos + 1);

            try
            {
                if (key == L"codec_name")
                {
                    info.codec =
                        wide_to_utf8(value.c_str());
                }
                else if (key == L"sample_rate")
                {
                    info.sample_rate =
                        std::stoi(value);
                }
                else if (key == L"channels")
                {
                    info.channels =
                        std::stoi(value);
                }
                else if (key == L"bit_rate")
                {
                    info.bitrate =
                        std::stoi(value);
                }
                else if (key == L"duration")
                {
                    info.duration =
                        std::stod(value);
                }
            }
            catch (...)
            {
                // Ignoră valorile invalide
            }
        }

        _pclose(pipe);

        return info;
    }

    // ========================================================
    // BATCH CONVERT
    // ========================================================

    int batch_convert(
        const std::vector<std::wstring>& input_files,
        const std::wstring& output_folder,
        const std::wstring& format,
        int quality = 2)
    {
        if (!initialized)
        {
            printf("FFmpeg not initialized!\n");
            return -1;
        }

        std::string output_folder_utf8 =
            wide_to_utf8(output_folder.c_str());

        std::string format_utf8 =
            wide_to_utf8(format.c_str());

        // ====================================================
        // CREATE OUTPUT DIRECTORY
        // ====================================================

        std::error_code ec;

        if (!fs::exists(
            fs::path(output_folder),
            ec))
        {
            fs::create_directories(
                fs::path(output_folder),
                ec
            );

            if (ec)
            {
                printf(
                    "Cannot create output folder: %s\n",
                    output_folder_utf8.c_str()
                );

                return -1;
            }
        }

        int success = 0;
        int failed = 0;

        printf("\n");
        printf("==================================================\n");
        printf("Starting batch conversion...\n");
        printf(
            "Output folder: %s\n",
            output_folder_utf8.c_str()
        );
        printf(
            "Format: %s\n",
            format_utf8.c_str()
        );
        printf("==================================================\n\n");

        // ====================================================
        // PROCESS FILES
        // ====================================================

        for (size_t i = 0;
            i < input_files.size();
            ++i)
        {
            const std::wstring& input =
                input_files[i];

            fs::path input_path(input);

            std::wstring base_name =
                input_path.stem().wstring();

            fs::path output_path =
                fs::path(output_folder) /
                (base_name + L"." + format);

            printf(
                "[%zu/%zu] Converting: %s\n",
                i + 1,
                input_files.size(),
                wide_to_utf8(
                    input_path.filename().wstring().c_str()
                ).c_str()
            );

            printf(
                "  -> %s\n",
                wide_to_utf8(
                    output_path.filename().wstring().c_str()
                ).c_str()
            );

            // =================================================
            // AUDIO INFO
            // =================================================

            AudioInfo info =
                get_audio_info(input);

            if (info.sample_rate > 0)
            {
                printf(
                    "  %s | %d Hz | %d channels | %.1f s\n",
                    info.codec.c_str(),
                    info.sample_rate,
                    info.channels,
                    info.duration
                );
            }

            // =================================================
            // CONVERSION
            // =================================================

            FF_AUDIO_OPTIONS options = {};
            options.bitrate_kbps = 192;
            options.sample_rate = 0;
            options.channels = 0;
            options.quality = quality;
            options.audio_stream_index = 0;
            options.preserve_metadata = 1;
            options.overwrite = 1;
            options.mp3_vbr = 1;
            options.flac_compression_level = 8;

            int result =
                convert_audio(
                    input,
                    output_path.wstring(),
                    format,
                    options
                );

            if (result == 0)
            {
                ++success;
                printf("  SUCCESS\n\n");
            }
            else
            {
                ++failed;

                printf(
                    "  FAILED (error code: %d)\n\n",
                    result
                );
            }
        }

        // ====================================================
        // SUMMARY
        // ====================================================

        printf("\n");
        printf("==================================================\n");
        printf("Conversion complete:\n");
        printf("  Success: %d\n", success);
        printf("  Failed:  %d\n", failed);
        printf("==================================================\n");

        return success;
    }
};

static std::wstring normalize_audio_format(
    const wchar_t* output_format)
{
    if (!output_format)
        return L"";

    std::wstring format(output_format);

    if (!format.empty() &&
        format.front() == L'.')
    {
        format.erase(format.begin());
    }

    std::transform(
        format.begin(),
        format.end(),
        format.begin(),
        [](wchar_t c)
        {
            return static_cast<wchar_t>(
                std::towlower(c)
                );
        }
    );

    return format;
}


// ============================================================
// EXTENDED AUDIO CONVERSION API
// ============================================================

extern "C" AUDIOCONVERTER_API int ff_audio_convert_ex(
    const wchar_t* input_file,
    const wchar_t* output_file,
    const wchar_t* output_format,
    const FF_AUDIO_OPTIONS* options)
{
    if (!input_file ||
        !output_file ||
        !output_format ||
        !options)
    {
        printf("Invalid audio conversion arguments.\n");
        set_audio_last_error("Invalid audio conversion arguments.");
        return 0;
    }

    std::wstring format =
        normalize_audio_format(output_format);

    if (format.empty())
    {
        printf("Invalid audio output format.\n");
        set_audio_last_error("The audio output format is empty.");
        return 0;
    }

    FF_AUDIO_OPTIONS normalized = *options;

    if (normalized.bitrate_kbps <= 0)
        normalized.bitrate_kbps = 192;

    normalized.bitrate_kbps = std::clamp(normalized.bitrate_kbps, 32, 1000);

    if (normalized.sample_rate != 0 &&
        (normalized.sample_rate < 8000 || normalized.sample_rate > 384000))
    {
        normalized.sample_rate = 0;
    }

    if (normalized.channels < 0 ||
        normalized.channels > 2)
    {
        normalized.channels = 0;
    }

    if (normalized.quality < 0 ||
        normalized.quality > 9)
    {
        normalized.quality = 2;
    }

    if (normalized.audio_stream_index < 0)
        normalized.audio_stream_index = 0;

    normalized.preserve_metadata =
        normalized.preserve_metadata ? 1 : 0;

    normalized.overwrite =
        normalized.overwrite ? 1 : 0;

    normalized.mp3_vbr =
        normalized.mp3_vbr ? 1 : 0;

    if (normalized.flac_compression_level < 0 ||
        normalized.flac_compression_level > 12)
    {
        normalized.flac_compression_level = 8;
    }

    g_audio_cancel_requested.store(false);
    set_audio_last_error("");

    FFmpegManager ffmpeg;

    if (!ffmpeg.is_available())
    {
        printf("FFmpeg not initialized!\n");
        return 0;
    }

    return ffmpeg.convert_audio(
        input_file,
        output_file,
        format,
        normalized
    ) == 0 ? 1 : 0;
}


extern "C" AUDIOCONVERTER_API const char* ff_audio_get_last_error(void)
{
    return g_audio_last_error;
}


extern "C" AUDIOCONVERTER_API void ff_audio_cancel(void)
{
    g_audio_cancel_requested.store(true);

    std::lock_guard<std::mutex> lock(g_audio_process_mutex);
    if (g_audio_process)
    {
        TerminateProcess(g_audio_process, ERROR_CANCELLED);
    }
}


// ============================================================
// LEGACY AUDIO CONVERSION API
// ============================================================

extern "C" AUDIOCONVERTER_API int ff_audio_convert(
    const wchar_t* input_file,
    const wchar_t* output_file,
    const wchar_t* output_format)
{
    FF_AUDIO_OPTIONS options = {};

    options.bitrate_kbps = 192;
    options.sample_rate = 0;
    options.channels = 0;

    options.quality = 2;
    options.audio_stream_index = 0;

    options.preserve_metadata = 1;
    options.overwrite = 1;

    options.mp3_vbr = 1;
    options.flac_compression_level = 8;

    return ff_audio_convert_ex(
        input_file,
        output_file,
        output_format,
        &options
    );
}


// ============================================================
// SELECT INPUT FILES
// ============================================================

static bool select_input_files(
    std::vector<std::wstring>& files)
{
    wchar_t buffer[32768] = {};

    OPENFILENAMEW open_file = {};

    open_file.lStructSize =
        sizeof(open_file);

    open_file.hwndOwner =
        nullptr;

    open_file.lpstrFile =
        buffer;

    open_file.nMaxFile =
        sizeof(buffer) /
        sizeof(wchar_t);

    open_file.lpstrFilter =
        L"Audio Files\0"
        L"*.mp3;*.wav;*.flac;*.ogg;*.opus;*.m4a;*.aac;*.wma;*.aiff;*.alac\0"
        L"All Files\0"
        L"*.*\0";

    open_file.nFilterIndex = 1;

    open_file.Flags =
        OFN_PATHMUSTEXIST |
        OFN_FILEMUSTEXIST |
        OFN_HIDEREADONLY |
        OFN_ALLOWMULTISELECT |
        OFN_EXPLORER;

    if (!GetOpenFileNameW(&open_file))
        return false;

    wchar_t* first = buffer;

    size_t first_length =
        wcslen(first);

    wchar_t* second =
        first + first_length + 1;

    // ========================================================
    // SINGLE FILE
    // ========================================================

    if (*second == L'\0')
    {
        files.emplace_back(first);
        return true;
    }

    // ========================================================
    // MULTIPLE FILES
    // ========================================================

    std::wstring directory(first);

    wchar_t* filename = second;

    while (*filename)
    {
        std::wstring full_path =
            directory + L"\\" + filename;

        files.push_back(full_path);

        filename +=
            wcslen(filename) + 1;
    }

    return !files.empty();
}

// ============================================================
// SELECT OUTPUT FOLDER
// ============================================================

static bool select_output_folder(
    wchar_t* output_folder)
{
    BROWSEINFOW bi = {};

    bi.hwndOwner = nullptr;

    bi.lpszTitle =
        L"Select output folder";

    bi.ulFlags =
        BIF_RETURNONLYFSDIRS |
        BIF_NEWDIALOGSTYLE;

    LPITEMIDLIST pidl =
        SHBrowseForFolderW(&bi);

    if (!pidl)
        return false;

    bool success =
        SHGetPathFromIDListW(
            pidl,
            output_folder
        ) != FALSE;

    CoTaskMemFree(pidl);

    return success;
}

// ============================================================
// MAIN
// ============================================================

int wmain()
{
    // ========================================================
    // CONSOLE UTF-8
    // ========================================================

    SetConsoleOutputCP(CP_UTF8);

    printf("==================================================\n");
    printf("                 FORMATFORGE\n");
    printf("            Audio Converter (FFmpeg)\n");
    printf("==================================================\n\n");

    // ========================================================
    // INITIALIZE FFMPEG
    // ========================================================

    FFmpegManager ffmpeg;

    if (!ffmpeg.is_available())
    {
        printf("ERROR: FFmpeg not found!\n\n");

        printf(
            "Please install FFmpeg and add it to PATH.\n"
        );

        printf(
            "Download: https://ffmpeg.org/download.html\n\n"
        );

        printf(
            "Or specify the path in your system environment.\n"
        );

        printf("\nPress ENTER to exit...");

        getchar();

        return 1;
    }

    // ========================================================
    // VERSION
    // ========================================================

    std::string version =
        ffmpeg.get_version();

    if (!version.empty())
    {
        printf("%s", version.c_str());
    }

    // ========================================================
    // SELECT INPUT FILES
    // ========================================================

    std::vector<std::wstring> input_files;

    if (!select_input_files(input_files))
    {
        printf("No input files selected.\n");

        printf("\nPress ENTER to exit...");

        getchar();

        return 0;
    }

    printf(
        "\nSelected files: %zu\n\n",
        input_files.size()
    );

    // ========================================================
    // SELECT OUTPUT FORMAT
    // ========================================================

    printf("Select output format:\n\n");

    printf(
        "  [1] MP3  (VBR quality: 0-9, default: 2)\n"
    );

    printf(
        "  [2] FLAC (lossless)\n"
    );

    printf(
        "  [3] WAV  (uncompressed)\n"
    );

    printf(
        "  [4] OGG  (Vorbis)\n"
    );

    printf(
        "  [5] OPUS (low latency)\n"
    );

    printf(
        "  [6] M4A  (AAC)\n"
    );

    printf(
        "  [7] AAC  (raw AAC)\n"
    );

    printf("\nChoice: ");

    int choice = 0;

    if (scanf_s(
        "%d",
        &choice) != 1)
    {
        printf("Invalid choice.\n");

        printf("\nPress ENTER to exit...");

        getchar();
        getchar();

        return 1;
    }

    std::wstring format;

    int quality = 2;

    // ========================================================
    // FORMAT SELECTION
    // ========================================================

    switch (choice)
    {
    case 1:

        format = L"mp3";

        printf(
            "\nMP3 Quality "
            "(0=best, 9=worst, default 2): "
        );

        if (scanf_s(
            "%d",
            &quality) != 1)
        {
            quality = 2;
        }

        if (quality < 0 ||
            quality > 9)
        {
            quality = 2;
        }

        break;

    case 2:
        format = L"flac";
        break;

    case 3:
        format = L"wav";
        break;

    case 4:
        format = L"ogg";
        break;

    case 5:
        format = L"opus";
        break;

    case 6:
        format = L"m4a";
        break;

    case 7:
        format = L"aac";
        break;

    default:

        printf("Invalid format.\n");

        printf("\nPress ENTER to exit...");

        getchar();
        getchar();

        return 1;
    }

    // ========================================================
    // SELECT OUTPUT FOLDER
    // ========================================================

    wchar_t output_folder[MAX_PATH] = {};

    if (!select_output_folder(output_folder))
    {
        printf("No output folder selected.\n");

        printf("\nPress ENTER to exit...");

        getchar();

        return 0;
    }

    // ========================================================
    // BATCH CONVERSION
    // ========================================================

    int result =
        ffmpeg.batch_convert(
            input_files,
            output_folder,
            format,
            quality
        );

    // ========================================================
    // EXIT
    // ========================================================

    printf("\nPress ENTER to exit...");

    getchar();
    getchar();

    return result;
}
