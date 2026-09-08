#define _CRT_SECURE_NO_WARNINGS
#define WIN32_LEAN_AND_MEAN

#include <Windows.h>
#include <commdlg.h>
#include <shlobj.h>
#include <shobjidl.h>

#include <cstdio>
#include <cstdlib>
#include <string>
#include <vector>
#include <filesystem>
#include <algorithm>
#include <thread>
#include <sstream>
#include <cwctype>

#include "video_converter.h"

#pragma comment(lib, "Comdlg32.lib")
#pragma comment(lib, "Shell32.lib")
#pragma comment(lib, "Ole32.lib")

namespace fs = std::filesystem;


// ============================================================
// UTF-16 -> UTF-8
// ============================================================

static std::string wide_to_utf8(const wchar_t* text)
{
    if (!text)
        return std::string();

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
        return std::string();

    std::string result(
        size - 1,
        '\0'
    );

    if (!result.empty())
    {
        WideCharToMultiByte(
            CP_UTF8,
            0,
            text,
            -1,
            &result[0],
            size,
            nullptr,
            nullptr
        );
    }

    return result;
}


// ============================================================
// UTF-8 -> UTF-16
// ============================================================

static std::wstring utf8_to_wide(
    const std::string& text)
{
    if (text.empty())
        return std::wstring();

    int size = MultiByteToWideChar(
        CP_UTF8,
        0,
        text.c_str(),
        -1,
        nullptr,
        0
    );

    if (size <= 0)
        return std::wstring();

    std::wstring result(
        size - 1,
        L'\0'
    );

    if (!result.empty())
    {
        MultiByteToWideChar(
            CP_UTF8,
            0,
            text.c_str(),
            -1,
            &result[0],
            size
        );
    }

    return result;
}


// ============================================================
// QUOTE WINDOWS COMMAND LINE ARGUMENT
//
// Handles:
//   spaces
//   Unicode
//   quotes
//   backslashes
//   parentheses
//   &
//   !
//   []
//   {}
//   apostrophes
//   etc.
//
// IMPORTANT:
// This is NOT passed through cmd.exe.
// ============================================================

static std::wstring quote_windows_argument(
    const std::wstring& value)
{
    std::wstring result;

    result.reserve(
        value.size() + 2
    );

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
            result.append(
                backslashes * 2 + 1,
                L'\\'
            );

            result += L'"';

            backslashes = 0;
        }
        else
        {
            result.append(
                backslashes,
                L'\\'
            );

            backslashes = 0;

            result += c;
        }
    }

    result.append(
        backslashes * 2,
        L'\\'
    );

    result += L'"';

    return result;
}


// ============================================================
// BUILD COMMAND LINE
// ============================================================

static std::wstring build_command_line(
    const std::wstring& executable,
    const std::wstring& arguments)
{
    std::wstring command_line;

    command_line += quote_windows_argument(
        executable
    );

    if (!arguments.empty())
    {
        command_line += L" ";
        command_line += arguments;
    }

    return command_line;
}


// ============================================================
// WINDOWS ERROR MESSAGE
// ============================================================

static std::wstring get_windows_error_message(
    DWORD error_code)
{
    LPWSTR message_buffer = nullptr;

    DWORD length = FormatMessageW(
        FORMAT_MESSAGE_ALLOCATE_BUFFER |
        FORMAT_MESSAGE_FROM_SYSTEM |
        FORMAT_MESSAGE_IGNORE_INSERTS,
        nullptr,
        error_code,
        MAKELANGID(
            LANG_NEUTRAL,
            SUBLANG_DEFAULT
        ),
        (LPWSTR)&message_buffer,
        0,
        nullptr
    );

    if (length == 0 ||
        message_buffer == nullptr)
    {
        return L"";
    }

    std::wstring message(
        message_buffer,
        length
    );

    LocalFree(
        message_buffer
    );

    while (!message.empty() &&
        (
            message.back() == L'\r' ||
            message.back() == L'\n' ||
            message.back() == L' '
            ))
    {
        message.pop_back();
    }

    return message;
}


// ============================================================
// PROCESS RESULT
// ============================================================

struct ProcessResult
{
    int exit_code = -1;

    std::wstring output;

    bool started = false;
};


// ============================================================
// RUN PROCESS + CAPTURE OUTPUT
//
// NO _wpopen()
// NO cmd.exe
//
// Output is read from a separate thread so a large FFmpeg
// error message cannot block the process.
// ============================================================

static ProcessResult run_process_capture(
    const std::wstring& executable,
    const std::wstring& arguments)
{
    ProcessResult result;


    // --------------------------------------------------------
    // PIPE SECURITY
    // --------------------------------------------------------

    SECURITY_ATTRIBUTES security_attributes = {};

    security_attributes.nLength =
        sizeof(security_attributes);

    security_attributes.bInheritHandle =
        TRUE;


    // --------------------------------------------------------
    // CREATE PIPE
    // --------------------------------------------------------

    HANDLE read_pipe = nullptr;
    HANDLE write_pipe = nullptr;

    if (!CreatePipe(
        &read_pipe,
        &write_pipe,
        &security_attributes,
        0))
    {
        return result;
    }


    // Parent keeps read handle.
    // Child receives only write handle.

    if (!SetHandleInformation(
        read_pipe,
        HANDLE_FLAG_INHERIT,
        0))
    {
        CloseHandle(read_pipe);
        CloseHandle(write_pipe);

        return result;
    }


    // --------------------------------------------------------
    // BUILD COMMAND LINE
    // --------------------------------------------------------

    std::wstring command_line =
        build_command_line(
            executable,
            arguments
        );

    std::vector<wchar_t> command_buffer(
        command_line.begin(),
        command_line.end()
    );

    command_buffer.push_back(
        L'\0'
    );


    // --------------------------------------------------------
    // STARTUP INFO
    // --------------------------------------------------------

    STARTUPINFOW startup_info = {};

    startup_info.cb =
        sizeof(startup_info);

    startup_info.dwFlags =
        STARTF_USESTDHANDLES;

    startup_info.hStdOutput =
        write_pipe;

    startup_info.hStdError =
        write_pipe;

    startup_info.hStdInput =
        GetStdHandle(
            STD_INPUT_HANDLE
        );


    PROCESS_INFORMATION process_info = {};


    // --------------------------------------------------------
    // CREATE PROCESS
    // --------------------------------------------------------

    BOOL created = CreateProcessW(
        nullptr,
        command_buffer.data(),
        nullptr,
        nullptr,
        TRUE,
        CREATE_NO_WINDOW |
        CREATE_UNICODE_ENVIRONMENT,
        nullptr,
        nullptr,
        &startup_info,
        &process_info
    );


    // Parent no longer needs write handle.

    CloseHandle(
        write_pipe
    );

    write_pipe = nullptr;


    // --------------------------------------------------------
    // PROCESS FAILED
    // --------------------------------------------------------

    if (!created)
    {
        DWORD error_code =
            GetLastError();

        std::wstring error_message =
            get_windows_error_message(
                error_code
            );

        printf(
            "\nCreateProcessW FAILED!\n\n"
        );

        printf(
            "Executable:\n%s\n\n",
            wide_to_utf8(
                executable.c_str()
            ).c_str()
        );

        printf(
            "Command line:\n%s\n\n",
            wide_to_utf8(
                command_line.c_str()
            ).c_str()
        );

        printf(
            "Error code: %lu\n",
            error_code
        );

        if (!error_message.empty())
        {
            printf(
                "Windows message: %s\n",
                wide_to_utf8(
                    error_message.c_str()
                ).c_str()
            );
        }

        CloseHandle(
            read_pipe
        );

        return result;
    }


    result.started = true;


    // --------------------------------------------------------
    // READ OUTPUT IN PARALLEL
    // --------------------------------------------------------

    std::string output_utf8;

    std::thread reader_thread(
        [&output_utf8, read_pipe]()
        {
            char buffer[4096];

            DWORD bytes_read = 0;

            while (true)
            {
                BOOL success = ReadFile(
                    read_pipe,
                    buffer,
                    sizeof(buffer),
                    &bytes_read,
                    nullptr
                );

                if (!success ||
                    bytes_read == 0)
                {
                    break;
                }

                output_utf8.append(
                    buffer,
                    bytes_read
                );
            }

            CloseHandle(
                read_pipe
            );
        }
    );


    // --------------------------------------------------------
    // WAIT FOR PROCESS
    // --------------------------------------------------------

    WaitForSingleObject(
        process_info.hProcess,
        INFINITE
    );


    // --------------------------------------------------------
    // WAIT FOR READER
    // --------------------------------------------------------

    reader_thread.join();


    // --------------------------------------------------------
    // GET EXIT CODE
    // --------------------------------------------------------

    DWORD exit_code = 0;

    if (!GetExitCodeProcess(
        process_info.hProcess,
        &exit_code))
    {
        CloseHandle(
            process_info.hThread
        );

        CloseHandle(
            process_info.hProcess
        );

        return result;
    }


    // --------------------------------------------------------
    // CLOSE PROCESS
    // --------------------------------------------------------

    CloseHandle(
        process_info.hThread
    );

    CloseHandle(
        process_info.hProcess
    );


    // --------------------------------------------------------
    // RESULT
    // --------------------------------------------------------

    result.exit_code =
        static_cast<int>(
            exit_code
            );

    result.output =
        utf8_to_wide(
            output_utf8
        );

    return result;
}


// ============================================================
// APPLICATION DIRECTORY
// ============================================================

static std::wstring get_application_directory()
{
    std::vector<wchar_t> buffer(
        32768
    );

    DWORD length =
        GetModuleFileNameW(
            nullptr,
            buffer.data(),
            static_cast<DWORD>(
                buffer.size()
                )
        );

    if (length == 0)
        return L"";

    if (length >= buffer.size())
        return L"";

    fs::path executable_path(
        std::wstring(
            buffer.data(),
            length
        )
    );

    return executable_path
        .parent_path()
        .wstring();
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


    // ========================================================
    // FIND EXECUTABLE USING SearchPathW
    // ========================================================

    std::wstring find_using_search_path(
        const std::wstring& exe_name)
    {
        DWORD buffer_size = MAX_PATH;

        while (true)
        {
            std::vector<wchar_t> buffer(
                buffer_size
            );

            DWORD length =
                SearchPathW(
                    nullptr,
                    exe_name.c_str(),
                    nullptr,
                    buffer_size,
                    buffer.data(),
                    nullptr
                );

            if (length == 0)
                return L"";

            if (length < buffer_size)
            {
                return std::wstring(
                    buffer.data(),
                    length
                );
            }

            buffer_size =
                length + 1;
        }
    }


    // ========================================================
    // GET POSSIBLE FFMPEG DIRECTORIES
    // ========================================================

    std::vector<std::wstring>
        get_search_directories()
    {
        std::vector<std::wstring>
            directories;


        auto add_directory =
            [&directories](
                const fs::path& path)
            {
                std::error_code ec;

                if (path.empty())
                    return;

                fs::path absolute_path =
                    fs::absolute(
                        path,
                        ec
                    );

                std::wstring value =
                    ec
                    ? path.wstring()
                    : absolute_path.wstring();

                if (std::find(
                    directories.begin(),
                    directories.end(),
                    value
                ) == directories.end())
                {
                    directories.push_back(
                        value
                    );
                }
            };


        // ----------------------------------------------------
        // ENVIRONMENT VARIABLE
        //
        // FFMPEG_BIN=C:\Something\FFmpeg\bin
        // ----------------------------------------------------

        std::vector<wchar_t> env_buffer(
            32768
        );

        DWORD env_length =
            GetEnvironmentVariableW(
                L"FFMPEG_BIN",
                env_buffer.data(),
                static_cast<DWORD>(
                    env_buffer.size()
                    )
            );

        if (env_length > 0 &&
            env_length < env_buffer.size())
        {
            add_directory(
                fs::path(
                    env_buffer.data()
                )
            );
        }


        // ----------------------------------------------------
        // APPLICATION DIRECTORY
        // ----------------------------------------------------

        std::wstring app_dir =
            get_application_directory();

        if (!app_dir.empty())
        {
            fs::path current(
                app_dir
            );


            // Search several parent levels.
            //
            // Example:
            //
            // Project\
            //   FFmpeg\bin\
            //   x64\Debug\
            //       FormatForge.exe
            //

            for (int level = 0;
                level < 6 && !current.empty();
                ++level)
            {
                add_directory(
                    current /
                    L"FFmpeg" /
                    L"bin"
                );

                add_directory(
                    current /
                    L"ffmpeg" /
                    L"bin"
                );

                add_directory(
                    current /
                    L"bin"
                );

                add_directory(
                    current /
                    L"bin" /
                    L"FFmpeg"
                );

                fs::path parent =
                    current.parent_path();

                if (parent == current)
                    break;

                current = parent;
            }
        }


        // ----------------------------------------------------
        // CURRENT WORKING DIRECTORY
        // ----------------------------------------------------

        std::error_code ec;

        fs::path working_directory =
            fs::current_path(
                ec
            );

        if (!ec)
        {
            add_directory(
                working_directory /
                L"FFmpeg" /
                L"bin"
            );

            add_directory(
                working_directory /
                L"ffmpeg" /
                L"bin"
            );

            add_directory(
                working_directory /
                L"bin"
            );
        }


        // ----------------------------------------------------
        // KNOWN LOCATIONS
        // ----------------------------------------------------

        add_directory(
            L"C:\\FFmpeg\\bin"
        );

        add_directory(
            L"C:\\ffmpeg\\bin"
        );

        add_directory(
            L"C:\\Program Files\\FFmpeg\\bin"
        );

        add_directory(
            L"C:\\Program Files (x86)\\FFmpeg\\bin"
        );

        add_directory(
            L"F:\\FormatForge\\FFmpeg\\bin"
        );


        return directories;
    }


    // ========================================================
    // FIND EXECUTABLE IN DIRECTORIES
    // ========================================================

    std::wstring find_in_directories(
        const std::wstring& exe_name)
    {
        auto directories =
            get_search_directories();


        for (const auto& directory :
            directories)
        {
            fs::path executable =
                fs::path(directory) /
                exe_name;

            std::error_code ec;

            if (fs::is_regular_file(
                executable,
                ec))
            {
                return executable.wstring();
            }
        }


        return L"";
    }


public:

    // ========================================================
    // CONSTRUCTOR
    // ========================================================

    FFmpegManager()
    {
        initialize();
    }


    // ========================================================
    // INITIALIZE
    // ========================================================

    bool initialize()
    {
        printf(
            "Searching for FFmpeg...\n\n"
        );


        // ----------------------------------------------------
        // SEARCH PATH
        // ----------------------------------------------------

        ffmpeg_path =
            find_using_search_path(
                L"ffmpeg.exe"
            );

        ffprobe_path =
            find_using_search_path(
                L"ffprobe.exe"
            );


        // ----------------------------------------------------
        // SEARCH CUSTOM DIRECTORIES
        // ----------------------------------------------------

        if (ffmpeg_path.empty())
        {
            ffmpeg_path =
                find_in_directories(
                    L"ffmpeg.exe"
                );
        }

        if (ffprobe_path.empty())
        {
            ffprobe_path =
                find_in_directories(
                    L"ffprobe.exe"
                );
        }


        // ----------------------------------------------------
        // FFmpeg IS THE IMPORTANT ONE
        //
        // ffprobe is optional.
        // Conversion can still work without it.
        // ----------------------------------------------------

        initialized =
            !ffmpeg_path.empty();


        if (!initialized)
        {
            printf(
                "FFmpeg initialization FAILED.\n\n"
            );

            printf(
                "ffmpeg.exe: NOT FOUND\n"
            );

            printf(
                "ffprobe.exe: %s\n\n",
                ffprobe_path.empty()
                ? "NOT FOUND"
                : wide_to_utf8(
                    ffprobe_path.c_str()
                ).c_str()
            );

            return false;
        }


        printf(
            "FFmpeg found successfully!\n\n"
        );


        printf(
            "ffmpeg:\n%s\n\n",
            wide_to_utf8(
                ffmpeg_path.c_str()
            ).c_str()
        );


        if (!ffprobe_path.empty())
        {
            printf(
                "ffprobe:\n%s\n\n",
                wide_to_utf8(
                    ffprobe_path.c_str()
                ).c_str()
            );
        }
        else
        {
            printf(
                "ffprobe: NOT FOUND\n"
            );

            printf(
                "Media information will be skipped.\n\n"
            );
        }


        return true;
    }


    // ========================================================
    // AVAILABILITY
    // ========================================================

    bool is_available() const
    {
        return initialized;
    }


    // ========================================================
    // HAS FFPROBE
    // ========================================================

    bool has_ffprobe() const
    {
        return !ffprobe_path.empty();
    }


    // ========================================================
    // VERSION
    // ========================================================

    std::string get_version()
    {
        if (!initialized)
            return "";


        ProcessResult result =
            run_process_capture(
                ffmpeg_path,
                L"-version"
            );


        if (!result.started)
            return "";


        if (result.exit_code != 0)
            return "";


        std::wstring first_line =
            result.output;


        size_t newline =
            first_line.find(
                L'\n'
            );


        if (newline !=
            std::wstring::npos)
        {
            first_line =
                first_line.substr(
                    0,
                    newline
                );
        }


        while (!first_line.empty() &&
            first_line.back() == L'\r')
        {
            first_line.pop_back();
        }


        return wide_to_utf8(
            first_line.c_str()
        );
    }


    // ========================================================
    // VIDEO INFO
    // ========================================================

    struct VideoInfo
    {
        std::string video_codec;
        std::string audio_codec;

        int width = 0;
        int height = 0;

        double fps = 0.0;
        double duration = 0.0;

        int video_bitrate = 0;
        int audio_bitrate = 0;

        int audio_channels = 0;
        int sample_rate = 0;
    };


private:

    // ========================================================
    // PARSE VIDEO STREAM
    // ========================================================

    void parse_video_info(
        const std::wstring& data,
        VideoInfo& info)
    {
        size_t start = 0;


        while (start < data.size())
        {
            size_t end =
                data.find(
                    L'\n',
                    start
                );


            if (end ==
                std::wstring::npos)
            {
                end =
                    data.size();
            }


            std::wstring line =
                data.substr(
                    start,
                    end - start
                );


            while (!line.empty() &&
                (
                    line.back() == L'\r' ||
                    line.back() == L'\n'
                    ))
            {
                line.pop_back();
            }


            size_t pos =
                line.find(
                    L'='
                );


            if (pos !=
                std::wstring::npos)
            {
                std::wstring key =
                    line.substr(
                        0,
                        pos
                    );

                std::wstring value =
                    line.substr(
                        pos + 1
                    );


                try
                {
                    if (key == L"codec_name")
                    {
                        info.video_codec =
                            wide_to_utf8(
                                value.c_str()
                            );
                    }
                    else if (key == L"width")
                    {
                        info.width =
                            std::stoi(
                                value
                            );
                    }
                    else if (key == L"height")
                    {
                        info.height =
                            std::stoi(
                                value
                            );
                    }
                    else if (key == L"bit_rate")
                    {
                        info.video_bitrate =
                            std::stoi(
                                value
                            );
                    }
                    else if (key == L"r_frame_rate")
                    {
                        size_t slash =
                            value.find(
                                L'/'
                            );


                        if (slash !=
                            std::wstring::npos)
                        {
                            double numerator =
                                std::stod(
                                    value.substr(
                                        0,
                                        slash
                                    )
                                );

                            double denominator =
                                std::stod(
                                    value.substr(
                                        slash + 1
                                    )
                                );


                            if (denominator != 0.0)
                            {
                                info.fps =
                                    numerator /
                                    denominator;
                            }
                        }
                    }
                }
                catch (...)
                {
                }
            }


            if (end == data.size())
                break;


            start =
                end + 1;
        }
    }


    // ========================================================
    // PARSE AUDIO STREAM
    // ========================================================

    void parse_audio_info(
        const std::wstring& data,
        VideoInfo& info)
    {
        size_t start = 0;


        while (start < data.size())
        {
            size_t end =
                data.find(
                    L'\n',
                    start
                );


            if (end ==
                std::wstring::npos)
            {
                end =
                    data.size();
            }


            std::wstring line =
                data.substr(
                    start,
                    end - start
                );


            while (!line.empty() &&
                (
                    line.back() == L'\r' ||
                    line.back() == L'\n'
                    ))
            {
                line.pop_back();
            }


            size_t pos =
                line.find(
                    L'='
                );


            if (pos !=
                std::wstring::npos)
            {
                std::wstring key =
                    line.substr(
                        0,
                        pos
                    );

                std::wstring value =
                    line.substr(
                        pos + 1
                    );


                try
                {
                    if (key == L"codec_name")
                    {
                        info.audio_codec =
                            wide_to_utf8(
                                value.c_str()
                            );
                    }
                    else if (key == L"sample_rate")
                    {
                        info.sample_rate =
                            std::stoi(
                                value
                            );
                    }
                    else if (key == L"channels")
                    {
                        info.audio_channels =
                            std::stoi(
                                value
                            );
                    }
                    else if (key == L"bit_rate")
                    {
                        info.audio_bitrate =
                            std::stoi(
                                value
                            );
                    }
                }
                catch (...)
                {
                }
            }


            if (end == data.size())
                break;


            start =
                end + 1;
        }
    }


public:

    // ========================================================
    // GET VIDEO INFO
    // ========================================================

    VideoInfo get_video_info(
        const std::wstring& input)
    {
        VideoInfo info;


        if (!initialized ||
            ffprobe_path.empty())
        {
            return info;
        }


        // ====================================================
        // VIDEO
        // ====================================================

        std::wstring video_arguments =
            L"-v error "
            L"-select_streams v:0 "
            L"-show_entries "
            L"stream=codec_name,width,height,r_frame_rate,bit_rate "
            L"-of default=noprint_wrappers=1 ";

        video_arguments +=
            quote_windows_argument(
                input
            );


        ProcessResult video_result =
            run_process_capture(
                ffprobe_path,
                video_arguments
            );


        if (video_result.started &&
            video_result.exit_code == 0)
        {
            parse_video_info(
                video_result.output,
                info
            );
        }


        // ====================================================
        // AUDIO
        // ====================================================

        std::wstring audio_arguments =
            L"-v error "
            L"-select_streams a:0 "
            L"-show_entries "
            L"stream=codec_name,sample_rate,channels,bit_rate "
            L"-of default=noprint_wrappers=1 ";

        audio_arguments +=
            quote_windows_argument(
                input
            );


        ProcessResult audio_result =
            run_process_capture(
                ffprobe_path,
                audio_arguments
            );


        if (audio_result.started &&
            audio_result.exit_code == 0)
        {
            parse_audio_info(
                audio_result.output,
                info
            );
        }


        // ====================================================
        // DURATION
        // ====================================================

        std::wstring duration_arguments =
            L"-v error "
            L"-show_entries format=duration "
            L"-of default=noprint_wrappers=1:nokey=1 ";

        duration_arguments +=
            quote_windows_argument(
                input
            );


        ProcessResult duration_result =
            run_process_capture(
                ffprobe_path,
                duration_arguments
            );


        if (duration_result.started &&
            duration_result.exit_code == 0)
        {
            try
            {
                info.duration =
                    std::stod(
                        duration_result.output
                    );
            }
            catch (...)
            {
            }
        }


        return info;
    }


    // ========================================================
    // CONVERT VIDEO
    // ========================================================

    int convert_video(
        const std::wstring& input,
        const std::wstring& output,
        const std::wstring& format = L"mp4",
        int quality = 23)
    {
        if (!initialized)
        {
            printf(
                "FFmpeg not initialized!\n"
            );

            return -1;
        }


        // ----------------------------------------------------
        // INPUT CHECK
        // ----------------------------------------------------

        std::error_code ec;

        if (!fs::is_regular_file(
            fs::path(input),
            ec))
        {
            printf(
                "Input file does not exist:\n%s\n",
                wide_to_utf8(
                    input.c_str()
                ).c_str()
            );

            return -1;
        }


        // ----------------------------------------------------
        // OUTPUT DIRECTORY
        // ----------------------------------------------------

        fs::path output_path(
            output
        );

        fs::path output_directory =
            output_path.parent_path();


        if (!output_directory.empty())
        {
            if (!fs::exists(
                output_directory,
                ec))
            {
                fs::create_directories(
                    output_directory,
                    ec
                );

                if (ec)
                {
                    printf(
                        "Cannot create output directory:\n%s\n",
                        wide_to_utf8(
                            output_directory.wstring().c_str()
                        ).c_str()
                    );

                    return -1;
                }
            }
        }


        // ----------------------------------------------------
        // QUALITY VALIDATION
        // ----------------------------------------------------

        if (quality < 18 ||
            quality > 28)
        {
            quality = 23;
        }


        // ----------------------------------------------------
        // BUILD ARGUMENTS
        // ----------------------------------------------------

        std::wstring arguments;


        // General options

        arguments +=
            L"-y ";

        arguments +=
            L"-nostdin ";

        arguments +=
            L"-hide_banner ";

        arguments +=
            L"-loglevel error ";


        // ----------------------------------------------------
        // INPUT
        // ----------------------------------------------------

        arguments +=
            L"-i ";

        arguments +=
            quote_windows_argument(
                input
            );

        arguments +=
            L" ";


        // ----------------------------------------------------
        // STREAM MAPPING
        //
        // Video is required.
        // Audio is optional.
        // ----------------------------------------------------

        arguments +=
            L"-map 0:v:0 ";

        arguments +=
            L"-map 0:a:0? ";


        // ----------------------------------------------------
        // CODECS
        // ----------------------------------------------------

        if (format == L"mp4" ||
            format == L"mkv" ||
            format == L"mov")
        {
            arguments +=
                L"-c:v libx264 ";

            arguments +=
                L"-crf ";

            arguments +=
                std::to_wstring(
                    quality
                );

            arguments +=
                L" ";

            arguments +=
                L"-preset medium ";

            arguments +=
                L"-c:a aac ";

            arguments +=
                L"-b:a 192k ";


            // Better compatibility.

            arguments +=
                L"-pix_fmt yuv420p ";


            if (format == L"mp4" ||
                format == L"mov")
            {
                arguments +=
                    L"-movflags +faststart ";
            }
        }
        else if (format == L"avi")
        {
            arguments +=
                L"-c:v mpeg4 ";

            arguments +=
                L"-q:v 5 ";

            arguments +=
                L"-c:a libmp3lame ";

            arguments +=
                L"-b:a 192k ";
        }
        else if (format == L"webm")
        {
            arguments +=
                L"-c:v libvpx-vp9 ";

            arguments +=
                L"-crf ";

            arguments +=
                std::to_wstring(
                    quality
                );

            arguments +=
                L" ";

            arguments +=
                L"-b:v 0 ";

            arguments +=
                L"-c:a libopus ";

            arguments +=
                L"-b:a 128k ";
        }
        else
        {
            printf(
                "Unsupported video format: %s\n",
                wide_to_utf8(
                    format.c_str()
                ).c_str()
            );

            return -1;
        }


        // ----------------------------------------------------
        // METADATA
        // ----------------------------------------------------

        arguments +=
            L"-map_metadata 0 ";


        // ----------------------------------------------------
        // OUTPUT
        // ----------------------------------------------------

        arguments +=
            quote_windows_argument(
                output
            );


        // ----------------------------------------------------
        // DISPLAY COMMAND
        // ----------------------------------------------------

        printf("\n");

        printf(
            "==================================================\n"
        );

        printf(
            "FFmpeg command:\n"
        );

        std::wstring full_command =
            build_command_line(
                ffmpeg_path,
                arguments
            );

        printf(
            "%s\n",
            wide_to_utf8(
                full_command.c_str()
            ).c_str()
        );

        printf(
            "==================================================\n\n"
        );


        // ----------------------------------------------------
        // RUN FFMPEG
        // ----------------------------------------------------

        ProcessResult result =
            run_process_capture(
                ffmpeg_path,
                arguments
            );


        if (!result.started)
        {
            printf(
                "Could not start FFmpeg.\n"
            );

            return -1;
        }


        // ----------------------------------------------------
        // DISPLAY FFMPEG ERROR
        // ----------------------------------------------------

        if (!result.output.empty())
        {
            printf(
                "%s\n",
                wide_to_utf8(
                    result.output.c_str()
                ).c_str()
            );
        }


        // ----------------------------------------------------
        // FFMPEG FAILED
        // ----------------------------------------------------

        if (result.exit_code != 0)
        {
            printf(
                "\nFFmpeg FAILED!\n"
            );

            printf(
                "FFmpeg exit code: %d\n",
                result.exit_code
            );

            return result.exit_code;
        }


        // ----------------------------------------------------
        // VERIFY OUTPUT
        // ----------------------------------------------------

        if (!fs::is_regular_file(
            output_path,
            ec))
        {
            printf(
                "\nERROR: FFmpeg returned success, "
                "but output file was not found.\n"
            );

            printf(
                "Expected output:\n%s\n",
                wide_to_utf8(
                    output.c_str()
                ).c_str()
            );

            return -1;
        }


        // ----------------------------------------------------
        // SUCCESS
        // ----------------------------------------------------

        printf(
            "\nFFmpeg finished successfully.\n"
        );

        printf(
            "Output file created successfully.\n"
        );


        return 0;
    }


    // ========================================================
    // BATCH CONVERSION
    // ========================================================

    int batch_convert(
        const std::vector<std::wstring>& input_files,
        const std::wstring& output_folder,
        const std::wstring& format,
        int quality = 23)
    {
        if (!initialized)
        {
            printf(
                "FFmpeg not initialized!\n"
            );

            return -1;
        }


        if (input_files.empty())
        {
            printf(
                "No input files.\n"
            );

            return -1;
        }


        // ----------------------------------------------------
        // OUTPUT DIRECTORY
        // ----------------------------------------------------

        std::error_code ec;

        fs::path output_dir(
            output_folder
        );


        if (!fs::exists(
            output_dir,
            ec))
        {
            fs::create_directories(
                output_dir,
                ec
            );


            if (ec)
            {
                printf(
                    "Cannot create output folder:\n%s\n",
                    wide_to_utf8(
                        output_folder.c_str()
                    ).c_str()
                );

                return -1;
            }
        }


        if (!fs::is_directory(
            output_dir,
            ec))
        {
            printf(
                "Output path is not a directory:\n%s\n",
                wide_to_utf8(
                    output_folder.c_str()
                ).c_str()
            );

            return -1;
        }


        // ----------------------------------------------------
        // HEADER
        // ----------------------------------------------------

        int success = 0;
        int failed = 0;


        printf("\n");

        printf(
            "==================================================\n"
        );

        printf(
            "             STARTING VIDEO CONVERSION\n"
        );

        printf(
            "==================================================\n\n"
        );


        printf(
            "Output folder:\n%s\n\n",
            wide_to_utf8(
                output_folder.c_str()
            ).c_str()
        );


        printf(
            "Format: %s\n",
            wide_to_utf8(
                format.c_str()
            ).c_str()
        );


        printf(
            "Quality: CRF %d\n\n",
            quality
        );


        // ----------------------------------------------------
        // FILE LOOP
        // ----------------------------------------------------

        for (size_t i = 0;
            i < input_files.size();
            ++i)
        {
            const std::wstring& input =
                input_files[i];


            fs::path input_path(
                input
            );


            std::wstring base_name =
                input_path.stem().wstring();


            fs::path output_path =
                output_dir /
                (
                    base_name +
                    L"." +
                    format
                    );


            printf(
                "--------------------------------------------------\n"
            );


            printf(
                "[%zu/%zu] Converting\n",
                i + 1,
                input_files.size()
            );


            printf(
                "  Input : %s\n",
                wide_to_utf8(
                    input_path.wstring().c_str()
                ).c_str()
            );


            printf(
                "  Output: %s\n",
                wide_to_utf8(
                    output_path.wstring().c_str()
                ).c_str()
            );


            // ------------------------------------------------
            // INPUT CHECK
            // ------------------------------------------------

            if (!fs::is_regular_file(
                input_path,
                ec))
            {
                printf(
                    "  ERROR: Input file does not exist.\n\n"
                );

                ++failed;

                continue;
            }


            // ------------------------------------------------
            // MEDIA INFORMATION
            // ------------------------------------------------

            if (has_ffprobe())
            {
                VideoInfo info =
                    get_video_info(
                        input
                    );


                if (!info.video_codec.empty())
                {
                    printf(
                        "  Video : %s | %dx%d | %.2f FPS\n",
                        info.video_codec.c_str(),
                        info.width,
                        info.height,
                        info.fps
                    );
                }


                if (!info.audio_codec.empty())
                {
                    printf(
                        "  Audio : %s | %d Hz | %d channels\n",
                        info.audio_codec.c_str(),
                        info.sample_rate,
                        info.audio_channels
                    );
                }


                if (info.duration > 0)
                {
                    printf(
                        "  Duration: %.1f seconds\n",
                        info.duration
                    );
                }
            }


            // ------------------------------------------------
            // CONVERT
            // ------------------------------------------------

            int conversion_result =
                convert_video(
                    input,
                    output_path.wstring(),
                    format,
                    quality
                );


            // ------------------------------------------------
            // RESULT
            // ------------------------------------------------

            if (conversion_result == 0)
            {
                ++success;

                printf(
                    "\n  [SUCCESS] File converted correctly.\n"
                );
            }
            else
            {
                ++failed;

                printf(
                    "\n  [FAILED] Conversion failed.\n"
                );

                printf(
                    "  Error code: %d\n",
                    conversion_result
                );
            }


            printf("\n");
        }


        // ====================================================
        // FINAL SUMMARY
        // ====================================================

        printf("\n");

        printf(
            "==================================================\n"
        );

        printf(
            "                 CONVERSION SUMMARY\n"
        );

        printf(
            "==================================================\n\n"
        );


        printf(
            "  Total files : %zu\n",
            input_files.size()
        );


        printf(
            "  Successful  : %d\n",
            success
        );


        printf(
            "  Failed      : %d\n",
            failed
        );


        printf("\n");


        // ====================================================
        // FINAL STATUS
        // ====================================================

        if (failed == 0 &&
            success ==
            static_cast<int>(
                input_files.size()
                ))
        {
            printf(
                "==================================================\n"
            );

            printf(
                "       CONVERSION FINISHED SUCCESSFULLY!\n"
            );

            printf(
                "       All files were converted correctly.\n"
            );

            printf(
                "==================================================\n"
            );
        }
        else
        {
            printf(
                "==================================================\n"
            );

            printf(
                "        CONVERSION FINISHED WITH ERRORS!\n"
            );

            printf(
                "        Some files could not be converted.\n"
            );

            printf(
                "==================================================\n"
            );
        }


        printf("\n");


        return failed == 0
            ? 0
            : 1;
    }
};


// ============================================================
// SELECT INPUT FILES
// ============================================================

static bool select_input_files(
    std::vector<std::wstring>& files)
{
    files.clear();


    // Large buffer for multiple files.

    std::vector<wchar_t> buffer(
        65536,
        L'\0'
    );


    OPENFILENAMEW open_file = {};


    open_file.lStructSize =
        sizeof(open_file);


    open_file.hwndOwner =
        nullptr;


    open_file.lpstrFile =
        buffer.data();


    open_file.nMaxFile =
        static_cast<DWORD>(
            buffer.size()
            );


    open_file.lpstrFilter =
        L"Video Files\0"
        L"*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.webm;"
        L"*.flv;*.mpeg;*.mpg;*.m4v;*.ts\0"
        L"All Files\0"
        L"*.*\0";


    open_file.nFilterIndex =
        1;


    open_file.Flags =
        OFN_PATHMUSTEXIST |
        OFN_FILEMUSTEXIST |
        OFN_HIDEREADONLY |
        OFN_ALLOWMULTISELECT |
        OFN_EXPLORER;


    if (!GetOpenFileNameW(
        &open_file))
    {
        return false;
    }


    wchar_t* first =
        buffer.data();


    size_t first_length =
        wcslen(first);


    wchar_t* second =
        first +
        first_length +
        1;


    // --------------------------------------------------------
    // SINGLE FILE
    // --------------------------------------------------------

    if (*second == L'\0')
    {
        files.emplace_back(
            first
        );

        return true;
    }


    // --------------------------------------------------------
    // MULTIPLE FILES
    //
    // first = directory
    // second+ = filenames
    // --------------------------------------------------------

    fs::path directory(
        first
    );


    wchar_t* filename =
        second;


    while (*filename)
    {
        fs::path full_path =
            directory /
            filename;


        files.push_back(
            full_path.wstring()
        );


        filename +=
            wcslen(filename) + 1;
    }


    return !files.empty();
}


// ============================================================
// SELECT OUTPUT FOLDER - MODERN WINDOWS DIALOG
// ============================================================

static bool select_output_folder(
    std::wstring& output_folder)
{
    output_folder.clear();


    IFileDialog* dialog =
        nullptr;


    HRESULT hr =
        CoCreateInstance(
            CLSID_FileOpenDialog,
            nullptr,
            CLSCTX_INPROC_SERVER,
            IID_PPV_ARGS(&dialog)
        );


    if (SUCCEEDED(hr) &&
        dialog != nullptr)
    {
        DWORD options = 0;


        if (SUCCEEDED(
            dialog->GetOptions(
                &options
            )))
        {
            options |=
                FOS_PICKFOLDERS;

            options |=
                FOS_FORCEFILESYSTEM;

            options |=
                FOS_PATHMUSTEXIST;


            dialog->SetOptions(
                options
            );
        }


        dialog->SetTitle(
            L"Select output folder"
        );


        hr =
            dialog->Show(
                nullptr
            );


        if (SUCCEEDED(hr))
        {
            IShellItem* item =
                nullptr;


            hr =
                dialog->GetResult(
                    &item
                );


            if (SUCCEEDED(hr) &&
                item != nullptr)
            {
                PWSTR path =
                    nullptr;


                hr =
                    item->GetDisplayName(
                        SIGDN_FILESYSPATH,
                        &path
                    );


                if (SUCCEEDED(hr) &&
                    path != nullptr)
                {
                    output_folder =
                        path;


                    CoTaskMemFree(
                        path
                    );


                    item->Release();
                    dialog->Release();


                    return !output_folder.empty();
                }


                if (path)
                {
                    CoTaskMemFree(
                        path
                    );
                }


                item->Release();
            }
        }


        dialog->Release();
    }


    // --------------------------------------------------------
    // FALLBACK TO OLD WINDOWS FOLDER DIALOG
    // --------------------------------------------------------

    BROWSEINFOW browse = {};


    browse.hwndOwner =
        nullptr;


    browse.lpszTitle =
        L"Select output folder";


    browse.ulFlags =
        BIF_RETURNONLYFSDIRS |
        BIF_NEWDIALOGSTYLE;


    LPITEMIDLIST pidl =
        SHBrowseForFolderW(
            &browse
        );


    if (!pidl)
        return false;


    wchar_t path[MAX_PATH] = {};


    bool success =
        SHGetPathFromIDListW(
            pidl,
            path
        ) != FALSE;


    if (success)
    {
        output_folder =
            path;
    }


    CoTaskMemFree(
        pidl
    );


    return success;
}


// ============================================================
// DISCARD INPUT LINE
// ============================================================

static void discard_input_line()
{
    int c;

    while ((c = getchar()) != '\n' &&
        c != EOF)
    {
    }
}


// ============================================================
// WAIT FOR ENTER
// ============================================================

static void wait_for_enter()
{
    printf(
        "\nPress ENTER to exit..."
    );

    fflush(stdout);

    getchar();
}


// ============================================================
// COM INITIALIZATION
// ============================================================

class ComInitializer
{
private:

    bool initialized = false;


public:

    ComInitializer()
    {
        HRESULT hr =
            CoInitializeEx(
                nullptr,
                COINIT_APARTMENTTHREADED |
                COINIT_DISABLE_OLE1DDE
            );


        if (SUCCEEDED(hr))
        {
            initialized = true;
        }
    }


    ~ComInitializer()
    {
        if (initialized)
        {
            CoUninitialize();
        }
    }
};

extern "C" VIDEOCONVERTER_API int ff_video_convert(
    const wchar_t* input_file,
    const wchar_t* output_file,
    const wchar_t* output_format)
{
    if (!input_file || !output_file || !output_format)
    {
        printf("Invalid video conversion arguments.\n");
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
        printf("Invalid video output format.\n");
        return 0;
    }

    FFmpegManager ffmpeg;

    if (!ffmpeg.is_available())
    {
        printf("FFmpeg not initialized!\n");
        return 0;
    }

    return ffmpeg.convert_video(
        input_file,
        output_file,
        format
    ) == 0 ? 1 : 0;
}


// ============================================================
// MAIN
// ============================================================

int wmain()
{
    // --------------------------------------------------------
    // UTF-8 CONSOLE
    // --------------------------------------------------------

    SetConsoleOutputCP(
        CP_UTF8
    );

    SetConsoleCP(
        CP_UTF8
    );


    // --------------------------------------------------------
    // COM
    // --------------------------------------------------------

    ComInitializer com;


    // --------------------------------------------------------
    // TITLE
    // --------------------------------------------------------

    SetConsoleTitleW(
        L"FormatForge - Video Converter"
    );


    printf(
        "==================================================\n"
    );

    printf(
        "                 FORMATFORGE\n"
    );

    printf(
        "             Video Converter (FFmpeg)\n"
    );

    printf(
        "==================================================\n\n"
    );


    // ========================================================
    // FFMPEG
    // ========================================================

    FFmpegManager ffmpeg;


    if (!ffmpeg.is_available())
    {
        printf(
            "\nERROR: FFmpeg not found!\n\n"
        );


        printf(
            "Expected locations include:\n\n"
        );


        printf(
            "  C:\\FFmpeg\\bin\n"
        );


        printf(
            "  C:\\Program Files\\FFmpeg\\bin\n"
        );


        printf(
            "  FormatForge\\FFmpeg\\bin\n"
        );


        printf(
            "  FormatForge\\bin\\FFmpeg\n\n"
        );


        printf(
            "You can also add FFmpeg to PATH.\n"
        );


        wait_for_enter();

        return 1;
    }


    // ========================================================
    // VERSION
    // ========================================================

    std::string version =
        ffmpeg.get_version();


    if (!version.empty())
    {
        printf(
            "%s\n\n",
            version.c_str()
        );
    }


    // ========================================================
    // INPUT FILES
    // ========================================================

    std::vector<std::wstring>
        input_files;


    if (!select_input_files(
        input_files))
    {
        printf(
            "No input files selected.\n"
        );


        wait_for_enter();

        return 0;
    }


    printf(
        "Selected files: %zu\n\n",
        input_files.size()
    );


    // ========================================================
    // OUTPUT FORMAT
    // ========================================================

    printf(
        "Select output format:\n\n"
    );


    printf(
        "  [1] MP4  (H.264 + AAC)\n"
    );


    printf(
        "  [2] MKV  (H.264 + AAC)\n"
    );


    printf(
        "  [3] MOV  (H.264 + AAC)\n"
    );


    printf(
        "  [4] AVI  (MPEG-4 + MP3)\n"
    );


    printf(
        "  [5] WEBM (VP9 + Opus)\n"
    );


    printf(
        "\nChoice: "
    );


    int choice = 0;


    if (scanf_s(
        "%d",
        &choice) != 1)
    {
        discard_input_line();

        printf(
            "Invalid choice.\n"
        );


        wait_for_enter();

        return 1;
    }


    discard_input_line();


    std::wstring format;


    switch (choice)
    {
    case 1:

        format = L"mp4";

        break;


    case 2:

        format = L"mkv";

        break;


    case 3:

        format = L"mov";

        break;


    case 4:

        format = L"avi";

        break;


    case 5:

        format = L"webm";

        break;


    default:

        printf(
            "Invalid format.\n"
        );


        wait_for_enter();

        return 1;
    }


    // ========================================================
    // QUALITY
    // ========================================================

    int quality = 23;


    if (format == L"mp4" ||
        format == L"mkv" ||
        format == L"mov" ||
        format == L"webm")
    {
        printf(
            "\nVideo Quality "
            "(CRF 18-28, default 23): "
        );


        if (scanf_s(
            "%d",
            &quality) != 1)
        {
            quality = 23;
        }


        discard_input_line();


        if (quality < 18 ||
            quality > 28)
        {
            quality = 23;
        }


        printf(
            "Using CRF: %d\n",
            quality
        );
    }


    // ========================================================
    // OUTPUT FOLDER
    // ========================================================

    std::wstring output_folder;


    if (!select_output_folder(
        output_folder))
    {
        printf(
            "No output folder selected.\n"
        );


        wait_for_enter();

        return 0;
    }


    printf(
        "\nOutput folder selected:\n%s\n",
        wide_to_utf8(
            output_folder.c_str()
        ).c_str()
    );


    // ========================================================
    // START BATCH CONVERSION
    // ========================================================

    int result =
        ffmpeg.batch_convert(
            input_files,
            output_folder,
            format,
            quality
        );


    // ========================================================
    // FINAL PROGRAM MESSAGE
    // ========================================================

    printf("\n");


    if (result == 0)
    {
        printf(
            "==================================================\n"
        );

        printf(
            "             FORMATFORGE FINISHED\n"
        );

        printf(
            "             All conversions completed.\n"
        );

        printf(
            "==================================================\n"
        );
    }
    else
    {
        printf(
            "==================================================\n"
        );

        printf(
            "             FORMATFORGE FINISHED\n"
        );

        printf(
            "             Conversion errors occurred.\n"
        );

        printf(
            "==================================================\n"
        );
    }


    wait_for_enter();


    return result;
}
