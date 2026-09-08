using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace FormatForge.App;

internal enum ConverterError
{
    Success = 0,
    Unknown = -1,
    InvalidInput = -2,
    InvalidOutput = -3,
    UnsupportedFormat = -4,
    Memory = -5,
    Io = -6,
    Codec = -7,
    Cancelled = -8,
    FfmpegNotFound = -9,
    PythonNotFound = -10
}

internal enum ConverterFormat
{
    Jpeg = 100,
    Png = 101,
    Bmp = 102,
    Tiff = 103,
    Webp = 104,
    Gif = 105,
    Mp3 = 200,
    Wav = 201,
    Flac = 202,
    Ogg = 203,
    Opus = 204,
    M4a = 205,
    Aac = 206,
    Mp4 = 300,
    Avi = 301,
    Mkv = 302,
    Mov = 303,
    Webm = 304,
    Wmv = 305,
    Pdf = 400,
    Docx = 401,
    Pptx = 402,
    Xlsx = 403
}

internal sealed class ConversionRequest
{
    public string InputPath { get; init; } = string.Empty;

    public string OutputPath { get; init; } = string.Empty;

    public ConverterFormat Format { get; init; }

    public int Quality { get; init; } = 85;

    public int Bitrate { get; init; } = 192;

    public int Width { get; init; }

    public int Height { get; init; }

    public bool PreserveMetadata { get; init; } = true;

    public bool Overwrite { get; init; } = true;
}

internal sealed class ConversionOutcome
{
    public bool Success { get; init; }

    public ConverterError Error { get; init; }

    public string OutputPath { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public static ConversionOutcome Ok(string outputPath)
    {
        return new ConversionOutcome
        {
            Success = true,
            Error = ConverterError.Success,
            OutputPath = outputPath,
            Message = "Success"
        };
    }

    public static ConversionOutcome Fail(ConverterError error, string message, string outputPath = "")
    {
        return new ConversionOutcome
        {
            Success = false,
            Error = error,
            OutputPath = outputPath,
            Message = message
        };
    }
}

internal static class ConverterCore
{
    private const string CoreLibrary = "converter_core.dll";
    private const string PythonLibrary = "PythonConverter.dll";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ProgressCallback(double progress, IntPtr userData);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void CompleteCallback(IntPtr result, IntPtr userData);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ErrorCallback(ConverterError error, IntPtr message, IntPtr userData);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeConversionOptions
    {
        public IntPtr InputPath;
        public IntPtr OutputPath;
        public ConverterFormat Format;
        public int Quality;
        public int Bitrate;
        public int Width;
        public int Height;
        [MarshalAs(UnmanagedType.I1)] public bool PreserveMetadata;
        [MarshalAs(UnmanagedType.I1)] public bool Overwrite;
        public IntPtr CustomOptions;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeConversionCallbacks
    {
        [MarshalAs(UnmanagedType.FunctionPtr)] public ProgressCallback? OnProgress;
        [MarshalAs(UnmanagedType.FunctionPtr)] public CompleteCallback? OnComplete;
        [MarshalAs(UnmanagedType.FunctionPtr)] public ErrorCallback? OnError;
        public IntPtr UserData;
        [MarshalAs(UnmanagedType.I1)] public bool CancelRequested;
    }

    [DllImport(CoreLibrary, EntryPoint = "converter_init", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool converter_init();

    [DllImport(CoreLibrary, EntryPoint = "converter_cleanup", CallingConvention = CallingConvention.Cdecl)]
    private static extern void converter_cleanup();

    [DllImport(CoreLibrary, EntryPoint = "converter_cancel", CallingConvention = CallingConvention.Cdecl)]
    private static extern void converter_cancel();

    [DllImport(CoreLibrary, EntryPoint = "converter_convert_with_options", CallingConvention = CallingConvention.Cdecl)]
    private static extern ConverterError converter_convert_with_options(ref NativeConversionOptions options, ref NativeConversionCallbacks callbacks);

    [DllImport(CoreLibrary, EntryPoint = "converter_error_string", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr converter_error_string(ConverterError error);

    [DllImport(PythonLibrary, EntryPoint = "ff_python_is_available", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ff_python_is_available();

    [DllImport(PythonLibrary, EntryPoint = "ff_python_get_last_error", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ff_python_get_last_error();

    public static bool TryInitialize(out string? error)
    {
        if (!NativeLibraryLoader.EnsureConfigured(out error))
        {
            return false;
        }

        try
        {
            if (!converter_init())
            {
                error = "converter_core.dll could not be initialized.";
                return false;
            }

            error = null;
            return true;
        }
        catch (Exception ex) when (ex is DllNotFoundException or BadImageFormatException or EntryPointNotFoundException)
        {
            error = ex.Message;
            return false;
        }
    }

    public static bool IsPythonAvailable(out string? error)
    {
        if (!NativeLibraryLoader.EnsureConfigured(out error))
        {
            return false;
        }

        try
        {
            if (ff_python_is_available() != 0)
            {
                error = null;
                return true;
            }

            error = "PythonRuntime was not found at " + FormatForgePaths.PythonRuntimeDirectory;
            return false;
        }
        catch (Exception ex) when (ex is DllNotFoundException or BadImageFormatException or EntryPointNotFoundException)
        {
            error = ex.Message;
            return false;
        }
    }

    public static ConversionOutcome Convert(ConversionRequest request, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.InputPath) || !File.Exists(request.InputPath))
        {
            return ConversionOutcome.Fail(ConverterError.InvalidInput, "Input file was not found.", request.OutputPath);
        }

        if (string.IsNullOrWhiteSpace(request.OutputPath))
        {
            return ConversionOutcome.Fail(ConverterError.InvalidOutput, "Output path is empty.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return ConversionOutcome.Fail(ConverterError.Cancelled, "Conversion was cancelled.", request.OutputPath);
        }

        if (!TryInitialize(out string? initializationError))
        {
            return ConversionOutcome.Fail(ConverterError.Unknown, initializationError ?? "Native converter could not be initialized.", request.OutputPath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(request.OutputPath) ?? AppContext.BaseDirectory);

        IntPtr inputPath = IntPtr.Zero;
        IntPtr outputPath = IntPtr.Zero;
        string? callbackError = null;

        ProgressCallback progressCallback = (value, _) =>
        {
            progress?.Report(Math.Clamp(value, 0.0, 1.0));
        };
        CompleteCallback completeCallback = (_, _) =>
        {
            progress?.Report(1.0);
        };
        ErrorCallback errorCallback = (_, message, _) =>
        {
            string? nativeMessage = PtrToUtf8(message);
            if (!string.IsNullOrWhiteSpace(nativeMessage))
            {
                callbackError = nativeMessage;
            }
        };

        try
        {
            inputPath = Marshal.StringToCoTaskMemUTF8(request.InputPath);
            outputPath = Marshal.StringToCoTaskMemUTF8(request.OutputPath);

            NativeConversionOptions options = new NativeConversionOptions
            {
                InputPath = inputPath,
                OutputPath = outputPath,
                Format = request.Format,
                Quality = request.Quality,
                Bitrate = request.Bitrate,
                Width = request.Width,
                Height = request.Height,
                PreserveMetadata = request.PreserveMetadata,
                Overwrite = request.Overwrite,
                CustomOptions = IntPtr.Zero
            };

            NativeConversionCallbacks callbacks = new NativeConversionCallbacks
            {
                OnProgress = progressCallback,
                OnComplete = completeCallback,
                OnError = errorCallback,
                UserData = IntPtr.Zero,
                CancelRequested = false
            };

            using CancellationTokenRegistration registration = cancellationToken.Register(Cancel);
            progress?.Report(0.0);
            ConverterError result = converter_convert_with_options(ref options, ref callbacks);
            progress?.Report(result == ConverterError.Success ? 1.0 : 0.0);

            GC.KeepAlive(progressCallback);
            GC.KeepAlive(completeCallback);
            GC.KeepAlive(errorCallback);
            GC.KeepAlive(callbacks);

            if (cancellationToken.IsCancellationRequested)
            {
                return ConversionOutcome.Fail(ConverterError.Cancelled, "Conversion was cancelled.", request.OutputPath);
            }

            if (result == ConverterError.Success)
            {
                if (!File.Exists(request.OutputPath))
                {
                    return ConversionOutcome.Fail(ConverterError.Io, "Conversion finished, but the output file was not created.", request.OutputPath);
                }

                return ConversionOutcome.Ok(request.OutputPath);
            }

            string message = callbackError ?? GetDetailedError(result, request.Format);
            return ConversionOutcome.Fail(result, message, request.OutputPath);
        }
        catch (Exception ex) when (ex is DllNotFoundException or BadImageFormatException or EntryPointNotFoundException or SEHException)
        {
            return ConversionOutcome.Fail(ConverterError.Unknown, ex.Message, request.OutputPath);
        }
        finally
        {
            if (inputPath != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(inputPath);
            }

            if (outputPath != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(outputPath);
            }
        }
    }

    public static void Cancel()
    {
        try
        {
            if (NativeLibraryLoader.EnsureConfigured(out _))
            {
                converter_cancel();
            }
        }
        catch
        {
        }
    }

    public static string GetExtension(ConverterFormat format)
    {
        return format switch
        {
            ConverterFormat.Jpeg => ".jpg",
            ConverterFormat.Png => ".png",
            ConverterFormat.Bmp => ".bmp",
            ConverterFormat.Tiff => ".tiff",
            ConverterFormat.Webp => ".webp",
            ConverterFormat.Gif => ".gif",
            ConverterFormat.Mp3 => ".mp3",
            ConverterFormat.Wav => ".wav",
            ConverterFormat.Flac => ".flac",
            ConverterFormat.Ogg => ".ogg",
            ConverterFormat.Opus => ".opus",
            ConverterFormat.M4a => ".m4a",
            ConverterFormat.Aac => ".aac",
            ConverterFormat.Mp4 => ".mp4",
            ConverterFormat.Avi => ".avi",
            ConverterFormat.Mkv => ".mkv",
            ConverterFormat.Mov => ".mov",
            ConverterFormat.Webm => ".webm",
            ConverterFormat.Wmv => ".wmv",
            ConverterFormat.Pdf => ".pdf",
            ConverterFormat.Docx => ".docx",
            ConverterFormat.Pptx => ".pptx",
            ConverterFormat.Xlsx => ".xlsx",
            _ => ".out"
        };
    }

    public static string GetFormatName(ConverterFormat format)
    {
        return format switch
        {
            ConverterFormat.Jpeg => "JPEG",
            ConverterFormat.Png => "PNG",
            ConverterFormat.Bmp => "BMP",
            ConverterFormat.Tiff => "TIFF",
            ConverterFormat.Webp => "WebP",
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
            ConverterFormat.Webm => "WebM",
            ConverterFormat.Wmv => "WMV",
            ConverterFormat.Pdf => "PDF",
            ConverterFormat.Docx => "DOCX",
            ConverterFormat.Pptx => "PPTX",
            ConverterFormat.Xlsx => "XLSX",
            _ => "Unknown"
        };
    }

    public static bool TryGetFormatFromExtension(string extension, out ConverterFormat format)
    {
        string ext = extension.TrimStart('.').ToLowerInvariant();
        switch (ext)
        {
            case "jpg":
            case "jpeg": format = ConverterFormat.Jpeg; return true;
            case "png": format = ConverterFormat.Png; return true;
            case "bmp": format = ConverterFormat.Bmp; return true;
            case "tif":
            case "tiff": format = ConverterFormat.Tiff; return true;
            case "webp": format = ConverterFormat.Webp; return true;
            case "gif": format = ConverterFormat.Gif; return true;
            case "mp3": format = ConverterFormat.Mp3; return true;
            case "wav": format = ConverterFormat.Wav; return true;
            case "flac": format = ConverterFormat.Flac; return true;
            case "ogg": format = ConverterFormat.Ogg; return true;
            case "opus": format = ConverterFormat.Opus; return true;
            case "m4a": format = ConverterFormat.M4a; return true;
            case "aac": format = ConverterFormat.Aac; return true;
            case "mp4": format = ConverterFormat.Mp4; return true;
            case "avi": format = ConverterFormat.Avi; return true;
            case "mkv": format = ConverterFormat.Mkv; return true;
            case "mov": format = ConverterFormat.Mov; return true;
            case "webm": format = ConverterFormat.Webm; return true;
            case "wmv": format = ConverterFormat.Wmv; return true;
            case "pdf": format = ConverterFormat.Pdf; return true;
            case "docx": format = ConverterFormat.Docx; return true;
            case "pptx": format = ConverterFormat.Pptx; return true;
            case "xlsx": format = ConverterFormat.Xlsx; return true;
            default: format = default; return false;
        }
    }

    public static bool IsImageFormat(ConverterFormat format)
    {
        return format >= ConverterFormat.Jpeg && format <= ConverterFormat.Gif;
    }

    public static bool IsAudioFormat(ConverterFormat format)
    {
        return format >= ConverterFormat.Mp3 && format <= ConverterFormat.Aac;
    }

    public static bool IsVideoFormat(ConverterFormat format)
    {
        return format >= ConverterFormat.Mp4 && format <= ConverterFormat.Wmv;
    }

    private static string GetDetailedError(ConverterError error, ConverterFormat format)
    {
        string? nativeError = PtrToUtf8(converter_error_string(error));
        if (format == ConverterFormat.Pdf)
        {
            string? pythonError = PtrToUtf8(ff_python_get_last_error());
            if (!string.IsNullOrWhiteSpace(pythonError))
            {
                return pythonError;
            }
        }

        return string.IsNullOrWhiteSpace(nativeError) ? "Conversion failed." : nativeError;
    }

    private static string? PtrToUtf8(IntPtr pointer)
    {
        return pointer == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(pointer);
    }
}
