using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FormatForge.App;

internal sealed record RuntimeStatus(string NativeStatus, string PythonStatus);

internal sealed class ConversionSettingsPanel : UserControl
{
    private readonly bool includeInterfaceSettings;
    private readonly RuntimeStatus? runtimeStatus;
    private readonly TextBox outputDirectoryTextBox = new();
    private readonly CheckBox openOutputCheckBox = new();
    private readonly CheckBox preserveMetadataCheckBox = new();
    private readonly CheckBox keepOriginalDateCheckBox = new();
    private readonly CheckBox overwriteExistingFilesCheckBox = new();
    private readonly CheckBox previewArtworkCheckBox = new();
    private readonly CheckBox confirmRemoveCheckBox = new();
    private readonly ComboBox themeComboBox = new();
    private readonly ComboBox imageFormatBox = new();
    private readonly ComboBox audioFormatBox = new();
    private readonly ComboBox videoFormatBox = new();
    private readonly ComboBox documentFormatBox = new();
    private readonly NumericUpDown qualityBox = new();
    private readonly NumericUpDown bitrateBox = new();
    private readonly NumericUpDown widthBox = new();
    private readonly NumericUpDown heightBox = new();

    public ConversionSettingsPanel(ConversionSettings settings, bool includeInterfaceSettings, RuntimeStatus? runtimeStatus = null)
    {
        Settings = settings.Clone();
        this.includeInterfaceSettings = includeInterfaceSettings;
        this.runtimeStatus = runtimeStatus;
        Dock = DockStyle.Fill;
        Font = new Font("Segoe UI", 9F);
        Build();
    }

    public ConversionSettings Settings { get; }

    public void ApplyChanges()
    {
        Settings.DefaultOutputDirectory = string.IsNullOrWhiteSpace(outputDirectoryTextBox.Text)
            ? FormatForgePaths.DefaultOutputDirectory
            : outputDirectoryTextBox.Text.Trim();
        Settings.OpenOutputFolderAfterConversion = openOutputCheckBox.Checked;
        Settings.PreserveMetadata = preserveMetadataCheckBox.Checked;
        Settings.KeepOriginalDate = keepOriginalDateCheckBox.Checked;
        Settings.OverwriteExistingFiles = overwriteExistingFilesCheckBox.Checked;
        Settings.Quality = (int)qualityBox.Value;
        Settings.Bitrate = (int)bitrateBox.Value;
        Settings.Width = (int)widthBox.Value;
        Settings.Height = (int)heightBox.Value;
        Settings.ImageOutputFormat = SelectedFormat(imageFormatBox);
        Settings.AudioOutputFormat = SelectedFormat(audioFormatBox);
        Settings.VideoOutputFormat = SelectedFormat(videoFormatBox);
        Settings.DocumentOutputFormat = SelectedFormat(documentFormatBox) ?? ConverterFormat.Pdf;

        if (includeInterfaceSettings)
        {
            Settings.PreviewEmbeddedArtwork = previewArtworkCheckBox.Checked;
            Settings.ConfirmRemoveFromQueue = confirmRemoveCheckBox.Checked;
            if (themeComboBox.SelectedItem is string selectedTheme &&
                Enum.TryParse(selectedTheme, ignoreCase: true, out AppThemeMode theme))
            {
                Settings.Theme = theme;
            }
        }

        Settings.Normalize();
    }

    private void Build()
    {
        TabControl tabs = new TabControl
        {
            Dock = DockStyle.Fill
        };
        Controls.Add(tabs);

        TabPage generalPage = new TabPage("General");
        TabPage formatsPage = new TabPage("Formats");
        tabs.TabPages.Add(generalPage);
        tabs.TabPages.Add(formatsPage);

        BuildGeneralPage(generalPage);
        BuildFormatsPage(formatsPage);

        if (includeInterfaceSettings)
        {
            TabPage interfacePage = new TabPage("Interface");
            TabPage runtimePage = new TabPage("Runtime");
            tabs.TabPages.Add(interfacePage);
            tabs.TabPages.Add(runtimePage);
            BuildInterfacePage(interfacePage);
            BuildRuntimePage(runtimePage);
        }

        ThemeManager.Apply(this, Settings.Theme);
    }

    private void BuildGeneralPage(TabPage page)
    {
        TableLayoutPanel layout = CreateLayout(8);
        page.Controls.Add(layout);

        AddLabel(layout, "Output folder", 0, 0);
        outputDirectoryTextBox.Dock = DockStyle.Fill;
        outputDirectoryTextBox.Text = Settings.DefaultOutputDirectory;
        layout.Controls.Add(outputDirectoryTextBox, 1, 0);

        Button browseButton = new Button
        {
            Text = "Browse...",
            Dock = DockStyle.Fill,
            Name = "browseOutputDirectoryButton",
            FlatStyle = FlatStyle.Flat
        };
        browseButton.Click += (_, _) => BrowseOutputDirectory();
        layout.Controls.Add(browseButton, 2, 0);

        AddCheckBox(layout, openOutputCheckBox, "Open output folder after conversion", Settings.OpenOutputFolderAfterConversion, 1);
        AddCheckBox(layout, preserveMetadataCheckBox, "Preserve metadata", Settings.PreserveMetadata, 2);
        AddCheckBox(layout, keepOriginalDateCheckBox, "Keep original file date", Settings.KeepOriginalDate, 3);
        AddCheckBox(layout, overwriteExistingFilesCheckBox, "Overwrite existing output files", Settings.OverwriteExistingFiles, 4);

        AddLabel(layout, "Quality", 0, 5);
        SetupNumeric(qualityBox, 1, 100, Settings.Quality);
        layout.Controls.Add(qualityBox, 1, 5);

        AddLabel(layout, "Bitrate kbps", 0, 6);
        SetupNumeric(bitrateBox, 32, 1000, Settings.Bitrate);
        layout.Controls.Add(bitrateBox, 1, 6);

        AddLabel(layout, "Resize", 0, 7);
        FlowLayoutPanel sizePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false
        };
        SetupNumeric(widthBox, 0, 20000, Settings.Width);
        SetupNumeric(heightBox, 0, 20000, Settings.Height);
        widthBox.Width = 84;
        heightBox.Width = 84;
        sizePanel.Controls.Add(widthBox);
        sizePanel.Controls.Add(new Label
        {
            Text = "x",
            AutoSize = true,
            Padding = new Padding(6, 7, 6, 0)
        });
        sizePanel.Controls.Add(heightBox);
        layout.Controls.Add(sizePanel, 1, 7);
        layout.SetColumnSpan(sizePanel, 2);
    }

    private void BuildFormatsPage(TabPage page)
    {
        TableLayoutPanel layout = CreateLayout(4);
        page.Controls.Add(layout);

        AddFormatRow(layout, 0, "Images", imageFormatBox, "Images", Settings.ImageOutputFormat, includeSame: true);
        AddFormatRow(layout, 1, "Audio", audioFormatBox, "Audio", Settings.AudioOutputFormat, includeSame: true);
        AddFormatRow(layout, 2, "Video", videoFormatBox, "Video", Settings.VideoOutputFormat, includeSame: true);
        AddFormatRow(layout, 3, "Documents / PDF", documentFormatBox, "Documents", Settings.DocumentOutputFormat, includeSame: false);
    }

    private void BuildInterfacePage(TabPage page)
    {
        TableLayoutPanel layout = CreateLayout(3);
        page.Controls.Add(layout);

        AddLabel(layout, "Theme", 0, 0);
        themeComboBox.Dock = DockStyle.Left;
        themeComboBox.Width = 180;
        themeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        themeComboBox.Items.AddRange(Enum.GetNames<AppThemeMode>().Cast<object>().ToArray());
        themeComboBox.SelectedItem = Settings.Theme.ToString();
        layout.Controls.Add(themeComboBox, 1, 0);
        layout.SetColumnSpan(themeComboBox, 2);

        AddCheckBox(layout, previewArtworkCheckBox, "Show image previews and audio album art", Settings.PreviewEmbeddedArtwork, 1);
        AddCheckBox(layout, confirmRemoveCheckBox, "Confirm before removing files from queue", Settings.ConfirmRemoveFromQueue, 2);
    }

    private void BuildRuntimePage(TabPage page)
    {
        TableLayoutPanel layout = CreateLayout(8);
        page.Controls.Add(layout);

        AddReadOnlyPath(layout, "Project root", FormatForgePaths.Root, 0);
        AddReadOnlyPath(layout, "Native DLLs", FormatForgePaths.NativeDllDirectory, 1);
        AddReadOnlyPath(layout, "Python scripts", FormatForgePaths.PythonDirectory, 2);
        AddReadOnlyPath(layout, "PythonRuntime", FormatForgePaths.PythonRuntimeDirectory, 3);
        AddReadOnlyPath(layout, "Settings file", ConversionSettings.SettingsFilePath, 4);
        AddReadOnlyPath(layout, "Last archive", Settings.LastUpdateArchive ?? "-", 5);
        AddReadOnlyPath(layout, "Native status", runtimeStatus?.NativeStatus ?? "Unknown", 6);
        AddReadOnlyPath(layout, "Python status", runtimeStatus?.PythonStatus ?? "Unknown", 7);
    }

    private static TableLayoutPanel CreateLayout(int rows)
    {
        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 3,
            RowCount = rows
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

        for (int i = 0; i < rows; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        }

        return layout;
    }

    private static void AddLabel(TableLayoutPanel layout, string text, int column, int row)
    {
        layout.Controls.Add(new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, column, row);
    }

    private static void AddCheckBox(TableLayoutPanel layout, CheckBox checkBox, string text, bool isChecked, int row)
    {
        checkBox.Text = text;
        checkBox.Checked = isChecked;
        checkBox.Dock = DockStyle.Fill;
        layout.Controls.Add(checkBox, 1, row);
        layout.SetColumnSpan(checkBox, 2);
    }

    private static void AddReadOnlyPath(TableLayoutPanel layout, string label, string value, int row)
    {
        AddLabel(layout, label, 0, row);
        TextBox textBox = new TextBox
        {
            Text = value,
            ReadOnly = true,
            Dock = DockStyle.Fill
        };
        layout.Controls.Add(textBox, 1, row);

        Button openButton = new Button
        {
            Text = "Open",
            Dock = DockStyle.Fill,
            Enabled = Directory.Exists(value) || File.Exists(value),
            FlatStyle = FlatStyle.Flat
        };
        openButton.Click += (_, _) =>
        {
            if (Directory.Exists(value) || File.Exists(value))
            {
                ShellFileOperations.OpenFolderAndSelect(value);
            }
        };
        layout.Controls.Add(openButton, 2, row);
    }

    private static void AddFormatRow(TableLayoutPanel layout, int row, string label, ComboBox comboBox, string category, ConverterFormat? value, bool includeSame)
    {
        AddLabel(layout, label, 0, row);
        comboBox.Dock = DockStyle.Fill;
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Items.AddRange(OutputFormatChoice.ForCategory(category, includeSame).Cast<object>().ToArray());
        SelectFormat(comboBox, value);
        layout.Controls.Add(comboBox, 1, row);
        layout.SetColumnSpan(comboBox, 2);
    }

    private static void SetupNumeric(NumericUpDown control, int min, int max, int value)
    {
        control.Minimum = min;
        control.Maximum = max;
        control.Value = Math.Clamp(value, min, max);
        control.Dock = DockStyle.Left;
        control.Width = 120;
    }

    private static void SelectFormat(ComboBox comboBox, ConverterFormat? value)
    {
        OutputFormatChoice? selected = comboBox.Items.Cast<OutputFormatChoice>().FirstOrDefault(choice => choice.Format == value)
            ?? comboBox.Items.Cast<OutputFormatChoice>().FirstOrDefault();
        if (selected != null)
        {
            comboBox.SelectedItem = selected;
        }
    }

    private static ConverterFormat? SelectedFormat(ComboBox comboBox)
    {
        return comboBox.SelectedItem is OutputFormatChoice choice ? choice.Format : null;
    }

    private void BrowseOutputDirectory()
    {
        using FolderBrowserDialog dialog = new FolderBrowserDialog
        {
            Description = "Select output directory",
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(outputDirectoryTextBox.Text) ? outputDirectoryTextBox.Text : FormatForgePaths.DefaultOutputDirectory
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            outputDirectoryTextBox.Text = dialog.SelectedPath;
        }
    }
}
