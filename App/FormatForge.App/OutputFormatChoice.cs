using System;
using System.Collections.Generic;

namespace FormatForge.App;

internal sealed class OutputFormatChoice
{
    public OutputFormatChoice(string displayName, ConverterFormat? format, string category)
    {
        DisplayName = displayName;
        Format = format;
        Category = category;
    }

    public string DisplayName { get; }

    public ConverterFormat? Format { get; }

    public string Category { get; }

    public bool IsSameAsSource => Format == null;

    public override string ToString()
    {
        return DisplayName;
    }

    public static IEnumerable<OutputFormatChoice> ForCategory(string? category, bool includeSameAsSource)
    {
        string normalized = NormalizeCategory(category);
        if (includeSameAsSource)
        {
            yield return new OutputFormatChoice("Same as source", null, normalized);
        }

        foreach (ConverterFormat format in GetFormatsForCategory(normalized))
        {
            yield return new OutputFormatChoice(GetDisplayName(format), format, normalized);
        }
    }

    public static IReadOnlyList<ConverterFormat> GetFormatsForCategory(string? category)
    {
        return NormalizeCategory(category) switch
        {
            "Images" => new[]
            {
                ConverterFormat.Jpeg,
                ConverterFormat.Png,
                ConverterFormat.Bmp,
                ConverterFormat.Tiff,
                ConverterFormat.Webp,
                ConverterFormat.Gif,
                ConverterFormat.Pdf
            },
            "Audio" => new[]
            {
                ConverterFormat.Mp3,
                ConverterFormat.Wav,
                ConverterFormat.Flac,
                ConverterFormat.Ogg,
                ConverterFormat.Opus,
                ConverterFormat.M4a,
                ConverterFormat.Aac
            },
            "Video" => new[]
            {
                ConverterFormat.Mp4,
                ConverterFormat.Avi,
                ConverterFormat.Mkv,
                ConverterFormat.Mov,
                ConverterFormat.Webm,
                ConverterFormat.Wmv
            },
            "Documents" or "PDF" => new[]
            {
                ConverterFormat.Pdf
            },
            _ => Array.Empty<ConverterFormat>()
        };
    }

    public static bool IsCompatible(string? category, ConverterFormat format)
    {
        string normalized = NormalizeCategory(category);
        if (format == ConverterFormat.Pdf)
        {
            return normalized is "Images" or "Documents" or "PDF";
        }

        return normalized switch
        {
            "Images" => ConverterCore.IsImageFormat(format),
            "Audio" => ConverterCore.IsAudioFormat(format),
            "Video" => ConverterCore.IsVideoFormat(format),
            "Documents" => format == ConverterFormat.Pdf,
            "PDF" => format == ConverterFormat.Pdf,
            _ => false
        };
    }

    public static string GetDisplayName(ConverterFormat format)
    {
        return format switch
        {
            ConverterFormat.Jpeg => "JPG",
            ConverterFormat.Png => "PNG",
            ConverterFormat.Bmp => "BMP",
            ConverterFormat.Tiff => "TIFF",
            ConverterFormat.Webp => "WEBP",
            ConverterFormat.Gif => "GIF",
            ConverterFormat.Mp3 => "MP3",
            ConverterFormat.Wav => "WAV",
            ConverterFormat.Flac => "FLAC",
            ConverterFormat.Ogg => "OGG",
            ConverterFormat.Opus => "OPUS",
            ConverterFormat.M4a => "M4A",
            ConverterFormat.Aac => "AAC",
            ConverterFormat.Mp4 => "MP4",
            ConverterFormat.Avi => "AVI",
            ConverterFormat.Mkv => "MKV",
            ConverterFormat.Mov => "MOV",
            ConverterFormat.Webm => "WEBM",
            ConverterFormat.Wmv => "WMV",
            ConverterFormat.Pdf => "PDF",
            ConverterFormat.Docx => "DOCX",
            ConverterFormat.Pptx => "PPTX",
            ConverterFormat.Xlsx => "XLSX",
            _ => "Unknown"
        };
    }

    public static string NormalizeCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "Mixed";
        }

        string value = category.Trim();
        if (value.Equals("Image", StringComparison.OrdinalIgnoreCase) || value.Equals("Images", StringComparison.OrdinalIgnoreCase)) return "Images";
        if (value.Equals("Audio", StringComparison.OrdinalIgnoreCase)) return "Audio";
        if (value.Equals("Video", StringComparison.OrdinalIgnoreCase)) return "Video";
        if (value.Equals("Document", StringComparison.OrdinalIgnoreCase) || value.Equals("Documents", StringComparison.OrdinalIgnoreCase)) return "Documents";
        if (value.Equals("PDF", StringComparison.OrdinalIgnoreCase)) return "PDF";
        return "Mixed";
    }
}
