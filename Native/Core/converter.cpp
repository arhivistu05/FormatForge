#define _CRT_SECURE_NO_WARNINGS
#define WIN32_LEAN_AND_MEAN

#include "converter.h"

#include <Windows.h>
#include <objbase.h>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <string>
#include <vector>
#include <filesystem>
#include <algorithm>
#include <cctype>

// Include-uri pentru modulele native
#include "../Image/image_converter.h"
#include "../Audio/audio_converter.h"
#include "../Video/video_converter.h"
#include "../PythonConverter/PythonConverter.h"

#pragma comment(lib, "Ole32.lib")
#pragma comment(lib, "Shell32.lib")

namespace fs = std::filesystem;

// ============================================================
// VARIABILE GLOBALE
// ============================================================

static ConversionCallbacks g_callbacks = {0};
static bool g_cancel_requested = false;
static bool g_initialized = false;

// ============================================================
// FUNCȚII DE UTILITATE
// ============================================================

const char* converter_error_string(ConverterError error)
{
    switch (error) {
        case CONVERTER_SUCCESS: return "Success";
        case CONVERTER_ERROR_UNKNOWN: return "Unknown error";
        case CONVERTER_ERROR_INVALID_INPUT: return "Invalid input file";
        case CONVERTER_ERROR_INVALID_OUTPUT: return "Invalid output file";
        case CONVERTER_ERROR_UNSUPPORTED_FORMAT: return "Unsupported format";
        case CONVERTER_ERROR_MEMORY: return "Out of memory";
        case CONVERTER_ERROR_IO: return "I/O error";
        case CONVERTER_ERROR_CODEC: return "Codec error";
        case CONVERTER_ERROR_CANCELLED: return "Operation cancelled";
        case CONVERTER_ERROR_FFMPEG_NOT_FOUND: return "FFmpeg not found";
        case CONVERTER_ERROR_PYTHON_NOT_FOUND: return "Python not found";
        default: return "Unknown error code";
    }
}

// ============================================================
// WCHAR -> UTF-8
// ============================================================

static std::string wide_to_utf8(const wchar_t* text)
{
    if (!text) return {};

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

    if (size <= 0) return {};

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

    if (!result.empty() && result.back() == '\0') {
        result.pop_back();
    }

    return result;
}

// ============================================================
// UTF-8 -> WCHAR
// ============================================================

static std::wstring utf8_to_wide(const std::string& text)
{
    if (text.empty()) return {};

    int size = MultiByteToWideChar(
        CP_UTF8,
        0,
        text.c_str(),
        -1,
        nullptr,
        0
    );

    if (size <= 0) return {};

    std::wstring result(size, L'\0');

    MultiByteToWideChar(
        CP_UTF8,
        0,
        text.c_str(),
        -1,
        result.data(),
        size
    );

    if (!result.empty() && result.back() == L'\0') {
        result.pop_back();
    }

    return result;
}

// ============================================================
// DETECTEAZĂ TIPUL FIȘIERULUI DUPĂ EXTENSIE
// ============================================================

ConvertType converter_detect_type(const char* file_path)
{
    if (!file_path) return CONVERT_TYPE_AUTO;

    const char* dot = strrchr(file_path, '.');
    if (!dot) return CONVERT_TYPE_AUTO;

    char ext[16] = {0};
    int i = 0;
    const char* p = dot + 1;
    while (*p && i < 15) {
        ext[i++] = tolower(*p++);
    }
    ext[i] = '\0';

    // ============================================================
    // IMAGE
    // ============================================================
    if (strcmp(ext, "jpg") == 0 || strcmp(ext, "jpeg") == 0 ||
        strcmp(ext, "png") == 0 || strcmp(ext, "bmp") == 0 ||
        strcmp(ext, "tiff") == 0 || strcmp(ext, "tif") == 0 ||
        strcmp(ext, "webp") == 0 || strcmp(ext, "gif") == 0 ||
        strcmp(ext, "ico") == 0) {
        return CONVERT_TYPE_IMAGE;
    }

    // ============================================================
    // AUDIO
    // ============================================================
    if (strcmp(ext, "mp3") == 0 || strcmp(ext, "wav") == 0 ||
        strcmp(ext, "flac") == 0 || strcmp(ext, "ogg") == 0 ||
        strcmp(ext, "opus") == 0 || strcmp(ext, "m4a") == 0 ||
        strcmp(ext, "aac") == 0 || strcmp(ext, "wma") == 0 ||
        strcmp(ext, "aiff") == 0 || strcmp(ext, "alac") == 0) {
        return CONVERT_TYPE_AUDIO;
    }

    // ============================================================
    // VIDEO
    // ============================================================
    if (strcmp(ext, "mp4") == 0 || strcmp(ext, "avi") == 0 ||
        strcmp(ext, "mkv") == 0 || strcmp(ext, "mov") == 0 ||
        strcmp(ext, "webm") == 0 || strcmp(ext, "wmv") == 0 ||
        strcmp(ext, "flv") == 0 || strcmp(ext, "m4v") == 0 ||
        strcmp(ext, "3gp") == 0) {
        return CONVERT_TYPE_VIDEO;
    }

    // ============================================================
    // DOCUMENTE - vor fi gestionate de Python
    // ============================================================
    if (strcmp(ext, "pdf") == 0 ||
        strcmp(ext, "docx") == 0 || strcmp(ext, "pptx") == 0 ||
        strcmp(ext, "xlsx") == 0 || strcmp(ext, "doc") == 0 ||
        strcmp(ext, "ppt") == 0 || strcmp(ext, "xls") == 0 ||
        strcmp(ext, "odt") == 0 || strcmp(ext, "odp") == 0 ||
        strcmp(ext, "ods") == 0 || strcmp(ext, "html") == 0 ||
        strcmp(ext, "htm") == 0 || strcmp(ext, "txt") == 0 ||
        strcmp(ext, "csv") == 0 || strcmp(ext, "tsv") == 0 ||
        strcmp(ext, "json") == 0 || strcmp(ext, "xml") == 0) {
        return CONVERT_TYPE_DOCUMENT;
    }

    return CONVERT_TYPE_AUTO;
}

// ============================================================
// OBȚINE EXTENSIA PENTRU UN FORMAT
// ============================================================

const char* converter_get_extension(ConverterFormat format)
{
    switch (format) {
        // IMAGE
        case FORMAT_JPEG: return ".jpg";
        case FORMAT_PNG:  return ".png";
        case FORMAT_BMP:  return ".bmp";
        case FORMAT_TIFF: return ".tiff";
        case FORMAT_WEBP: return ".webp";
        case FORMAT_GIF:  return ".gif";

        // AUDIO
        case FORMAT_MP3:  return ".mp3";
        case FORMAT_WAV:  return ".wav";
        case FORMAT_FLAC: return ".flac";
        case FORMAT_OGG:  return ".ogg";
        case FORMAT_OPUS: return ".opus";
        case FORMAT_M4A:  return ".m4a";
        case FORMAT_AAC:  return ".aac";

        // VIDEO
        case FORMAT_MP4:  return ".mp4";
        case FORMAT_AVI:  return ".avi";
        case FORMAT_MKV:  return ".mkv";
        case FORMAT_MOV:  return ".mov";
        case FORMAT_WEBM: return ".webm";
        case FORMAT_WMV:  return ".wmv";

        // DOCUMENTE - gestionate de Python
        case FORMAT_PDF:  return ".pdf";
        case FORMAT_DOCX: return ".docx";
        case FORMAT_PPTX: return ".pptx";
        case FORMAT_XLSX: return ".xlsx";

        default: return ".unknown";
    }
}

// ============================================================
// VERIFICĂ DACĂ FORMATUL ESTE SUPORTAT NATIV
// ============================================================

bool converter_is_format_supported(ConverterFormat format)
{
    switch (format) {
        // IMAGE
        case FORMAT_JPEG:
        case FORMAT_PNG:
        case FORMAT_BMP:
        case FORMAT_TIFF:
        case FORMAT_WEBP:
        case FORMAT_GIF:

        // AUDIO
        case FORMAT_MP3:
        case FORMAT_WAV:
        case FORMAT_FLAC:
        case FORMAT_OGG:
        case FORMAT_OPUS:
        case FORMAT_M4A:
        case FORMAT_AAC:

        // VIDEO
        case FORMAT_MP4:
        case FORMAT_AVI:
        case FORMAT_MKV:
        case FORMAT_MOV:
        case FORMAT_WEBM:
        case FORMAT_WMV:
            return true;

        // DOCUMENTE - suportate doar prin Python
        case FORMAT_PDF:
        case FORMAT_DOCX:
        case FORMAT_PPTX:
        case FORMAT_XLSX:
            return true; // suportate, dar prin Python

        default:
            return false;
    }
}

// ============================================================
// VERIFICĂ DACĂ FORMATUL ESTE DE TIP DOCUMENT (PYTHON)
// ============================================================

static bool is_document_format(ConverterFormat format)
{
    return (format == FORMAT_PDF ||
            format == FORMAT_DOCX ||
            format == FORMAT_PPTX ||
            format == FORMAT_XLSX);
}

// ============================================================
// OBȚINE NUMELE FORMATULUI (PENTRU UI)
// ============================================================

const char* converter_get_format_name(ConverterFormat format)
{
    switch (format) {
        // IMAGE
        case FORMAT_JPEG: return "JPEG";
        case FORMAT_PNG:  return "PNG";
        case FORMAT_BMP:  return "BMP";
        case FORMAT_TIFF: return "TIFF";
        case FORMAT_WEBP: return "WebP";
        case FORMAT_GIF:  return "GIF";

        // AUDIO
        case FORMAT_MP3:  return "MP3";
        case FORMAT_WAV:  return "WAV";
        case FORMAT_FLAC: return "FLAC";
        case FORMAT_OGG:  return "OGG";
        case FORMAT_OPUS: return "OPUS";
        case FORMAT_M4A:  return "M4A";
        case FORMAT_AAC:  return "AAC";

        // VIDEO
        case FORMAT_MP4:  return "MP4";
        case FORMAT_AVI:  return "AVI";
        case FORMAT_MKV:  return "MKV";
        case FORMAT_MOV:  return "MOV";
        case FORMAT_WEBM: return "WebM";
        case FORMAT_WMV:  return "WMV";

        // DOCUMENTE
        case FORMAT_PDF:  return "PDF";
        case FORMAT_DOCX: return "DOCX";
        case FORMAT_PPTX: return "PPTX";
        case FORMAT_XLSX: return "XLSX";

        default: return "Unknown";
    }
}

// ============================================================
// CONVERTOR PRINCIPAL - CU OPȚIUNI
// ============================================================

ConverterError converter_convert_with_options(
    const ConversionOptions* options,
    ConversionCallbacks* callbacks)
{
    if (!options || !options->input_path || !options->output_path) {
        if (callbacks && callbacks->on_error) {
            callbacks->on_error(CONVERTER_ERROR_INVALID_INPUT,
                               "Invalid input or output path",
                               callbacks->user_data);
        }
        return CONVERTER_ERROR_INVALID_INPUT;
    }

    // Salvează callbacks
    if (callbacks) {
        g_callbacks = *callbacks;
    } else {
        memset(&g_callbacks, 0, sizeof(g_callbacks));
    }

    g_cancel_requested = false;

    // Verifică dacă formatul este suportat
    if (!converter_is_format_supported(options->format)) {
        if (g_callbacks.on_error) {
            g_callbacks.on_error(CONVERTER_ERROR_UNSUPPORTED_FORMAT,
                                "Unsupported format",
                                g_callbacks.user_data);
        }
        return CONVERTER_ERROR_UNSUPPORTED_FORMAT;
    }

    // Determină tipul
    ConvertType type = CONVERT_TYPE_AUTO;

    // Folosește formatul specificat pentru a determina tipul
    if (options->format >= FORMAT_JPEG && options->format < FORMAT_MP3) {
        type = CONVERT_TYPE_IMAGE;
    } else if (options->format >= FORMAT_MP3 && options->format < FORMAT_MP4) {
        type = CONVERT_TYPE_AUDIO;
    } else if (options->format >= FORMAT_MP4 && options->format < FORMAT_PDF) {
        type = CONVERT_TYPE_VIDEO;
    } else if (options->format >= FORMAT_PDF) {
        type = CONVERT_TYPE_DOCUMENT;
    } else {
        // Detectare automată
        type = converter_detect_type(options->input_path);
    }

    if (type == CONVERT_TYPE_AUTO) {
        if (g_callbacks.on_error) {
            g_callbacks.on_error(CONVERTER_ERROR_UNSUPPORTED_FORMAT,
                                "Could not detect file type",
                                g_callbacks.user_data);
        }
        return CONVERTER_ERROR_UNSUPPORTED_FORMAT;
    }

    // ============================================================
    // DOCUMENTE - TRIMITE LA PYTHON
    // ============================================================

    if (type == CONVERT_TYPE_DOCUMENT || is_document_format(options->format)) {
        if (options->format != FORMAT_PDF) {
            if (g_callbacks.on_error) {
                g_callbacks.on_error(CONVERTER_ERROR_UNSUPPORTED_FORMAT,
                                    "Python conversion currently supports only PDF output.",
                                    g_callbacks.user_data);
            }
            return CONVERTER_ERROR_UNSUPPORTED_FORMAT;
        }

        std::wstring input_wide = utf8_to_wide(options->input_path);
        std::wstring output_wide = utf8_to_wide(options->output_path);

        if (!ff_python_is_available()) {
            if (g_callbacks.on_error) {
                g_callbacks.on_error(CONVERTER_ERROR_PYTHON_NOT_FOUND,
                                    "Python runtime not found for document conversion.",
                                    g_callbacks.user_data);
            }
            return CONVERTER_ERROR_PYTHON_NOT_FOUND;
        }

        if (g_callbacks.on_progress) {
            g_callbacks.on_progress(0.0, g_callbacks.user_data);
        }

        int conv_result = ff_python_convert(
            input_wide.c_str(),
            output_wide.c_str(),
            L"pdf"
        );

        if (g_callbacks.on_progress) {
            g_callbacks.on_progress(1.0, g_callbacks.user_data);
        }

        if (conv_result) {
            if (g_callbacks.on_complete) {
                ConversionResult dummy_result = {0};
                dummy_result.error_code = CONVERTER_SUCCESS;
                strcpy(dummy_result.output_path, options->output_path);
                dummy_result.progress = 1.0;
                g_callbacks.on_complete(&dummy_result, g_callbacks.user_data);
            }
            return CONVERTER_SUCCESS;
        }

        if (g_callbacks.on_error) {
            const char* error_message = ff_python_get_last_error();
            g_callbacks.on_error(CONVERTER_ERROR_CODEC,
                                error_message && error_message[0] ? error_message : "Python document conversion failed.",
                                g_callbacks.user_data);
        }
        return CONVERTER_ERROR_CODEC;
    }

    // Convertește căile la wide
    std::wstring input_wide = utf8_to_wide(options->input_path);
    std::wstring output_wide = utf8_to_wide(options->output_path);

    ConverterError result = CONVERTER_ERROR_UNKNOWN;

    // ============================================================
    // RUTEZARE CĂTRE CONVERTORUL CORESPUNZĂTOR
    // ============================================================

    switch (type) {
        case CONVERT_TYPE_IMAGE: {
            if (g_callbacks.on_progress) {
                g_callbacks.on_progress(0.0, g_callbacks.user_data);
            }

            std::wstring format_name = utf8_to_wide(converter_get_extension(options->format));

            int conv_result = ff_image_convert(
                input_wide.c_str(),
                output_wide.c_str(),
                format_name.c_str()
            );

            if (g_callbacks.on_progress) {
                g_callbacks.on_progress(1.0, g_callbacks.user_data);
            }

            if (conv_result) {
                result = CONVERTER_SUCCESS;
                if (g_callbacks.on_complete) {
                    ConversionResult dummy_result = {0};
                    dummy_result.error_code = CONVERTER_SUCCESS;
                    strcpy(dummy_result.output_path, options->output_path);
                    dummy_result.progress = 1.0;
                    g_callbacks.on_complete(&dummy_result, g_callbacks.user_data);
                }
            } else {
                result = CONVERTER_ERROR_CODEC;
                if (g_callbacks.on_error) {
                    g_callbacks.on_error(CONVERTER_ERROR_CODEC,
                                        "Image conversion failed",
                                        g_callbacks.user_data);
                }
            }
            break;
        }

        case CONVERT_TYPE_AUDIO: {
            // Mapează formatul
            std::wstring format_name;

            switch (options->format) {
                case FORMAT_MP3:  format_name = L"mp3";  break;
                case FORMAT_WAV:  format_name = L"wav";  break;
                case FORMAT_FLAC: format_name = L"flac"; break;
                case FORMAT_OGG:  format_name = L"ogg";  break;
                case FORMAT_OPUS: format_name = L"opus"; break;
                case FORMAT_M4A:  format_name = L"m4a";  break;
                case FORMAT_AAC:  format_name = L"aac";  break;
                default: format_name = L"mp3"; break;
            }

            if (g_callbacks.on_progress) {
                g_callbacks.on_progress(0.0, g_callbacks.user_data);
            }

            int conv_result = ff_audio_convert(
                input_wide.c_str(),
                output_wide.c_str(),
                format_name.c_str()
            );

            if (g_callbacks.on_progress) {
                g_callbacks.on_progress(1.0, g_callbacks.user_data);
            }

            if (conv_result) {
                result = CONVERTER_SUCCESS;
                if (g_callbacks.on_complete) {
                    ConversionResult dummy_result = {0};
                    dummy_result.error_code = CONVERTER_SUCCESS;
                    strcpy(dummy_result.output_path, options->output_path);
                    dummy_result.progress = 1.0;
                    g_callbacks.on_complete(&dummy_result, g_callbacks.user_data);
                }
            } else {
                result = CONVERTER_ERROR_CODEC;
                if (g_callbacks.on_error) {
                    g_callbacks.on_error(CONVERTER_ERROR_CODEC,
                                        "Audio conversion failed",
                                        g_callbacks.user_data);
                }
            }
            break;
        }

        case CONVERT_TYPE_VIDEO: {
            // Mapează formatul
            std::wstring format_name;

            switch (options->format) {
                case FORMAT_MP4:  format_name = L"mp4";  break;
                case FORMAT_AVI:  format_name = L"avi";  break;
                case FORMAT_MKV:  format_name = L"mkv";  break;
                case FORMAT_MOV:  format_name = L"mov";  break;
                case FORMAT_WEBM: format_name = L"webm"; break;
                case FORMAT_WMV:  format_name = L"wmv";  break;
                default: format_name = L"mp4"; break;
            }

            if (g_callbacks.on_progress) {
                g_callbacks.on_progress(0.0, g_callbacks.user_data);
            }

            int conv_result = ff_video_convert(
                input_wide.c_str(),
                output_wide.c_str(),
                format_name.c_str()
            );

            if (g_callbacks.on_progress) {
                g_callbacks.on_progress(1.0, g_callbacks.user_data);
            }

            if (conv_result) {
                result = CONVERTER_SUCCESS;
                if (g_callbacks.on_complete) {
                    ConversionResult dummy_result = {0};
                    dummy_result.error_code = CONVERTER_SUCCESS;
                    strcpy(dummy_result.output_path, options->output_path);
                    dummy_result.progress = 1.0;
                    g_callbacks.on_complete(&dummy_result, g_callbacks.user_data);
                }
            } else {
                result = CONVERTER_ERROR_CODEC;
                if (g_callbacks.on_error) {
                    g_callbacks.on_error(CONVERTER_ERROR_CODEC,
                                        "Video conversion failed",
                                        g_callbacks.user_data);
                }
            }
            break;
        }

        default:
            result = CONVERTER_ERROR_UNSUPPORTED_FORMAT;
            if (g_callbacks.on_error) {
                g_callbacks.on_error(CONVERTER_ERROR_UNSUPPORTED_FORMAT,
                                    "Unsupported file type",
                                    g_callbacks.user_data);
            }
            break;
    }

    return result;
}

// ============================================================
// CONVERTOR PRINCIPAL - VERSIUNE SIMPLIFICATĂ
// ============================================================

ConverterError converter_convert(
    const char* input_path,
    const char* output_path,
    ConverterFormat format,
    ConversionCallbacks* callbacks)
{
    ConversionOptions options = {0};
    options.input_path = input_path;
    options.output_path = output_path;
    options.format = format;
    options.quality = 85;
    options.bitrate = 192;
    options.preserve_metadata = true;
    options.overwrite = true;

    return converter_convert_with_options(&options, callbacks);
}

// ============================================================
// ANULEAZĂ CONVERSIA
// ============================================================

void converter_cancel(void)
{
    g_cancel_requested = true;
}

// ============================================================
// INIȚIALIZARE
// ============================================================

bool converter_init(void)
{
    if (g_initialized) return true;

    // Inițializează COM (necesar pentru WIC / Image)
    HRESULT hr = CoInitializeEx(NULL, COINIT_MULTITHREADED);
    if (FAILED(hr) && hr != RPC_E_CHANGED_MODE) {
        // Nu e fatal, dar poate cauza probleme
        printf("Warning: CoInitializeEx failed\n");
    }

    g_initialized = true;
    return true;
}

// ============================================================
// CURĂȚARE
// ============================================================

void converter_cleanup(void)
{
    if (!g_initialized) return;

    CoUninitialize();
    g_initialized = false;
}

// ============================================================
// FUNCȚII EXPORTATE PENTRU DLL
// ============================================================

#ifdef _WIN32
extern "C" {

CONVERTER_CORE_API ConverterError converter_convert_export(
    const char* input_path,
    const char* output_path,
    ConverterFormat format,
    ConversionCallbacks* callbacks)
{
    return converter_convert(input_path, output_path, format, callbacks);
}

CONVERTER_CORE_API ConverterError converter_convert_with_options_export(
    const ConversionOptions* options,
    ConversionCallbacks* callbacks)
{
    return converter_convert_with_options(options, callbacks);
}

CONVERTER_CORE_API const char* converter_error_string_export(ConverterError error)
{
    return converter_error_string(error);
}

CONVERTER_CORE_API bool converter_init_export(void)
{
    return converter_init();
}

CONVERTER_CORE_API void converter_cleanup_export(void)
{
    converter_cleanup();
}

CONVERTER_CORE_API void converter_cancel_export(void)
{
    converter_cancel();
}

CONVERTER_CORE_API const char* converter_get_extension_export(ConverterFormat format)
{
    return converter_get_extension(format);
}

CONVERTER_CORE_API const char* converter_get_format_name_export(ConverterFormat format)
{
    return converter_get_format_name(format);
}

CONVERTER_CORE_API ConvertType converter_detect_type_export(const char* file_path)
{
    return converter_detect_type(file_path);
}

CONVERTER_CORE_API bool converter_is_format_supported_export(ConverterFormat format)
{
    return converter_is_format_supported(format);
}

} // extern "C"
#endif // _WIN32
