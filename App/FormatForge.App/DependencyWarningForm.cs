using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FormatForge.App;

internal sealed class DependencyWarningForm : Form
{
    private readonly IReadOnlyList<DependencyCheckResult> missingDependencies;
    private readonly AppThemeMode theme;

    public DependencyWarningForm(IReadOnlyList<DependencyCheckResult> missingDependencies, AppThemeMode theme)
    {
        this.missingDependencies = missingDependencies;
        this.theme = theme;
        InitializeWarningForm();
    }

    private void InitializeWarningForm()
    {
        ThemePalette palette = ThemeManager.GetPalette(theme);
        Text = "FormatForge Requirements";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(520, 270);
        Font = new Font("Segoe UI", 9F);
        BackColor = palette.Window;
        ForeColor = palette.Foreground;

        PictureBox warningIcon = new PictureBox
        {
            Image = UiIconFactory.CreateIcon(UiIconKind.Warning, Color.FromArgb(255, 175, 116), 34),
            Location = new Point(22, 24),
            Size = new Size(38, 38),
            SizeMode = PictureBoxSizeMode.CenterImage
        };
        Controls.Add(warningIcon);

        Label titleLabel = new Label
        {
            AutoSize = false,
            Text = "Required Windows components were not found",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Location = new Point(72, 24),
            Size = new Size(420, 26),
            ForeColor = palette.Foreground
        };
        Controls.Add(titleLabel);

        Label messageLabel = new Label
        {
            AutoSize = false,
            Text = "Some conversions may fail until these components are installed:",
            Location = new Point(72, 54),
            Size = new Size(420, 22),
            ForeColor = palette.MutedForeground
        };
        Controls.Add(messageLabel);

        TextBox detailsBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            ScrollBars = ScrollBars.Vertical,
            Location = new Point(72, 88),
            Size = new Size(420, 118),
            Text = string.Join(Environment.NewLine, missingDependencies.Select(item => "- " + item.Name + ": " + item.Message)),
            BackColor = palette.Input,
            ForeColor = palette.Foreground
        };
        Controls.Add(detailsBox);

        Panel divider = new Panel
        {
            BackColor = palette.Separator,
            Location = new Point(0, 224),
            Size = new Size(520, 1),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        Controls.Add(divider);

        Button okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(414, 236),
            Size = new Size(84, 28),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            FlatStyle = FlatStyle.Flat,
            BackColor = palette.SurfaceAlt,
            ForeColor = palette.Foreground
        };
        okButton.FlatAppearance.BorderColor = palette.Border;
        okButton.FlatAppearance.MouseOverBackColor = palette.Hover;
        okButton.FlatAppearance.MouseDownBackColor = palette.Input;
        Controls.Add(okButton);

        AcceptButton = okButton;
        CancelButton = okButton;
    }
}
