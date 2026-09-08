using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace FormatForge.App;

internal sealed record DependencyCheckResult(string Name, bool IsAvailable, string Message);

internal static class WindowsDependencyChecker
{
    public static IReadOnlyList<DependencyCheckResult> CheckRequiredDependencies()
    {
        return new[]
        {
            CheckFFmpeg(),
            CheckMicrosoftOffice()
        };
    }

    private static DependencyCheckResult CheckFFmpeg()
    {
        string? foundPath = FindExecutableOnPath("ffmpeg.exe")
            ?? FindFirstExistingPath(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin", "ffmpeg.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ffmpeg", "bin", "ffmpeg.exe"));

        return foundPath == null
            ? new DependencyCheckResult("FFmpeg", false, "FFmpeg was not found in Windows PATH or common install folders.")
            : new DependencyCheckResult("FFmpeg", true, "FFmpeg found: " + foundPath);
    }

    private static DependencyCheckResult CheckMicrosoftOffice()
    {
        string? foundPath = FindOfficeApplicationPath("WINWORD.EXE")
            ?? FindOfficeApplicationPath("EXCEL.EXE")
            ?? FindOfficeApplicationPath("POWERPNT.EXE")
            ?? FindOfficeClickToRunPath()
            ?? FindFirstExistingPath(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft Office"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Office"));

        return foundPath == null
            ? new DependencyCheckResult("Microsoft Office", false, "Microsoft Office was not found in Windows registry or common install folders.")
            : new DependencyCheckResult("Microsoft Office", true, "Microsoft Office found: " + foundPath);
    }

    private static string? FindExecutableOnPath(string executableName)
    {
        string? path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        foreach (string folder in path.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                continue;
            }

            try
            {
                string candidate = Path.Combine(folder.Trim(), executableName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static string? FindOfficeApplicationPath(string executableName)
    {
        foreach (RegistryHive hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
        {
            foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view);
                    using RegistryKey? appPathKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + executableName);
                    string? value = appPathKey?.GetValue(null) as string;
                    if (!string.IsNullOrWhiteSpace(value) && File.Exists(value))
                    {
                        return value;
                    }
                }
                catch
                {
                }
            }
        }

        return null;
    }

    private static string? FindOfficeClickToRunPath()
    {
        foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using RegistryKey? configuration = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Office\ClickToRun\Configuration");
                string? installPath = configuration?.GetValue("InstallationPath") as string;
                if (!string.IsNullOrWhiteSpace(installPath) && Directory.Exists(installPath))
                {
                    return installPath;
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static string? FindFirstExistingPath(params string[] paths)
    {
        foreach (string path in paths)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            if (File.Exists(path) || Directory.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
