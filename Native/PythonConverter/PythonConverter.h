#ifndef FORMATFORGE_PYTHON_CONVERTER_H
#define FORMATFORGE_PYTHON_CONVERTER_H

#include <wchar.h>

#ifdef PYTHONCONVERTER_EXPORTS
#define PYTHONCONVERTER_API __declspec(dllexport)
#elif defined(PYTHONCONVERTER_IMPORTS)
#define PYTHONCONVERTER_API __declspec(dllimport)
#else
#define PYTHONCONVERTER_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

    PYTHONCONVERTER_API int ff_python_convert(
        const wchar_t* input_file,
        const wchar_t* output_file,
        const wchar_t* output_format
    );

    PYTHONCONVERTER_API int ff_python_is_available(void);

    PYTHONCONVERTER_API const char* ff_python_get_last_error(void);

#ifdef __cplusplus
}
#endif

#endif