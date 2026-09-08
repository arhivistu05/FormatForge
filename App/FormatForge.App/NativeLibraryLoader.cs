using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FormatForge.App;

internal static class NativeLibraryLoader
{
    private static readonly object SyncRoot = new();
    private static bool configured;
    private static string? configurationError;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetDllDirectory(string? lpPathName);

    public static bool EnsureConfigured(out string? error)
    {
        lock (SyncRoot)
        {
            if (configured)
            {
                error = configurationError;
                return error == null;
            }

            configured = true;

            string nativeDirectory = FormatForgePaths.NativeDllDirectory;
            if (!Directory.Exists(nativeDirectory))
            {
                configurationError = "Native DLL folder was not found: " + nativeDirectory;
                error = configurationError;
                return false;
            }

            string coreDll = Path.Combine(nativeDirectory, "converter_core.dll");
            if (!File.Exists(coreDll))
            {
                configurationError = "converter_core.dll was not found: " + coreDll;
                error = configurationError;
                return false;
            }

            SetDllDirectory(nativeDirectory);

            try
            {
                NativeLibrary.SetDllImportResolver(typeof(NativeLibraryLoader).Assembly, ResolveDllImport);
            }
            catch (InvalidOperationException)
            {
            }

            string[] requiredLibraries =
            {
                "image_converter.dll",
                "audio_converter.dll",
                "video_converter.dll",
                "PythonConverter.dll"
            };

            foreach (string library in requiredLibraries)
            {
                string libraryPath = Path.Combine(nativeDirectory, library);
                if (!File.Exists(libraryPath))
                {
                    configurationError = library + " was not found in " + nativeDirectory;
                    error = configurationError;
                    return false;
                }
            }

            configurationError = null;
            error = null;
            return true;
        }
    }

    private static IntPtr ResolveDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        string fileName = libraryName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
            ? libraryName
            : libraryName + ".dll";

        string[] candidates =
        {
            Path.Combine(AppContext.BaseDirectory, fileName),
            Path.Combine(FormatForgePaths.NativeDllDirectory, fileName)
        };

        foreach (string candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(candidate))
            {
                continue;
            }

            try
            {
                string? candidateDirectory = Path.GetDirectoryName(candidate);
                if (!string.IsNullOrWhiteSpace(candidateDirectory))
                {
                    SetDllDirectory(candidateDirectory);
                }
                return NativeLibrary.Load(candidate);
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        return IntPtr.Zero;
    }
}
