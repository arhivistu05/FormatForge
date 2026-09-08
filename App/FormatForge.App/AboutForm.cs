using System.Drawing;
using System.Windows.Forms;

namespace FormatForge.App;

internal sealed class AboutForm : Form
{
    private readonly AppThemeMode theme;

    public AboutForm(AppThemeMode theme)
    {
        this.theme = theme;
        InitializeAboutForm();
    }

    private void InitializeAboutForm()
    {
        ThemePalette palette = ThemeManager.GetPalette(theme);
        Text = "About FormatForge";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(350, 235);
        Font = new Font("Segoe UI", 9F);
        BackColor = palette.Window;
        ForeColor = palette.Foreground;

        PictureBox iconBox = new PictureBox
        {
            Image = UiIconFactory.CreateAppLogoBitmap(44),
            Location = new Point(22, 42),
            Size = new Size(44, 44),
            SizeMode = PictureBoxSizeMode.Zoom
        };
        Controls.Add(iconBox);

        Label productLabel = new Label
        {
            AutoSize = false,
            Text = AppInfo.ProductName + " v" + AppInfo.Version,
            Location = new Point(78, 42),
            Size = new Size(230, 20),
            ForeColor = palette.Foreground
        };
        Controls.Add(productLabel);

        Label creatorLabel = new Label
        {
            AutoSize = false,
            Text = AppInfo.CopyrightText,
            Location = new Point(78, 64),
            Size = new Size(230, 20),
            ForeColor = palette.Foreground
        };
        Controls.Add(creatorLabel);

        Label descriptionLabel = new Label
        {
            AutoSize = false,
            Text = "Universal file conversion interface.",
            Location = new Point(78, 96),
            Size = new Size(240, 22),
            ForeColor = palette.MutedForeground
        };
        Controls.Add(descriptionLabel);

        Panel divider = new Panel
        {
            BackColor = palette.Separator,
            Location = new Point(0, 184),
            Size = new Size(350, 1),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        Controls.Add(divider);

        Button okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(244, 196),
            Size = new Size(84, 28),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            FlatStyle = FlatStyle.Flat
        };
        okButton.FlatAppearance.BorderColor = palette.Border;
        okButton.FlatAppearance.MouseOverBackColor = palette.Hover;
        okButton.FlatAppearance.MouseDownBackColor = palette.Input;
        okButton.BackColor = palette.SurfaceAlt;
        okButton.ForeColor = palette.Foreground;
        Controls.Add(okButton);

        AcceptButton = okButton;
        CancelButton = okButton;
    }
}
