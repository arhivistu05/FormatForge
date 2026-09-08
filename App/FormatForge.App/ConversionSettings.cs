using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FormatForge.App;

internal sealed class ConversionSettings
{
    public string DefaultOutputDirectory { get; set; } = FormatForgePaths.DefaultOutputDirectory;
    public bool OpenOutputFolderAfterConversion { get; set; }
    public bool PreserveMetadata { get; set; } = true;
    public bool KeepOriginalDate { get; set; } = true;
    public bool OverwriteExistingFiles { get; set; }
    public int Quality { get; set; } = 85;
    public int Bitrate { get; set; } = 192;
    public int Width { get; set; }
    public int Height { get; set; }
    public ConverterFormat? ImageOutputFormat { get; set; }
    public ConverterFormat? AudioOutputFormat { get; set; }
    public ConverterFormat? VideoOutputFormat { get; set; }
    public ConverterFormat? DocumentOutputFormat { get; set; } = ConverterFormat.Pdf;
    public bool PreviewEmbeddedArtwork { get; set; } = true;
    public bool ConfirmRemoveFromQueue { get; set; }
    public AppThemeMode Theme { get; set; } = AppThemeMode.Light;
    public string? LastUpdateArchive { get; set; }

    [JsonIgnore]
    public static string SettingsFilePath
    {
        get
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrWhiteSpace(appData))
            {
                appData = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }

            return Path.Combine(appData, "FormatForge", "settings.json");
        }
    }

    public static ConversionSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                ConversionSettings? settings = JsonSerializer.Deserialize<ConversionSettings>(json);
                if (settings != null)
                {
                    settings.Normalize();
                    return settings;
                }
            }
        }
        catch
        {
        }

        ConversionSettings fallback = new ConversionSettings();
        fallback.Normalize();
        return fallback;
    }

    public void Save()
    {
        Normalize();
        string? directory = Path.GetDirectoryName(SettingsFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(this, options));
    }

    public ConversionSettings Clone()
    {
        return new ConversionSettings
        {
            DefaultOutputDirectory = DefaultOutputDirectory,
            OpenOutputFolderAfterConversion = OpenOutputFolderAfterConversion,
            PreserveMetadata = PreserveMetadata,
            KeepOriginalDate = KeepOriginalDate,
            OverwriteExistingFiles = OverwriteExistingFiles,
            Quality = Quality,
            Bitrate = Bitrate,
            Width = Width,
            Height = Height,
            ImageOutputFormat = ImageOutputFormat,
            AudioOutputFormat = AudioOutputFormat,
            VideoOutputFormat = VideoOutputFormat,
            DocumentOutputFormat = DocumentOutputFormat,
            PreviewEmbeddedArtwork = PreviewEmbeddedArtwork,
            ConfirmRemoveFromQueue = ConfirmRemoveFromQueue,
            Theme = Theme,
            LastUpdateArchive = LastUpdateArchive
        };
    }

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(DefaultOutputDirectory))
        {
            DefaultOutputDirectory = FormatForgePaths.DefaultOutputDirectory;
        }

        Quality = Math.Clamp(Quality, 1, 100);
        Bitrate = Math.Clamp(Bitrate, 32, 1000);
        Width = Math.Max(0, Width);
        Height = Math.Max(0, Height);
        ImageOutputFormat = NormalizeFormat("Images", ImageOutputFormat);
        AudioOutputFormat = NormalizeFormat("Audio", AudioOutputFormat);
        VideoOutputFormat = NormalizeFormat("Video", VideoOutputFormat);
        DocumentOutputFormat = NormalizeFormat("Documents", DocumentOutputFormat) ?? ConverterFormat.Pdf;
        if (!Enum.IsDefined(Theme))
        {
            Theme = AppThemeMode.Light;
        }
    }

    public ConverterFormat? GetDefaultFormatForCategory(string? category)
    {
        return OutputFormatChoice.NormalizeCategory(category) switch
        {
            "Images" => ImageOutputFormat,
            "Audio" => AudioOutputFormat,
            "Video" => VideoOutputFormat,
            "Documents" => DocumentOutputFormat,
            "PDF" => ConverterFormat.Pdf,
            _ => null
        };
    }

    public ConverterFormat? GetFallbackFormatForCategory(string? category)
    {
        return OutputFormatChoice.NormalizeCategory(category) switch
        {
            "Images" => ConverterFormat.Png,
            "Audio" => ConverterFormat.Mp3,
            "Video" => ConverterFormat.Mp4,
            "Documents" => ConverterFormat.Pdf,
            "PDF" => ConverterFormat.Pdf,
            _ => null
        };
    }

    public void SetDefaultFormatForCategory(string? category, ConverterFormat? format)
    {
        string normalized = OutputFormatChoice.NormalizeCategory(category);
        ConverterFormat? safeFormat = NormalizeFormat(normalized, format);
        switch (normalized)
        {
            case "Images": ImageOutputFormat = safeFormat; break;
            case "Audio": AudioOutputFormat = safeFormat; break;
            case "Video": VideoOutputFormat = safeFormat; break;
            case "Documents":
            case "PDF": DocumentOutputFormat = safeFormat ?? ConverterFormat.Pdf; break;
        }
    }

    private static ConverterFormat? NormalizeFormat(string category, ConverterFormat? format)
    {
        if (format == null)
        {
            return null;
        }

        return OutputFormatChoice.IsCompatible(category, format.Value) ? format : null;
    }
}
