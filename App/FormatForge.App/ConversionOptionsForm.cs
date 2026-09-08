using System.Drawing;
using System.Windows.Forms;

namespace FormatForge.App;

internal sealed class ConversionOptionsForm : Form
{
    private readonly ConversionSettingsPanel settingsPanel;

    public ConversionOptionsForm(ConversionSettings settings, string contextCategory, int queuedCount, int selectedCount)
    {
        Settings = settings.Clone();
        settingsPanel = new ConversionSettingsPanel(Settings, includeInterfaceSettings: false);
        InitializeOptionsForm(contextCategory, queuedCount, selectedCount);
    }

    public ConversionSettings Settings { get; private set; }

    private void InitializeOptionsForm(string contextCategory, int queuedCount, int selectedCount)
    {
        Text = "Convert Options";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(640, 530);
        Font = new Font("Segoe UI", 9F);

        Label titleLabel = new Label
        {
            AutoSize = false,
            Text = "Convert Options",
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            Location = new Point(18, 16),
            Size = new Size(360, 30)
        };
        Controls.Add(titleLabel);

        Label contextLabel = new Label
        {
            AutoSize = false,
            Text = BuildContextText(contextCategory, queuedCount, selectedCount),
            Location = new Point(20, 50),
            Size = new Size(590, 22)
        };
        Controls.Add(contextLabel);

        settingsPanel.Location = new Point(18, 84);
        settingsPanel.Size = new Size(604, 380);
        settingsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        Controls.Add(settingsPanel);

        FlowLayoutPanel buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Location = new Point(18, 478),
            Size = new Size(604, 36),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        Controls.Add(buttons);

        Button cancelButton = new Button
        {
            Text = "Cancel",
            Size = new Size(88, 30),
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.Flat
        };

        Button saveButton = new Button
        {
            Name = "saveOptionsButton",
            Text = "Save",
            Size = new Size(96, 30),
            FlatStyle = FlatStyle.Flat
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += (_, _) => SaveAndClose();

        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(saveButton);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
        ThemeManager.Apply(this, Settings.Theme);
    }

    private static string BuildContextText(string contextCategory, int queuedCount, int selectedCount)
    {
        string countText = selectedCount > 0 ? selectedCount + " selected file(s)" : queuedCount + " queued file(s)";
        return string.IsNullOrWhiteSpace(contextCategory) || contextCategory == "Mixed"
            ? countText
            : countText + " - " + contextCategory;
    }

    private void SaveAndClose()
    {
        settingsPanel.ApplyChanges();
        Settings = settingsPanel.Settings;
        DialogResult = DialogResult.OK;
        Close();
    }
}
