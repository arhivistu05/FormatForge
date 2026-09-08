using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace FormatForge.App;

internal sealed class UpdateAvailableForm : Form
{
    private readonly AppUpdateInfo updateInfo;
    private readonly AppThemeMode theme;

    public UpdateAvailableForm(AppUpdateInfo updateInfo, AppThemeMode theme)
    {
        this.updateInfo = updateInfo;
        this.theme = theme;
        InitializeUpdateForm();
    }

    private void InitializeUpdateForm()
    {
        ThemePalette palette = ThemeManager.GetPalette(theme);
        Text = "FormatForge Update";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(460, 220);
        Font = new Font("Segoe UI", 9F);
        BackColor = palette.Window;
        ForeColor = palette.Foreground;

        PictureBox iconBox = new PictureBox
        {
            Image = UiIconFactory.CreateIcon(UiIconKind.Convert, palette.Primary, 34),
            Location = new Point(24, 26),
            Size = new Size(38, 38),
            SizeMode = PictureBoxSizeMode.CenterImage
        };
        Controls.Add(iconBox);

        Label titleLabel = new Label
        {
            AutoSize = false,
            Text = "A new version is available",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Location = new Point(76, 26),
            Size = new Size(350, 26),
            ForeColor = palette.Foreground
        };
        Controls.Add(titleLabel);

        Label messageLabel = new Label
        {
            AutoSize = false,
            Text = "Installed: v" + AppInfo.Version + Environment.NewLine +
                   "Available: " + updateInfo.LatestVersion,
            Location = new Point(78, 64),
            Size = new Size(340, 48),
            ForeColor = palette.MutedForeground
        };
        Controls.Add(messageLabel);

        Panel divider = new Panel
        {
            BackColor = palette.Separator,
            Location = new Point(0, 166),
            Size = new Size(460, 1),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        Controls.Add(divider);

        Button laterButton = new Button
        {
            Text = "Later",
            DialogResult = DialogResult.Cancel,
            Location = new Point(252, 180),
            Size = new Size(84, 28),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            FlatStyle = FlatStyle.Flat,
            BackColor = palette.SurfaceAlt,
            ForeColor = palette.Foreground
        };
        laterButton.FlatAppearance.BorderColor = palette.Border;
        laterButton.FlatAppearance.MouseOverBackColor = palette.Hover;
        laterButton.FlatAppearance.MouseDownBackColor = palette.Input;
        Controls.Add(laterButton);

        Button releasesButton = new Button
        {
            Name = "openReleasesButton",
            Text = "Open Release",
            Location = new Point(342, 180),
            Size = new Size(96, 28),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            FlatStyle = FlatStyle.Flat,
            BackColor = palette.Primary,
            ForeColor = Color.White
        };
        releasesButton.FlatAppearance.BorderColor = palette.Primary;
        releasesButton.FlatAppearance.MouseOverBackColor = palette.PrimaryHover;
        releasesButton.FlatAppearance.MouseDownBackColor = palette.PrimaryHover;
        releasesButton.Click += (_, _) => OpenReleasePage();
        Controls.Add(releasesButton);

        AcceptButton = releasesButton;
        CancelButton = laterButton;
    }

    private void OpenReleasePage()
    {
        string url = string.IsNullOrWhiteSpace(updateInfo.ReleaseUrl) ? AppInfo.ReleasesPageUrl : updateInfo.ReleaseUrl;
        if (!string.IsNullOrWhiteSpace(url))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
