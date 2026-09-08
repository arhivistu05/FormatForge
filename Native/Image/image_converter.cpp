#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <wincodec.h>
#include <commdlg.h>
#include <wchar.h>
#include <stdio.h>
#include <stdlib.h>
#include <malloc.h>
#include <Shlwapi.h>
#include "image_converter.h"

#pragma comment(lib, "Comdlg32.lib")
#pragma comment(lib, "Windowscodecs.lib")
#pragma comment(lib, "Shlwapi.lib")
#pragma comment(lib, "Ole32.lib")

#define MAX_FILES_TO_CONVERT 100
#define BUFFER_SIZE 32768

static const char* supportedFormats[] =
{
	".bmp",
		".jpg",
		".jpeg",
		".png",
		".gif",
		".tiff",
		".tif"
};

static const int FormatsCount = sizeof(supportedFormats) / sizeof(supportedFormats[0]);

static int select_images_window(wchar_t* buffer, DWORD buffer_size)
{
	OPENFILENAMEW ofn;

	ZeroMemory(&ofn, sizeof(ofn));

	ofn.lStructSize = sizeof(ofn);
	ofn.hwndOwner = NULL;
	ofn.lpstrFile = buffer;
	ofn.nMaxFile = buffer_size;

	ofn.lpstrFilter =
		L"Image Files\0"
		L"*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif;*.tiff;*.ico;*.webp\0"
		L"All Files\0"
		L"*.*\0";

	ofn.nFilterIndex = 1;

	ofn.Flags =
		OFN_PATHMUSTEXIST |
		OFN_FILEMUSTEXIST |
		OFN_ALLOWMULTISELECT |
		OFN_EXPLORER;

	if (GetOpenFileNameW(&ofn) == TRUE)
	{
		return 1;
	}
	else
	{
		return 0;
	}
}

static int select_output_format(void)
{
	printf("\n");
	printf("=====================================\n");
	printf("       FORMATFORGE - OUTPUT FORMAT\n");
	printf("=====================================\n\n");
	for (int i = 0; i < FormatsCount; ++i)
	{
		printf("%d. %s\n", i + 1, supportedFormats[i]);
	}
	printf("\nSelect the output format (1-%d): ", FormatsCount);
	int choice;
	if (scanf_s("%d", &choice) != 1)
		return -1;

	if (choice < 1 || choice > FormatsCount)
		return -1;

	return choice - 1;
}

extern "C" IMAGECONVERTER_API int ff_image_convert(
    const wchar_t* input_file,
    const wchar_t* output_file,
    const wchar_t* output_format)
{
    HRESULT hr;

    IWICImagingFactory* factory = NULL;
    IWICBitmapDecoder* decoder = NULL;
    IWICBitmapFrameDecode* frame = NULL;
    IWICFormatConverter* converter = NULL;
    IWICBitmapEncoder* encoder = NULL;
    IWICBitmapFrameEncode* encoder_frame = NULL;
    IStream* stream = NULL;

    // Initialize COM
    hr = CoInitializeEx(NULL, COINIT_MULTITHREADED);

    if (FAILED(hr) && hr != RPC_E_CHANGED_MODE)
    {
        return 0;
    }

    // Create WIC factory
    hr = CoCreateInstance(
        CLSID_WICImagingFactory,
        NULL,
        CLSCTX_INPROC_SERVER,
        IID_PPV_ARGS(&factory)
    );

    if (FAILED(hr))
    {
        printf("Failed to create WIC factory.\n");
        goto cleanup;
    }

    // Create decoder
    hr = factory->CreateDecoderFromFilename(
        input_file,
        NULL,
        GENERIC_READ,
        WICDecodeMetadataCacheOnLoad,
        &decoder
    );

    if (FAILED(hr))
    {
        printf("Failed to open input image.\n");
        goto cleanup;
    }

    // Get first frame
    hr = decoder->GetFrame(0, &frame);

    if (FAILED(hr))
    {
        printf("Failed to read image frame.\n");
        goto cleanup;
    }

    // Create format converter
    hr = factory->CreateFormatConverter(&converter);

    if (FAILED(hr))
    {
        printf("Failed to create format converter.\n");
        goto cleanup;
    }

    // Convert image to 32-bit BGRA
    hr = converter->Initialize(
        frame,
        GUID_WICPixelFormat32bppBGRA,
        WICBitmapDitherTypeNone,
        NULL,
        0.0,
        WICBitmapPaletteTypeCustom
    );

    if (FAILED(hr))
    {
        printf("Failed to convert image pixel format.\n");
        goto cleanup;
    }

    // Create output stream
    hr = SHCreateStreamOnFileEx(
        output_file,
        STGM_CREATE | STGM_WRITE,
        FILE_ATTRIBUTE_NORMAL,
        TRUE,
        NULL,
        &stream
    );

    if (FAILED(hr))
    {
        printf("Failed to create output file.\n");
        goto cleanup;
    }

    // Create encoder
    hr = factory->CreateEncoder(
        GUID_ContainerFormatPng,
        NULL,
        &encoder
    );

    /*
        We replace the encoder according to the selected format below.
    */

    if (encoder != NULL)
    {
        encoder->Release();
        encoder = NULL;
    }

    if (_wcsicmp(output_format, L".png") == 0)
    {
        hr = factory->CreateEncoder(
            GUID_ContainerFormatPng,
            NULL,
            &encoder
        );
    }
    else if (_wcsicmp(output_format, L".jpg") == 0 ||
        _wcsicmp(output_format, L".jpeg") == 0)
    {
        hr = factory->CreateEncoder(
            GUID_ContainerFormatJpeg,
            NULL,
            &encoder
        );
    }
    else if (_wcsicmp(output_format, L".bmp") == 0)
    {
        hr = factory->CreateEncoder(
            GUID_ContainerFormatBmp,
            NULL,
            &encoder
        );
    }
    else if (_wcsicmp(output_format, L".gif") == 0)
    {
        hr = factory->CreateEncoder(
            GUID_ContainerFormatGif,
            NULL,
            &encoder
        );
    }
    else if (_wcsicmp(output_format, L".tif") == 0 ||
        _wcsicmp(output_format, L".tiff") == 0)
    {
        hr = factory->CreateEncoder(
            GUID_ContainerFormatTiff,
            NULL,
            &encoder
        );
    }
    else if (_wcsicmp(output_format, L".ico") == 0)
    {
        hr = factory->CreateEncoder(
            GUID_ContainerFormatIco,
            NULL,
            &encoder
        );
    }
    else
    {
        printf("Unsupported output format: %ls\n", output_format);
        goto cleanup;
    }

    if (FAILED(hr))
    {
        printf("Failed to create image encoder.\n");
        goto cleanup;
    }

    // Initialize encoder
    hr = encoder->Initialize(
        stream,
        WICBitmapEncoderNoCache
    );

    if (FAILED(hr))
    {
        printf("Failed to initialize encoder.\n");
        goto cleanup;
    }

    // Create encoder frame
    hr = encoder->CreateNewFrame(
        &encoder_frame,
        NULL
    );

    if (FAILED(hr))
    {
        printf("Failed to create encoder frame.\n");
        goto cleanup;
    }

    hr = encoder_frame->Initialize(NULL);

    if (FAILED(hr))
    {
        printf("Failed to initialize encoder frame.\n");
        goto cleanup;
    }

    // Set image dimensions
    UINT width;
    UINT height;

    hr = frame->GetSize(&width, &height);

    if (FAILED(hr))
    {
        printf("Failed to get image dimensions.\n");
        goto cleanup;
    }

    hr = encoder_frame->SetSize(width, height);

    if (FAILED(hr))
    {
        printf("Failed to set output dimensions.\n");
        goto cleanup;
    }

    // Set pixel format
    WICPixelFormatGUID pixel_format;
    pixel_format = GUID_WICPixelFormat32bppBGRA;

    hr = encoder_frame->SetPixelFormat(&pixel_format);

    if (FAILED(hr))
    {
        printf("Failed to set output pixel format.\n");
        goto cleanup;
    }

    // Write image
    hr = encoder_frame->WriteSource(
        converter,
        NULL
    );

    if (FAILED(hr))
    {
        printf("Failed to write image data.\n");
        goto cleanup;
    }

    // Finish frame
    hr = encoder_frame->Commit();

    if (FAILED(hr))
    {
        printf("Failed to commit encoder frame.\n");
        goto cleanup;
    }

    // Finish encoder
    hr = encoder->Commit();

    if (FAILED(hr))
    {
        printf("Failed to commit encoder.\n");
        goto cleanup;
    }

    printf(
        "Converted: %ls -> %ls\n",
        input_file,
        output_file
    );

cleanup:

    if (encoder_frame)
        encoder_frame->Release();

    if (encoder)
        encoder->Release();

    if (stream)
        stream->Release();

    if (converter)
        converter->Release();

    if (frame)
        frame->Release();

    if (decoder)
        decoder->Release();

    if (factory)
        factory->Release();

    CoUninitialize();

    return SUCCEEDED(hr) ? 1 : 0;
}

int main()
{
	wchar_t* file_buffer = (wchar_t*)malloc(BUFFER_SIZE * sizeof(wchar_t));
	if (!file_buffer)
	{
		fprintf(stderr, "Failed to allocate memory for file buffer.\n");
		return 1;
	}

	ZeroMemory(file_buffer, BUFFER_SIZE * sizeof(wchar_t));

	printf("=====================================\n");
	printf("        FormatForge Image Converter\n");
	printf("=====================================\n\n");

	printf("Select image files...\n");

	if (!select_images_window(file_buffer, BUFFER_SIZE))
	{
		printf("No files selected or an error occurred.\n");
		return 1;
	}
	printf("Files selected successfully.\n");
	wchar_t* file_pointer = file_buffer;
	file_pointer += wcslen(file_pointer) + 1; // Move to the next file
	while (*file_pointer != '\0')
	{
		printf("Selected file: %ls\n", file_pointer);
		file_pointer += wcslen(file_pointer) + 1; // Move to the next file
	}

	int format = select_output_format();
	if (format < 0)
	{
		printf("Invalid format selection.\n");
		free(file_buffer);
		return 1;
	}

	printf("\nOutput format: %s\n", supportedFormats[format]);

    /*
        Actual image conversion: build output filenames and call ff_image_convert
    */

    // Convert selected output extension (ascii) to wide string
    wchar_t out_ext[16] = { 0 };
    MultiByteToWideChar(CP_UTF8, 0, supportedFormats[format], -1, out_ext, _countof(out_ext));

    // Parse file_buffer which may contain either a single full path or
    // multiple entries: first is directory, then file names.
    wchar_t* p = file_buffer;
    size_t first_len = wcslen(p);
    wchar_t* second = p + first_len + 1;

    if (*second == L'\0')
    {
        // Single file selected: p contains full path
        wchar_t input_path[BUFFER_SIZE];
        wchar_t output_path[BUFFER_SIZE];

        wcscpy_s(input_path, BUFFER_SIZE, p);
        wcscpy_s(output_path, BUFFER_SIZE, input_path);

        wchar_t* dot = wcsrchr(output_path, L'.');
        if (dot)
            *dot = L'\0';
        wcscat_s(output_path, BUFFER_SIZE, out_ext);

        if (!ff_image_convert(input_path, output_path, out_ext))
        {
            printf("Conversion failed: %ls\n", input_path);
        }
    }
    else
    {
        // Multiple files: first entry is directory
        wchar_t* dir = p;
        wchar_t* name = second;

        while (*name != L'\0')
        {
            wchar_t input_path[BUFFER_SIZE];
            wchar_t output_path[BUFFER_SIZE];

            // build full input path = dir + '\\' + name
            wcscpy_s(input_path, BUFFER_SIZE, dir);
            size_t dlen = wcslen(dir);
            if (dlen > 0 && dir[dlen - 1] != L'\\' && dir[dlen - 1] != L'/')
            {
                wcscat_s(input_path, BUFFER_SIZE, L"\\");
            }
            wcscat_s(input_path, BUFFER_SIZE, name);

            wcscpy_s(output_path, BUFFER_SIZE, input_path);
            wchar_t* dot = wcsrchr(output_path, L'.');
            if (dot)
                *dot = L'\0';
            wcscat_s(output_path, BUFFER_SIZE, out_ext);

            if (!ff_image_convert(input_path, output_path, out_ext))
            {
                printf("Conversion failed: %ls\n", input_path);
            }

            name += wcslen(name) + 1;
        }
    }

	free(file_buffer);

	return 0;
}
