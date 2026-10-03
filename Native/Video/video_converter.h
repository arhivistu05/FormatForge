#ifndef FORMATFORGE_VIDEO_CONVERTER_H
#define FORMATFORGE_VIDEO_CONVERTER_H

#include <stdint.h>
#include <wchar.h>

#ifdef VIDEOCONVERTER_EXPORTS
#define VIDEOCONVERTER_API __declspec(dllexport)
#elif defined(VIDEOCONVERTER_IMPORTS)
#define VIDEOCONVERTER_API __declspec(dllimport)
#else
#define VIDEOCONVERTER_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

    typedef struct
    {
        int quality;
        int video_bitrate_kbps;
        int audio_bitrate_kbps;
        int width;
        int height;
        int preserve_metadata;
        int overwrite;
    } FF_VIDEO_OPTIONS;

    VIDEOCONVERTER_API int ff_video_convert(
        const wchar_t* input_file,
        const wchar_t* output_file,
        const wchar_t* output_format
    );

    VIDEOCONVERTER_API int ff_video_convert_ex(
        const wchar_t* input_file,
        const wchar_t* output_file,
        const wchar_t* output_format,
        const FF_VIDEO_OPTIONS* options
    );

    VIDEOCONVERTER_API const char* ff_video_get_last_error(void);

    VIDEOCONVERTER_API void ff_video_cancel(void);

#ifdef __cplusplus
}
#endif

#endif
