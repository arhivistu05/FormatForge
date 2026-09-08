
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
#include <cwctype>

#include "audio_converter.h"

#pragma comment(lib, "Comdlg32.lib")
#pragma comment(lib, "Shell32.lib")
#pragma comment(lib, "Ole32.lib")

namespace fs = std::filesystem;

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

static int run_process(
    const std::wstring& executable,
    const std::wstring& arguments)
{
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

    STARTUPINFOW startup_info = {};
    startup_info.cb = sizeof(startup_info);

    PROCESS_INFORMATION process_info = {};

    BOOL created = CreateProcessW(
        nullptr,
        command_buffer.data(),
        nullptr,
        nullptr,
        FALSE,
        CREATE_NO_WINDOW,
        nullptr,
        nullptr,
        &startup_info,
        &process_info
    );

    if (!created)
    {
        DWORD error = GetLastError();

        printf(
            "CreateProcessW failed. Windows error: %lu\n",
            static_cast<unsigned long>(error)
        );

        return -1;
    }

    WaitForSingleObject(process_info.hProcess, INFINITE);

    DWORD exit_code = 0;

    if (!GetExitCodeProcess(
        process_info.hProcess,
        &exit_code))
    {
        CloseHandle(process_info.hThread);
        CloseHandle(process_info.hProcess);
        return -1;
    }

    CloseHandle(process_info.hThread);
    CloseHandle(process_info.hProcess);

    return static_cast<int>(exit_code);
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
        return
        {
            L"C:\\FFmpeg\\bin",
            L"C:\\Program Files\\FFmpeg\\bin",
            L"C:\\Program Files (x86)\\FFmpeg\\bin",
            L"F:\\FormatForge\\FFmpeg\\bin",
            L".\\FFmpeg\\bin",
            L".\\bin\\FFmpeg",
            L"C:\\ffmpeg\\bin"
        };
    }

    // ========================================================
    // INITIALIZE
    // ========================================================

    bool initialize()
    {
        // 1. PATH
        ffmpeg_path = find_in_path(L"ffmpeg.exe");
        ffprobe_path = find_in_path(L"ffprobe.exe");

        // 2. Common locations
        if (ffmpeg_path.empty() || ffprobe_path.empty())
        {
            auto common_paths = get_common_locations();

            for (const auto& directory : common_paths)
            {
                fs::path ffmpeg_test =
                    fs::path(directory) / L"ffmpeg.exe";

                fs::path ffprobe_test =
                    fs::path(directory) / L"ffprobe.exe";

                std::error_code ec;

                if (ffmpeg_path.empty() &&
                    fs::exists(ffmpeg_test, ec))
                {
                    ffmpeg_path = ffmpeg_test.wstring();
                }

                if (ffprobe_path.empty() &&
                    fs::exists(ffprobe_test, ec))
                {
                    ffprobe_path = ffprobe_test.wstring();
                }

                if (!ffmpeg_path.empty() &&
                    !ffprobe_path.empty())
                {
                    break;
                }
            }
        }

        initialized =
            !ffmpeg_path.empty() &&
            !ffprobe_path.empty();

        if (initialized)
        {
            printf("FFmpeg found:\n");
            printf(
                "  ffmpeg:  %s\n",
                wide_to_utf8(ffmpeg_path.c_str()).c_str()
            );

            printf(
                "  ffprobe: %s\n\n",
                wide_to_utf8(ffprobe_path.c_str()).c_str()
            );
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
        const std::wstring& format = L"mp3",
        int quality = 2,
        bool preserve_metadata = true,
        bool show_progress = false)
    {
        if (!initialized)
        {
            printf("FFmpeg not initialized!\n");
            return -1;
        }

        std::wstring arguments;

        // ====================================================
        // GENERAL OPTIONS
        // ====================================================

        arguments += L"-y ";

        if (!show_progress)
        {
            arguments += L"-hide_banner -loglevel error ";
        }

        // ====================================================
        // INPUT
        // ====================================================

        arguments += L"-i ";
        arguments += quote_windows_argument(input);
        arguments += L" ";

        // ====================================================
        // CODEC
        // ====================================================

        if (format == L"mp3")
        {
            arguments += L"-c:a libmp3lame ";
            arguments += L"-q:a ";
            arguments += std::to_wstring(quality);
            arguments += L" ";
        }
        else if (format == L"aac" ||
            format == L"m4a")
        {
            arguments += L"-c:a aac ";
            arguments += L"-b:a 192k ";
        }
        else if (format == L"flac")
        {
            arguments += L"-c:a flac ";
            arguments += L"-compression_level 8 ";
        }
        else if (format == L"wav")
        {
            arguments += L"-c:a pcm_s16le ";
        }
        else if (format == L"ogg")
        {
            arguments += L"-c:a libvorbis ";
            arguments += L"-q:a 4 ";
        }
        else if (format == L"opus")
        {
            arguments += L"-c:a libopus ";
            arguments += L"-b:a 128k ";
        }
        else
        {
            printf(
                "Unsupported format: %s\n",
                wide_to_utf8(format.c_str()).c_str()
            );

            return -1;
        }

        // ====================================================
        // METADATA
        // ====================================================

        if (preserve_metadata)
        {
            arguments += L"-map_metadata 0 ";
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

        int result =
            run_process(
                ffmpeg_path,
                arguments
            );

        if (result != 0)
        {
            printf(
                "FFmpeg error code: %d\n",
                result
            );
        }

        return result;
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

            int result =
                convert_audio(
                    input,
                    output_path.wstring(),
                    format,
                    quality
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

extern "C" AUDIOCONVERTER_API int ff_audio_convert(
    const wchar_t* input_file,
    const wchar_t* output_file,
    const wchar_t* output_format)
{
    if (!input_file || !output_file || !output_format)
    {
        printf("Invalid audio conversion arguments.\n");
        return 0;
    }

    std::wstring format(output_format);

    if (!format.empty() && format.front() == L'.')
    {
        format.erase(format.begin());
    }

    std::transform(
        format.begin(),
        format.end(),
        format.begin(),
        [](wchar_t c)
        {
            return static_cast<wchar_t>(std::towlower(c));
        }
    );

    if (format.empty())
    {
        printf("Invalid audio output format.\n");
        return 0;
    }

    FFmpegManager ffmpeg;

    if (!ffmpeg.is_available())
    {
        printf("FFmpeg not initialized!\n");
        return 0;
    }

    return ffmpeg.convert_audio(
        input_file,
        output_file,
        format
    ) == 0 ? 1 : 0;
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
