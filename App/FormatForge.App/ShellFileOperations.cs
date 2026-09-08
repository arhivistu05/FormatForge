using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FormatForge.App;

internal static class ShellFileOperations
{
    private const int ShowNormal = 1;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr ShellExecute(IntPtr hwnd, string? operation, string file, string? parameters, string? directory, int showCommand);

    public static bool ShowProperties(IWin32Window owner, string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return false;
        }

        IntPtr result = ShellExecute(owner.Handle, "properties", path, null, null, ShowNormal);
        return result.ToInt64() > 32;
    }

    public static void OpenFolderAndSelect(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (File.Exists(path))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "/select,\"" + path + "\"",
                UseShellExecute = true
            });
            return;
        }

        string? directory = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = directory,
                UseShellExecute = true
            });
        }
    }
}
