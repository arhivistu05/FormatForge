using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace FormatForge.App;

internal enum AppThemeMode
{
    Light,
    Dark
}

internal sealed class ThemePalette
{
    public Color Window { get; init; }
    public Color Surface { get; init; }
    public Color SurfaceAlt { get; init; }
    public Color Input { get; init; }
    public Color Border { get; init; }
    public Color Foreground { get; init; }
    public Color MutedForeground { get; init; }
    public Color Icon { get; init; }
    public Color MenuBack { get; init; }
    public Color Hover { get; init; }
    public Color Separator { get; init; }
    public Color Primary { get; init; }
    public Color PrimaryHover { get; init; }
    public Color PreviewBack { get; init; }
    public Color PreviewBorder { get; init; }
    public Color PreviewIcon { get; init; }
    public Color ListBack { get; init; }
    public Color SuccessBack { get; init; }
    public Color SuccessFore { get; init; }
    public Color ErrorBack { get; init; }
    public Color ErrorFore { get; init; }
    public Color ProgressBack { get; init; }
    public Color ProgressFore { get; init; }
    public Color CancelledBack { get; init; }
    public Color CancelledFore { get; init; }
}

internal static class ThemeManager
{
    private static readonly ConditionalWeakTable<ComboBox, ComboBoxThemeHolder> ComboBoxThemes = new();
    private static readonly ConditionalWeakTable<TabControl, TabControlThemeHolder> TabControlThemes = new();

    public static ThemePalette GetPalette(AppThemeMode mode)
    {
        if (mode == AppThemeMode.Dark)
        {
            return new ThemePalette
            {
                Window = Color.FromArgb(18, 22, 29),
                Surface = Color.FromArgb(27, 33, 43),
                SurfaceAlt = Color.FromArgb(34, 41, 53),
                Input = Color.FromArgb(22, 27, 36),
                Border = Color.FromArgb(58, 68, 84),
                Foreground = Color.FromArgb(235, 239, 246),
                MutedForeground = Color.FromArgb(166, 176, 192),
                Icon = Color.FromArgb(191, 205, 224),
                MenuBack = Color.FromArgb(24, 29, 38),
                Hover = Color.FromArgb(43, 52, 68),
                Separator = Color.FromArgb(66, 77, 94),
                Primary = Color.FromArgb(54, 126, 246),
                PrimaryHover = Color.FromArgb(81, 148, 255),
                PreviewBack = Color.FromArgb(12, 16, 23),
                PreviewBorder = Color.FromArgb(42, 51, 66),
                PreviewIcon = Color.White,
                ListBack = Color.FromArgb(23, 28, 37),
                SuccessBack = Color.FromArgb(25, 60, 42),
                SuccessFore = Color.FromArgb(139, 232, 166),
                ErrorBack = Color.FromArgb(70, 35, 41),
                ErrorFore = Color.FromArgb(255, 151, 161),
                ProgressBack = Color.FromArgb(29, 52, 89),
                ProgressFore = Color.FromArgb(150, 198, 255),
                CancelledBack = Color.FromArgb(38, 45, 56),
                CancelledFore = Color.FromArgb(179, 188, 202)
            };
        }

        return new ThemePalette
        {
            Window = Color.FromArgb(239, 246, 253),
            Surface = Color.White,
            SurfaceAlt = Color.FromArgb(247, 250, 253),
            Input = Color.White,
            Border = Color.FromArgb(214, 224, 236),
            Foreground = Color.FromArgb(18, 24, 33),
            MutedForeground = Color.FromArgb(83, 91, 105),
            Icon = Color.FromArgb(63, 85, 111),
            MenuBack = Color.FromArgb(250, 252, 255),
            Hover = Color.FromArgb(232, 241, 252),
            Separator = Color.FromArgb(214, 224, 236),
            Primary = Color.FromArgb(35, 117, 238),
            PrimaryHover = Color.FromArgb(24, 103, 214),
            PreviewBack = Color.FromArgb(38, 45, 58),
            PreviewBorder = Color.FromArgb(38, 45, 58),
            PreviewIcon = Color.White,
            ListBack = Color.White,
            SuccessBack = Color.FromArgb(238, 252, 242),
            SuccessFore = Color.FromArgb(19, 113, 43),
            ErrorBack = Color.FromArgb(255, 246, 246),
            ErrorFore = Color.FromArgb(170, 40, 52),
            ProgressBack = Color.FromArgb(232, 243, 255),
            ProgressFore = Color.FromArgb(20, 82, 168),
            CancelledBack = Color.FromArgb(247, 249, 252),
            CancelledFore = Color.FromArgb(92, 99, 112)
        };
    }

    public static void Apply(Control root, AppThemeMode mode)
    {
        ThemePalette palette = GetPalette(mode);
        ApplyControl(root, palette);
    }

    private static void ApplyControl(Control control, ThemePalette palette)
    {
        switch (control)
        {
            case Form form:
                form.BackColor = palette.Window;
                form.ForeColor = palette.Foreground;
                break;
            case UserControl userControl:
                userControl.BackColor = palette.Surface;
                userControl.ForeColor = palette.Foreground;
                break;
            case RoundedPanel roundedPanel:
                roundedPanel.BackColor = IsNamed(roundedPanel, "detailsPreviewPanel") ? palette.PreviewBack : palette.Surface;
                roundedPanel.BorderColor = IsNamed(roundedPanel, "detailsPreviewPanel") ? palette.PreviewBorder : palette.Border;
                roundedPanel.ForeColor = palette.Foreground;
                break;
            case TabPage tabPage:
                tabPage.BackColor = palette.Surface;
                tabPage.ForeColor = palette.Foreground;
                break;
            case Panel panel:
                panel.BackColor = GetPanelBackColor(panel, palette);
                panel.ForeColor = palette.Foreground;
                break;
            case SplitContainer split:
                split.BackColor = palette.Window;
                break;
            case Label label:
                if (label.Name.Contains("Subtitle") || label.Name.Contains("Text") || label.Name.Contains("PathLabel") ||
                    label.Name.Contains("TypeLabel") || label.Name.Contains("SizeLabel") || label.Name.Contains("DurationLabel") ||
                    label.Name.Contains("BitrateLabel"))
                {
                    label.ForeColor = palette.MutedForeground;
                }
                else
                {
                    label.ForeColor = palette.Foreground;
                }
                break;
            case TextBox textBox:
                textBox.BackColor = palette.Input;
                textBox.ForeColor = palette.Foreground;
                textBox.BorderStyle = BorderStyle.FixedSingle;
                break;
            case ComboBox comboBox:
                comboBox.BackColor = palette.Input;
                comboBox.ForeColor = palette.Foreground;
                comboBox.FlatStyle = FlatStyle.Flat;
                comboBox.DrawMode = DrawMode.OwnerDrawFixed;
                ComboBoxThemes.Remove(comboBox);
                ComboBoxThemes.Add(comboBox, new ComboBoxThemeHolder(palette));
                comboBox.DrawItem -= ComboBox_DrawItem;
                comboBox.DrawItem += ComboBox_DrawItem;
                break;
            case NumericUpDown numericUpDown:
                numericUpDown.BackColor = palette.Input;
                numericUpDown.ForeColor = palette.Foreground;
                break;
            case ListView listView:
                listView.BackColor = palette.ListBack;
                listView.ForeColor = palette.Foreground;
                listView.GridLines = palette.Window.GetBrightness() > 0.5f;
                break;
            case CheckBox checkBox:
                checkBox.FlatStyle = FlatStyle.Flat;
                checkBox.BackColor = Color.Transparent;
                checkBox.ForeColor = palette.Foreground;
                checkBox.UseVisualStyleBackColor = false;
                checkBox.FlatAppearance.BorderColor = palette.Border;
                checkBox.FlatAppearance.CheckedBackColor = palette.Primary;
                checkBox.FlatAppearance.MouseOverBackColor = palette.Hover;
                checkBox.FlatAppearance.MouseDownBackColor = palette.Input;
                break;
            case TabControl tabControl:
                tabControl.BackColor = palette.Surface;
                tabControl.ForeColor = palette.Foreground;
                tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
                TabControlThemes.Remove(tabControl);
                TabControlThemes.Add(tabControl, new TabControlThemeHolder(palette));
                tabControl.DrawItem -= TabControl_DrawItem;
                tabControl.DrawItem += TabControl_DrawItem;
                break;
            case Button button:
                ApplyButton(button, palette);
                break;
            case ToolStrip toolStrip:
                ApplyToolStrip(toolStrip, palette);
                break;
        }

        foreach (Control child in control.Controls)
        {
            ApplyControl(child, palette);
        }
    }

    private static void ApplyButton(Button button, ThemePalette palette)
    {
        bool primary = button.Name.Contains("convert", StringComparison.OrdinalIgnoreCase) ||
                       button.Name.Contains("merge", StringComparison.OrdinalIgnoreCase) ||
                       button.Name.Contains("save", StringComparison.OrdinalIgnoreCase);

        button.UseVisualStyleBackColor = false;
        button.BackColor = primary ? palette.Primary : palette.SurfaceAlt;
        button.ForeColor = primary ? Color.White : palette.Foreground;

        if (button.FlatStyle == FlatStyle.Flat)
        {
            button.FlatAppearance.BorderColor = primary ? palette.Primary : palette.Border;
            button.FlatAppearance.MouseOverBackColor = primary ? palette.PrimaryHover : palette.Hover;
            button.FlatAppearance.MouseDownBackColor = primary ? palette.PrimaryHover : palette.Input;
        }
    }

    private static void ApplyToolStrip(ToolStrip toolStrip, ThemePalette palette)
    {
        toolStrip.BackColor = palette.MenuBack;
        toolStrip.ForeColor = palette.Foreground;
        toolStrip.Renderer = new ToolStripProfessionalRenderer(new FormatForgeColorTable(palette));
        foreach (ToolStripItem item in toolStrip.Items)
        {
            ApplyToolStripItem(item, palette);
        }
    }

    private static void ApplyToolStripItem(ToolStripItem item, ThemePalette palette)
    {
        item.BackColor = palette.MenuBack;
        item.ForeColor = palette.Foreground;

        if (item is ToolStripDropDownItem dropDownItem)
        {
            dropDownItem.DropDown.BackColor = palette.Surface;
            dropDownItem.DropDown.ForeColor = palette.Foreground;
            foreach (ToolStripItem child in dropDownItem.DropDownItems)
            {
                ApplyToolStripItem(child, palette);
            }
        }
    }

    private static bool IsNamed(Control control, string name)
    {
        return string.Equals(control.Name, name, StringComparison.OrdinalIgnoreCase);
    }

    private static void ComboBox_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ComboBox comboBox || e.Index < 0)
        {
            return;
        }

        ThemePalette palette = ComboBoxThemes.TryGetValue(comboBox, out ComboBoxThemeHolder? holder)
            ? holder.Palette
            : GetPalette(AppThemeMode.Light);
        bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        Color backColor = selected ? palette.Hover : palette.Input;
        Color foreColor = selected ? palette.Foreground : palette.Foreground;

        using SolidBrush backgroundBrush = new SolidBrush(backColor);
        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
        TextRenderer.DrawText(
            e.Graphics,
            comboBox.GetItemText(comboBox.Items[e.Index]),
            e.Font ?? comboBox.Font,
            Rectangle.Inflate(e.Bounds, -6, 0),
            foreColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
    }

    private static void TabControl_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not TabControl tabControl || e.Index < 0 || e.Index >= tabControl.TabPages.Count)
        {
            return;
        }

        ThemePalette palette = TabControlThemes.TryGetValue(tabControl, out TabControlThemeHolder? holder)
            ? holder.Palette
            : GetPalette(AppThemeMode.Light);
        bool selected = e.Index == tabControl.SelectedIndex;
        Rectangle bounds = tabControl.GetTabRect(e.Index);
        Color backColor = selected ? palette.Surface : palette.SurfaceAlt;
        Color foreColor = selected ? palette.Foreground : palette.MutedForeground;

        using SolidBrush backgroundBrush = new SolidBrush(backColor);
        using Pen borderPen = new Pen(palette.Border);
        e.Graphics.FillRectangle(backgroundBrush, bounds);
        e.Graphics.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height);

        TextRenderer.DrawText(
            e.Graphics,
            tabControl.TabPages[e.Index].Text,
            tabControl.Font,
            Rectangle.Inflate(bounds, -8, 0),
            foreColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
    }

    private static Color GetPanelBackColor(Panel panel, ThemePalette palette)
    {
        if (IsNamed(panel, "headerPanel"))
        {
            return Color.Transparent;
        }

        if (panel.Name.Contains("Separator", StringComparison.OrdinalIgnoreCase) ||
            panel.Name.Contains("Divider", StringComparison.OrdinalIgnoreCase))
        {
            return palette.Separator;
        }

        if (IsNamed(panel, "shellPanel") || IsNamed(panel, "workspacePanel") || panel.GetType().Name == "SplitterPanel")
        {
            return palette.Window;
        }

        return palette.Surface;
    }

    private sealed class FormatForgeColorTable : ProfessionalColorTable
    {
        private readonly ThemePalette palette;

        public FormatForgeColorTable(ThemePalette palette)
        {
            this.palette = palette;
        }

        public override Color ToolStripDropDownBackground => palette.Surface;
        public override Color ImageMarginGradientBegin => palette.Surface;
        public override Color ImageMarginGradientMiddle => palette.Surface;
        public override Color ImageMarginGradientEnd => palette.Surface;
        public override Color MenuBorder => palette.Border;
        public override Color MenuItemBorder => palette.Hover;
        public override Color MenuItemSelected => palette.Hover;
        public override Color MenuItemSelectedGradientBegin => palette.Hover;
        public override Color MenuItemSelectedGradientEnd => palette.Hover;
        public override Color MenuItemPressedGradientBegin => palette.SurfaceAlt;
        public override Color MenuItemPressedGradientMiddle => palette.SurfaceAlt;
        public override Color MenuItemPressedGradientEnd => palette.SurfaceAlt;
        public override Color SeparatorDark => palette.Separator;
        public override Color SeparatorLight => palette.Separator;
        public override Color ToolStripBorder => palette.Border;
        public override Color ToolStripGradientBegin => palette.MenuBack;
        public override Color ToolStripGradientMiddle => palette.MenuBack;
        public override Color ToolStripGradientEnd => palette.MenuBack;
        public override Color ButtonSelectedBorder => palette.Hover;
        public override Color ButtonSelectedGradientBegin => palette.Hover;
        public override Color ButtonSelectedGradientMiddle => palette.Hover;
        public override Color ButtonSelectedGradientEnd => palette.Hover;
    }

    private sealed class ComboBoxThemeHolder
    {
        public ComboBoxThemeHolder(ThemePalette palette)
        {
            Palette = palette;
        }

        public ThemePalette Palette { get; }
    }

    private sealed class TabControlThemeHolder
    {
        public TabControlThemeHolder(ThemePalette palette)
        {
            Palette = palette;
        }

        public ThemePalette Palette { get; }
    }
}
