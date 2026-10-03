namespace FormatForge.App
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            mainMenu = new MenuStrip();
            fileMenu = new ToolStripMenuItem();
            fileAddFilesMenuItem = new ToolStripMenuItem();
            fileAddFolderMenuItem = new ToolStripMenuItem();
            fileRemoveSelectedMenuItem = new ToolStripMenuItem();
            fileClearListMenuItem = new ToolStripMenuItem();
            fileExitMenuItem = new ToolStripMenuItem();
            editMenu = new ToolStripMenuItem();
            editCutMenuItem = new ToolStripMenuItem();
            editCopyMenuItem = new ToolStripMenuItem();
            editPasteMenuItem = new ToolStripMenuItem();
            editRenameMenuItem = new ToolStripMenuItem();
            editDeleteMenuItem = new ToolStripMenuItem();
            editSelectAllMenuItem = new ToolStripMenuItem();
            editPropertiesMenuItem = new ToolStripMenuItem();
            viewMenu = new ToolStripMenuItem();
            viewDetailsMenuItem = new ToolStripMenuItem();
            viewLargeIconsMenuItem = new ToolStripMenuItem();
            viewTagsPanelMenuItem = new ToolStripMenuItem();
            viewRefreshMenuItem = new ToolStripMenuItem();
            helpMenu = new ToolStripMenuItem();
            helpUpdatePythonMenuItem = new ToolStripMenuItem();
            helpDocumentationMenuItem = new ToolStripMenuItem();
            helpAboutMenuItem = new ToolStripMenuItem();
            shellPanel = new Panel();
            mainSplit = new SplitContainer();
            sidebarPanel = new RoundedPanel();
            sidebarFlow = new FlowLayoutPanel();
            sidebarConvertButton = new Button();
            sidebarAddFilesButton = new Button();
            sidebarAddFolderButton = new Button();
            sidebarRemoveButton = new Button();
            sidebarClearAllButton = new Button();
            sidebarSeparatorTop = new Panel();
            tagsHeaderLabel = new Label();
            tagsFlow = new FlowLayoutPanel();
            tagImagesCheckBox = new CheckBox();
            tagAudioCheckBox = new CheckBox();
            tagVideoCheckBox = new CheckBox();
            tagDocumentsCheckBox = new CheckBox();
            tagPdfCheckBox = new CheckBox();
            tagConvertedCheckBox = new CheckBox();
            tagFailedCheckBox = new CheckBox();
            tagFavoritesCheckBox = new CheckBox();
            clearTagsButton = new Button();
            sidebarSeparatorBottom = new Panel();
            sidebarSettingsButton = new Button();
            sidebarAboutButton = new Button();
            workspacePanel = new Panel();
            contentSplit = new SplitContainer();
            filesPanel = new RoundedPanel();
            filesList = new ListView();
            fileNameColumn = new ColumnHeader();
            fileTypeColumn = new ColumnHeader();
            fileSizeColumn = new ColumnHeader();
            fileStatusColumn = new ColumnHeader();
            fileTagsColumn = new ColumnHeader();
            filePathColumn = new ColumnHeader();
            fileContextMenu = new ContextMenuStrip(components);
            contextCutMenuItem = new ToolStripMenuItem();
            contextCopyMenuItem = new ToolStripMenuItem();
            contextPasteMenuItem = new ToolStripMenuItem();
            contextRenameMenuItem = new ToolStripMenuItem();
            contextDeleteMenuItem = new ToolStripMenuItem();
            contextPropertiesMenuItem = new ToolStripMenuItem();
            fileTypeLargeImages = new ImageList(components);
            fileTypeImages = new ImageList(components);
            listHeaderPanel = new Panel();
            searchTextBox = new TextBox();
            filesTitleLabel = new Label();
            detailsPanel = new RoundedPanel();
            detailsTagChipLabel = new Label();
            detailsAddTagButton = new Button();
            detailsTagsLabel = new Label();
            detailsDividerPanel = new Panel();
            detailsGridPanel = new TableLayoutPanel();
            detailsTypeLabel = new Label();
            detailsTypeValueLabel = new Label();
            detailsSizeLabel = new Label();
            detailsSizeValueLabel = new Label();
            detailsDurationLabel = new Label();
            detailsDurationValueLabel = new Label();
            detailsBitrateLabel = new Label();
            detailsBitrateValueLabel = new Label();
            detailsPathLabel = new Label();
            detailsPathValueLabel = new Label();
            detailsTitleLabel = new Label();
            detailsPreviewPanel = new RoundedPanel();
            detailsPreviewIcon = new PictureBox();
            optionsPanel = new RoundedPanel();
            optionsLayout = new TableLayoutPanel();
            outputPanel = new Panel();
            openOutputFolderCheckBox = new CheckBox();
            browseOutputButton = new Button();
            outputDirectoryTextBox = new TextBox();
            outputDirectoryLabel = new Label();
            conversionOptionsPanel = new Panel();
            overwriteExistingFilesCheckBox = new CheckBox();
            keepOriginalDateCheckBox = new CheckBox();
            preserveMetadataCheckBox = new CheckBox();
            optionsLabel = new Label();
            progressPanel = new Panel();
            progressTextLabel = new Label();
            progressBar = new ThemedProgressBar();
            progressTitleLabel = new Label();
            conversionBarPanel = new RoundedPanel();
            categoryFlow = new FlowLayoutPanel();
            categoryImagesButton = new Button();
            categoryAudioButton = new Button();
            categoryVideoButton = new Button();
            categoryDocumentsButton = new Button();
            outputFormatPanel = new Panel();
            outputFormatComboBox = new ComboBox();
            outputFormatLabel = new Label();
            advancedButton = new Button();
            mergeImagesPdfButton = new Button();
            convertNowButton = new Button();
            headerPanel = new Panel();
            appSubtitle = new Label();
            appTitle = new Label();
            appIcon = new PictureBox();
            statusStrip = new StatusStrip();
            filesCountStatusLabel = new ToolStripStatusLabel();
            statusSeparatorLabel1 = new ToolStripStatusLabel();
            selectedCountStatusLabel = new ToolStripStatusLabel();
            statusSeparatorLabel2 = new ToolStripStatusLabel();
            checkedCountStatusLabel = new ToolStripStatusLabel();
            statusTextLabel = new ToolStripStatusLabel();
            versionStatusLabel = new ToolStripStatusLabel();
            openFileDialog = new OpenFileDialog();
            folderBrowserDialog = new FolderBrowserDialog();
            mainMenu.SuspendLayout();
            shellPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainSplit).BeginInit();
            mainSplit.Panel1.SuspendLayout();
            mainSplit.Panel2.SuspendLayout();
            mainSplit.SuspendLayout();
            sidebarPanel.SuspendLayout();
            sidebarFlow.SuspendLayout();
            tagsFlow.SuspendLayout();
            workspacePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)contentSplit).BeginInit();
            contentSplit.Panel1.SuspendLayout();
            contentSplit.Panel2.SuspendLayout();
            contentSplit.SuspendLayout();
            filesPanel.SuspendLayout();
            fileContextMenu.SuspendLayout();
            listHeaderPanel.SuspendLayout();
            detailsPanel.SuspendLayout();
            detailsGridPanel.SuspendLayout();
            detailsPreviewPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)detailsPreviewIcon).BeginInit();
            optionsPanel.SuspendLayout();
            optionsLayout.SuspendLayout();
            outputPanel.SuspendLayout();
            conversionOptionsPanel.SuspendLayout();
            progressPanel.SuspendLayout();
            conversionBarPanel.SuspendLayout();
            categoryFlow.SuspendLayout();
            outputFormatPanel.SuspendLayout();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)appIcon).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // mainMenu
            // 
            mainMenu.BackColor = Color.FromArgb(250, 252, 255);
            mainMenu.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, viewMenu, helpMenu });
            mainMenu.Location = new Point(0, 0);
            mainMenu.Name = "mainMenu";
            mainMenu.Padding = new Padding(8, 2, 0, 2);
            mainMenu.Size = new Size(1320, 24);
            mainMenu.TabIndex = 0;
            mainMenu.Text = "mainMenu";
            // 
            // fileMenu
            // 
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { fileAddFilesMenuItem, fileAddFolderMenuItem, fileRemoveSelectedMenuItem, fileClearListMenuItem, fileExitMenuItem });
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new Size(37, 20);
            fileMenu.Text = "&File";
            // 
            // fileAddFilesMenuItem
            // 
            fileAddFilesMenuItem.Image = (Image)resources.GetObject("fileAddFilesMenuItem.Image");
            fileAddFilesMenuItem.Name = "fileAddFilesMenuItem";
            fileAddFilesMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            fileAddFilesMenuItem.Size = new Size(214, 22);
            fileAddFilesMenuItem.Text = "&Add files...";
            fileAddFilesMenuItem.Click += AddFilesButton_Click;
            // 
            // fileAddFolderMenuItem
            // 
            fileAddFolderMenuItem.Image = (Image)resources.GetObject("fileAddFolderMenuItem.Image");
            fileAddFolderMenuItem.Name = "fileAddFolderMenuItem";
            fileAddFolderMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.O;
            fileAddFolderMenuItem.Size = new Size(214, 22);
            fileAddFolderMenuItem.Text = "Add &folder...";
            fileAddFolderMenuItem.Click += AddFolderButton_Click;
            // 
            // fileRemoveSelectedMenuItem
            // 
            fileRemoveSelectedMenuItem.Name = "fileRemoveSelectedMenuItem";
            fileRemoveSelectedMenuItem.Size = new Size(214, 22);
            fileRemoveSelectedMenuItem.Text = "&Remove selected";
            fileRemoveSelectedMenuItem.Click += RemoveSelectedButton_Click;
            // 
            // fileClearListMenuItem
            // 
            fileClearListMenuItem.Name = "fileClearListMenuItem";
            fileClearListMenuItem.Size = new Size(214, 22);
            fileClearListMenuItem.Text = "Clear &list";
            fileClearListMenuItem.Click += ClearAllButton_Click;
            // 
            // fileExitMenuItem
            // 
            fileExitMenuItem.Name = "fileExitMenuItem";
            fileExitMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
            fileExitMenuItem.Size = new Size(214, 22);
            fileExitMenuItem.Text = "E&xit";
            fileExitMenuItem.Click += FileExitMenuItem_Click;
            // 
            // editMenu
            // 
            editMenu.DropDownItems.AddRange(new ToolStripItem[] { editCutMenuItem, editCopyMenuItem, editPasteMenuItem, editRenameMenuItem, editDeleteMenuItem, editSelectAllMenuItem, editPropertiesMenuItem });
            editMenu.Name = "editMenu";
            editMenu.Size = new Size(39, 20);
            editMenu.Text = "&Edit";
            // 
            // editCutMenuItem
            // 
            editCutMenuItem.Name = "editCutMenuItem";
            editCutMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            editCutMenuItem.Size = new Size(184, 22);
            editCutMenuItem.Text = "Cu&t";
            editCutMenuItem.Click += CutMenuItem_Click;
            // 
            // editCopyMenuItem
            // 
            editCopyMenuItem.Name = "editCopyMenuItem";
            editCopyMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            editCopyMenuItem.Size = new Size(184, 22);
            editCopyMenuItem.Text = "&Copy";
            editCopyMenuItem.Click += CopyMenuItem_Click;
            // 
            // editPasteMenuItem
            // 
            editPasteMenuItem.Name = "editPasteMenuItem";
            editPasteMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            editPasteMenuItem.Size = new Size(184, 22);
            editPasteMenuItem.Text = "&Paste";
            editPasteMenuItem.Click += PasteMenuItem_Click;
            // 
            // editRenameMenuItem
            // 
            editRenameMenuItem.Name = "editRenameMenuItem";
            editRenameMenuItem.ShortcutKeys = Keys.F2;
            editRenameMenuItem.Size = new Size(184, 22);
            editRenameMenuItem.Text = "&Rename";
            editRenameMenuItem.Click += RenameMenuItem_Click;
            // 
            // editDeleteMenuItem
            // 
            editDeleteMenuItem.Name = "editDeleteMenuItem";
            editDeleteMenuItem.ShortcutKeys = Keys.Delete;
            editDeleteMenuItem.Size = new Size(184, 22);
            editDeleteMenuItem.Text = "&Delete";
            editDeleteMenuItem.Click += DeleteMenuItem_Click;
            // 
            // editSelectAllMenuItem
            // 
            editSelectAllMenuItem.Name = "editSelectAllMenuItem";
            editSelectAllMenuItem.ShortcutKeys = Keys.Control | Keys.A;
            editSelectAllMenuItem.Size = new Size(184, 22);
            editSelectAllMenuItem.Text = "Select &all";
            editSelectAllMenuItem.Click += SelectAllMenuItem_Click;
            // 
            // editPropertiesMenuItem
            // 
            editPropertiesMenuItem.Name = "editPropertiesMenuItem";
            editPropertiesMenuItem.ShortcutKeys = Keys.Alt | Keys.Enter;
            editPropertiesMenuItem.Size = new Size(184, 22);
            editPropertiesMenuItem.Text = "&Properties";
            editPropertiesMenuItem.Click += PropertiesMenuItem_Click;
            // 
            // viewMenu
            // 
            viewMenu.DropDownItems.AddRange(new ToolStripItem[] { viewDetailsMenuItem, viewLargeIconsMenuItem, viewTagsPanelMenuItem, viewRefreshMenuItem });
            viewMenu.Name = "viewMenu";
            viewMenu.Size = new Size(44, 20);
            viewMenu.Text = "&View";
            // 
            // viewDetailsMenuItem
            // 
            viewDetailsMenuItem.Checked = true;
            viewDetailsMenuItem.CheckState = CheckState.Checked;
            viewDetailsMenuItem.Name = "viewDetailsMenuItem";
            viewDetailsMenuItem.Size = new Size(160, 22);
            viewDetailsMenuItem.Text = "&Details";
            viewDetailsMenuItem.Click += ViewDetailsMenuItem_Click;
            // 
            // viewLargeIconsMenuItem
            // 
            viewLargeIconsMenuItem.Name = "viewLargeIconsMenuItem";
            viewLargeIconsMenuItem.Size = new Size(160, 22);
            viewLargeIconsMenuItem.Text = "&Large icons";
            viewLargeIconsMenuItem.Click += ViewLargeIconsMenuItem_Click;
            // 
            // viewTagsPanelMenuItem
            // 
            viewTagsPanelMenuItem.Checked = true;
            viewTagsPanelMenuItem.CheckOnClick = true;
            viewTagsPanelMenuItem.CheckState = CheckState.Checked;
            viewTagsPanelMenuItem.Name = "viewTagsPanelMenuItem";
            viewTagsPanelMenuItem.Size = new Size(160, 22);
            viewTagsPanelMenuItem.Text = "Show &tags panel";
            viewTagsPanelMenuItem.CheckedChanged += ViewTagsPanelMenuItem_CheckedChanged;
            // 
            // viewRefreshMenuItem
            // 
            viewRefreshMenuItem.Image = (Image)resources.GetObject("viewRefreshMenuItem.Image");
            viewRefreshMenuItem.Name = "viewRefreshMenuItem";
            viewRefreshMenuItem.ShortcutKeys = Keys.F5;
            viewRefreshMenuItem.Size = new Size(160, 22);
            viewRefreshMenuItem.Text = "&Refresh";
            viewRefreshMenuItem.Click += ViewRefreshMenuItem_Click;
            // 
            // helpMenu
            // 
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] { helpUpdatePythonMenuItem, helpDocumentationMenuItem, helpAboutMenuItem });
            helpMenu.Name = "helpMenu";
            helpMenu.Size = new Size(44, 20);
            helpMenu.Text = "&Help";
            // 
            // helpUpdatePythonMenuItem
            // 
            helpUpdatePythonMenuItem.Image = (Image)resources.GetObject("helpUpdatePythonMenuItem.Image");
            helpUpdatePythonMenuItem.Name = "helpUpdatePythonMenuItem";
            helpUpdatePythonMenuItem.Size = new Size(232, 22);
            helpUpdatePythonMenuItem.Text = "Update Python and Libraries...";
            helpUpdatePythonMenuItem.Click += HelpUpdatePythonMenuItem_Click;
            // 
            // helpDocumentationMenuItem
            // 
            helpDocumentationMenuItem.Name = "helpDocumentationMenuItem";
            helpDocumentationMenuItem.Size = new Size(232, 22);
            helpDocumentationMenuItem.Text = "&Documentation";
            helpDocumentationMenuItem.Click += HelpDocumentationMenuItem_Click;
            // 
            // helpAboutMenuItem
            // 
            helpAboutMenuItem.Name = "helpAboutMenuItem";
            helpAboutMenuItem.Size = new Size(232, 22);
            helpAboutMenuItem.Text = "&About FormatForge";
            helpAboutMenuItem.Click += HelpAboutMenuItem_Click;
            // 
            // shellPanel
            // 
            shellPanel.BackColor = Color.FromArgb(238, 244, 251);
            shellPanel.Controls.Add(mainSplit);
            shellPanel.Controls.Add(headerPanel);
            shellPanel.Dock = DockStyle.Fill;
            shellPanel.Location = new Point(0, 24);
            shellPanel.Name = "shellPanel";
            shellPanel.Padding = new Padding(12);
            shellPanel.Size = new Size(1320, 714);
            shellPanel.TabIndex = 1;
            // 
            // mainSplit
            // 
            mainSplit.Dock = DockStyle.Fill;
            mainSplit.FixedPanel = FixedPanel.Panel1;
            mainSplit.Location = new Point(12, 94);
            mainSplit.Name = "mainSplit";
            // 
            // mainSplit.Panel1
            // 
            mainSplit.Panel1.Controls.Add(sidebarPanel);
            mainSplit.Panel1MinSize = 170;
            // 
            // mainSplit.Panel2
            // 
            mainSplit.Panel2.Controls.Add(workspacePanel);
            mainSplit.Panel2MinSize = 860;
            mainSplit.Size = new Size(1296, 608);
            mainSplit.SplitterDistance = 178;
            mainSplit.SplitterWidth = 10;
            mainSplit.TabIndex = 1;
            // 
            // sidebarPanel
            // 
            sidebarPanel.BackColor = Color.FromArgb(250, 252, 255);
            sidebarPanel.BorderColor = Color.FromArgb(216, 226, 238);
            sidebarPanel.Controls.Add(sidebarFlow);
            sidebarPanel.Dock = DockStyle.Fill;
            sidebarPanel.Location = new Point(0, 0);
            sidebarPanel.Name = "sidebarPanel";
            sidebarPanel.Padding = new Padding(12);
            sidebarPanel.Size = new Size(178, 608);
            sidebarPanel.TabIndex = 0;
            // 
            // sidebarFlow
            // 
            sidebarFlow.AutoScroll = true;
            sidebarFlow.Controls.Add(sidebarConvertButton);
            sidebarFlow.Controls.Add(sidebarAddFilesButton);
            sidebarFlow.Controls.Add(sidebarAddFolderButton);
            sidebarFlow.Controls.Add(sidebarRemoveButton);
            sidebarFlow.Controls.Add(sidebarClearAllButton);
            sidebarFlow.Controls.Add(sidebarSeparatorTop);
            sidebarFlow.Controls.Add(tagsHeaderLabel);
            sidebarFlow.Controls.Add(tagsFlow);
            sidebarFlow.Controls.Add(clearTagsButton);
            sidebarFlow.Controls.Add(sidebarSeparatorBottom);
            sidebarFlow.Controls.Add(sidebarSettingsButton);
            sidebarFlow.Controls.Add(sidebarAboutButton);
            sidebarFlow.Dock = DockStyle.Fill;
            sidebarFlow.FlowDirection = FlowDirection.TopDown;
            sidebarFlow.Location = new Point(12, 12);
            sidebarFlow.Name = "sidebarFlow";
            sidebarFlow.Size = new Size(154, 584);
            sidebarFlow.TabIndex = 0;
            sidebarFlow.WrapContents = false;
            // 
            // sidebarConvertButton
            // 
            sidebarConvertButton.BackColor = Color.FromArgb(18, 112, 226);
            sidebarConvertButton.FlatAppearance.BorderSize = 0;
            sidebarConvertButton.FlatStyle = FlatStyle.Flat;
            sidebarConvertButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            sidebarConvertButton.ForeColor = Color.White;
            sidebarConvertButton.Image = (Image)resources.GetObject("sidebarConvertButton.Image");
            sidebarConvertButton.ImageAlign = ContentAlignment.MiddleLeft;
            sidebarConvertButton.Location = new Point(0, 0);
            sidebarConvertButton.Margin = new Padding(0, 0, 0, 12);
            sidebarConvertButton.Name = "sidebarConvertButton";
            sidebarConvertButton.Padding = new Padding(12, 0, 0, 0);
            sidebarConvertButton.Size = new Size(154, 40);
            sidebarConvertButton.TabIndex = 0;
            sidebarConvertButton.Text = "   Convert";
            sidebarConvertButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            sidebarConvertButton.UseVisualStyleBackColor = false;
            sidebarConvertButton.Click += ConvertNowButton_Click;
            // 
            // sidebarAddFilesButton
            // 
            sidebarAddFilesButton.FlatAppearance.BorderSize = 0;
            sidebarAddFilesButton.FlatStyle = FlatStyle.Flat;
            sidebarAddFilesButton.Image = (Image)resources.GetObject("sidebarAddFilesButton.Image");
            sidebarAddFilesButton.ImageAlign = ContentAlignment.MiddleLeft;
            sidebarAddFilesButton.Location = new Point(0, 52);
            sidebarAddFilesButton.Margin = new Padding(0, 0, 0, 6);
            sidebarAddFilesButton.Name = "sidebarAddFilesButton";
            sidebarAddFilesButton.Padding = new Padding(12, 0, 0, 0);
            sidebarAddFilesButton.Size = new Size(154, 36);
            sidebarAddFilesButton.TabIndex = 1;
            sidebarAddFilesButton.Text = "   Add Files";
            sidebarAddFilesButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            sidebarAddFilesButton.UseVisualStyleBackColor = true;
            sidebarAddFilesButton.Click += AddFilesButton_Click;
            // 
            // sidebarAddFolderButton
            // 
            sidebarAddFolderButton.FlatAppearance.BorderSize = 0;
            sidebarAddFolderButton.FlatStyle = FlatStyle.Flat;
            sidebarAddFolderButton.Image = (Image)resources.GetObject("sidebarAddFolderButton.Image");
            sidebarAddFolderButton.ImageAlign = ContentAlignment.MiddleLeft;
            sidebarAddFolderButton.Location = new Point(0, 94);
            sidebarAddFolderButton.Margin = new Padding(0, 0, 0, 6);
            sidebarAddFolderButton.Name = "sidebarAddFolderButton";
            sidebarAddFolderButton.Padding = new Padding(12, 0, 0, 0);
            sidebarAddFolderButton.Size = new Size(154, 36);
            sidebarAddFolderButton.TabIndex = 2;
            sidebarAddFolderButton.Text = "   Add Folder";
            sidebarAddFolderButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            sidebarAddFolderButton.UseVisualStyleBackColor = true;
            sidebarAddFolderButton.Click += AddFolderButton_Click;
            // 
            // sidebarRemoveButton
            // 
            sidebarRemoveButton.FlatAppearance.BorderSize = 0;
            sidebarRemoveButton.FlatStyle = FlatStyle.Flat;
            sidebarRemoveButton.Image = (Image)resources.GetObject("sidebarRemoveButton.Image");
            sidebarRemoveButton.ImageAlign = ContentAlignment.MiddleLeft;
            sidebarRemoveButton.Location = new Point(0, 136);
            sidebarRemoveButton.Margin = new Padding(0, 0, 0, 6);
            sidebarRemoveButton.Name = "sidebarRemoveButton";
            sidebarRemoveButton.Padding = new Padding(12, 0, 0, 0);
            sidebarRemoveButton.Size = new Size(154, 36);
            sidebarRemoveButton.TabIndex = 3;
            sidebarRemoveButton.Text = "   Remove";
            sidebarRemoveButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            sidebarRemoveButton.UseVisualStyleBackColor = true;
            sidebarRemoveButton.Click += RemoveSelectedButton_Click;
            // 
            // sidebarClearAllButton
            // 
            sidebarClearAllButton.FlatAppearance.BorderSize = 0;
            sidebarClearAllButton.FlatStyle = FlatStyle.Flat;
            sidebarClearAllButton.Image = (Image)resources.GetObject("sidebarClearAllButton.Image");
            sidebarClearAllButton.ImageAlign = ContentAlignment.MiddleLeft;
            sidebarClearAllButton.Location = new Point(0, 178);
            sidebarClearAllButton.Margin = new Padding(0, 0, 0, 10);
            sidebarClearAllButton.Name = "sidebarClearAllButton";
            sidebarClearAllButton.Padding = new Padding(12, 0, 0, 0);
            sidebarClearAllButton.Size = new Size(154, 36);
            sidebarClearAllButton.TabIndex = 4;
            sidebarClearAllButton.Text = "   Clear All";
            sidebarClearAllButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            sidebarClearAllButton.UseVisualStyleBackColor = true;
            sidebarClearAllButton.Click += ClearAllButton_Click;
            // 
            // sidebarSeparatorTop
            // 
            sidebarSeparatorTop.BackColor = Color.FromArgb(221, 230, 241);
            sidebarSeparatorTop.Location = new Point(0, 224);
            sidebarSeparatorTop.Margin = new Padding(0, 0, 0, 16);
            sidebarSeparatorTop.Name = "sidebarSeparatorTop";
            sidebarSeparatorTop.Size = new Size(154, 1);
            sidebarSeparatorTop.TabIndex = 5;
            // 
            // tagsHeaderLabel
            // 
            tagsHeaderLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            tagsHeaderLabel.ForeColor = Color.FromArgb(24, 30, 39);
            tagsHeaderLabel.Image = (Image)resources.GetObject("tagsHeaderLabel.Image");
            tagsHeaderLabel.ImageAlign = ContentAlignment.MiddleLeft;
            tagsHeaderLabel.Location = new Point(0, 241);
            tagsHeaderLabel.Margin = new Padding(0, 0, 0, 8);
            tagsHeaderLabel.Name = "tagsHeaderLabel";
            tagsHeaderLabel.Size = new Size(154, 28);
            tagsHeaderLabel.TabIndex = 6;
            tagsHeaderLabel.Text = "      Tags";
            tagsHeaderLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tagsFlow
            // 
            tagsFlow.Controls.Add(tagImagesCheckBox);
            tagsFlow.Controls.Add(tagAudioCheckBox);
            tagsFlow.Controls.Add(tagVideoCheckBox);
            tagsFlow.Controls.Add(tagDocumentsCheckBox);
            tagsFlow.Controls.Add(tagPdfCheckBox);
            tagsFlow.Controls.Add(tagConvertedCheckBox);
            tagsFlow.Controls.Add(tagFailedCheckBox);
            tagsFlow.Controls.Add(tagFavoritesCheckBox);
            tagsFlow.FlowDirection = FlowDirection.TopDown;
            tagsFlow.Location = new Point(0, 277);
            tagsFlow.Margin = new Padding(0, 0, 0, 8);
            tagsFlow.Name = "tagsFlow";
            tagsFlow.Size = new Size(154, 226);
            tagsFlow.TabIndex = 7;
            tagsFlow.WrapContents = false;
            // 
            // tagImagesCheckBox
            // 
            tagImagesCheckBox.Checked = true;
            tagImagesCheckBox.CheckState = CheckState.Checked;
            tagImagesCheckBox.Image = (Image)resources.GetObject("tagImagesCheckBox.Image");
            tagImagesCheckBox.Location = new Point(0, 0);
            tagImagesCheckBox.Margin = new Padding(0, 0, 0, 3);
            tagImagesCheckBox.Name = "tagImagesCheckBox";
            tagImagesCheckBox.Size = new Size(154, 25);
            tagImagesCheckBox.TabIndex = 0;
            tagImagesCheckBox.Tag = "Images";
            tagImagesCheckBox.Text = "  Images";
            tagImagesCheckBox.TextImageRelation = TextImageRelation.ImageBeforeText;
            tagImagesCheckBox.UseVisualStyleBackColor = true;
            tagImagesCheckBox.CheckedChanged += TagCheckBox_CheckedChanged;
            // 
            // tagAudioCheckBox
            // 
            tagAudioCheckBox.Checked = true;
            tagAudioCheckBox.CheckState = CheckState.Checked;
            tagAudioCheckBox.Image = (Image)resources.GetObject("tagAudioCheckBox.Image");
            tagAudioCheckBox.Location = new Point(0, 28);
            tagAudioCheckBox.Margin = new Padding(0, 0, 0, 3);
            tagAudioCheckBox.Name = "tagAudioCheckBox";
            tagAudioCheckBox.Size = new Size(154, 25);
            tagAudioCheckBox.TabIndex = 1;
            tagAudioCheckBox.Tag = "Audio";
            tagAudioCheckBox.Text = "  Audio";
            tagAudioCheckBox.TextImageRelation = TextImageRelation.ImageBeforeText;
            tagAudioCheckBox.UseVisualStyleBackColor = true;
            tagAudioCheckBox.CheckedChanged += TagCheckBox_CheckedChanged;
            // 
            // tagVideoCheckBox
            // 
            tagVideoCheckBox.Checked = true;
            tagVideoCheckBox.CheckState = CheckState.Checked;
            tagVideoCheckBox.Image = (Image)resources.GetObject("tagVideoCheckBox.Image");
            tagVideoCheckBox.Location = new Point(0, 56);
            tagVideoCheckBox.Margin = new Padding(0, 0, 0, 3);
            tagVideoCheckBox.Name = "tagVideoCheckBox";
            tagVideoCheckBox.Size = new Size(154, 25);
            tagVideoCheckBox.TabIndex = 2;
            tagVideoCheckBox.Tag = "Video";
            tagVideoCheckBox.Text = "  Video";
            tagVideoCheckBox.TextImageRelation = TextImageRelation.ImageBeforeText;
            tagVideoCheckBox.UseVisualStyleBackColor = true;
            tagVideoCheckBox.CheckedChanged += TagCheckBox_CheckedChanged;
            // 
            // tagDocumentsCheckBox
            // 
            tagDocumentsCheckBox.Checked = true;
            tagDocumentsCheckBox.CheckState = CheckState.Checked;
            tagDocumentsCheckBox.Image = (Image)resources.GetObject("tagDocumentsCheckBox.Image");
            tagDocumentsCheckBox.Location = new Point(0, 84);
            tagDocumentsCheckBox.Margin = new Padding(0, 0, 0, 3);
            tagDocumentsCheckBox.Name = "tagDocumentsCheckBox";
            tagDocumentsCheckBox.Size = new Size(154, 25);
            tagDocumentsCheckBox.TabIndex = 3;
            tagDocumentsCheckBox.Tag = "Documents";
            tagDocumentsCheckBox.Text = "  Documents";
            tagDocumentsCheckBox.TextImageRelation = TextImageRelation.ImageBeforeText;
            tagDocumentsCheckBox.UseVisualStyleBackColor = true;
            tagDocumentsCheckBox.CheckedChanged += TagCheckBox_CheckedChanged;
            // 
            // tagPdfCheckBox
            // 
            tagPdfCheckBox.Checked = true;
            tagPdfCheckBox.CheckState = CheckState.Checked;
            tagPdfCheckBox.Image = (Image)resources.GetObject("tagPdfCheckBox.Image");
            tagPdfCheckBox.Location = new Point(0, 112);
            tagPdfCheckBox.Margin = new Padding(0, 0, 0, 3);
            tagPdfCheckBox.Name = "tagPdfCheckBox";
            tagPdfCheckBox.Size = new Size(154, 25);
            tagPdfCheckBox.TabIndex = 4;
            tagPdfCheckBox.Tag = "PDF";
            tagPdfCheckBox.Text = "  PDF";
            tagPdfCheckBox.TextImageRelation = TextImageRelation.ImageBeforeText;
            tagPdfCheckBox.UseVisualStyleBackColor = true;
            tagPdfCheckBox.CheckedChanged += TagCheckBox_CheckedChanged;
            // 
            // tagConvertedCheckBox
            // 
            tagConvertedCheckBox.Checked = true;
            tagConvertedCheckBox.CheckState = CheckState.Checked;
            tagConvertedCheckBox.Image = (Image)resources.GetObject("tagConvertedCheckBox.Image");
            tagConvertedCheckBox.Location = new Point(0, 140);
            tagConvertedCheckBox.Margin = new Padding(0, 0, 0, 3);
            tagConvertedCheckBox.Name = "tagConvertedCheckBox";
            tagConvertedCheckBox.Size = new Size(154, 25);
            tagConvertedCheckBox.TabIndex = 5;
            tagConvertedCheckBox.Tag = "Converted";
            tagConvertedCheckBox.Text = "  Converted";
            tagConvertedCheckBox.TextImageRelation = TextImageRelation.ImageBeforeText;
            tagConvertedCheckBox.UseVisualStyleBackColor = true;
            tagConvertedCheckBox.CheckedChanged += TagCheckBox_CheckedChanged;
            // 
            // tagFailedCheckBox
            // 
            tagFailedCheckBox.Checked = true;
            tagFailedCheckBox.CheckState = CheckState.Checked;
            tagFailedCheckBox.Image = (Image)resources.GetObject("tagFailedCheckBox.Image");
            tagFailedCheckBox.Location = new Point(0, 168);
            tagFailedCheckBox.Margin = new Padding(0, 0, 0, 3);
            tagFailedCheckBox.Name = "tagFailedCheckBox";
            tagFailedCheckBox.Size = new Size(154, 25);
            tagFailedCheckBox.TabIndex = 6;
            tagFailedCheckBox.Tag = "Failed";
            tagFailedCheckBox.Text = "  Failed";
            tagFailedCheckBox.TextImageRelation = TextImageRelation.ImageBeforeText;
            tagFailedCheckBox.UseVisualStyleBackColor = true;
            tagFailedCheckBox.CheckedChanged += TagCheckBox_CheckedChanged;
            // 
            // tagFavoritesCheckBox
            // 
            tagFavoritesCheckBox.Checked = true;
            tagFavoritesCheckBox.CheckState = CheckState.Checked;
            tagFavoritesCheckBox.Image = (Image)resources.GetObject("tagFavoritesCheckBox.Image");
            tagFavoritesCheckBox.Location = new Point(0, 196);
            tagFavoritesCheckBox.Margin = new Padding(0, 0, 0, 3);
            tagFavoritesCheckBox.Name = "tagFavoritesCheckBox";
            tagFavoritesCheckBox.Size = new Size(154, 25);
            tagFavoritesCheckBox.TabIndex = 7;
            tagFavoritesCheckBox.Tag = "Favorites";
            tagFavoritesCheckBox.Text = "  Favorites";
            tagFavoritesCheckBox.TextImageRelation = TextImageRelation.ImageBeforeText;
            tagFavoritesCheckBox.UseVisualStyleBackColor = true;
            tagFavoritesCheckBox.CheckedChanged += TagCheckBox_CheckedChanged;
            // 
            // clearTagsButton
            // 
            clearTagsButton.FlatAppearance.BorderColor = Color.FromArgb(211, 222, 235);
            clearTagsButton.FlatStyle = FlatStyle.Flat;
            clearTagsButton.Location = new Point(0, 511);
            clearTagsButton.Margin = new Padding(0, 0, 0, 10);
            clearTagsButton.Name = "clearTagsButton";
            clearTagsButton.Size = new Size(154, 30);
            clearTagsButton.TabIndex = 8;
            clearTagsButton.Text = "Clear tags";
            clearTagsButton.UseVisualStyleBackColor = true;
            clearTagsButton.Click += ClearTagsButton_Click;
            // 
            // sidebarSeparatorBottom
            // 
            sidebarSeparatorBottom.BackColor = Color.FromArgb(221, 230, 241);
            sidebarSeparatorBottom.Location = new Point(0, 551);
            sidebarSeparatorBottom.Margin = new Padding(0, 0, 0, 10);
            sidebarSeparatorBottom.Name = "sidebarSeparatorBottom";
            sidebarSeparatorBottom.Size = new Size(154, 1);
            sidebarSeparatorBottom.TabIndex = 9;
            // 
            // sidebarSettingsButton
            // 
            sidebarSettingsButton.FlatAppearance.BorderSize = 0;
            sidebarSettingsButton.FlatStyle = FlatStyle.Flat;
            sidebarSettingsButton.Image = (Image)resources.GetObject("sidebarSettingsButton.Image");
            sidebarSettingsButton.ImageAlign = ContentAlignment.MiddleLeft;
            sidebarSettingsButton.Location = new Point(0, 562);
            sidebarSettingsButton.Margin = new Padding(0, 0, 0, 6);
            sidebarSettingsButton.Name = "sidebarSettingsButton";
            sidebarSettingsButton.Padding = new Padding(12, 0, 0, 0);
            sidebarSettingsButton.Size = new Size(154, 36);
            sidebarSettingsButton.TabIndex = 10;
            sidebarSettingsButton.Text = "   Settings";
            sidebarSettingsButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            sidebarSettingsButton.UseVisualStyleBackColor = true;
            sidebarSettingsButton.Click += SettingsSidebarButton_Click;
            // 
            // sidebarAboutButton
            // 
            sidebarAboutButton.FlatAppearance.BorderSize = 0;
            sidebarAboutButton.FlatStyle = FlatStyle.Flat;
            sidebarAboutButton.Image = (Image)resources.GetObject("sidebarAboutButton.Image");
            sidebarAboutButton.ImageAlign = ContentAlignment.MiddleLeft;
            sidebarAboutButton.Location = new Point(0, 604);
            sidebarAboutButton.Margin = new Padding(0);
            sidebarAboutButton.Name = "sidebarAboutButton";
            sidebarAboutButton.Padding = new Padding(12, 0, 0, 0);
            sidebarAboutButton.Size = new Size(154, 36);
            sidebarAboutButton.TabIndex = 11;
            sidebarAboutButton.Text = "   About";
            sidebarAboutButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            sidebarAboutButton.UseVisualStyleBackColor = true;
            sidebarAboutButton.Click += HelpAboutMenuItem_Click;
            // 
            // workspacePanel
            // 
            workspacePanel.Controls.Add(contentSplit);
            workspacePanel.Controls.Add(optionsPanel);
            workspacePanel.Controls.Add(conversionBarPanel);
            workspacePanel.Dock = DockStyle.Fill;
            workspacePanel.Location = new Point(0, 0);
            workspacePanel.Name = "workspacePanel";
            workspacePanel.Size = new Size(1108, 608);
            workspacePanel.TabIndex = 0;
            // 
            // contentSplit
            // 
            contentSplit.Dock = DockStyle.Fill;
            contentSplit.Location = new Point(0, 76);
            contentSplit.Name = "contentSplit";
            // 
            // contentSplit.Panel1
            // 
            contentSplit.Panel1.Controls.Add(filesPanel);
            contentSplit.Panel1MinSize = 620;
            // 
            // contentSplit.Panel2
            // 
            contentSplit.Panel2.Controls.Add(detailsPanel);
            contentSplit.Panel2MinSize = 340;
            contentSplit.Size = new Size(1108, 420);
            contentSplit.SplitterDistance = 744;
            contentSplit.SplitterWidth = 10;
            contentSplit.TabIndex = 1;
            // 
            // filesPanel
            // 
            filesPanel.BackColor = Color.White;
            filesPanel.BorderColor = Color.FromArgb(214, 224, 236);
            filesPanel.BorderRadius = 9;
            filesPanel.Controls.Add(filesList);
            filesPanel.Controls.Add(listHeaderPanel);
            filesPanel.Dock = DockStyle.Fill;
            filesPanel.Location = new Point(0, 0);
            filesPanel.Name = "filesPanel";
            filesPanel.Padding = new Padding(1);
            filesPanel.Size = new Size(744, 420);
            filesPanel.TabIndex = 0;
            // 
            // filesList
            // 
            filesList.AllowDrop = true;
            filesList.BackColor = Color.White;
            filesList.BorderStyle = BorderStyle.None;
            filesList.CheckBoxes = true;
            filesList.Columns.AddRange(new ColumnHeader[] { fileNameColumn, fileTypeColumn, fileSizeColumn, fileStatusColumn, fileTagsColumn, filePathColumn });
            filesList.ContextMenuStrip = fileContextMenu;
            filesList.Dock = DockStyle.Fill;
            filesList.Font = new Font("Segoe UI", 9.5F);
            filesList.FullRowSelect = true;
            filesList.GridLines = true;
            filesList.LabelEdit = true;
            filesList.LargeImageList = fileTypeLargeImages;
            filesList.Location = new Point(1, 43);
            filesList.Name = "filesList";
            filesList.ShowItemToolTips = true;
            filesList.Size = new Size(742, 376);
            filesList.SmallImageList = fileTypeImages;
            filesList.TabIndex = 1;
            filesList.UseCompatibleStateImageBehavior = false;
            filesList.View = View.Details;
            filesList.AfterLabelEdit += FilesList_AfterLabelEdit;
            filesList.ItemChecked += FilesList_ItemChecked;
            filesList.SelectedIndexChanged += FilesList_SelectedIndexChanged;
            // 
            // fileNameColumn
            // 
            fileNameColumn.Text = "Name";
            fileNameColumn.Width = 220;
            // 
            // fileTypeColumn
            // 
            fileTypeColumn.Text = "Type";
            fileTypeColumn.Width = 86;
            // 
            // fileSizeColumn
            // 
            fileSizeColumn.Text = "Size";
            fileSizeColumn.Width = 96;
            // 
            // fileStatusColumn
            // 
            fileStatusColumn.Text = "Status";
            fileStatusColumn.Width = 100;
            // 
            // fileTagsColumn
            // 
            fileTagsColumn.Text = "Tags";
            fileTagsColumn.Width = 130;
            // 
            // filePathColumn
            // 
            filePathColumn.Text = "Path";
            filePathColumn.Width = 300;
            // 
            // fileContextMenu
            // 
            fileContextMenu.Items.AddRange(new ToolStripItem[] { contextCutMenuItem, contextCopyMenuItem, contextPasteMenuItem, contextRenameMenuItem, contextDeleteMenuItem, contextPropertiesMenuItem });
            fileContextMenu.Name = "fileContextMenu";
            fileContextMenu.Size = new Size(128, 148);
            fileContextMenu.Opening += FileContextMenu_Opening;
            // 
            // contextCutMenuItem
            // 
            contextCutMenuItem.Name = "contextCutMenuItem";
            contextCutMenuItem.Size = new Size(127, 22);
            contextCutMenuItem.Text = "Cut";
            contextCutMenuItem.Click += CutMenuItem_Click;
            // 
            // contextCopyMenuItem
            // 
            contextCopyMenuItem.Name = "contextCopyMenuItem";
            contextCopyMenuItem.Size = new Size(127, 22);
            contextCopyMenuItem.Text = "Copy";
            contextCopyMenuItem.Click += CopyMenuItem_Click;
            // 
            // contextPasteMenuItem
            // 
            contextPasteMenuItem.Name = "contextPasteMenuItem";
            contextPasteMenuItem.Size = new Size(127, 22);
            contextPasteMenuItem.Text = "Paste";
            contextPasteMenuItem.Click += PasteMenuItem_Click;
            // 
            // contextRenameMenuItem
            // 
            contextRenameMenuItem.Name = "contextRenameMenuItem";
            contextRenameMenuItem.Size = new Size(127, 22);
            contextRenameMenuItem.Text = "Rename";
            contextRenameMenuItem.Click += RenameMenuItem_Click;
            // 
            // contextDeleteMenuItem
            // 
            contextDeleteMenuItem.Name = "contextDeleteMenuItem";
            contextDeleteMenuItem.Size = new Size(127, 22);
            contextDeleteMenuItem.Text = "Delete";
            contextDeleteMenuItem.Click += DeleteMenuItem_Click;
            // 
            // contextPropertiesMenuItem
            // 
            contextPropertiesMenuItem.Name = "contextPropertiesMenuItem";
            contextPropertiesMenuItem.Size = new Size(127, 22);
            contextPropertiesMenuItem.Text = "Properties";
            contextPropertiesMenuItem.Click += PropertiesMenuItem_Click;
            // 
            // fileTypeLargeImages
            // 
            fileTypeLargeImages.ColorDepth = ColorDepth.Depth32Bit;
            fileTypeLargeImages.ImageSize = new Size(48, 48);
            fileTypeLargeImages.TransparentColor = Color.Transparent;
            // 
            // fileTypeImages
            // 
            fileTypeImages.ColorDepth = ColorDepth.Depth32Bit;
            fileTypeImages.ImageSize = new Size(24, 24);
            fileTypeImages.TransparentColor = Color.Transparent;
            // 
            // listHeaderPanel
            // 
            listHeaderPanel.BackColor = Color.White;
            listHeaderPanel.Controls.Add(searchTextBox);
            listHeaderPanel.Controls.Add(filesTitleLabel);
            listHeaderPanel.Dock = DockStyle.Top;
            listHeaderPanel.Location = new Point(1, 1);
            listHeaderPanel.Name = "listHeaderPanel";
            listHeaderPanel.Padding = new Padding(14, 8, 14, 7);
            listHeaderPanel.Size = new Size(742, 42);
            listHeaderPanel.TabIndex = 0;
            // 
            // searchTextBox
            // 
            searchTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            searchTextBox.Location = new Point(516, 9);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.PlaceholderText = "Search files";
            searchTextBox.Size = new Size(210, 23);
            searchTextBox.TabIndex = 1;
            searchTextBox.TextChanged += SearchTextBox_TextChanged;
            // 
            // filesTitleLabel
            // 
            filesTitleLabel.AutoSize = true;
            filesTitleLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            filesTitleLabel.Location = new Point(14, 11);
            filesTitleLabel.Name = "filesTitleLabel";
            filesTitleLabel.Size = new Size(38, 19);
            filesTitleLabel.TabIndex = 0;
            filesTitleLabel.Text = "Files";
            // 
            // detailsPanel
            // 
            detailsPanel.AutoScroll = true;
            detailsPanel.BackColor = Color.White;
            detailsPanel.BorderColor = Color.FromArgb(214, 224, 236);
            detailsPanel.BorderRadius = 9;
            detailsPanel.Controls.Add(detailsTagChipLabel);
            detailsPanel.Controls.Add(detailsAddTagButton);
            detailsPanel.Controls.Add(detailsTagsLabel);
            detailsPanel.Controls.Add(detailsDividerPanel);
            detailsPanel.Controls.Add(detailsGridPanel);
            detailsPanel.Controls.Add(detailsTitleLabel);
            detailsPanel.Controls.Add(detailsPreviewPanel);
            detailsPanel.Dock = DockStyle.Fill;
            detailsPanel.Location = new Point(0, 0);
            detailsPanel.Name = "detailsPanel";
            detailsPanel.Padding = new Padding(16);
            detailsPanel.Size = new Size(354, 420);
            detailsPanel.TabIndex = 0;
            // 
            // detailsTagChipLabel
            // 
            detailsTagChipLabel.BackColor = Color.FromArgb(255, 219, 230);
            detailsTagChipLabel.ForeColor = Color.FromArgb(176, 40, 78);
            detailsTagChipLabel.Location = new Point(16, 454);
            detailsTagChipLabel.Name = "detailsTagChipLabel";
            detailsTagChipLabel.Padding = new Padding(10, 0, 10, 0);
            detailsTagChipLabel.Size = new Size(82, 28);
            detailsTagChipLabel.TabIndex = 6;
            detailsTagChipLabel.Text = "Audio";
            detailsTagChipLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // detailsAddTagButton
            // 
            detailsAddTagButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            detailsAddTagButton.FlatAppearance.BorderColor = Color.FromArgb(211, 222, 235);
            detailsAddTagButton.FlatStyle = FlatStyle.Flat;
            detailsAddTagButton.Image = (Image)resources.GetObject("detailsAddTagButton.Image");
            detailsAddTagButton.Location = new Point(302, 418);
            detailsAddTagButton.Name = "detailsAddTagButton";
            detailsAddTagButton.Size = new Size(36, 30);
            detailsAddTagButton.TabIndex = 5;
            detailsAddTagButton.UseVisualStyleBackColor = true;
            // 
            // detailsTagsLabel
            // 
            detailsTagsLabel.AutoSize = true;
            detailsTagsLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            detailsTagsLabel.Location = new Point(16, 424);
            detailsTagsLabel.Name = "detailsTagsLabel";
            detailsTagsLabel.Size = new Size(39, 19);
            detailsTagsLabel.TabIndex = 4;
            detailsTagsLabel.Text = "Tags";
            // 
            // detailsDividerPanel
            // 
            detailsDividerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            detailsDividerPanel.BackColor = Color.FromArgb(221, 230, 241);
            detailsDividerPanel.Location = new Point(16, 410);
            detailsDividerPanel.Name = "detailsDividerPanel";
            detailsDividerPanel.Size = new Size(322, 1);
            detailsDividerPanel.TabIndex = 3;
            // 
            // detailsGridPanel
            // 
            detailsGridPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            detailsGridPanel.ColumnCount = 2;
            detailsGridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
            detailsGridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            detailsGridPanel.Controls.Add(detailsTypeLabel, 0, 0);
            detailsGridPanel.Controls.Add(detailsTypeValueLabel, 1, 0);
            detailsGridPanel.Controls.Add(detailsSizeLabel, 0, 1);
            detailsGridPanel.Controls.Add(detailsSizeValueLabel, 1, 1);
            detailsGridPanel.Controls.Add(detailsDurationLabel, 0, 2);
            detailsGridPanel.Controls.Add(detailsDurationValueLabel, 1, 2);
            detailsGridPanel.Controls.Add(detailsBitrateLabel, 0, 3);
            detailsGridPanel.Controls.Add(detailsBitrateValueLabel, 1, 3);
            detailsGridPanel.Controls.Add(detailsPathLabel, 0, 4);
            detailsGridPanel.Controls.Add(detailsPathValueLabel, 1, 4);
            detailsGridPanel.Location = new Point(16, 252);
            detailsGridPanel.Name = "detailsGridPanel";
            detailsGridPanel.RowCount = 5;
            detailsGridPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            detailsGridPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            detailsGridPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            detailsGridPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            detailsGridPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            detailsGridPanel.Size = new Size(322, 148);
            detailsGridPanel.TabIndex = 2;
            // 
            // detailsTypeLabel
            // 
            detailsTypeLabel.ForeColor = Color.FromArgb(83, 91, 105);
            detailsTypeLabel.Location = new Point(3, 0);
            detailsTypeLabel.Name = "detailsTypeLabel";
            detailsTypeLabel.Size = new Size(80, 23);
            detailsTypeLabel.TabIndex = 0;
            detailsTypeLabel.Text = "Type:";
            // 
            // detailsTypeValueLabel
            // 
            detailsTypeValueLabel.AutoEllipsis = true;
            detailsTypeValueLabel.Location = new Point(89, 0);
            detailsTypeValueLabel.Name = "detailsTypeValueLabel";
            detailsTypeValueLabel.Size = new Size(230, 23);
            detailsTypeValueLabel.TabIndex = 1;
            detailsTypeValueLabel.Text = "MP3 Audio";
            // 
            // detailsSizeLabel
            // 
            detailsSizeLabel.ForeColor = Color.FromArgb(83, 91, 105);
            detailsSizeLabel.Location = new Point(3, 25);
            detailsSizeLabel.Name = "detailsSizeLabel";
            detailsSizeLabel.Size = new Size(80, 23);
            detailsSizeLabel.TabIndex = 2;
            detailsSizeLabel.Text = "Size:";
            // 
            // detailsSizeValueLabel
            // 
            detailsSizeValueLabel.AutoEllipsis = true;
            detailsSizeValueLabel.Location = new Point(89, 25);
            detailsSizeValueLabel.Name = "detailsSizeValueLabel";
            detailsSizeValueLabel.Size = new Size(230, 23);
            detailsSizeValueLabel.TabIndex = 3;
            detailsSizeValueLabel.Text = "5.21 MB";
            // 
            // detailsDurationLabel
            // 
            detailsDurationLabel.ForeColor = Color.FromArgb(83, 91, 105);
            detailsDurationLabel.Location = new Point(3, 50);
            detailsDurationLabel.Name = "detailsDurationLabel";
            detailsDurationLabel.Size = new Size(80, 23);
            detailsDurationLabel.TabIndex = 4;
            detailsDurationLabel.Text = "Duration:";
            // 
            // detailsDurationValueLabel
            // 
            detailsDurationValueLabel.AutoEllipsis = true;
            detailsDurationValueLabel.Location = new Point(89, 50);
            detailsDurationValueLabel.Name = "detailsDurationValueLabel";
            detailsDurationValueLabel.Size = new Size(230, 23);
            detailsDurationValueLabel.TabIndex = 5;
            detailsDurationValueLabel.Text = "03:42";
            // 
            // detailsBitrateLabel
            // 
            detailsBitrateLabel.ForeColor = Color.FromArgb(83, 91, 105);
            detailsBitrateLabel.Location = new Point(3, 75);
            detailsBitrateLabel.Name = "detailsBitrateLabel";
            detailsBitrateLabel.Size = new Size(80, 23);
            detailsBitrateLabel.TabIndex = 6;
            detailsBitrateLabel.Text = "Bitrate:";
            // 
            // detailsBitrateValueLabel
            // 
            detailsBitrateValueLabel.AutoEllipsis = true;
            detailsBitrateValueLabel.Location = new Point(89, 75);
            detailsBitrateValueLabel.Name = "detailsBitrateValueLabel";
            detailsBitrateValueLabel.Size = new Size(230, 23);
            detailsBitrateValueLabel.TabIndex = 7;
            detailsBitrateValueLabel.Text = "320 kbps";
            // 
            // detailsPathLabel
            // 
            detailsPathLabel.ForeColor = Color.FromArgb(83, 91, 105);
            detailsPathLabel.Location = new Point(3, 100);
            detailsPathLabel.Name = "detailsPathLabel";
            detailsPathLabel.Size = new Size(80, 23);
            detailsPathLabel.TabIndex = 8;
            detailsPathLabel.Text = "Path:";
            // 
            // detailsPathValueLabel
            // 
            detailsPathValueLabel.AutoEllipsis = true;
            detailsPathValueLabel.Location = new Point(89, 100);
            detailsPathValueLabel.Name = "detailsPathValueLabel";
            detailsPathValueLabel.Size = new Size(230, 46);
            detailsPathValueLabel.TabIndex = 9;
            detailsPathValueLabel.Text = "C:\\Music\\song.mp3";
            // 
            // detailsTitleLabel
            // 
            detailsTitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            detailsTitleLabel.AutoEllipsis = true;
            detailsTitleLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            detailsTitleLabel.Location = new Point(16, 218);
            detailsTitleLabel.Name = "detailsTitleLabel";
            detailsTitleLabel.Size = new Size(322, 26);
            detailsTitleLabel.TabIndex = 1;
            detailsTitleLabel.Text = "song.mp3";
            // 
            // detailsPreviewPanel
            // 
            detailsPreviewPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            detailsPreviewPanel.BackColor = Color.FromArgb(38, 45, 58);
            detailsPreviewPanel.BorderColor = Color.FromArgb(38, 45, 58);
            detailsPreviewPanel.BorderRadius = 9;
            detailsPreviewPanel.Controls.Add(detailsPreviewIcon);
            detailsPreviewPanel.Location = new Point(16, 16);
            detailsPreviewPanel.Name = "detailsPreviewPanel";
            detailsPreviewPanel.Size = new Size(322, 202);
            detailsPreviewPanel.TabIndex = 0;
            // 
            // detailsPreviewIcon
            // 
            detailsPreviewIcon.Dock = DockStyle.Fill;
            detailsPreviewIcon.Image = (Image)resources.GetObject("detailsPreviewIcon.Image");
            detailsPreviewIcon.Location = new Point(0, 0);
            detailsPreviewIcon.Name = "detailsPreviewIcon";
            detailsPreviewIcon.Size = new Size(322, 202);
            detailsPreviewIcon.SizeMode = PictureBoxSizeMode.Zoom;
            detailsPreviewIcon.TabIndex = 0;
            detailsPreviewIcon.TabStop = false;
            // 
            // optionsPanel
            // 
            optionsPanel.BackColor = Color.White;
            optionsPanel.BorderColor = Color.FromArgb(214, 224, 236);
            optionsPanel.BorderRadius = 9;
            optionsPanel.Controls.Add(optionsLayout);
            optionsPanel.Dock = DockStyle.Bottom;
            optionsPanel.Location = new Point(0, 496);
            optionsPanel.Margin = new Padding(0, 10, 0, 0);
            optionsPanel.Name = "optionsPanel";
            optionsPanel.Padding = new Padding(14);
            optionsPanel.Size = new Size(1108, 112);
            optionsPanel.TabIndex = 2;
            // 
            // optionsLayout
            // 
            optionsLayout.ColumnCount = 3;
            optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            optionsLayout.Controls.Add(outputPanel, 0, 0);
            optionsLayout.Controls.Add(conversionOptionsPanel, 1, 0);
            optionsLayout.Controls.Add(progressPanel, 2, 0);
            optionsLayout.Dock = DockStyle.Fill;
            optionsLayout.Location = new Point(14, 14);
            optionsLayout.Name = "optionsLayout";
            optionsLayout.RowCount = 1;
            optionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            optionsLayout.Size = new Size(1080, 84);
            optionsLayout.TabIndex = 0;
            // 
            // outputPanel
            // 
            outputPanel.Controls.Add(openOutputFolderCheckBox);
            outputPanel.Controls.Add(browseOutputButton);
            outputPanel.Controls.Add(outputDirectoryTextBox);
            outputPanel.Controls.Add(outputDirectoryLabel);
            outputPanel.Dock = DockStyle.Fill;
            outputPanel.Location = new Point(3, 3);
            outputPanel.Name = "outputPanel";
            outputPanel.Size = new Size(447, 78);
            outputPanel.TabIndex = 0;
            // 
            // openOutputFolderCheckBox
            // 
            openOutputFolderCheckBox.Location = new Point(0, 58);
            openOutputFolderCheckBox.Name = "openOutputFolderCheckBox";
            openOutputFolderCheckBox.Size = new Size(220, 19);
            openOutputFolderCheckBox.TabIndex = 3;
            openOutputFolderCheckBox.Text = "Open output folder after conversion";
            openOutputFolderCheckBox.UseVisualStyleBackColor = true;
            // 
            // browseOutputButton
            // 
            browseOutputButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseOutputButton.FlatAppearance.BorderColor = Color.FromArgb(211, 222, 235);
            browseOutputButton.FlatStyle = FlatStyle.Flat;
            browseOutputButton.Location = new Point(340, 26);
            browseOutputButton.Name = "browseOutputButton";
            browseOutputButton.Size = new Size(92, 26);
            browseOutputButton.TabIndex = 2;
            browseOutputButton.Text = "Browse...";
            browseOutputButton.UseVisualStyleBackColor = true;
            browseOutputButton.Click += BrowseOutputButton_Click;
            // 
            // outputDirectoryTextBox
            // 
            outputDirectoryTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            outputDirectoryTextBox.Location = new Point(0, 27);
            outputDirectoryTextBox.Name = "outputDirectoryTextBox";
            outputDirectoryTextBox.Size = new Size(330, 23);
            outputDirectoryTextBox.TabIndex = 1;
            outputDirectoryTextBox.Text = "C:\\FormatForge\\Converted";
            // 
            // outputDirectoryLabel
            // 
            outputDirectoryLabel.AutoSize = true;
            outputDirectoryLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            outputDirectoryLabel.Image = (Image)resources.GetObject("outputDirectoryLabel.Image");
            outputDirectoryLabel.ImageAlign = ContentAlignment.MiddleLeft;
            outputDirectoryLabel.Location = new Point(0, 2);
            outputDirectoryLabel.Name = "outputDirectoryLabel";
            outputDirectoryLabel.Size = new Size(121, 15);
            outputDirectoryLabel.TabIndex = 0;
            outputDirectoryLabel.Text = "      Output Directory";
            // 
            // conversionOptionsPanel
            // 
            conversionOptionsPanel.Controls.Add(overwriteExistingFilesCheckBox);
            conversionOptionsPanel.Controls.Add(keepOriginalDateCheckBox);
            conversionOptionsPanel.Controls.Add(preserveMetadataCheckBox);
            conversionOptionsPanel.Controls.Add(optionsLabel);
            conversionOptionsPanel.Dock = DockStyle.Fill;
            conversionOptionsPanel.Location = new Point(456, 3);
            conversionOptionsPanel.Name = "conversionOptionsPanel";
            conversionOptionsPanel.Size = new Size(264, 78);
            conversionOptionsPanel.TabIndex = 1;
            // 
            // overwriteExistingFilesCheckBox
            // 
            overwriteExistingFilesCheckBox.Location = new Point(0, 60);
            overwriteExistingFilesCheckBox.Name = "overwriteExistingFilesCheckBox";
            overwriteExistingFilesCheckBox.Size = new Size(140, 19);
            overwriteExistingFilesCheckBox.TabIndex = 3;
            overwriteExistingFilesCheckBox.Text = "Overwrite existing files";
            overwriteExistingFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // keepOriginalDateCheckBox
            // 
            keepOriginalDateCheckBox.Checked = true;
            keepOriginalDateCheckBox.CheckState = CheckState.Checked;
            keepOriginalDateCheckBox.Location = new Point(0, 42);
            keepOriginalDateCheckBox.Name = "keepOriginalDateCheckBox";
            keepOriginalDateCheckBox.Size = new Size(119, 19);
            keepOriginalDateCheckBox.TabIndex = 2;
            keepOriginalDateCheckBox.Text = "Keep original date";
            keepOriginalDateCheckBox.UseVisualStyleBackColor = true;
            // 
            // preserveMetadataCheckBox
            // 
            preserveMetadataCheckBox.Checked = true;
            preserveMetadataCheckBox.CheckState = CheckState.Checked;
            preserveMetadataCheckBox.Location = new Point(0, 21);
            preserveMetadataCheckBox.Name = "preserveMetadataCheckBox";
            preserveMetadataCheckBox.Size = new Size(124, 19);
            preserveMetadataCheckBox.TabIndex = 1;
            preserveMetadataCheckBox.Text = "Preserve metadata";
            preserveMetadataCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionsLabel
            // 
            optionsLabel.AutoSize = true;
            optionsLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            optionsLabel.Location = new Point(0, 2);
            optionsLabel.Name = "optionsLabel";
            optionsLabel.Size = new Size(50, 15);
            optionsLabel.TabIndex = 0;
            optionsLabel.Text = "Options";
            // 
            // progressPanel
            // 
            progressPanel.Controls.Add(progressTextLabel);
            progressPanel.Controls.Add(progressBar);
            progressPanel.Controls.Add(progressTitleLabel);
            progressPanel.Dock = DockStyle.Fill;
            progressPanel.Location = new Point(726, 3);
            progressPanel.Name = "progressPanel";
            progressPanel.Size = new Size(351, 78);
            progressPanel.TabIndex = 2;
            // 
            // progressTextLabel
            // 
            progressTextLabel.AutoSize = true;
            progressTextLabel.ForeColor = Color.FromArgb(53, 63, 79);
            progressTextLabel.Location = new Point(0, 58);
            progressTextLabel.Name = "progressTextLabel";
            progressTextLabel.Size = new Size(137, 15);
            progressTextLabel.TabIndex = 2;
            progressTextLabel.Text = "Ready to convert 0 file(s)";
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(0, 31);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(338, 18);
            progressBar.TabIndex = 1;
            // 
            // progressTitleLabel
            // 
            progressTitleLabel.AutoSize = true;
            progressTitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            progressTitleLabel.Location = new Point(0, 2);
            progressTitleLabel.Name = "progressTitleLabel";
            progressTitleLabel.Size = new Size(55, 15);
            progressTitleLabel.TabIndex = 0;
            progressTitleLabel.Text = "Progress";
            // 
            // conversionBarPanel
            // 
            conversionBarPanel.BackColor = Color.White;
            conversionBarPanel.BorderColor = Color.FromArgb(214, 224, 236);
            conversionBarPanel.BorderRadius = 9;
            conversionBarPanel.Controls.Add(categoryFlow);
            conversionBarPanel.Controls.Add(outputFormatPanel);
            conversionBarPanel.Controls.Add(advancedButton);
            conversionBarPanel.Controls.Add(mergeImagesPdfButton);
            conversionBarPanel.Controls.Add(convertNowButton);
            conversionBarPanel.Dock = DockStyle.Top;
            conversionBarPanel.Location = new Point(0, 0);
            conversionBarPanel.Name = "conversionBarPanel";
            conversionBarPanel.Padding = new Padding(12);
            conversionBarPanel.Size = new Size(1108, 76);
            conversionBarPanel.TabIndex = 0;
            // 
            // categoryFlow
            // 
            categoryFlow.Controls.Add(categoryImagesButton);
            categoryFlow.Controls.Add(categoryAudioButton);
            categoryFlow.Controls.Add(categoryVideoButton);
            categoryFlow.Controls.Add(categoryDocumentsButton);
            categoryFlow.Dock = DockStyle.Fill;
            categoryFlow.Location = new Point(12, 12);
            categoryFlow.Name = "categoryFlow";
            categoryFlow.Size = new Size(454, 52);
            categoryFlow.TabIndex = 0;
            categoryFlow.WrapContents = false;
            // 
            // categoryImagesButton
            // 
            categoryImagesButton.Location = new Point(3, 3);
            categoryImagesButton.Name = "categoryImagesButton";
            categoryImagesButton.Size = new Size(75, 23);
            categoryImagesButton.TabIndex = 0;
            // 
            // categoryAudioButton
            // 
            categoryAudioButton.Location = new Point(84, 3);
            categoryAudioButton.Name = "categoryAudioButton";
            categoryAudioButton.Size = new Size(75, 23);
            categoryAudioButton.TabIndex = 1;
            // 
            // categoryVideoButton
            // 
            categoryVideoButton.Location = new Point(165, 3);
            categoryVideoButton.Name = "categoryVideoButton";
            categoryVideoButton.Size = new Size(75, 23);
            categoryVideoButton.TabIndex = 2;
            // 
            // categoryDocumentsButton
            // 
            categoryDocumentsButton.Location = new Point(246, 3);
            categoryDocumentsButton.Name = "categoryDocumentsButton";
            categoryDocumentsButton.Size = new Size(75, 23);
            categoryDocumentsButton.TabIndex = 3;
            // 
            // outputFormatPanel
            // 
            outputFormatPanel.Controls.Add(outputFormatComboBox);
            outputFormatPanel.Controls.Add(outputFormatLabel);
            outputFormatPanel.Dock = DockStyle.Right;
            outputFormatPanel.Location = new Point(466, 12);
            outputFormatPanel.Name = "outputFormatPanel";
            outputFormatPanel.Padding = new Padding(10, 0, 10, 0);
            outputFormatPanel.Size = new Size(216, 52);
            outputFormatPanel.TabIndex = 1;
            // 
            // outputFormatComboBox
            // 
            outputFormatComboBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            outputFormatComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            outputFormatComboBox.FormattingEnabled = true;
            outputFormatComboBox.Items.AddRange(new object[] { "Select a file first" });
            outputFormatComboBox.Location = new Point(10, 24);
            outputFormatComboBox.Name = "outputFormatComboBox";
            outputFormatComboBox.Size = new Size(192, 23);
            outputFormatComboBox.TabIndex = 1;
            // 
            // outputFormatLabel
            // 
            outputFormatLabel.AutoSize = true;
            outputFormatLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            outputFormatLabel.Location = new Point(10, 0);
            outputFormatLabel.Name = "outputFormatLabel";
            outputFormatLabel.Size = new Size(93, 15);
            outputFormatLabel.TabIndex = 0;
            outputFormatLabel.Text = "Output Format:";
            // 
            // advancedButton
            // 
            advancedButton.Dock = DockStyle.Right;
            advancedButton.FlatAppearance.BorderColor = Color.FromArgb(211, 222, 235);
            advancedButton.FlatStyle = FlatStyle.Flat;
            advancedButton.Image = (Image)resources.GetObject("advancedButton.Image");
            advancedButton.Location = new Point(682, 12);
            advancedButton.Margin = new Padding(8, 0, 8, 0);
            advancedButton.Name = "advancedButton";
            advancedButton.Size = new Size(130, 52);
            advancedButton.TabIndex = 2;
            advancedButton.Text = " Options";
            advancedButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            advancedButton.UseVisualStyleBackColor = true;
            advancedButton.Click += AdvancedButton_Click;
            // 
            // mergeImagesPdfButton
            // 
            mergeImagesPdfButton.BackColor = Color.FromArgb(35, 117, 238);
            mergeImagesPdfButton.Dock = DockStyle.Right;
            mergeImagesPdfButton.FlatAppearance.BorderSize = 0;
            mergeImagesPdfButton.FlatStyle = FlatStyle.Flat;
            mergeImagesPdfButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            mergeImagesPdfButton.ForeColor = Color.White;
            mergeImagesPdfButton.Image = (Image)resources.GetObject("mergeImagesPdfButton.Image");
            mergeImagesPdfButton.Location = new Point(812, 12);
            mergeImagesPdfButton.Name = "mergeImagesPdfButton";
            mergeImagesPdfButton.Size = new Size(142, 52);
            mergeImagesPdfButton.TabIndex = 3;
            mergeImagesPdfButton.Text = " Images to PDF";
            mergeImagesPdfButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            mergeImagesPdfButton.UseVisualStyleBackColor = false;
            mergeImagesPdfButton.Click += MergeImagesPdfButton_Click;
            // 
            // convertNowButton
            // 
            convertNowButton.BackColor = Color.FromArgb(35, 117, 238);
            convertNowButton.Dock = DockStyle.Right;
            convertNowButton.FlatAppearance.BorderSize = 0;
            convertNowButton.FlatStyle = FlatStyle.Flat;
            convertNowButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            convertNowButton.ForeColor = Color.White;
            convertNowButton.Image = (Image)resources.GetObject("convertNowButton.Image");
            convertNowButton.Location = new Point(954, 12);
            convertNowButton.Name = "convertNowButton";
            convertNowButton.Size = new Size(142, 52);
            convertNowButton.TabIndex = 4;
            convertNowButton.Text = "  Convert";
            convertNowButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            convertNowButton.UseVisualStyleBackColor = false;
            convertNowButton.Click += ConvertNowButton_Click;
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.Transparent;
            headerPanel.Controls.Add(appSubtitle);
            headerPanel.Controls.Add(appTitle);
            headerPanel.Controls.Add(appIcon);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(12, 12);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(1296, 82);
            headerPanel.TabIndex = 0;
            // 
            // appSubtitle
            // 
            appSubtitle.AutoSize = true;
            appSubtitle.ForeColor = Color.FromArgb(74, 86, 104);
            appSubtitle.Location = new Point(92, 51);
            appSubtitle.Name = "appSubtitle";
            appSubtitle.Size = new Size(220, 15);
            appSubtitle.TabIndex = 2;
            appSubtitle.Text = "Convert, organize and manage your files";
            // 
            // appTitle
            // 
            appTitle.AutoSize = true;
            appTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            appTitle.ForeColor = Color.FromArgb(10, 14, 22);
            appTitle.Location = new Point(88, 12);
            appTitle.Name = "appTitle";
            appTitle.Size = new Size(184, 37);
            appTitle.TabIndex = 1;
            appTitle.Text = "FormatForge";
            // 
            // appIcon
            // 
            appIcon.Image = (Image)resources.GetObject("appIcon.Image");
            appIcon.Location = new Point(14, 8);
            appIcon.Name = "appIcon";
            appIcon.Size = new Size(64, 64);
            appIcon.SizeMode = PictureBoxSizeMode.Zoom;
            appIcon.TabIndex = 0;
            appIcon.TabStop = false;
            // 
            // statusStrip
            // 
            statusStrip.BackColor = Color.FromArgb(250, 252, 255);
            statusStrip.Items.AddRange(new ToolStripItem[] { filesCountStatusLabel, statusSeparatorLabel1, selectedCountStatusLabel, statusSeparatorLabel2, checkedCountStatusLabel, statusTextLabel, versionStatusLabel });
            statusStrip.Location = new Point(0, 738);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1320, 22);
            statusStrip.SizingGrip = false;
            statusStrip.TabIndex = 2;
            statusStrip.Text = "statusStrip";
            // 
            // filesCountStatusLabel
            // 
            filesCountStatusLabel.Name = "filesCountStatusLabel";
            filesCountStatusLabel.Size = new Size(45, 17);
            filesCountStatusLabel.Text = "0 file(s)";
            // 
            // statusSeparatorLabel1
            // 
            statusSeparatorLabel1.ForeColor = Color.FromArgb(160, 171, 186);
            statusSeparatorLabel1.Name = "statusSeparatorLabel1";
            statusSeparatorLabel1.Size = new Size(10, 17);
            statusSeparatorLabel1.Text = "|";
            // 
            // selectedCountStatusLabel
            // 
            selectedCountStatusLabel.Name = "selectedCountStatusLabel";
            selectedCountStatusLabel.Size = new Size(59, 17);
            selectedCountStatusLabel.Text = "0 selected";
            // 
            // statusSeparatorLabel2
            // 
            statusSeparatorLabel2.ForeColor = Color.FromArgb(160, 171, 186);
            statusSeparatorLabel2.Name = "statusSeparatorLabel2";
            statusSeparatorLabel2.Size = new Size(10, 17);
            statusSeparatorLabel2.Text = "|";
            // 
            // checkedCountStatusLabel
            // 
            checkedCountStatusLabel.Name = "checkedCountStatusLabel";
            checkedCountStatusLabel.Size = new Size(56, 17);
            checkedCountStatusLabel.Text = "0 queued";
            // 
            // statusTextLabel
            // 
            statusTextLabel.Name = "statusTextLabel";
            statusTextLabel.Size = new Size(1017, 17);
            statusTextLabel.Spring = true;
            statusTextLabel.Text = "Ready";
            statusTextLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // versionStatusLabel
            // 
            versionStatusLabel.Name = "versionStatusLabel";
            versionStatusLabel.Size = new Size(108, 17);
            versionStatusLabel.Text = "FormatForge v1.1.0";
            // 
            // openFileDialog
            // 
            openFileDialog.Filter = resources.GetString("openFileDialog.Filter");
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Select files";
            // 
            // folderBrowserDialog
            // 
            folderBrowserDialog.Description = "Select a folder";
            folderBrowserDialog.UseDescriptionForTitle = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(238, 244, 251);
            ClientSize = new Size(1320, 760);
            Controls.Add(shellPanel);
            Controls.Add(statusStrip);
            Controls.Add(mainMenu);
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 9F);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = mainMenu;
            MinimumSize = new Size(1200, 680);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FormatForge";
            KeyDown += Form1_KeyDown;
            mainMenu.ResumeLayout(false);
            mainMenu.PerformLayout();
            shellPanel.ResumeLayout(false);
            mainSplit.Panel1.ResumeLayout(false);
            mainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
            mainSplit.ResumeLayout(false);
            sidebarPanel.ResumeLayout(false);
            sidebarFlow.ResumeLayout(false);
            tagsFlow.ResumeLayout(false);
            workspacePanel.ResumeLayout(false);
            contentSplit.Panel1.ResumeLayout(false);
            contentSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)contentSplit).EndInit();
            contentSplit.ResumeLayout(false);
            filesPanel.ResumeLayout(false);
            fileContextMenu.ResumeLayout(false);
            listHeaderPanel.ResumeLayout(false);
            listHeaderPanel.PerformLayout();
            detailsPanel.ResumeLayout(false);
            detailsPanel.PerformLayout();
            detailsGridPanel.ResumeLayout(false);
            detailsPreviewPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)detailsPreviewIcon).EndInit();
            optionsPanel.ResumeLayout(false);
            optionsLayout.ResumeLayout(false);
            outputPanel.ResumeLayout(false);
            outputPanel.PerformLayout();
            conversionOptionsPanel.ResumeLayout(false);
            conversionOptionsPanel.PerformLayout();
            progressPanel.ResumeLayout(false);
            progressPanel.PerformLayout();
            conversionBarPanel.ResumeLayout(false);
            categoryFlow.ResumeLayout(false);
            outputFormatPanel.ResumeLayout(false);
            outputFormatPanel.PerformLayout();
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)appIcon).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private void ConfigureCategoryButton(Button button, string text, UiIconKind iconKind, Color color, string tag)
        {
            button.BackColor = Color.White;
            button.FlatAppearance.BorderColor = Color.FromArgb(211, 222, 235);
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.ForeColor = Color.FromArgb(18, 24, 33);
            button.Image = UiIconFactory.CreateIcon(iconKind, color, 20);
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.Location = new Point(0, 0);
            button.Margin = new Padding(0, 0, 8, 0);
            button.Padding = new Padding(10, 0, 6, 0);
            button.Size = new Size(96, 52);
            button.Tag = tag;
            button.Text = " " + text;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
            button.UseVisualStyleBackColor = false;
            button.Click += CategoryButton_Click;
        }

        #endregion

        private MenuStrip mainMenu;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem fileAddFilesMenuItem;
        private ToolStripMenuItem fileAddFolderMenuItem;
        private ToolStripMenuItem fileRemoveSelectedMenuItem;
        private ToolStripMenuItem fileClearListMenuItem;
        private ToolStripMenuItem fileExitMenuItem;
        private ToolStripMenuItem editMenu;
        private ToolStripMenuItem editCutMenuItem;
        private ToolStripMenuItem editCopyMenuItem;
        private ToolStripMenuItem editPasteMenuItem;
        private ToolStripMenuItem editRenameMenuItem;
        private ToolStripMenuItem editDeleteMenuItem;
        private ToolStripMenuItem editSelectAllMenuItem;
        private ToolStripMenuItem editPropertiesMenuItem;
        private ToolStripMenuItem viewMenu;
        private ToolStripMenuItem viewDetailsMenuItem;
        private ToolStripMenuItem viewLargeIconsMenuItem;
        private ToolStripMenuItem viewTagsPanelMenuItem;
        private ToolStripMenuItem viewRefreshMenuItem;
        private ToolStripMenuItem helpMenu;
        private ToolStripMenuItem helpUpdatePythonMenuItem;
        private ToolStripMenuItem helpDocumentationMenuItem;
        private ToolStripMenuItem helpAboutMenuItem;
        private Panel shellPanel;
        private Panel headerPanel;
        private PictureBox appIcon;
        private Label appTitle;
        private Label appSubtitle;
        private SplitContainer mainSplit;
        private RoundedPanel sidebarPanel;
        private FlowLayoutPanel sidebarFlow;
        private Button sidebarConvertButton;
        private Button sidebarAddFilesButton;
        private Button sidebarAddFolderButton;
        private Button sidebarRemoveButton;
        private Button sidebarClearAllButton;
        private Panel sidebarSeparatorTop;
        private Label tagsHeaderLabel;
        private FlowLayoutPanel tagsFlow;
        private CheckBox tagImagesCheckBox;
        private CheckBox tagAudioCheckBox;
        private CheckBox tagVideoCheckBox;
        private CheckBox tagDocumentsCheckBox;
        private CheckBox tagPdfCheckBox;
        private CheckBox tagConvertedCheckBox;
        private CheckBox tagFailedCheckBox;
        private CheckBox tagFavoritesCheckBox;
        private Button clearTagsButton;
        private Panel sidebarSeparatorBottom;
        private Button sidebarSettingsButton;
        private Button sidebarAboutButton;
        private Panel workspacePanel;
        private RoundedPanel conversionBarPanel;
        private FlowLayoutPanel categoryFlow;
        private Button categoryImagesButton;
        private Button categoryAudioButton;
        private Button categoryVideoButton;
        private Button categoryDocumentsButton;
        private Panel outputFormatPanel;
        private Label outputFormatLabel;
        private ComboBox outputFormatComboBox;
        private Button advancedButton;
        private Button mergeImagesPdfButton;
        private Button convertNowButton;
        private SplitContainer contentSplit;
        private RoundedPanel filesPanel;
        private Panel listHeaderPanel;
        private Label filesTitleLabel;
        private TextBox searchTextBox;
        private ListView filesList;
        private ColumnHeader fileNameColumn;
        private ColumnHeader fileTypeColumn;
        private ColumnHeader fileSizeColumn;
        private ColumnHeader fileStatusColumn;
        private ColumnHeader fileTagsColumn;
        private ColumnHeader filePathColumn;
        private ContextMenuStrip fileContextMenu;
        private ToolStripMenuItem contextCutMenuItem;
        private ToolStripMenuItem contextCopyMenuItem;
        private ToolStripMenuItem contextPasteMenuItem;
        private ToolStripMenuItem contextRenameMenuItem;
        private ToolStripMenuItem contextDeleteMenuItem;
        private ToolStripMenuItem contextPropertiesMenuItem;
        private ImageList fileTypeImages;
        private ImageList fileTypeLargeImages;
        private RoundedPanel detailsPanel;
        private RoundedPanel detailsPreviewPanel;
        private PictureBox detailsPreviewIcon;
        private Label detailsTitleLabel;
        private TableLayoutPanel detailsGridPanel;
        private Label detailsTypeLabel;
        private Label detailsTypeValueLabel;
        private Label detailsSizeLabel;
        private Label detailsSizeValueLabel;
        private Label detailsDurationLabel;
        private Label detailsDurationValueLabel;
        private Label detailsBitrateLabel;
        private Label detailsBitrateValueLabel;
        private Label detailsPathLabel;
        private Label detailsPathValueLabel;
        private Panel detailsDividerPanel;
        private Label detailsTagsLabel;
        private Button detailsAddTagButton;
        private Label detailsTagChipLabel;
        private RoundedPanel optionsPanel;
        private TableLayoutPanel optionsLayout;
        private Panel outputPanel;
        private Label outputDirectoryLabel;
        private TextBox outputDirectoryTextBox;
        private Button browseOutputButton;
        private CheckBox openOutputFolderCheckBox;
        private Panel conversionOptionsPanel;
        private Label optionsLabel;
        private CheckBox preserveMetadataCheckBox;
        private CheckBox keepOriginalDateCheckBox;
        private CheckBox overwriteExistingFilesCheckBox;
        private Panel progressPanel;
        private Label progressTitleLabel;
        private ThemedProgressBar progressBar;
        private Label progressTextLabel;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel filesCountStatusLabel;
        private ToolStripStatusLabel statusSeparatorLabel1;
        private ToolStripStatusLabel selectedCountStatusLabel;
        private ToolStripStatusLabel statusSeparatorLabel2;
        private ToolStripStatusLabel checkedCountStatusLabel;
        private ToolStripStatusLabel statusTextLabel;
        private ToolStripStatusLabel versionStatusLabel;
        private OpenFileDialog openFileDialog;
        private FolderBrowserDialog folderBrowserDialog;
    }
}
