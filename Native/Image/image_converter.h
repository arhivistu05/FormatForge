#ifndef FORMATFORGE_IMAGE_CONVERTER_H
#define FORMATFORGE_IMAGE_CONVERTER_H

#ifdef IMAGECONVERTER_EXPORTS
#define IMAGECONVERTER_API __declspec(dllexport)
#elif defined(IMAGECONVERTER_IMPORTS)
#define IMAGECONVERTER_API __declspec(dllimport)
#else
#define IMAGECONVERTER_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

    IMAGECONVERTER_API int ff_image_convert(
        const wchar_t* input_file,
        const wchar_t* output_file,
        const wchar_t* output_format
    );

#ifdef __cplusplus
}
#endif

#endif
