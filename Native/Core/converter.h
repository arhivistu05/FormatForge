#pragma once

#include <stdbool.h>

#ifdef _WIN32
#ifdef CONVERTER_CORE_EXPORTS
#define CONVERTER_CORE_API __declspec(dllexport)
#elif defined(CONVERTER_CORE_IMPORTS)
#define CONVERTER_CORE_API __declspec(dllimport)
#else
#define CONVERTER_CORE_API
#endif
#else
#define CONVERTER_CORE_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

// ============================================================
// TIPURI DE BAZĂ
// ============================================================

typedef enum {
    CONVERTER_SUCCESS = 0,
    CONVERTER_ERROR_UNKNOWN = -1,
    CONVERTER_ERROR_INVALID_INPUT = -2,
    CONVERTER_ERROR_INVALID_OUTPUT = -3,
    CONVERTER_ERROR_UNSUPPORTED_FORMAT = -4,
    CONVERTER_ERROR_MEMORY = -5,
    CONVERTER_ERROR_IO = -6,
    CONVERTER_ERROR_CODEC = -7,
    CONVERTER_ERROR_CANCELLED = -8,
    CONVERTER_ERROR_FFMPEG_NOT_FOUND = -9,
    CONVERTER_ERROR_PYTHON_NOT_FOUND = -10
} ConverterError;

typedef enum {
    CONVERT_TYPE_IMAGE = 1,
    CONVERT_TYPE_AUDIO = 2,
    CONVERT_TYPE_VIDEO = 3,
    CONVERT_TYPE_DOCUMENT = 4,    // PDF, DOCX, PPTX, XLSX - gestionat de Python
    CONVERT_TYPE_AUTO = 0
} ConvertType;

typedef enum {
    // IMAGE (100-199)
    FORMAT_JPEG = 100,
    FORMAT_PNG = 101,
    FORMAT_BMP = 102,
    FORMAT_TIFF = 103,
    FORMAT_WEBP = 104,
    FORMAT_GIF = 105,
    
    // AUDIO (200-299)
    FORMAT_MP3 = 200,
    FORMAT_WAV = 201,
    FORMAT_FLAC = 202,
    FORMAT_OGG = 203,
    FORMAT_OPUS = 204,
    FORMAT_M4A = 205,
    FORMAT_AAC = 206,
    
    // VIDEO (300-399)
    FORMAT_MP4 = 300,
    FORMAT_AVI = 301,
    FORMAT_MKV = 302,
    FORMAT_MOV = 303,
    FORMAT_WEBM = 304,
    FORMAT_WMV = 305,
    
    // DOCUMENTE (400-499) - gestionate de Python
    FORMAT_PDF = 400,
    FORMAT_DOCX = 401,
    FORMAT_PPTX = 402,
    FORMAT_XLSX = 403
} ConverterFormat;

// ============================================================
// STRUCTURI
// ============================================================

typedef struct {
    const char* input_path;
    const char* output_path;
    ConverterFormat format;
    int quality;              // 0-100 pentru imagini, 0-9 pentru MP3, 18-28 pentru video
    int bitrate;              // pentru audio/video (kbps)
    int width;                // pentru imagini/video (opțional)
    int height;               // pentru imagini/video (opțional)
    bool preserve_metadata;
    bool overwrite;
    void* custom_options;     // pentru opțiuni specifice
} ConversionOptions;

typedef struct {
    ConverterError error_code;
    char error_message[512];
    char output_path[1024];
    double progress;          // 0.0 - 1.0
    long long total_bytes;
    long long processed_bytes;
} ConversionResult;

typedef struct {
    void (*on_progress)(double progress, void* user_data);
    void (*on_complete)(ConversionResult* result, void* user_data);
    void (*on_error)(ConverterError error, const char* message, void* user_data);
    void* user_data;
    bool cancel_requested;
} ConversionCallbacks;

// ============================================================
// FUNCȚII API PUBLIC
// ============================================================

// Inițializare / curățare
CONVERTER_CORE_API bool converter_init(void);
CONVERTER_CORE_API void converter_cleanup(void);

// Funcția principală de conversie
CONVERTER_CORE_API ConverterError converter_convert(
    const char* input_path,
    const char* output_path,
    ConverterFormat format,
    ConversionCallbacks* callbacks
);

// Versiune cu opțiuni avansate
CONVERTER_CORE_API ConverterError converter_convert_with_options(
    const ConversionOptions* options,
    ConversionCallbacks* callbacks
);

// Utilitare
CONVERTER_CORE_API ConvertType converter_detect_type(const char* file_path);
CONVERTER_CORE_API bool converter_is_format_supported(ConverterFormat format);
CONVERTER_CORE_API const char* converter_get_extension(ConverterFormat format);
CONVERTER_CORE_API const char* converter_get_format_name(ConverterFormat format);
CONVERTER_CORE_API const char* converter_error_string(ConverterError error);

// Anulare
CONVERTER_CORE_API void converter_cancel(void);

#ifdef __cplusplus
}
#endif
