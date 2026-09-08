using System.Drawing;
using System.Windows.Forms;

namespace FormatForge.App;

internal sealed class SettingsForm : Form
{
    private readonly ConversionSettingsPanel settingsPanel;

    public SettingsForm(ConversionSettings settings, RuntimeStatus runtimeStatus)
    {
        Settings = settings.Clone();
        settingsPanel = new ConversionSettingsPanel(Settings, includeInterfaceSettings: true, runtimeStatus);
        InitializeSettingsForm();
    }

    public ConversionSettings Settings { get; private set; }

    private void InitializeSettingsForm()
    {
        Text = "FormatForge Settings";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(720, 620);
        Font = new Font("Segoe UI", 9F);

        Controls.Add(new Label
        {
            AutoSize = false,
            Text = "Settings",
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            Location = new Point(18, 16),
            Size = new Size(360, 30)
        });

        settingsPanel.Location = new Point(18, 58);
        settingsPanel.Size = new Size(684, 500);
        settingsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        Controls.Add(settingsPanel);

        FlowLayoutPanel buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Location = new Point(18, 574),
            Size = new Size(684, 34),
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
            Name = "saveSettingsButton",
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

    private void SaveAndClose()
    {
        settingsPanel.ApplyChanges();
        Settings = settingsPanel.Settings;
        DialogResult = DialogResult.OK;
        Close();
    }
}
