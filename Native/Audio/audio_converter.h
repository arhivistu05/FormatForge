#ifndef FORMATFORGE_AUDIO_CONVERTER_H
#define FORMATFORGE_AUDIO_CONVERTER_H

#include <stdint.h>
#include <wchar.h>

#ifdef AUDIOCONVERTER_EXPORTS
#define AUDIOCONVERTER_API __declspec(dllexport)
#elif defined(AUDIOCONVERTER_IMPORTS)
#define AUDIOCONVERTER_API __declspec(dllimport)
#else
#define AUDIOCONVERTER_API
#endif

#ifdef __cplusplus
extern "C" {
#endif


#define FF_METADATA_STRING_SIZE 1024


	typedef struct
	{
		char title[FF_METADATA_STRING_SIZE];
		char artist[FF_METADATA_STRING_SIZE];
		char album[FF_METADATA_STRING_SIZE];
		char album_artist[FF_METADATA_STRING_SIZE];
		char genre[FF_METADATA_STRING_SIZE];
		char date[FF_METADATA_STRING_SIZE];
		char track[FF_METADATA_STRING_SIZE];
		char disc[FF_METADATA_STRING_SIZE];
		char composer[FF_METADATA_STRING_SIZE];
		char comment[FF_METADATA_STRING_SIZE];
		char copyright[FF_METADATA_STRING_SIZE];
		char encoder[FF_METADATA_STRING_SIZE];

		uint8_t* cover_data;
		int cover_size;

		char cover_mime_type[128];

	} METADATA;


	AUDIOCONVERTER_API int ff_audio_convert(
		const wchar_t* input_file,
		const wchar_t* output_file,
		const wchar_t* output_format
	);


#ifdef __cplusplus
}
#endif

#endif
