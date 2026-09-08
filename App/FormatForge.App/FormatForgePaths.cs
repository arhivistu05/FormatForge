using System.IO;

namespace FormatForge.App;

internal static class FormatForgePaths
{
    public static string Root => FindRoot();

    public static string NativeDllDirectory => Path.Combine(Root, "DLL");

    public static string PythonRuntimeDirectory => Path.Combine(NativeDllDirectory, "PythonRuntime");

    public static string PythonDirectory => Path.Combine(Root, "Python");

    public static string UpgradeDirectory => Path.Combine(NativeDllDirectory, "upgrade");

    public static string DefaultOutputDirectory
    {
        get
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrWhiteSpace(documents))
            {
                documents = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }

            return Path.Combine(documents, "FormatForge", "Converted");
        }
    }

    private static string FindRoot()
    {
        string? configuredRoot = Environment.GetEnvironmentVariable("FORMATFORGE_HOME");
        if (LooksLikeRoot(configuredRoot))
        {
            return Path.GetFullPath(configuredRoot!);
        }

        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 12 && directory != null; i++)
        {
            if (LooksLikeRoot(directory.FullName))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        const string fallbackRoot = @"F:\FormatForge";
        if (Directory.Exists(fallbackRoot))
        {
            return fallbackRoot;
        }

        return AppContext.BaseDirectory;
    }

    private static bool LooksLikeRoot(string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        try
        {
            string root = Path.GetFullPath(candidate);
            return File.Exists(Path.Combine(root, "DLL", "converter_core.dll")) ||
                   Directory.Exists(Path.Combine(root, "Native")) ||
                   Directory.Exists(Path.Combine(root, "Python"));
        }
        catch
        {
            return false;
        }
    }
}
