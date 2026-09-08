#define WIN32_LEAN_AND_MEAN

#include "PythonConverter.h"

#include <Windows.h>
#include <cstdio>
#include <cstring>
#include <cwctype>
#include <filesystem>
#include <string>
#include <vector>
#include <algorithm>

namespace fs = std::filesystem;

static char g_last_error[1024] = { 0 };

static void set_last_error(const char* message)
{
    if (!message)
    {
        message = "Unknown PythonConverter error";
    }

    strncpy_s(g_last_error, message, _TRUNCATE);
}

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

    result.append(backslashes * 2, L'\\');
    result += L'"';
    return result;
}

static std::wstring lowercase_extension(const wchar_t* file_path)
{
    if (!file_path)
    {
        return {};
    }

    fs::path path(file_path);
    std::wstring extension = path.extension().wstring();

    std::transform(
        extension.begin(),
        extension.end(),
        extension.begin(),
        [](wchar_t c)
        {
            return static_cast<wchar_t>(std::towlower(c));
        }
    );

    return extension;
}

static fs::path get_module_path()
{
    HMODULE module = nullptr;

    BOOL ok = GetModuleHandleExW(
        GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS |
        GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
        reinterpret_cast<LPCWSTR>(&ff_python_convert),
        &module
    );

    if (!ok)
    {
        return {};
    }

    wchar_t path_buffer[MAX_PATH] = {};
    DWORD length = GetModuleFileNameW(
        module,
        path_buffer,
        static_cast<DWORD>(sizeof(path_buffer) / sizeof(path_buffer[0]))
    );

    if (length == 0 || length >= sizeof(path_buffer) / sizeof(path_buffer[0]))
    {
        return {};
    }

    return fs::path(path_buffer);
}

static fs::path get_formatforge_root()
{
    fs::path module_path = get_module_path();

    if (!module_path.empty())
    {
        fs::path module_dir = module_path.parent_path();

        if (_wcsicmp(module_dir.filename().c_str(), L"DLL") == 0)
        {
            return module_dir.parent_path();
        }
    }

    return fs::path(L"F:\\FormatForge");
}

static fs::path get_python_executable()
{
    fs::path root = get_formatforge_root();

    std::vector<fs::path> candidates =
    {
        root / L"DLL" / L"PythonRuntime" / L"python.exe",
        root / L"Python" / L"venv" / L"Scripts" / L"python.exe",
        root / L"Python" / L"python.exe",
        fs::path(L"F:\\FormatForge\\DLL\\PythonRuntime\\python.exe"),
        fs::path(L"F:\\FormatForge\\Python\\venv\\Scripts\\python.exe")
    };

    for (const fs::path& candidate : candidates)
    {
        std::error_code ec;

        if (fs::is_regular_file(candidate, ec))
        {
            return candidate;
        }
    }

    return {};
}

static bool is_pdf_format(const wchar_t* output_format)
{
    if (!output_format || output_format[0] == L'\0')
    {
        return true;
    }

    return _wcsicmp(output_format, L"pdf") == 0 ||
        _wcsicmp(output_format, L".pdf") == 0;
}

static fs::path get_converter_script(const wchar_t* input_file)
{
    fs::path python_root = get_formatforge_root() / L"Python";
    std::wstring extension = lowercase_extension(input_file);

    if (extension == L".doc" || extension == L".docx" ||
        extension == L".xls" || extension == L".xlsx" ||
        extension == L".ppt" || extension == L".pptx")
    {
        return python_root / L"office_converter.py";
    }

    if (extension == L".html" || extension == L".htm")
    {
        return python_root / L"html_converter.py";
    }

    if (extension == L".jpg" || extension == L".jpeg" ||
        extension == L".png" || extension == L".bmp" ||
        extension == L".gif" || extension == L".tiff" ||
        extension == L".tif" || extension == L".webp" ||
        extension == L".ico")
    {
        return python_root / L"image_converter.py";
    }

    if (extension == L".txt" || extension == L".csv" ||
        extension == L".tsv" || extension == L".json" ||
        extension == L".xml")
    {
        return python_root / L"reportlab_converter.py";
    }

    return {};
}

static int run_process(
    const std::wstring& executable,
    const std::wstring& arguments,
    const std::wstring& working_directory)
{
    std::wstring command_line =
        quote_windows_argument(executable) +
        L" " +
        arguments;

    std::vector<wchar_t> command_buffer(command_line.begin(), command_line.end());
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
        working_directory.empty() ? nullptr : working_directory.c_str(),
        &startup_info,
        &process_info
    );

    if (!created)
    {
        DWORD error = GetLastError();
        char buffer[256] = {};
        sprintf_s(buffer, "CreateProcessW failed. Windows error: %lu", error);
        set_last_error(buffer);
        return -1;
    }

    WaitForSingleObject(process_info.hProcess, INFINITE);

    DWORD exit_code = 1;

    if (!GetExitCodeProcess(process_info.hProcess, &exit_code))
    {
        set_last_error("GetExitCodeProcess failed");
        CloseHandle(process_info.hThread);
        CloseHandle(process_info.hProcess);
        return -1;
    }

    CloseHandle(process_info.hThread);
    CloseHandle(process_info.hProcess);
    return static_cast<int>(exit_code);
}

extern "C" PYTHONCONVERTER_API int ff_python_is_available(void)
{
    return !get_python_executable().empty();
}

extern "C" PYTHONCONVERTER_API const char* ff_python_get_last_error(void)
{
    return g_last_error;
}

extern "C" PYTHONCONVERTER_API int ff_python_convert(
    const wchar_t* input_file,
    const wchar_t* output_file,
    const wchar_t* output_format)
{
    if (!input_file || !output_file)
    {
        set_last_error("Invalid PythonConverter input or output path");
        return 0;
    }

    if (!is_pdf_format(output_format))
    {
        set_last_error("PythonConverter only supports PDF output");
        return 0;
    }

    std::wstring extension = lowercase_extension(input_file);

    if (extension == L".pdf")
    {
        if (_wcsicmp(input_file, output_file) == 0)
        {
            set_last_error("");
            return 1;
        }

        fs::path output_path(output_file);
        fs::path output_directory = output_path.parent_path();

        if (!output_directory.empty())
        {
            std::error_code ec;
            fs::create_directories(output_directory, ec);
        }

        if (CopyFileW(input_file, output_file, FALSE))
        {
            set_last_error("");
            return 1;
        }

        DWORD error = GetLastError();
        char buffer[256] = {};
        sprintf_s(buffer, "CopyFileW failed. Windows error: %lu", error);
        set_last_error(buffer);
        return 0;
    }

    fs::path python_exe = get_python_executable();
    fs::path script = get_converter_script(input_file);

    if (python_exe.empty())
    {
        set_last_error("Python executable not found");
        return 0;
    }

    if (script.empty())
    {
        set_last_error("No Python converter script for input format");
        return 0;
    }

    std::error_code ec;

    if (!fs::is_regular_file(script, ec))
    {
        set_last_error("Python converter script not found");
        return 0;
    }

    std::wstring arguments =
        L"-X utf8 " +
        quote_windows_argument(script.wstring()) +
        L" --output " +
        quote_windows_argument(output_file) +
        L" " +
        quote_windows_argument(input_file);

    int exit_code = run_process(
        python_exe.wstring(),
        arguments,
        script.parent_path().wstring()
    );

    if (exit_code != 0)
    {
        char buffer[256] = {};
        sprintf_s(buffer, "Python conversion failed. Exit code: %d", exit_code);
        set_last_error(buffer);
        return 0;
    }

    set_last_error("");
    return 1;
}