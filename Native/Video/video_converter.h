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

    VIDEOCONVERTER_API int ff_video_convert(
        const wchar_t* input_file,
        const wchar_t* output_file,
        const wchar_t* output_format
    );

#ifdef __cplusplus
}
#endif

#endif
