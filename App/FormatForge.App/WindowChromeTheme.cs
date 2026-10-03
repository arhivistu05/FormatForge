using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FormatForge.App;

internal static class WindowChromeTheme
{
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaCaptionColor = 35;
    private const int DwmwaTextColor = 36;

    private static readonly ConditionalWeakTable<Form, ThemeState> States = new();

    public static void Apply(Form form, ThemePalette palette)
    {
        if (!States.TryGetValue(form, out ThemeState? state))
        {
            state = new ThemeState(palette);
            States.Add(form, state);
            form.HandleCreated += Form_HandleCreated;
        }
        else
        {
            state.Palette = palette;
        }

        if (form.IsHandleCreated)
        {
            ApplyNativeColors(form, palette);
        }
    }

    private static void Form_HandleCreated(object? sender, EventArgs e)
    {
        if (sender is Form form && States.TryGetValue(form, out ThemeState? state))
        {
            ApplyNativeColors(form, state.Palette);
        }
    }

    private static void ApplyNativeColors(Form form, ThemePalette palette)
    {
        if (SystemInformation.HighContrast || !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            return;
        }

        int isDark = palette.Window.GetBrightness() < 0.5f ? 1 : 0;
        int captionColor = ToColorRef(palette.MenuBack);
        int textColor = ToColorRef(palette.Foreground);

        try
        {
            DwmSetWindowAttribute(form.Handle, DwmwaUseImmersiveDarkMode, ref isDark, sizeof(int));
            DwmSetWindowAttribute(form.Handle, DwmwaCaptionColor, ref captionColor, sizeof(int));
            DwmSetWindowAttribute(form.Handle, DwmwaTextColor, ref textColor, sizeof(int));
        }
        catch (DllNotFoundException)
        {
        }
        catch (EntryPointNotFoundException)
        {
        }
    }

    private static int ToColorRef(Color color)
    {
        return color.R | (color.G << 8) | (color.B << 16);
    }

    [DllImport("dwmapi.dll", ExactSpelling = true)]
    private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int valueSize);

    private sealed class ThemeState
    {
        public ThemeState(ThemePalette palette)
        {
            Palette = palette;
        }

        public ThemePalette Palette { get; set; }
    }
}
