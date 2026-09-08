using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace FormatForge.App
{
    public partial class Form1 : Form
    {
        private readonly List<FileQueueItem> queuedFiles = new();
        private readonly Dictionary<string, CheckBox> tagCheckBoxes = new(StringComparer.OrdinalIgnoreCase);
        private ConversionSettings conversionSettings = ConversionSettings.Load();
        private CancellationTokenSource? activeConversion;
        private bool refreshingList;
        private bool changingTagFilters;
        private bool refreshingOutputFormatOptions;
        private bool startupChecksRan;
        private Image? currentPreviewImage;

        public Form1()
        {
            InitializeComponent();
            ConfigureRuntimeUi();
        }

        private void ConfigureRuntimeUi()
        {
            outputFormatComboBox.Items.Clear();
            outputFormatComboBox.DisplayMember = nameof(OutputFormatChoice.DisplayName);
            outputFormatComboBox.ValueMember = nameof(OutputFormatChoice.Format);
            ApplySettingsToMainControls();

            detailsDurationLabel.Text = "Status:";
            detailsBitrateLabel.Text = "Output:";
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            versionStatusLabel.Text = AppInfo.ProductName + " v" + AppInfo.Version;
            convertNowButton.Text = "  Convert";
            convertNowButton.Image = UiIconFactory.CreateIcon(UiIconKind.Convert, Color.White, 26);
            mergeImagesPdfButton.Image = UiIconFactory.CreateIcon(UiIconKind.Pdf, Color.White, 22);
            detailsPreviewIcon.Dock = DockStyle.Fill;
            detailsPreviewIcon.Location = new Point(0, 0);
            detailsPreviewIcon.SizeMode = PictureBoxSizeMode.Zoom;

            filesList.DragEnter += FilesList_DragEnter;
            filesList.DragDrop += FilesList_DragDrop;
            filesList.DoubleClick += FilesList_DoubleClick;
            filesList.OwnerDraw = true;
            filesList.DrawColumnHeader += FilesList_DrawColumnHeader;
            filesList.DrawItem += FilesList_DrawItem;
            filesList.DrawSubItem += FilesList_DrawSubItem;
            detailsAddTagButton.Click += DetailsAddTagButton_Click;
            outputFormatComboBox.SelectedIndexChanged += OutputFormatComboBox_SelectedIndexChanged;

            RegisterTagCheckBoxes();
            ApplyTheme();
            RefreshOutputFormatOptions();
            RefreshFileList();
            UpdateDetailsPanel();
            UpdateStatusText();

            if (ConverterCore.TryInitialize(out string? nativeError))
            {
                string pythonStatus = ConverterCore.IsPythonAvailable(out string? pythonError)
                    ? "PythonRuntime ready"
                    : "PythonRuntime unavailable: " + pythonError;
                statusTextLabel.Text = "Native converters ready. " + pythonStatus;
            }
            else
            {
                statusTextLabel.Text = "Native converters unavailable: " + nativeError;
            }
        }

        private void RegisterTagCheckBoxes()
        {
            tagCheckBoxes.Clear();
            foreach (Control control in tagsFlow.Controls)
            {
                if (control is CheckBox checkBox && checkBox.Tag is string tag)
                {
                    tagCheckBoxes[tag] = checkBox;
                }
            }
        }


        private void ApplySettingsToMainControls()
        {
            outputDirectoryTextBox.Text = string.IsNullOrWhiteSpace(conversionSettings.DefaultOutputDirectory)
                ? FormatForgePaths.DefaultOutputDirectory
                : conversionSettings.DefaultOutputDirectory;
            openOutputFolderCheckBox.Checked = conversionSettings.OpenOutputFolderAfterConversion;
            preserveMetadataCheckBox.Checked = conversionSettings.PreserveMetadata;
            keepOriginalDateCheckBox.Checked = conversionSettings.KeepOriginalDate;
            overwriteExistingFilesCheckBox.Checked = conversionSettings.OverwriteExistingFiles;
        }

        private void PullMainControlsIntoSettings()
        {
            conversionSettings.DefaultOutputDirectory = string.IsNullOrWhiteSpace(outputDirectoryTextBox.Text)
                ? FormatForgePaths.DefaultOutputDirectory
                : outputDirectoryTextBox.Text.Trim();
            conversionSettings.OpenOutputFolderAfterConversion = openOutputFolderCheckBox.Checked;
            conversionSettings.PreserveMetadata = preserveMetadataCheckBox.Checked;
            conversionSettings.KeepOriginalDate = keepOriginalDateCheckBox.Checked;
            conversionSettings.OverwriteExistingFiles = overwriteExistingFilesCheckBox.Checked;
            conversionSettings.Normalize();
        }

        private void SaveCurrentSettings()
        {
            PullMainControlsIntoSettings();
            try
            {
                conversionSettings.Save();
            }
            catch (Exception ex)
            {
                statusTextLabel.Text = "Settings could not be saved: " + ex.Message;
            }
        }

        private void ApplyTheme()
        {
            ThemePalette palette = ThemeManager.GetPalette(conversionSettings.Theme);
            ThemeManager.Apply(this, conversionSettings.Theme);
            ThemeManager.Apply(fileContextMenu, conversionSettings.Theme);

            shellPanel.BackColor = palette.Window;
            workspacePanel.BackColor = palette.Window;
            headerPanel.BackColor = Color.Transparent;
            filesList.BackColor = palette.ListBack;
            filesList.ForeColor = palette.Foreground;
            filesList.GridLines = conversionSettings.Theme == AppThemeMode.Light;
            progressTextLabel.ForeColor = palette.MutedForeground;
            progressBar.TrackColor = palette.Input;
            progressBar.FillColor = palette.Primary;
            progressBar.BorderColor = palette.Border;
            detailsDividerPanel.BackColor = palette.Separator;
            sidebarSeparatorTop.BackColor = palette.Separator;
            sidebarSeparatorBottom.BackColor = palette.Separator;

            convertNowButton.BackColor = palette.Primary;
            convertNowButton.ForeColor = Color.White;
            convertNowButton.FlatAppearance.BorderColor = palette.Primary;
            convertNowButton.Image = UiIconFactory.CreateIcon(UiIconKind.Convert, Color.White, 26);

            sidebarConvertButton.BackColor = palette.Primary;
            sidebarConvertButton.ForeColor = Color.White;
            sidebarConvertButton.FlatAppearance.BorderColor = palette.Primary;
            sidebarConvertButton.Image = UiIconFactory.CreateIcon(UiIconKind.Convert, Color.White, 20);

            mergeImagesPdfButton.BackColor = palette.Primary;
            mergeImagesPdfButton.ForeColor = Color.White;
            mergeImagesPdfButton.FlatAppearance.BorderColor = palette.Primary;
            mergeImagesPdfButton.Image = UiIconFactory.CreateIcon(UiIconKind.Pdf, Color.White, 22);

            sidebarAddFilesButton.Image = UiIconFactory.CreateIcon(UiIconKind.AddFile, palette.Icon, 22);
            sidebarAddFolderButton.Image = UiIconFactory.CreateIcon(UiIconKind.AddFolder, palette.Icon, 22);
            sidebarRemoveButton.Image = UiIconFactory.CreateIcon(UiIconKind.Remove, Color.FromArgb(245, 96, 108), 22);
            sidebarClearAllButton.Image = UiIconFactory.CreateIcon(UiIconKind.Clear, palette.Icon, 22);
            sidebarSettingsButton.Image = UiIconFactory.CreateIcon(UiIconKind.Settings, palette.Icon, 22);
            sidebarAboutButton.Image = UiIconFactory.CreateIcon(UiIconKind.Info, palette.Icon, 22);
            tagsHeaderLabel.Image = UiIconFactory.CreateIcon(UiIconKind.Tags, palette.Icon, 20);
            ApplyTagLayout(palette);

            advancedButton.Image = UiIconFactory.CreateIcon(UiIconKind.Advanced, palette.Icon, 20);
            browseOutputButton.Image = null;
            detailsAddTagButton.Image = UiIconFactory.CreateIcon(UiIconKind.Plus, palette.Icon, 16);
            outputDirectoryLabel.Image = UiIconFactory.CreateIcon(UiIconKind.Folder, palette.Icon, 16);
            appIcon.Image = UiIconFactory.CreateAppLogoBitmap(64);
            RefreshFileTypeIcons(palette);
        }

        private void ApplyTagLayout(ThemePalette palette)
        {
            tagsHeaderLabel.Text = "      Tags";
            tagsHeaderLabel.TextAlign = ContentAlignment.MiddleLeft;
            tagsHeaderLabel.ImageAlign = ContentAlignment.MiddleLeft;

            ConfigureTagCheckBox(tagImagesCheckBox, "   Images", UiIconKind.Image, Color.FromArgb(24, 120, 232));
            ConfigureTagCheckBox(tagAudioCheckBox, "   Audio", UiIconKind.Audio, Color.FromArgb(232, 55, 102));
            ConfigureTagCheckBox(tagVideoCheckBox, "   Video", UiIconKind.Video, Color.FromArgb(138, 65, 226));
            ConfigureTagCheckBox(tagDocumentsCheckBox, "   Documents", UiIconKind.Document, palette.Icon);
            ConfigureTagCheckBox(tagPdfCheckBox, "   PDF", UiIconKind.Pdf, Color.FromArgb(239, 61, 74));
            ConfigureTagCheckBox(tagConvertedCheckBox, "   Converted", UiIconKind.Check, Color.FromArgb(27, 166, 67));
            ConfigureTagCheckBox(tagFailedCheckBox, "   Failed", UiIconKind.Warning, Color.FromArgb(242, 116, 36));
            ConfigureTagCheckBox(tagFavoritesCheckBox, "   Favorites", UiIconKind.Plus, Color.FromArgb(238, 172, 22));
        }

        private static void ConfigureTagCheckBox(CheckBox checkBox, string text, UiIconKind iconKind, Color iconColor)
        {
            checkBox.AutoSize = false;
            checkBox.CheckAlign = ContentAlignment.MiddleLeft;
            checkBox.Image = UiIconFactory.CreateIcon(iconKind, iconColor, 18);
            checkBox.ImageAlign = ContentAlignment.MiddleLeft;
            checkBox.Text = text;
            checkBox.TextAlign = ContentAlignment.MiddleLeft;
            checkBox.TextImageRelation = TextImageRelation.ImageBeforeText;
            checkBox.Size = new Size(154, 25);
        }

        private void RefreshFileTypeIcons(ThemePalette palette)
        {
            ReplaceImageListImage(fileTypeImages, "file", UiIconFactory.CreateIcon(UiIconKind.Document, palette.Icon, 24));
            ReplaceImageListImage(fileTypeImages, "image", UiIconFactory.CreateIcon(UiIconKind.Image, Color.FromArgb(44, 174, 222), 24));
            ReplaceImageListImage(fileTypeImages, "audio", UiIconFactory.CreateIcon(UiIconKind.Audio, Color.FromArgb(242, 86, 127), 24));
            ReplaceImageListImage(fileTypeImages, "video", UiIconFactory.CreateIcon(UiIconKind.Video, Color.FromArgb(158, 101, 242), 24));
            ReplaceImageListImage(fileTypeImages, "document", UiIconFactory.CreateIcon(UiIconKind.Document, palette.Icon, 24));
            ReplaceImageListImage(fileTypeImages, "pdf", UiIconFactory.CreateIcon(UiIconKind.Pdf, Color.FromArgb(250, 92, 104), 24));

            ReplaceImageListImage(fileTypeLargeImages, "file", UiIconFactory.CreateIcon(UiIconKind.Document, palette.Icon, 48));
            ReplaceImageListImage(fileTypeLargeImages, "image", UiIconFactory.CreateIcon(UiIconKind.Image, Color.FromArgb(44, 174, 222), 48));
            ReplaceImageListImage(fileTypeLargeImages, "audio", UiIconFactory.CreateIcon(UiIconKind.Audio, Color.FromArgb(242, 86, 127), 48));
            ReplaceImageListImage(fileTypeLargeImages, "video", UiIconFactory.CreateIcon(UiIconKind.Video, Color.FromArgb(158, 101, 242), 48));
            ReplaceImageListImage(fileTypeLargeImages, "document", UiIconFactory.CreateIcon(UiIconKind.Document, palette.Icon, 48));
            ReplaceImageListImage(fileTypeLargeImages, "pdf", UiIconFactory.CreateIcon(UiIconKind.Pdf, Color.FromArgb(250, 92, 104), 48));
        }

        private static void ReplaceImageListImage(ImageList imageList, string key, Image image)
        {
            int index = imageList.Images.IndexOfKey(key);
            if (index >= 0)
            {
                imageList.Images.RemoveAt(index);
                imageList.Images.Add(key, image);
                return;
            }

            imageList.Images.Add(key, image);
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (startupChecksRan)
            {
                return;
            }

            startupChecksRan = true;
            ShowMissingDependencyWarnings();
            await CheckForUpdatesOnStartupAsync();
        }

        private void ShowMissingDependencyWarnings()
        {
            IReadOnlyList<DependencyCheckResult> missingDependencies = WindowsDependencyChecker
                .CheckRequiredDependencies()
                .Where(result => !result.IsAvailable)
                .ToList();

            if (missingDependencies.Count == 0)
            {
                return;
            }

            using DependencyWarningForm dialog = new DependencyWarningForm(missingDependencies, conversionSettings.Theme);
            dialog.ShowDialog(this);
            statusTextLabel.Text = "Missing: " + string.Join(", ", missingDependencies.Select(result => result.Name)) + ".";
        }

        private async Task CheckForUpdatesOnStartupAsync()
        {
            AppUpdateInfo updateInfo = await AppUpdateChecker.CheckLatestReleaseAsync(CancellationToken.None);
            if (!updateInfo.IsConfigured)
            {
                return;
            }

            if (updateInfo.IsUpdateAvailable)
            {
                using UpdateAvailableForm dialog = new UpdateAvailableForm(updateInfo, conversionSettings.Theme);
                dialog.ShowDialog(this);
                statusTextLabel.Text = "New version available: " + updateInfo.LatestVersion + ".";
                return;
            }

            if (updateInfo.Message.StartsWith("Update check failed", StringComparison.OrdinalIgnoreCase))
            {
                statusTextLabel.Text = updateInfo.Message;
            }
        }

        private bool ShowConversionOptionsDialog()
        {
            PullMainControlsIntoSettings();
            string contextCategory = GetOutputFormatContextCategory() ?? "Mixed";
            int selectedCount = GetSelectedModels().Count();
            int queuedCount = queuedFiles.Count(item => item.IsChecked);

            using ConversionOptionsForm dialog = new ConversionOptionsForm(conversionSettings, contextCategory, queuedCount, selectedCount);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                ApplySettingsToMainControls();
                RefreshOutputFormatOptions();
                return false;
            }

            conversionSettings = dialog.Settings;
            conversionSettings.Save();
            ApplySettingsToMainControls();
            ApplyTheme();
            RefreshOutputFormatOptions();
            RefreshFileList();
            UpdateDetailsPanel();
            statusTextLabel.Text = "Conversion options saved.";
            return true;
        }

        private void RefreshOutputFormatOptions()
        {
            if (refreshingOutputFormatOptions)
            {
                return;
            }

            refreshingOutputFormatOptions = true;
            try
            {
                string? category = GetOutputFormatContextCategory();
                List<OutputFormatChoice> choices = OutputFormatChoice.ForCategory(category, includeSameAsSource: true).ToList();
                OutputFormatChoice? selectedChoice = GetOutputChoiceForContext(category, choices);

                outputFormatComboBox.BeginUpdate();
                try
                {
                    outputFormatComboBox.Items.Clear();
                    outputFormatComboBox.Items.AddRange(choices.Cast<object>().ToArray());
                    outputFormatComboBox.Enabled = choices.Count > 1 && activeConversion == null;
                    outputFormatLabel.Text = string.IsNullOrWhiteSpace(category) || category == "Mixed"
                        ? "Output Format:"
                        : category + " Output:";

                    if (selectedChoice != null)
                    {
                        outputFormatComboBox.SelectedItem = selectedChoice;
                    }
                    else if (outputFormatComboBox.Items.Count > 0)
                    {
                        outputFormatComboBox.SelectedIndex = 0;
                    }
                }
                finally
                {
                    outputFormatComboBox.EndUpdate();
                }
            }
            finally
            {
                refreshingOutputFormatOptions = false;
            }
        }

        private OutputFormatChoice? GetOutputChoiceForContext(string? category, IEnumerable<OutputFormatChoice> choices)
        {
            List<FileQueueItem> selected = GetSelectedModels().ToList();
            ConverterFormat? desired = null;

            if (selected.Count > 0)
            {
                ConverterFormat? first = selected[0].OutputFormatOverride;
                if (selected.All(item => item.OutputFormatOverride == first))
                {
                    desired = first;
                }
            }
            else if (!string.IsNullOrWhiteSpace(category) && category != "Mixed")
            {
                desired = conversionSettings.GetDefaultFormatForCategory(category);
            }

            return choices.FirstOrDefault(choice => choice.Format == desired) ?? choices.FirstOrDefault(choice => choice.IsSameAsSource);
        }

        private string? GetOutputFormatContextCategory()
        {
            List<FileQueueItem> selected = GetSelectedModels().ToList();
            string? selectedCategory = TryGetSingleCategory(selected);
            if (!string.IsNullOrWhiteSpace(selectedCategory))
            {
                return selectedCategory;
            }

            List<FileQueueItem> checkedItems = queuedFiles.Where(item => item.IsChecked).ToList();
            string? checkedCategory = TryGetSingleCategory(checkedItems);
            if (!string.IsNullOrWhiteSpace(checkedCategory))
            {
                return checkedCategory;
            }

            List<string> activeTags = tagCheckBoxes
                .Where(pair => pair.Value.Checked)
                .Select(pair => pair.Key)
                .Where(tag => tag is "Images" or "Audio" or "Video" or "Documents" or "PDF")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return activeTags.Count == 1 ? activeTags[0] : "Mixed";
        }

        private static string? TryGetSingleCategory(IEnumerable<FileQueueItem> items)
        {
            List<string> categories = items
                .Select(item => item.Category)
                .Where(category => category is "Images" or "Audio" or "Video" or "Documents" or "PDF")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return categories.Count == 1 ? categories[0] : null;
        }

        private void OutputFormatComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (refreshingOutputFormatOptions || outputFormatComboBox.SelectedItem is not OutputFormatChoice choice)
            {
                return;
            }

            List<FileQueueItem> selected = GetSelectedModels().ToList();
            if (selected.Count > 0)
            {
                foreach (FileQueueItem item in selected)
                {
                    if (choice.Format == null || OutputFormatChoice.IsCompatible(item.Category, choice.Format.Value))
                    {
                        item.OutputFormatOverride = choice.Format;
                        UpdateVisibleRow(item);
                    }
                }

                statusTextLabel.Text = choice.IsSameAsSource
                    ? "Selected file(s) will use source format."
                    : "Selected file(s) output: " + choice.DisplayName;
                UpdateDetailsPanel();
                return;
            }

            string? category = GetOutputFormatContextCategory();
            if (!string.IsNullOrWhiteSpace(category) && category != "Mixed")
            {
                conversionSettings.SetDefaultFormatForCategory(category, choice.Format);
                SaveCurrentSettings();
                statusTextLabel.Text = choice.IsSameAsSource
                    ? category + " output: same as source."
                    : category + " output: " + choice.DisplayName;
            }
        }

        private void AddFilesButton_Click(object? sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            AddPathsToQueue(openFileDialog.FileNames);
        }

        private void AddFolderButton_Click(object? sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            AddPathsToQueue(EnumerateFilesSafe(folderBrowserDialog.SelectedPath));
        }

        private void BrowseOutputButton_Click(object? sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                outputDirectoryTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private async void ConvertNowButton_Click(object? sender, EventArgs e)
        {
            if (activeConversion != null)
            {
                activeConversion.Cancel();
                ConverterCore.Cancel();
                statusTextLabel.Text = "Cancelling conversion...";
                return;
            }

            SaveCurrentSettings();

            List<FileQueueItem> items = queuedFiles.Where(item => item.IsChecked).ToList();
            if (items.Count == 0)
            {
                MessageBox.Show(this, "Select at least one queued file before conversion.", "FormatForge", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string outputDirectory = outputDirectoryTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                MessageBox.Show(this, "Choose an output directory first.", "FormatForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Directory.CreateDirectory(outputDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Output directory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            activeConversion = new CancellationTokenSource();
            SetConversionUiState(true);

            int completed = 0;
            int failed = 0;
            progressBar.Value = 0;

            try
            {
                for (int index = 0; index < items.Count; index++)
                {
                    FileQueueItem item = items[index];
                    CancellationToken token = activeConversion.Token;
                    if (token.IsCancellationRequested)
                    {
                        item.Status = "Cancelled";
                        UpdateVisibleRow(item);
                        break;
                    }

                    if (!File.Exists(item.InputPath))
                    {
                        item.Status = "Missing";
                        item.ErrorMessage = "Input file was not found.";
                        item.Tags.Add("Failed");
                        failed++;
                        completed++;
                        UpdateVisibleRow(item);
                        continue;
                    }

                    if (!TryResolveOutputFormat(item, out ConverterFormat format, out string? formatError))
                    {
                        item.Status = "Unsupported";
                        item.ErrorMessage = formatError;
                        item.Tags.Add("Failed");
                        failed++;
                        completed++;
                        UpdateVisibleRow(item);
                        continue;
                    }

                    if (!IsOutputCompatible(item, format))
                    {
                        item.Status = "Unsupported";
                        item.ErrorMessage = "The selected output format is not compatible with " + item.Category + " files.";
                        item.Tags.Add("Failed");
                        failed++;
                        completed++;
                        UpdateVisibleRow(item);
                        continue;
                    }

                    string outputPath = BuildOutputPath(item, format, outputDirectory);
                    item.OutputPath = outputPath;
                    item.Status = "Converting";
                    item.ErrorMessage = null;
                    UpdateVisibleRow(item);
                    SelectModel(item);

                    int baseProgress = (int)Math.Round(index * 100.0 / items.Count);
                    int nextProgress = (int)Math.Round((index + 1) * 100.0 / items.Count);
                    Progress<double> fileProgress = new Progress<double>(value =>
                    {
                        int progressValue = baseProgress + (int)Math.Round((nextProgress - baseProgress) * Math.Clamp(value, 0.0, 1.0));
                        progressBar.Value = Math.Clamp(progressValue, 0, 100);
                    });

                    ConversionRequest request = new ConversionRequest
                    {
                        InputPath = item.InputPath,
                        OutputPath = outputPath,
                        Format = format,
                        Quality = conversionSettings.Quality,
                        Bitrate = conversionSettings.Bitrate,
                        Width = conversionSettings.Width,
                        Height = conversionSettings.Height,
                        PreserveMetadata = conversionSettings.PreserveMetadata,
                        Overwrite = conversionSettings.OverwriteExistingFiles
                    };

                    ConversionOutcome outcome = await Task.Run(() => ConverterCore.Convert(request, fileProgress, token));

                    if (outcome.Success)
                    {
                        item.Status = "Converted";
                        item.OutputPath = outcome.OutputPath;
                        item.Tags.Remove("Failed");
                        item.Tags.Add("Converted");
                        ApplyOutputTimestamp(item);
                        completed++;
                    }
                    else if (outcome.Error == ConverterError.Cancelled || token.IsCancellationRequested)
                    {
                        item.Status = "Cancelled";
                        item.ErrorMessage = outcome.Message;
                        completed++;
                        UpdateVisibleRow(item);
                        break;
                    }
                    else
                    {
                        item.Status = "Failed";
                        item.ErrorMessage = outcome.Message;
                        item.Tags.Add("Failed");
                        failed++;
                        completed++;
                    }

                    progressBar.Value = Math.Clamp(nextProgress, 0, 100);
                    UpdateVisibleRow(item);
                    UpdateStatusText();
                }
            }
            finally
            {
                bool cancelled = activeConversion.IsCancellationRequested;
                activeConversion.Dispose();
                activeConversion = null;
                SetConversionUiState(false);
                RefreshFileList();
                RefreshOutputFormatOptions();
                UpdateDetailsPanel();
                UpdateStatusText();
                SaveCurrentSettings();

                if (cancelled)
                {
                    statusTextLabel.Text = "Conversion cancelled.";
                }
                else
                {
                    statusTextLabel.Text = "Done: " + completed + " processed, " + failed + " failed.";
                    progressBar.Value = items.Count == 0 ? 0 : 100;
                    if (failed == 0 && openOutputFolderCheckBox.Checked)
                    {
                        ShellFileOperations.OpenFolderAndSelect(outputDirectory);
                    }
                }
            }
        }

        private async void MergeImagesPdfButton_Click(object? sender, EventArgs e)
        {
            if (activeConversion != null)
            {
                return;
            }

            SaveCurrentSettings();

            List<FileQueueItem> imageItems = GetImageItemsForPdf().ToList();
            if (imageItems.Count < 2)
            {
                MessageBox.Show(this, "Select at least two image files, or check at least two image files in the queue.", "Images to PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string outputDirectory = outputDirectoryTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                outputDirectory = FormatForgePaths.DefaultOutputDirectory;
                outputDirectoryTextBox.Text = outputDirectory;
            }

            try
            {
                Directory.CreateDirectory(outputDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Output directory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Save combined PDF",
                Filter = "PDF file (*.pdf)|*.pdf",
                AddExtension = true,
                DefaultExt = "pdf",
                InitialDirectory = outputDirectory,
                FileName = BuildMergedPdfFileName()
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string outputPath = overwriteExistingFilesCheckBox.Checked ? dialog.FileName : GetUniquePath(dialog.FileName);
            activeConversion = new CancellationTokenSource();
            SetConversionUiState(true);
            progressBar.Value = 0;
            progressTextLabel.Text = "Creating PDF from " + imageItems.Count + " image(s)...";
            statusTextLabel.Text = "Creating combined PDF...";

            foreach (FileQueueItem item in imageItems)
            {
                item.Status = "Adding to PDF";
                item.ErrorMessage = null;
                item.OutputPath = outputPath;
                UpdateVisibleRow(item);
            }

            Progress<double> progress = new Progress<double>(value =>
            {
                progressBar.Value = Math.Clamp((int)Math.Round(Math.Clamp(value, 0.0, 1.0) * 100), 0, 100);
            });

            PdfCompositionOutcome outcome;
            try
            {
                outcome = await ImagePdfComposer.MergeImagesToPdfAsync(
                    imageItems.Select(item => item.InputPath).ToArray(),
                    outputPath,
                    progress,
                    activeConversion.Token);
            }
            finally
            {
                bool cancelled = activeConversion.IsCancellationRequested;
                activeConversion.Dispose();
                activeConversion = null;
                SetConversionUiState(false);

                if (cancelled)
                {
                    foreach (FileQueueItem item in imageItems)
                    {
                        item.Status = "Cancelled";
                        item.ErrorMessage = "PDF creation was cancelled.";
                    }
                }
            }

            if (outcome.Success)
            {
                foreach (FileQueueItem item in imageItems)
                {
                    item.Status = "Added to PDF";
                    item.ErrorMessage = null;
                    item.OutputPath = outcome.OutputPath;
                    item.Tags.Remove("Failed");
                    item.Tags.Add("Converted");
                }

                bool addedPdf = AddFileToQueue(outcome.OutputPath);
                FileQueueItem? pdfItem = queuedFiles.FirstOrDefault(item => string.Equals(item.InputPath, outcome.OutputPath, StringComparison.OrdinalIgnoreCase));
                if (pdfItem != null)
                {
                    pdfItem.Status = addedPdf ? "Ready" : pdfItem.Status;
                    pdfItem.Tags.Add("PDF");
                    pdfItem.Tags.Add("Converted");
                }

                progressBar.Value = 100;
                statusTextLabel.Text = "Combined PDF created: " + outcome.OutputPath;
                if (openOutputFolderCheckBox.Checked)
                {
                    ShellFileOperations.OpenFolderAndSelect(outcome.OutputPath);
                }
            }
            else if (!string.Equals(outcome.Message, "PDF creation was cancelled.", StringComparison.OrdinalIgnoreCase))
            {
                foreach (FileQueueItem item in imageItems)
                {
                    item.Status = "Failed";
                    item.ErrorMessage = outcome.Message;
                    item.Tags.Add("Failed");
                }

                MessageBox.Show(this, outcome.Message, "Images to PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                statusTextLabel.Text = "Images to PDF failed.";
            }
            else
            {
                statusTextLabel.Text = "Images to PDF cancelled.";
            }

            RefreshFileList();
            if (outcome.Success)
            {
                FileQueueItem? pdfItem = queuedFiles.FirstOrDefault(item => string.Equals(item.InputPath, outcome.OutputPath, StringComparison.OrdinalIgnoreCase));
                if (pdfItem != null)
                {
                    SelectModel(pdfItem);
                }
            }

            RefreshOutputFormatOptions();
            UpdateDetailsPanel();
            UpdateStatusText();
        }

        private void AdvancedButton_Click(object? sender, EventArgs e)
        {
            ShowConversionOptionsDialog();
        }

        private void SettingsSidebarButton_Click(object? sender, EventArgs e)
        {
            PullMainControlsIntoSettings();
            string nativeStatus = ConverterCore.TryInitialize(out string? nativeError) ? "Ready" : nativeError ?? "Unavailable";
            string pythonStatus = ConverterCore.IsPythonAvailable(out string? pythonError) ? "Ready" : pythonError ?? "Unavailable";

            using SettingsForm dialog = new SettingsForm(conversionSettings, new RuntimeStatus(nativeStatus, pythonStatus));
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                conversionSettings = dialog.Settings;
                conversionSettings.Save();
                ApplySettingsToMainControls();
                ApplyTheme();
                RefreshOutputFormatOptions();
                RefreshFileList();
                UpdateDetailsPanel();
                statusTextLabel.Text = "Settings saved.";
            }
        }

        private void HelpUpdatePythonMenuItem_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select Python embedded runtime archive",
                Filter = "Python embedded archive (*.zip)|*.zip|All files|*.*",
                Multiselect = false
            };

            string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (Directory.Exists(downloads))
            {
                dialog.InitialDirectory = downloads;
            }

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(FormatForgePaths.UpgradeDirectory);
                string targetFile = Path.Combine(
                    FormatForgePaths.UpgradeDirectory,
                    "python_runtime_update_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + Path.GetExtension(dialog.FileName));
                File.Copy(dialog.FileName, targetFile, overwrite: false);
                conversionSettings.LastUpdateArchive = targetFile;
                SaveCurrentSettings();
                MessageBox.Show(this, "Archive staged for update:" + Environment.NewLine + targetFile, "Update Python and Libraries", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Update Python and Libraries", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void HelpDocumentationMenuItem_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(this, "Documentation will be added after the conversion pipeline is finalized.", "FormatForge", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void HelpAboutMenuItem_Click(object? sender, EventArgs e)
        {
            using AboutForm dialog = new AboutForm(conversionSettings.Theme);
            dialog.ShowDialog(this);
        }

        private void ViewRefreshMenuItem_Click(object? sender, EventArgs e)
        {
            RefreshFileList();
            RefreshOutputFormatOptions();
            UpdateDetailsPanel();
            UpdateStatusText();
            statusTextLabel.Text = "View refreshed.";
        }

        private void ClearAllButton_Click(object? sender, EventArgs e)
        {
            if (queuedFiles.Count == 0)
            {
                return;
            }

            if (conversionSettings.ConfirmRemoveFromQueue &&
                MessageBox.Show(this, "Clear all queued files?", "Clear queue", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            queuedFiles.Clear();
            progressBar.Value = 0;
            RefreshFileList();
            UpdateDetailsPanel();
            UpdateStatusText();
            statusTextLabel.Text = "Queue cleared.";
        }

        private void RemoveSelectedButton_Click(object? sender, EventArgs e)
        {
            RemoveSelectedItems();
        }

        private void CopyMenuItem_Click(object? sender, EventArgs e)
        {
            CopySelectedPathsToClipboard(false);
        }

        private void CutMenuItem_Click(object? sender, EventArgs e)
        {
            CopySelectedPathsToClipboard(true);
        }

        private void PasteMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!Clipboard.ContainsFileDropList())
                {
                    return;
                }

                StringCollection fileDropList = Clipboard.GetFileDropList();
                AddPathsToQueue(fileDropList.Cast<string>());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Paste files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RenameMenuItem_Click(object? sender, EventArgs e)
        {
            BeginRenameSelectedItem();
        }

        private void DeleteMenuItem_Click(object? sender, EventArgs e)
        {
            RemoveSelectedItems();
        }

        private void SelectAllMenuItem_Click(object? sender, EventArgs e)
        {
            filesList.Focus();
            foreach (ListViewItem item in filesList.Items)
            {
                item.Selected = true;
            }

            UpdateDetailsPanel();
            UpdateStatusText();
        }

        private void PropertiesMenuItem_Click(object? sender, EventArgs e)
        {
            FileQueueItem? item = GetSelectedModels().FirstOrDefault();
            if (item == null)
            {
                return;
            }

            if (File.Exists(item.InputPath) && ShellFileOperations.ShowProperties(this, item.InputPath))
            {
                return;
            }

            string message = "Name: " + item.DisplayName + Environment.NewLine +
                             "Type: " + item.TypeName + Environment.NewLine +
                             "Size: " + item.SizeText + Environment.NewLine +
                             "Status: " + item.Status + Environment.NewLine +
                             "Input: " + item.InputPath + Environment.NewLine +
                             "Output: " + (item.OutputPath ?? "-") + Environment.NewLine +
                             "Tags: " + string.Join(", ", item.Tags);

            MessageBox.Show(this, message, "File properties", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ViewDetailsMenuItem_Click(object? sender, EventArgs e)
        {
            filesList.View = View.Details;
            viewDetailsMenuItem.Checked = true;
            viewLargeIconsMenuItem.Checked = false;
        }

        private void ViewLargeIconsMenuItem_Click(object? sender, EventArgs e)
        {
            filesList.View = View.LargeIcon;
            viewLargeIconsMenuItem.Checked = true;
            viewDetailsMenuItem.Checked = false;
        }

        private void ViewTagsPanelMenuItem_CheckedChanged(object? sender, EventArgs e)
        {
            mainSplit.Panel1Collapsed = !viewTagsPanelMenuItem.Checked;
        }

        private void ClearTagsButton_Click(object? sender, EventArgs e)
        {
            changingTagFilters = true;
            try
            {
                foreach (CheckBox checkBox in tagCheckBoxes.Values)
                {
                    checkBox.Checked = false;
                }
            }
            finally
            {
                changingTagFilters = false;
            }

            RefreshFileList();
            RefreshOutputFormatOptions();
            statusTextLabel.Text = "Tag filters cleared.";
        }

        private void FileExitMenuItem_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void FileContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            bool hasSelection = filesList.SelectedItems.Count > 0;
            contextCutMenuItem.Enabled = hasSelection;
            contextCopyMenuItem.Enabled = hasSelection;
            contextRenameMenuItem.Enabled = hasSelection && filesList.SelectedItems.Count == 1;
            contextDeleteMenuItem.Enabled = hasSelection;
            contextPropertiesMenuItem.Enabled = hasSelection;

            try
            {
                contextPasteMenuItem.Enabled = Clipboard.ContainsFileDropList();
            }
            catch
            {
                contextPasteMenuItem.Enabled = false;
            }
        }

        private void FilesList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            RefreshOutputFormatOptions();
            UpdateDetailsPanel();
            UpdateStatusText();
        }

        private void FilesList_ItemChecked(object? sender, ItemCheckedEventArgs e)
        {
            if (refreshingList)
            {
                return;
            }

            if (e.Item.Tag is FileQueueItem model)
            {
                model.IsChecked = e.Item.Checked;
            }

            RefreshOutputFormatOptions();
            UpdateStatusText();
        }

        private void FilesList_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (conversionSettings.Theme == AppThemeMode.Light)
            {
                e.DrawDefault = true;
                return;
            }

            ThemePalette palette = ThemeManager.GetPalette(conversionSettings.Theme);
            using SolidBrush backgroundBrush = new SolidBrush(palette.SurfaceAlt);
            using Pen borderPen = new Pen(palette.Separator);
            e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
            e.Graphics.DrawLine(borderPen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            e.Graphics.DrawLine(borderPen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);

            Rectangle textBounds = Rectangle.Inflate(e.Bounds, -8, 0);
            TextRenderer.DrawText(
                e.Graphics,
                e.Header?.Text ?? string.Empty,
                filesList.Font,
                textBounds,
                palette.Foreground,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private static void FilesList_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private static void FilesList_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void FilesList_AfterLabelEdit(object? sender, LabelEditEventArgs e)
        {
            if (e.Label == null || e.Item < 0 || e.Item >= filesList.Items.Count)
            {
                return;
            }

            e.CancelEdit = true;
            string newName = e.Label.Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            if (filesList.Items[e.Item].Tag is not FileQueueItem model)
            {
                return;
            }

            RenameQueueItem(model, newName);
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                SelectAllMenuItem_Click(sender, EventArgs.Empty);
                e.SuppressKeyPress = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedPathsToClipboard(false);
                e.SuppressKeyPress = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.X)
            {
                CopySelectedPathsToClipboard(true);
                e.SuppressKeyPress = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.V)
            {
                PasteMenuItem_Click(sender, EventArgs.Empty);
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedItems();
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.F2)
            {
                BeginRenameSelectedItem();
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Escape && activeConversion != null)
            {
                activeConversion.Cancel();
                ConverterCore.Cancel();
                e.SuppressKeyPress = true;
            }
        }

        private void CategoryButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button button || button.Tag is not string tag)
            {
                return;
            }

            changingTagFilters = true;
            try
            {
                foreach ((string key, CheckBox checkBox) in tagCheckBoxes)
                {
                    checkBox.Checked = string.Equals(key, tag, StringComparison.OrdinalIgnoreCase);
                }
            }
            finally
            {
                changingTagFilters = false;
            }

            RefreshFileList();
            RefreshOutputFormatOptions();
            statusTextLabel.Text = "Filter selected: " + tag;
        }

        private void TagCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (!changingTagFilters)
            {
                RefreshFileList();
                RefreshOutputFormatOptions();
                UpdateStatusText();
            }
        }

        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            RefreshFileList();
            statusTextLabel.Text = string.IsNullOrWhiteSpace(searchTextBox.Text)
                ? "Ready"
                : "Search: " + searchTextBox.Text.Trim();
        }

        private void FilesList_DragEnter(object? sender, DragEventArgs e)
        {
            e.Effect = e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void FilesList_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] paths)
            {
                AddPathsToQueue(paths);
            }
        }

        private void FilesList_DoubleClick(object? sender, EventArgs e)
        {
            FileQueueItem? item = GetSelectedModels().FirstOrDefault();
            if (item != null)
            {
                ShellFileOperations.OpenFolderAndSelect(item.OutputPath ?? item.InputPath);
            }
        }

        private void DetailsAddTagButton_Click(object? sender, EventArgs e)
        {
            FileQueueItem? item = GetSelectedModels().FirstOrDefault();
            if (item == null)
            {
                return;
            }

            if (item.Tags.Contains("Favorites"))
            {
                item.Tags.Remove("Favorites");
            }
            else
            {
                item.Tags.Add("Favorites");
            }

            RefreshFileList();
            SelectModel(item);
            UpdateDetailsPanel();
        }

        private void AddPathsToQueue(IEnumerable<string> paths)
        {
            int added = 0;
            foreach (string candidate in paths)
            {
                if (Directory.Exists(candidate))
                {
                    foreach (string file in EnumerateFilesSafe(candidate))
                    {
                        if (AddFileToQueue(file))
                        {
                            added++;
                        }
                    }
                }
                else if (AddFileToQueue(candidate))
                {
                    added++;
                }
            }

            RefreshFileList();
            RefreshOutputFormatOptions();
            UpdateStatusText();
            statusTextLabel.Text = added == 1 ? "Added 1 file." : "Added " + added + " files.";
        }

        private bool AddFileToQueue(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            string fullPath = Path.GetFullPath(filePath);
            if (queuedFiles.Any(item => string.Equals(item.InputPath, fullPath, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            FileInfo info = new FileInfo(fullPath);
            FileTypeInfo typeInfo = GetFileTypeInfo(info.Extension);
            FileQueueItem item = new FileQueueItem
            {
                InputPath = fullPath,
                DisplayName = info.Name,
                TypeName = typeInfo.TypeName,
                Category = typeInfo.Category,
                ImageKey = typeInfo.ImageKey,
                SizeBytes = info.Length,
                SizeText = FormatSize(info.Length),
                IsChecked = typeInfo.IsSupported,
                Status = typeInfo.IsSupported ? "Ready" : "Unsupported"
            };
            item.Tags.Add(typeInfo.Category);
            if (!typeInfo.IsSupported)
            {
                item.Tags.Add("Failed");
                item.ErrorMessage = "This input format is not supported yet.";
            }

            queuedFiles.Add(item);
            return true;
        }

        private IEnumerable<string> EnumerateFilesSafe(string folder)
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(folder);

            while (pending.Count > 0)
            {
                string current = pending.Pop();
                IEnumerable<string> files = Array.Empty<string>();
                IEnumerable<string> directories = Array.Empty<string>();

                try
                {
                    files = Directory.EnumerateFiles(current);
                    directories = Directory.EnumerateDirectories(current);
                }
                catch
                {
                }

                foreach (string file in files)
                {
                    yield return file;
                }

                foreach (string directory in directories)
                {
                    pending.Push(directory);
                }
            }
        }

        private void RefreshFileList()
        {
            string[] selectedPaths = GetSelectedModels().Select(item => item.InputPath).ToArray();
            refreshingList = true;
            filesList.BeginUpdate();
            try
            {
                filesList.Items.Clear();
                foreach (FileQueueItem model in queuedFiles.Where(FilterMatches))
                {
                    ListViewItem item = CreateListViewItem(model);
                    filesList.Items.Add(item);
                    if (selectedPaths.Contains(model.InputPath, StringComparer.OrdinalIgnoreCase))
                    {
                        item.Selected = true;
                        item.Focused = true;
                    }
                }
            }
            finally
            {
                filesList.EndUpdate();
                refreshingList = false;
            }
        }

        private ListViewItem CreateListViewItem(FileQueueItem model)
        {
            ListViewItem item = new ListViewItem(model.DisplayName)
            {
                Tag = model,
                Checked = model.IsChecked,
                ImageKey = model.ImageKey,
                ToolTipText = model.ErrorMessage ?? model.InputPath
            };
            item.SubItems.Add(model.TypeName);
            item.SubItems.Add(model.SizeText);
            item.SubItems.Add(model.Status);
            item.SubItems.Add(string.Join(", ", model.Tags.OrderBy(tag => tag)));
            item.SubItems.Add(model.InputPath);
            ApplyItemVisualState(item, model);
            return item;
        }

        private void UpdateVisibleRow(FileQueueItem model)
        {
            foreach (ListViewItem item in filesList.Items)
            {
                if (!ReferenceEquals(item.Tag, model))
                {
                    continue;
                }

                item.Text = model.DisplayName;
                item.Checked = model.IsChecked;
                item.ImageKey = model.ImageKey;
                item.SubItems[1].Text = model.TypeName;
                item.SubItems[2].Text = model.SizeText;
                item.SubItems[3].Text = model.Status;
                item.SubItems[4].Text = string.Join(", ", model.Tags.OrderBy(tag => tag));
                item.SubItems[5].Text = model.InputPath;
                item.ToolTipText = model.ErrorMessage ?? model.InputPath;
                ApplyItemVisualState(item, model);
                break;
            }

            UpdateDetailsPanel();
        }

        private void ApplyItemVisualState(ListViewItem item, FileQueueItem model)
        {
            ThemePalette palette = ThemeManager.GetPalette(conversionSettings.Theme);
            switch (model.Status)
            {
                case "Converted":
                case "Added to PDF":
                    item.BackColor = palette.SuccessBack;
                    item.ForeColor = palette.SuccessFore;
                    break;
                case "Failed":
                case "Missing":
                case "Unsupported":
                    item.BackColor = palette.ErrorBack;
                    item.ForeColor = palette.ErrorFore;
                    break;
                case "Converting":
                case "Adding to PDF":
                    item.BackColor = palette.ProgressBack;
                    item.ForeColor = palette.ProgressFore;
                    break;
                case "Cancelled":
                    item.BackColor = palette.CancelledBack;
                    item.ForeColor = palette.CancelledFore;
                    break;
                default:
                    item.BackColor = palette.ListBack;
                    item.ForeColor = palette.Foreground;
                    break;
            }
        }

        private bool FilterMatches(FileQueueItem item)
        {
            string query = searchTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(query) &&
                !item.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) &&
                !item.InputPath.Contains(query, StringComparison.OrdinalIgnoreCase) &&
                !item.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            HashSet<string> activeTags = tagCheckBoxes
                .Where(pair => pair.Value.Checked)
                .Select(pair => pair.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (activeTags.Count == 0)
            {
                return true;
            }

            return item.Category == "Unsupported" || activeTags.Contains(item.Category) || item.Tags.Any(activeTags.Contains);
        }

        private void RemoveSelectedItems()
        {
            List<FileQueueItem> selected = GetSelectedModels().ToList();
            if (selected.Count == 0)
            {
                return;
            }

            if (conversionSettings.ConfirmRemoveFromQueue &&
                MessageBox.Show(this, "Remove selected file(s) from the queue?", "Remove from queue", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            foreach (FileQueueItem item in selected)
            {
                queuedFiles.Remove(item);
            }

            RefreshFileList();
            RefreshOutputFormatOptions();
            UpdateDetailsPanel();
            UpdateStatusText();
            statusTextLabel.Text = selected.Count == 1 ? "Removed 1 file from queue." : "Removed " + selected.Count + " files from queue.";
        }

        private void CopySelectedPathsToClipboard(bool cut)
        {
            List<FileQueueItem> selected = GetSelectedModels().ToList();
            if (selected.Count == 0)
            {
                return;
            }

            StringCollection paths = new StringCollection();
            foreach (FileQueueItem item in selected)
            {
                if (File.Exists(item.InputPath))
                {
                    paths.Add(item.InputPath);
                }
            }

            if (paths.Count == 0)
            {
                return;
            }

            DataObject dataObject = new DataObject();
            dataObject.SetFileDropList(paths);
            MemoryStream dropEffect = new MemoryStream(BitConverter.GetBytes(cut ? 2 : 5));
            dataObject.SetData("Preferred DropEffect", dropEffect);
            Clipboard.SetDataObject(dataObject, true);
            statusTextLabel.Text = cut ? "Selected files copied to clipboard for cut." : "Selected files copied to clipboard.";
        }

        private void BeginRenameSelectedItem()
        {
            if (filesList.SelectedItems.Count == 1)
            {
                filesList.SelectedItems[0].BeginEdit();
            }
        }

        private void RenameQueueItem(FileQueueItem model, string newName)
        {
            string? directory = Path.GetDirectoryName(model.InputPath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return;
            }

            if (Path.GetExtension(newName).Length == 0 && Path.GetExtension(model.InputPath).Length > 0)
            {
                newName += Path.GetExtension(model.InputPath);
            }

            string newPath = Path.Combine(directory, newName);
            if (string.Equals(model.InputPath, newPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                if (File.Exists(model.InputPath))
                {
                    if (File.Exists(newPath))
                    {
                        MessageBox.Show(this, "A file with this name already exists.", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    File.Move(model.InputPath, newPath);
                }

                FileInfo info = new FileInfo(newPath);
                FileTypeInfo typeInfo = GetFileTypeInfo(info.Extension);
                model.InputPath = newPath;
                model.DisplayName = info.Name;
                model.TypeName = typeInfo.TypeName;
                model.Category = typeInfo.Category;
                model.ImageKey = typeInfo.ImageKey;
                model.SizeBytes = info.Exists ? info.Length : model.SizeBytes;
                model.SizeText = info.Exists ? FormatSize(info.Length) : model.SizeText;
                model.Tags.Clear();
                model.Tags.Add(typeInfo.Category);
                model.Status = typeInfo.IsSupported ? "Ready" : "Unsupported";
                model.OutputFormatOverride = null;
                model.OutputPath = null;
                model.ErrorMessage = typeInfo.IsSupported ? null : "This input format is not supported yet.";
                RefreshFileList();
                SelectModel(model);
                UpdateDetailsPanel();
                UpdateStatusText();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Rename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateDetailsPanel()
        {
            FileQueueItem? selected = GetSelectedModels().FirstOrDefault();
            if (selected == null)
            {
                detailsTitleLabel.Text = "No file selected";
                detailsTypeValueLabel.Text = "-";
                detailsSizeValueLabel.Text = "-";
                detailsDurationValueLabel.Text = "-";
                detailsBitrateValueLabel.Text = "-";
                detailsPathValueLabel.Text = "-";
                detailsTagChipLabel.Text = "No tag";
                ThemePalette palette = ThemeManager.GetPalette(conversionSettings.Theme);
                detailsTagChipLabel.BackColor = palette.SurfaceAlt;
                detailsTagChipLabel.ForeColor = palette.MutedForeground;
                SetDetailsPreview(null);
                return;
            }

            string primaryTag = selected.Tags.FirstOrDefault() ?? selected.Category;
            detailsTitleLabel.Text = selected.DisplayName;
            detailsTypeValueLabel.Text = selected.TypeName;
            detailsSizeValueLabel.Text = selected.SizeText;
            detailsDurationValueLabel.Text = selected.ErrorMessage == null ? selected.Status : selected.Status + ": " + selected.ErrorMessage;
            detailsBitrateValueLabel.Text = selected.OutputPath ?? GetOutputFormatDisplayForItem(selected);
            detailsPathValueLabel.Text = selected.InputPath;
            detailsTagChipLabel.Text = primaryTag;
            detailsTagChipLabel.BackColor = GetTagBackColor(primaryTag);
            detailsTagChipLabel.ForeColor = GetTagForeColor(primaryTag);
            SetDetailsPreview(selected);
        }



        private string GetOutputFormatDisplayForItem(FileQueueItem item)
        {
            ConverterFormat? configured = item.OutputFormatOverride ?? conversionSettings.GetDefaultFormatForCategory(item.Category);
            if (configured.HasValue)
            {
                return OutputFormatChoice.GetDisplayName(configured.Value);
            }

            if (ConverterCore.TryGetFormatFromExtension(Path.GetExtension(item.InputPath), out ConverterFormat detected) &&
                detected != ConverterFormat.Docx && detected != ConverterFormat.Pptx && detected != ConverterFormat.Xlsx)
            {
                return "Same as source (" + OutputFormatChoice.GetDisplayName(detected) + ")";
            }

            ConverterFormat? fallback = conversionSettings.GetFallbackFormatForCategory(item.Category);
            return fallback.HasValue ? OutputFormatChoice.GetDisplayName(fallback.Value) : "Same as source";
        }

        private void SetDetailsPreview(FileQueueItem? item)
        {
            ThemePalette palette = ThemeManager.GetPalette(conversionSettings.Theme);
            if (item != null && PreviewImageLoader.TryLoadPreview(item.InputPath, item.Category, conversionSettings.PreviewEmbeddedArtwork, detailsPreviewPanel.ClientSize, out Image preview))
            {
                SetPreviewImage(preview, PictureBoxSizeMode.Zoom);
                detailsPreviewPanel.BackColor = palette.PreviewBack;
                detailsPreviewPanel.BorderColor = palette.PreviewBorder;
                return;
            }

            UiIconKind iconKind = item == null ? UiIconKind.Document : GetIconKindFromCategory(item.Category);
            SetPreviewImage(UiIconFactory.CreateIcon(iconKind, palette.PreviewIcon, 86), PictureBoxSizeMode.CenterImage);
            detailsPreviewPanel.BackColor = palette.PreviewBack;
            detailsPreviewPanel.BorderColor = palette.PreviewBorder;
        }

        private void SetPreviewImage(Image image, PictureBoxSizeMode sizeMode)
        {
            Image? oldImage = currentPreviewImage;
            currentPreviewImage = image;
            detailsPreviewIcon.Image = image;
            detailsPreviewIcon.SizeMode = sizeMode;
            detailsPreviewIcon.Dock = DockStyle.Fill;

            if (oldImage != null && !ReferenceEquals(oldImage, image))
            {
                oldImage.Dispose();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            SaveCurrentSettings();
            currentPreviewImage?.Dispose();
            base.OnFormClosed(e);
        }

        private void UpdateStatusText()
        {
            filesCountStatusLabel.Text = queuedFiles.Count + " file(s)";
            selectedCountStatusLabel.Text = filesList.SelectedItems.Count + " selected";
            checkedCountStatusLabel.Text = queuedFiles.Count(item => item.IsChecked) + " queued";
            if (activeConversion == null)
            {
                progressTextLabel.Text = "Ready to convert " + queuedFiles.Count(item => item.IsChecked) + " file(s)";
            }
        }

        private void SetConversionUiState(bool converting)
        {
            sidebarAddFilesButton.Enabled = !converting;
            sidebarAddFolderButton.Enabled = !converting;
            sidebarRemoveButton.Enabled = !converting;
            sidebarClearAllButton.Enabled = !converting;
            fileAddFilesMenuItem.Enabled = !converting;
            fileAddFolderMenuItem.Enabled = !converting;
            fileRemoveSelectedMenuItem.Enabled = !converting;
            fileClearListMenuItem.Enabled = !converting;
            outputFormatComboBox.Enabled = !converting;
            outputDirectoryTextBox.Enabled = !converting;
            browseOutputButton.Enabled = !converting;
            advancedButton.Enabled = !converting;
            mergeImagesPdfButton.Enabled = !converting;
            filesList.Enabled = !converting;

            sidebarConvertButton.Text = converting ? "   Cancel" : "   Convert";
            convertNowButton.Text = converting ? "  Cancel" : "  Convert";
            progressTextLabel.Text = converting ? "Conversion in progress..." : "Ready to convert " + queuedFiles.Count(item => item.IsChecked) + " file(s)";
            outputFormatComboBox.Enabled = !converting && outputFormatComboBox.Items.Count > 1;
        }

        private List<FileQueueItem> GetSelectedModels()
        {
            return filesList.SelectedItems
                .Cast<ListViewItem>()
                .Select(item => item.Tag)
                .OfType<FileQueueItem>()
                .ToList();
        }

        private IEnumerable<FileQueueItem> GetImageItemsForPdf()
        {
            List<FileQueueItem> selectedImages = GetSelectedModels()
                .Where(item => item.Category == "Images" && File.Exists(item.InputPath))
                .ToList();

            if (selectedImages.Count >= 2)
            {
                return selectedImages;
            }

            return queuedFiles.Where(item => item.IsChecked && item.Category == "Images" && File.Exists(item.InputPath));
        }

        private static string BuildMergedPdfFileName()
        {
            return "FormatForge_images_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
        }

        private void SelectModel(FileQueueItem model)
        {
            foreach (ListViewItem item in filesList.Items)
            {
                bool selected = ReferenceEquals(item.Tag, model);
                item.Selected = selected;
                if (selected)
                {
                    item.Focused = true;
                    item.EnsureVisible();
                }
            }
        }

        private bool TryResolveOutputFormat(FileQueueItem item, out ConverterFormat format, out string? error)
        {
            ConverterFormat? configured = item.OutputFormatOverride ?? conversionSettings.GetDefaultFormatForCategory(item.Category);
            if (configured.HasValue)
            {
                format = configured.Value;
                error = null;
                return true;
            }

            if (ConverterCore.TryGetFormatFromExtension(Path.GetExtension(item.InputPath), out format) &&
                format != ConverterFormat.Docx && format != ConverterFormat.Pptx && format != ConverterFormat.Xlsx)
            {
                error = null;
                return true;
            }

            ConverterFormat? fallback = conversionSettings.GetFallbackFormatForCategory(item.Category);
            if (fallback.HasValue)
            {
                format = fallback.Value;
                error = null;
                return true;
            }

            format = default;
            error = "No compatible output format was found for this file.";
            return false;
        }

        private static bool TryGetFormatFromDisplayName(string displayName, out ConverterFormat format)
        {
            switch (displayName.Trim().ToUpperInvariant())
            {
                case "JPG":
                case "JPEG": format = ConverterFormat.Jpeg; return true;
                case "PNG": format = ConverterFormat.Png; return true;
                case "BMP": format = ConverterFormat.Bmp; return true;
                case "TIFF": format = ConverterFormat.Tiff; return true;
                case "WEBP": format = ConverterFormat.Webp; return true;
                case "GIF": format = ConverterFormat.Gif; return true;
                case "MP3": format = ConverterFormat.Mp3; return true;
                case "WAV": format = ConverterFormat.Wav; return true;
                case "FLAC": format = ConverterFormat.Flac; return true;
                case "OGG": format = ConverterFormat.Ogg; return true;
                case "OPUS": format = ConverterFormat.Opus; return true;
                case "M4A": format = ConverterFormat.M4a; return true;
                case "AAC": format = ConverterFormat.Aac; return true;
                case "MP4": format = ConverterFormat.Mp4; return true;
                case "AVI": format = ConverterFormat.Avi; return true;
                case "MKV": format = ConverterFormat.Mkv; return true;
                case "MOV": format = ConverterFormat.Mov; return true;
                case "WEBM": format = ConverterFormat.Webm; return true;
                case "WMV": format = ConverterFormat.Wmv; return true;
                case "PDF": format = ConverterFormat.Pdf; return true;
                default: format = default; return false;
            }
        }

        private static bool IsOutputCompatible(FileQueueItem item, ConverterFormat format)
        {
            return OutputFormatChoice.IsCompatible(item.Category, format);
        }

        private string BuildOutputPath(FileQueueItem item, ConverterFormat format, string outputDirectory)
        {
            string extension = ConverterCore.GetExtension(format);
            string baseName = Path.GetFileNameWithoutExtension(item.DisplayName);
            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = Path.GetFileNameWithoutExtension(item.InputPath);
            }

            string candidate = Path.Combine(outputDirectory, baseName + extension);
            if (string.Equals(Path.GetFullPath(candidate), Path.GetFullPath(item.InputPath), StringComparison.OrdinalIgnoreCase))
            {
                candidate = Path.Combine(outputDirectory, baseName + "_converted" + extension);
            }

            return overwriteExistingFilesCheckBox.Checked ? candidate : GetUniquePath(candidate);
        }

        private static string GetUniquePath(string path)
        {
            if (!File.Exists(path))
            {
                return path;
            }

            string? directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            for (int i = 1; i < 10000; i++)
            {
                string candidate = Path.Combine(directory ?? string.Empty, fileName + " (" + i + ")" + extension);
                if (!File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return Path.Combine(directory ?? string.Empty, fileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + extension);
        }

        private void ApplyOutputTimestamp(FileQueueItem item)
        {
            if (!keepOriginalDateCheckBox.Checked || string.IsNullOrWhiteSpace(item.OutputPath))
            {
                return;
            }

            try
            {
                if (File.Exists(item.InputPath) && File.Exists(item.OutputPath))
                {
                    File.SetCreationTime(item.OutputPath, File.GetCreationTime(item.InputPath));
                    File.SetLastWriteTime(item.OutputPath, File.GetLastWriteTime(item.InputPath));
                }
            }
            catch
            {
            }
        }

        private static string FormatSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double value = bytes;
            int unit = 0;

            while (value >= 1024 && unit < units.Length - 1)
            {
                value /= 1024;
                unit++;
            }

            return value.ToString(unit == 0 ? "0" : "0.##") + " " + units[unit];
        }

        private static FileTypeInfo GetFileTypeInfo(string extension)
        {
            string ext = extension.TrimStart('.').ToLowerInvariant();
            return ext switch
            {
                "jpg" or "jpeg" or "png" or "webp" or "bmp" or "gif" or "tiff" or "tif" or "ico" => new FileTypeInfo(ext.ToUpperInvariant(), "Images", "image", true),
                "mp3" or "wav" or "flac" or "aac" or "ogg" or "opus" or "m4a" => new FileTypeInfo(ext.ToUpperInvariant(), "Audio", "audio", true),
                "mp4" or "mkv" or "avi" or "mov" or "webm" or "wmv" or "flv" or "m4v" or "3gp" => new FileTypeInfo(ext.ToUpperInvariant(), "Video", "video", true),
                "pdf" => new FileTypeInfo("PDF", "PDF", "pdf", true),
                "doc" or "docx" or "txt" or "rtf" or "ppt" or "pptx" or "xls" or "xlsx" or "html" or "htm" or "csv" or "tsv" or "json" or "xml" or "odt" or "ods" or "odp" => new FileTypeInfo(ext.ToUpperInvariant(), "Documents", "document", true),
                _ => new FileTypeInfo(string.IsNullOrWhiteSpace(ext) ? "FILE" : ext.ToUpperInvariant(), "Unsupported", "file", false)
            };
        }

        private static UiIconKind GetIconKindFromCategory(string category)
        {
            return category switch
            {
                "Audio" => UiIconKind.Audio,
                "Video" => UiIconKind.Video,
                "Images" => UiIconKind.Image,
                "PDF" => UiIconKind.Pdf,
                "Documents" => UiIconKind.Document,
                _ => UiIconKind.Document
            };
        }

        private Color GetTagBackColor(string tag)
        {
            if (conversionSettings.Theme == AppThemeMode.Dark)
            {
                return tag switch
                {
                    "Audio" => Color.FromArgb(76, 37, 52),
                    "Video" => Color.FromArgb(59, 43, 91),
                    "Images" => Color.FromArgb(31, 59, 88),
                    "Documents" => Color.FromArgb(31, 70, 47),
                    "PDF" => Color.FromArgb(82, 39, 45),
                    "Converted" => Color.FromArgb(32, 74, 49),
                    "Failed" => Color.FromArgb(85, 49, 31),
                    "Favorites" => Color.FromArgb(82, 65, 30),
                    "Unsupported" => Color.FromArgb(47, 55, 68),
                    _ => Color.FromArgb(47, 55, 68)
                };
            }

            return tag switch
            {
                "Audio" => Color.FromArgb(255, 219, 230),
                "Video" => Color.FromArgb(231, 216, 255),
                "Images" => Color.FromArgb(215, 232, 255),
                "Documents" => Color.FromArgb(213, 244, 221),
                "PDF" => Color.FromArgb(255, 221, 224),
                "Converted" => Color.FromArgb(217, 246, 223),
                "Failed" => Color.FromArgb(255, 230, 211),
                "Favorites" => Color.FromArgb(255, 241, 196),
                "Unsupported" => Color.FromArgb(238, 241, 245),
                _ => Color.FromArgb(238, 241, 245)
            };
        }

        private Color GetTagForeColor(string tag)
        {
            if (conversionSettings.Theme == AppThemeMode.Dark)
            {
                return tag switch
                {
                    "Audio" => Color.FromArgb(255, 153, 184),
                    "Video" => Color.FromArgb(201, 172, 255),
                    "Images" => Color.FromArgb(146, 201, 255),
                    "Documents" => Color.FromArgb(145, 230, 165),
                    "PDF" => Color.FromArgb(255, 147, 157),
                    "Converted" => Color.FromArgb(139, 232, 166),
                    "Failed" => Color.FromArgb(255, 175, 116),
                    "Favorites" => Color.FromArgb(255, 217, 105),
                    "Unsupported" => Color.FromArgb(179, 188, 202),
                    _ => Color.FromArgb(179, 188, 202)
                };
            }

            return tag switch
            {
                "Audio" => Color.FromArgb(176, 40, 78),
                "Video" => Color.FromArgb(95, 45, 198),
                "Images" => Color.FromArgb(12, 96, 202),
                "Documents" => Color.FromArgb(31, 123, 53),
                "PDF" => Color.FromArgb(202, 44, 54),
                "Converted" => Color.FromArgb(28, 128, 49),
                "Failed" => Color.FromArgb(188, 74, 16),
                "Favorites" => Color.FromArgb(148, 102, 11),
                "Unsupported" => Color.FromArgb(83, 91, 105),
                _ => Color.FromArgb(83, 91, 105)
            };
        }

        private sealed class FileQueueItem
        {
            public string InputPath { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string TypeName { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string ImageKey { get; set; } = "file";
            public long SizeBytes { get; set; }
            public string SizeText { get; set; } = string.Empty;
            public string Status { get; set; } = "Ready";
            public bool IsChecked { get; set; } = true;
            public ConverterFormat? OutputFormatOverride { get; set; }
            public string? OutputPath { get; set; }
            public string? ErrorMessage { get; set; }
            public HashSet<string> Tags { get; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private readonly record struct FileTypeInfo(string TypeName, string Category, string ImageKey, bool IsSupported);
    }
}
