using System;
using System.Windows.Forms;

namespace NoteHighlightAddin
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.btnFont = new System.Windows.Forms.Button();
            this.cbShowTableBorder = new System.Windows.Forms.CheckBox();
            this.lblLanguages = new System.Windows.Forms.Label();
            this.lbxLanguages = new System.Windows.Forms.ListBox();
            this.btnRemoveLanguage = new System.Windows.Forms.Button();
            this.lblAddLanguage = new System.Windows.Forms.Label();
            this.cmbAvailableLanguages = new System.Windows.Forms.ComboBox();
            this.btnAddLanguage = new System.Windows.Forms.Button();
            this.lbxKeywordGroups = new System.Windows.Forms.ListBox();
            this.lbxGroupWords = new System.Windows.Forms.ListBox();
            this.txtGroupName = new System.Windows.Forms.TextBox();
            this.txtGroupDescription = new System.Windows.Forms.TextBox();
            this.nudGroupId = new System.Windows.Forms.NumericUpDown();
            this.txtNewGroupWord = new System.Windows.Forms.TextBox();
            this.btnAddGroupWord = new System.Windows.Forms.Button();
            this.btnRemoveGroupWord = new System.Windows.Forms.Button();
            this.lblGroupName = new System.Windows.Forms.Label();
            this.lblGroupDescription = new System.Windows.Forms.Label();
            this.lblGroupId = new System.Windows.Forms.Label();
            this.lblGroupColour = new System.Windows.Forms.Label();
            this.lblThemeGroupName = new System.Windows.Forms.Label();
            this.lblThemeUses = new System.Windows.Forms.Label();
            this.lblThemeStyleSlot = new System.Windows.Forms.Label();
            this.lblThemeSelector = new System.Windows.Forms.Label();
            this.cmbThemes = new System.Windows.Forms.ComboBox();
            this.pnlGroupColourPreview = new System.Windows.Forms.Panel();
            this.lblGroupColourValue = new System.Windows.Forms.Label();
            this.lblThemeStyleStatus = new System.Windows.Forms.Label();
            this.btnChangeThemeColour = new System.Windows.Forms.Button();
            this.btnSaveTheme = new System.Windows.Forms.Button();
            this.btnNewTheme = new System.Windows.Forms.Button();
            this.btnDuplicateTheme = new System.Windows.Forms.Button();
            this.btnRenameTheme = new System.Windows.Forms.Button();
            this.btnDeleteTheme = new System.Windows.Forms.Button();
            this.btnResetTheme = new System.Windows.Forms.Button();
            this.btnExportConfiguration = new System.Windows.Forms.Button();
            this.btnImportConfiguration = new System.Windows.Forms.Button();
            this.chkThemeBold = new System.Windows.Forms.CheckBox();
            this.chkThemeItalic = new System.Windows.Forms.CheckBox();
            this.lblThemeFormatting = new System.Windows.Forms.Label();
            this.lblThemeStyleTarget = new System.Windows.Forms.Label();
            this.cmbThemeStyleTarget = new System.Windows.Forms.ComboBox();
            this.grpPreview = new System.Windows.Forms.GroupBox();
            this.pnlPreview = new System.Windows.Forms.Panel();
            this.lblPreviewStatus = new System.Windows.Forms.Label();
            this.btnSaveLanguage = new System.Windows.Forms.Button();
            this.tabSettings = new System.Windows.Forms.TabControl();
            this.tabLanguageGroups = new System.Windows.Forms.TabPage();
            this.tabThemeEditor = new System.Windows.Forms.TabPage();
            this.grpThemeManagement = new System.Windows.Forms.GroupBox();
            this.grpThemeStyleEditor = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudGroupId)).BeginInit();
            this.grpPreview.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.tabLanguageGroups.SuspendLayout();
            this.tabThemeEditor.SuspendLayout();
            this.grpThemeManagement.SuspendLayout();
            this.grpThemeStyleEditor.SuspendLayout();
            this.SuspendLayout();
            // 
            // fontDialog1
            // 
            this.fontDialog1.AllowScriptChange = false;
            this.fontDialog1.AllowSimulations = false;
            this.fontDialog1.AllowVerticalFonts = false;
            this.fontDialog1.FontMustExist = true;
            this.fontDialog1.ShowEffects = false;
            // 
            // btnFont
            // 
            this.btnFont.Location = new System.Drawing.Point(24, 18);
            this.btnFont.Name = "btnFont";
            this.btnFont.Size = new System.Drawing.Size(338, 23);
            this.btnFont.TabIndex = 1;
            this.btnFont.Text = "Font";
            this.btnFont.UseVisualStyleBackColor = true;
            this.btnFont.Click += new System.EventHandler(this.BtnFont_Click);
            // 
            // cbShowTableBorder
            // 
            this.cbShowTableBorder.AutoSize = true;
            this.cbShowTableBorder.Location = new System.Drawing.Point(585, 26);
            this.cbShowTableBorder.Name = "cbShowTableBorder";
            this.cbShowTableBorder.Size = new System.Drawing.Size(117, 17);
            this.cbShowTableBorder.TabIndex = 2;
            this.cbShowTableBorder.Text = "Show Table Border";
            this.cbShowTableBorder.UseVisualStyleBackColor = true;
            this.cbShowTableBorder.CheckedChanged += new System.EventHandler(this.ChShowTableBorder_CheckedChanged);
            // 
            // lblLanguages
            // 
            this.lblLanguages.AutoSize = true;
            this.lblLanguages.Location = new System.Drawing.Point(568, 14);
            this.lblLanguages.Name = "lblLanguages";
            this.lblLanguages.Size = new System.Drawing.Size(96, 13);
            this.lblLanguages.TabIndex = 3;
            this.lblLanguages.Text = "Active Languages:";
            // 
            // lbxLanguages
            // 
            this.lbxLanguages.FormattingEnabled = true;
            this.lbxLanguages.Location = new System.Drawing.Point(571, 32);
            this.lbxLanguages.Name = "lbxLanguages";
            this.lbxLanguages.Size = new System.Drawing.Size(150, 147);
            this.lbxLanguages.TabIndex = 4;
            this.lbxLanguages.SelectedIndexChanged += new System.EventHandler(this.lbxLanguages_SelectedIndexChanged);
            // 
            // btnRemoveLanguage
            // 
            this.btnRemoveLanguage.Location = new System.Drawing.Point(571, 227);
            this.btnRemoveLanguage.Name = "btnRemoveLanguage";
            this.btnRemoveLanguage.Size = new System.Drawing.Size(150, 36);
            this.btnRemoveLanguage.TabIndex = 5;
            this.btnRemoveLanguage.Text = "Remove Selected Language";
            this.btnRemoveLanguage.UseVisualStyleBackColor = true;
            this.btnRemoveLanguage.Click += new System.EventHandler(this.BtnRemoveLanguage_Click);
            // 
            // lblAddLanguage
            // 
            this.lblAddLanguage.AutoSize = true;
            this.lblAddLanguage.Location = new System.Drawing.Point(568, 185);
            this.lblAddLanguage.Name = "lblAddLanguage";
            this.lblAddLanguage.Size = new System.Drawing.Size(105, 13);
            this.lblAddLanguage.TabIndex = 6;
            this.lblAddLanguage.Text = "Add New Language:";
            this.lblAddLanguage.Click += new System.EventHandler(this.lblAddLanguage_Click);
            // 
            // cmbAvailableLanguages
            // 
            this.cmbAvailableLanguages.FormattingEnabled = true;
            this.cmbAvailableLanguages.Location = new System.Drawing.Point(571, 201);
            this.cmbAvailableLanguages.Name = "cmbAvailableLanguages";
            this.cmbAvailableLanguages.Size = new System.Drawing.Size(150, 21);
            this.cmbAvailableLanguages.TabIndex = 7;
            // 
            // btnAddLanguage
            // 
            this.btnAddLanguage.Location = new System.Drawing.Point(571, 269);
            this.btnAddLanguage.Name = "btnAddLanguage";
            this.btnAddLanguage.Size = new System.Drawing.Size(150, 37);
            this.btnAddLanguage.TabIndex = 8;
            this.btnAddLanguage.Text = "Add Language to Ribbon";
            this.btnAddLanguage.UseVisualStyleBackColor = true;
            this.btnAddLanguage.Click += new System.EventHandler(this.BtnAddLanguage_Click);
            // 
            // lbxKeywordGroups
            // 
            this.lbxKeywordGroups.FormattingEnabled = true;
            this.lbxKeywordGroups.Location = new System.Drawing.Point(10, 14);
            this.lbxKeywordGroups.Name = "lbxKeywordGroups";
            this.lbxKeywordGroups.Size = new System.Drawing.Size(171, 95);
            this.lbxKeywordGroups.TabIndex = 10;
            this.lbxKeywordGroups.SelectedIndexChanged += new System.EventHandler(this.lbxKeywordGroups_SelectedIndexChanged);
            // 
            // lbxGroupWords
            // 
            this.lbxGroupWords.FormattingEnabled = true;
            this.lbxGroupWords.Location = new System.Drawing.Point(229, 14);
            this.lbxGroupWords.Name = "lbxGroupWords";
            this.lbxGroupWords.Size = new System.Drawing.Size(119, 95);
            this.lbxGroupWords.TabIndex = 11;
            // 
            // txtGroupName
            // 
            this.txtGroupName.Location = new System.Drawing.Point(391, 38);
            this.txtGroupName.Name = "txtGroupName";
            this.txtGroupName.Size = new System.Drawing.Size(145, 20);
            this.txtGroupName.TabIndex = 12;
            this.txtGroupName.TextChanged += new System.EventHandler(this.txtGroupName_TextChanged);
            // 
            // txtGroupDescription
            // 
            this.txtGroupDescription.Location = new System.Drawing.Point(391, 98);
            this.txtGroupDescription.Name = "txtGroupDescription";
            this.txtGroupDescription.Size = new System.Drawing.Size(145, 20);
            this.txtGroupDescription.TabIndex = 13;
            this.txtGroupDescription.TextChanged += new System.EventHandler(this.txtGroupDescription_TextChanged);
            // 
            // nudGroupId
            // 
            this.nudGroupId.Enabled = false;
            this.nudGroupId.Location = new System.Drawing.Point(391, 156);
            this.nudGroupId.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudGroupId.Name = "nudGroupId";
            this.nudGroupId.Size = new System.Drawing.Size(145, 20);
            this.nudGroupId.TabIndex = 14;
            this.nudGroupId.ValueChanged += new System.EventHandler(this.nudGroupId_ValueChanged);
            // 
            // txtNewGroupWord
            // 
            this.txtNewGroupWord.Location = new System.Drawing.Point(10, 119);
            this.txtNewGroupWord.Name = "txtNewGroupWord";
            this.txtNewGroupWord.Size = new System.Drawing.Size(171, 20);
            this.txtNewGroupWord.TabIndex = 19;
            // 
            // btnAddGroupWord
            // 
            this.btnAddGroupWord.Location = new System.Drawing.Point(10, 146);
            this.btnAddGroupWord.Name = "btnAddGroupWord";
            this.btnAddGroupWord.Size = new System.Drawing.Size(75, 23);
            this.btnAddGroupWord.TabIndex = 20;
            this.btnAddGroupWord.Text = "Add Word";
            this.btnAddGroupWord.UseVisualStyleBackColor = true;
            // 
            // btnRemoveGroupWord
            // 
            this.btnRemoveGroupWord.Location = new System.Drawing.Point(106, 146);
            this.btnRemoveGroupWord.Name = "btnRemoveGroupWord";
            this.btnRemoveGroupWord.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveGroupWord.TabIndex = 21;
            this.btnRemoveGroupWord.Text = "Remove Word";
            this.btnRemoveGroupWord.UseVisualStyleBackColor = true;
            // 
            // lblGroupName
            // 
            this.lblGroupName.AutoSize = true;
            this.lblGroupName.Location = new System.Drawing.Point(388, 14);
            this.lblGroupName.Name = "lblGroupName";
            this.lblGroupName.Size = new System.Drawing.Size(70, 13);
            this.lblGroupName.TabIndex = 22;
            this.lblGroupName.Text = "Group Name:";
            // 
            // lblGroupDescription
            // 
            this.lblGroupDescription.AutoSize = true;
            this.lblGroupDescription.Location = new System.Drawing.Point(388, 72);
            this.lblGroupDescription.Name = "lblGroupDescription";
            this.lblGroupDescription.Size = new System.Drawing.Size(63, 13);
            this.lblGroupDescription.TabIndex = 23;
            this.lblGroupDescription.Text = "Description:";
            // 
            // lblGroupId
            // 
            this.lblGroupId.AutoSize = true;
            this.lblGroupId.Location = new System.Drawing.Point(388, 133);
            this.lblGroupId.Name = "lblGroupId";
            this.lblGroupId.Size = new System.Drawing.Size(53, 13);
            this.lblGroupId.TabIndex = 24;
            this.lblGroupId.Text = "Group ID:";
            // 
            // lblGroupColour
            // 
            this.lblGroupColour.AutoSize = true;
            this.lblGroupColour.Location = new System.Drawing.Point(16, 112);
            this.lblGroupColour.Name = "lblGroupColour";
            this.lblGroupColour.Size = new System.Drawing.Size(40, 13);
            this.lblGroupColour.TabIndex = 25;
            this.lblGroupColour.Text = "Colour:";
            this.lblGroupColour.Visible = true;
            // 
            // lblThemeStyleTarget
            // 
            this.lblThemeStyleTarget.AutoSize = true;
            this.lblThemeStyleTarget.Location = new System.Drawing.Point(16, 26);
            this.lblThemeStyleTarget.Name = "lblThemeStyleTarget";
            this.lblThemeStyleTarget.Size = new System.Drawing.Size(66, 13);
            this.lblThemeStyleTarget.TabIndex = 44;
            this.lblThemeStyleTarget.Text = "Theme Style:";
            // 
            // cmbThemeStyleTarget
            // 
            this.cmbThemeStyleTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbThemeStyleTarget.Enabled = false;
            this.cmbThemeStyleTarget.FormattingEnabled = true;
            this.cmbThemeStyleTarget.Location = new System.Drawing.Point(16, 43);
            this.cmbThemeStyleTarget.Name = "cmbThemeStyleTarget";
            this.cmbThemeStyleTarget.Size = new System.Drawing.Size(410, 21);
            this.cmbThemeStyleTarget.TabIndex = 45;
            // 
            // lblThemeGroupName
            // 
            this.lblThemeGroupName.AutoEllipsis = true;
            this.lblThemeGroupName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblThemeGroupName.Location = new System.Drawing.Point(16, 78);
            this.lblThemeGroupName.Name = "lblThemeGroupName";
            this.lblThemeGroupName.Size = new System.Drawing.Size(170, 17);
            this.lblThemeGroupName.TabIndex = 35;
            this.lblThemeGroupName.Text = "(no group selected)";
            // 
            // lblThemeUses
            // 
            this.lblThemeUses.AutoSize = true;
            this.lblThemeUses.Location = new System.Drawing.Point(16, 100);
            this.lblThemeUses.Name = "lblThemeUses";
            this.lblThemeUses.Size = new System.Drawing.Size(34, 13);
            this.lblThemeUses.TabIndex = 36;
            this.lblThemeUses.Text = "Uses:";
            // 
            // lblThemeStyleSlot
            // 
            this.lblThemeStyleSlot.AutoEllipsis = true;
            this.lblThemeStyleSlot.AutoSize = false;
            this.lblThemeStyleSlot.Location = new System.Drawing.Point(54, 100);
            this.lblThemeStyleSlot.Name = "lblThemeStyleSlot";
            this.lblThemeStyleSlot.Size = new System.Drawing.Size(350, 16);
            this.lblThemeStyleSlot.TabIndex = 37;
            this.lblThemeStyleSlot.Text = "-";
            // 
            // lblThemeSelector
            // 
            this.lblThemeSelector.AutoSize = true;
            this.lblThemeSelector.Location = new System.Drawing.Point(14, 28);
            this.lblThemeSelector.Name = "lblThemeSelector";
            this.lblThemeSelector.Size = new System.Drawing.Size(43, 13);
            this.lblThemeSelector.TabIndex = 38;
            this.lblThemeSelector.Text = "Theme:";
            // 
            // cmbThemes
            // 
            this.cmbThemes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbThemes.FormattingEnabled = true;
            this.cmbThemes.Location = new System.Drawing.Point(63, 24);
            this.cmbThemes.Name = "cmbThemes";
            this.cmbThemes.Size = new System.Drawing.Size(125, 21);
            this.cmbThemes.TabIndex = 39;
            // 
            // btnNewTheme
            // 
            this.btnNewTheme.Enabled = false;
            this.btnNewTheme.Location = new System.Drawing.Point(14, 58);
            this.btnNewTheme.Name = "btnNewTheme";
            this.btnNewTheme.Size = new System.Drawing.Size(82, 23);
            this.btnNewTheme.TabIndex = 40;
            this.btnNewTheme.Text = "New...";
            this.btnNewTheme.UseVisualStyleBackColor = true;
            // 
            // btnDuplicateTheme
            // 
            this.btnDuplicateTheme.Enabled = false;
            this.btnDuplicateTheme.Location = new System.Drawing.Point(102, 58);
            this.btnDuplicateTheme.Name = "btnDuplicateTheme";
            this.btnDuplicateTheme.Size = new System.Drawing.Size(82, 23);
            this.btnDuplicateTheme.TabIndex = 41;
            this.btnDuplicateTheme.Text = "Duplicate...";
            this.btnDuplicateTheme.UseVisualStyleBackColor = true;
            // 
            // btnRenameTheme
            // 
            this.btnRenameTheme.Enabled = false;
            this.btnRenameTheme.Location = new System.Drawing.Point(14, 87);
            this.btnRenameTheme.Name = "btnRenameTheme";
            this.btnRenameTheme.Size = new System.Drawing.Size(82, 23);
            this.btnRenameTheme.TabIndex = 42;
            this.btnRenameTheme.Text = "Rename";
            this.btnRenameTheme.UseVisualStyleBackColor = true;
            // 
            // btnDeleteTheme
            // 
            this.btnDeleteTheme.Enabled = false;
            this.btnDeleteTheme.Location = new System.Drawing.Point(102, 87);
            this.btnDeleteTheme.Name = "btnDeleteTheme";
            this.btnDeleteTheme.Size = new System.Drawing.Size(82, 23);
            this.btnDeleteTheme.TabIndex = 43;
            this.btnDeleteTheme.Text = "Delete";
            this.btnDeleteTheme.UseVisualStyleBackColor = true;
            // 
            // btnExportConfiguration
            // 
            this.btnExportConfiguration.Location = new System.Drawing.Point(368, 16);
            this.btnExportConfiguration.Name = "btnExportConfiguration";
            this.btnExportConfiguration.Size = new System.Drawing.Size(102, 23);
            this.btnExportConfiguration.TabIndex = 47;
            this.btnExportConfiguration.Text = "Export...";
            this.btnExportConfiguration.UseVisualStyleBackColor = true;
            // 
            // 
            // btnImportConfiguration
            // 
            this.btnImportConfiguration.Location = new System.Drawing.Point(476, 16);
            this.btnImportConfiguration.Name = "btnImportConfiguration";
            this.btnImportConfiguration.Size = new System.Drawing.Size(102, 23);
            this.btnImportConfiguration.TabIndex = 48;
            this.btnImportConfiguration.Text = "Import...";
            this.btnImportConfiguration.UseVisualStyleBackColor = true;
            // 
            // 
            // btnResetTheme
            // 
            this.btnResetTheme.Enabled = false;
            this.btnResetTheme.Location = new System.Drawing.Point(14, 116);
            this.btnResetTheme.Name = "btnResetTheme";
            this.btnResetTheme.Size = new System.Drawing.Size(170, 23);
            this.btnResetTheme.TabIndex = 44;
            this.btnResetTheme.Text = "Reset Theme";
            this.btnResetTheme.UseVisualStyleBackColor = true;
            // btnChangeThemeColour
            // 
            this.btnChangeThemeColour.Enabled = false;
            this.btnChangeThemeColour.Location = new System.Drawing.Point(162, 127);
            this.btnChangeThemeColour.Name = "btnChangeThemeColour";
            this.btnChangeThemeColour.Size = new System.Drawing.Size(81, 23);
            this.btnChangeThemeColour.TabIndex = 27;
            this.btnChangeThemeColour.Text = "Colour...";
            this.btnChangeThemeColour.UseVisualStyleBackColor = true;
            // 
            // btnSaveTheme
            // 
            this.btnSaveTheme.Enabled = false;
            this.btnSaveTheme.Location = new System.Drawing.Point(330, 181);
            this.btnSaveTheme.Name = "btnSaveTheme";
            this.btnSaveTheme.Size = new System.Drawing.Size(95, 23);
            this.btnSaveTheme.TabIndex = 33;
            this.btnSaveTheme.Text = "Save Theme";
            this.btnSaveTheme.UseVisualStyleBackColor = true;
            // 
            // chkThemeBold
            // 
            this.chkThemeBold.AutoSize = true;
            this.chkThemeBold.Enabled = false;
            this.chkThemeBold.Location = new System.Drawing.Point(19, 185);
            this.chkThemeBold.Name = "chkThemeBold";
            this.chkThemeBold.Size = new System.Drawing.Size(47, 17);
            this.chkThemeBold.TabIndex = 31;
            this.chkThemeBold.Text = "Bold";
            this.chkThemeBold.UseVisualStyleBackColor = true;
            // 
            // chkThemeItalic
            // 
            this.chkThemeItalic.AutoSize = true;
            this.chkThemeItalic.Enabled = false;
            this.chkThemeItalic.Location = new System.Drawing.Point(74, 185);
            this.chkThemeItalic.Name = "chkThemeItalic";
            this.chkThemeItalic.Size = new System.Drawing.Size(48, 17);
            this.chkThemeItalic.TabIndex = 32;
            this.chkThemeItalic.Text = "Italic";
            this.chkThemeItalic.UseVisualStyleBackColor = true;
            // 
            // pnlGroupColourPreview
            // 
            this.pnlGroupColourPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGroupColourPreview.Location = new System.Drawing.Point(16, 128);
            this.pnlGroupColourPreview.Name = "pnlGroupColourPreview";
            this.pnlGroupColourPreview.Size = new System.Drawing.Size(48, 25);
            this.pnlGroupColourPreview.TabIndex = 28;
            // 
            // lblGroupColourValue
            // 
            this.lblGroupColourValue.AutoSize = true;
            this.lblGroupColourValue.Location = new System.Drawing.Point(72, 134);
            this.lblGroupColourValue.Name = "lblGroupColourValue";
            this.lblGroupColourValue.Size = new System.Drawing.Size(101, 13);
            this.lblGroupColourValue.TabIndex = 29;
            this.lblGroupColourValue.Text = "(no group selected)";
            // 
            // lblThemeStyleStatus
            // 
            this.lblThemeStyleStatus.AutoSize = false;
            this.lblThemeStyleStatus.Location = new System.Drawing.Point(240, 150);
            this.lblThemeStyleStatus.Name = "lblThemeStyleStatus";
            this.lblThemeStyleStatus.Visible = false;
            this.lblThemeStyleStatus.Size = new System.Drawing.Size(170, 17);
            this.lblThemeStyleStatus.TabIndex = 30;
            this.lblThemeStyleStatus.Text = "Theme style not loaded.";
            // 
            // lblThemeFormatting
            // 
            this.lblThemeFormatting.AutoSize = true;
            this.lblThemeFormatting.Location = new System.Drawing.Point(16, 166);
            this.lblThemeFormatting.Name = "lblThemeFormatting";
            this.lblThemeFormatting.Size = new System.Drawing.Size(73, 13);
            this.lblThemeFormatting.TabIndex = 34;
            this.lblThemeFormatting.Text = "Formatting:";
            // 
            // tabSettings
            // 
            this.tabSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabSettings.Controls.Add(this.tabLanguageGroups);
            this.tabSettings.Controls.Add(this.tabThemeEditor);
            this.tabSettings.Location = new System.Drawing.Point(16, 50);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.SelectedIndex = 0;
            this.tabSettings.Size = new System.Drawing.Size(728, 352);
            this.tabSettings.TabIndex = 46;
            // 
            // tabLanguageGroups
            // 
            this.tabLanguageGroups.Controls.Add(this.btnSaveLanguage);
            this.tabLanguageGroups.Controls.Add(this.lblGroupId);
            this.tabLanguageGroups.Controls.Add(this.lblGroupDescription);
            this.tabLanguageGroups.Controls.Add(this.lblGroupName);
            this.tabLanguageGroups.Controls.Add(this.btnRemoveGroupWord);
            this.tabLanguageGroups.Controls.Add(this.btnAddGroupWord);
            this.tabLanguageGroups.Controls.Add(this.txtNewGroupWord);
            this.tabLanguageGroups.Controls.Add(this.nudGroupId);
            this.tabLanguageGroups.Controls.Add(this.txtGroupDescription);
            this.tabLanguageGroups.Controls.Add(this.txtGroupName);
            this.tabLanguageGroups.Controls.Add(this.lbxGroupWords);
            this.tabLanguageGroups.Controls.Add(this.lbxKeywordGroups);
            this.tabLanguageGroups.Controls.Add(this.btnAddLanguage);
            this.tabLanguageGroups.Controls.Add(this.cmbAvailableLanguages);
            this.tabLanguageGroups.Controls.Add(this.lblAddLanguage);
            this.tabLanguageGroups.Controls.Add(this.btnRemoveLanguage);
            this.tabLanguageGroups.Controls.Add(this.lbxLanguages);
            this.tabLanguageGroups.Controls.Add(this.lblLanguages);
            this.tabLanguageGroups.Location = new System.Drawing.Point(4, 22);
            this.tabLanguageGroups.Name = "tabLanguageGroups";
            this.tabLanguageGroups.Padding = new System.Windows.Forms.Padding(3);
            this.tabLanguageGroups.Size = new System.Drawing.Size(720, 326);
            this.tabLanguageGroups.TabIndex = 0;
            this.tabLanguageGroups.Text = "Language && Groups";
            this.tabLanguageGroups.UseVisualStyleBackColor = true;
            // 
            // tabThemeEditor
            // 
            this.tabThemeEditor.Controls.Add(this.grpThemeStyleEditor);
            this.tabThemeEditor.Controls.Add(this.grpThemeManagement);
            this.tabThemeEditor.Location = new System.Drawing.Point(4, 22);
            this.tabThemeEditor.Name = "tabThemeEditor";
            this.tabThemeEditor.Padding = new System.Windows.Forms.Padding(3);
            this.tabThemeEditor.Size = new System.Drawing.Size(720, 326);
            this.tabThemeEditor.TabIndex = 1;
            this.tabThemeEditor.Text = "Theme Editor";
            this.tabThemeEditor.UseVisualStyleBackColor = true;
            // grpThemeManagement
            // 
            this.grpThemeManagement.Controls.Add(this.btnResetTheme);
            this.grpThemeManagement.Controls.Add(this.cmbThemes);
            this.grpThemeManagement.Controls.Add(this.lblThemeSelector);
            this.grpThemeManagement.Controls.Add(this.btnDeleteTheme);
            this.grpThemeManagement.Controls.Add(this.btnRenameTheme);
            this.grpThemeManagement.Controls.Add(this.btnDuplicateTheme);
            this.grpThemeManagement.Controls.Add(this.btnNewTheme);
            this.grpThemeManagement.Location = new System.Drawing.Point(18, 16);
            this.grpThemeManagement.Name = "grpThemeManagement";
            this.grpThemeManagement.Size = new System.Drawing.Size(205, 154);
            this.grpThemeManagement.TabIndex = 0;
            this.grpThemeManagement.TabStop = false;
            this.grpThemeManagement.Text = "Theme";
            // 
            // grpThemeStyleEditor
            // 
            this.grpThemeStyleEditor.Controls.Add(this.cmbThemeStyleTarget);
            this.grpThemeStyleEditor.Controls.Add(this.lblThemeStyleTarget);
            this.grpThemeStyleEditor.Controls.Add(this.lblThemeStyleSlot);
            this.grpThemeStyleEditor.Controls.Add(this.lblThemeUses);
            this.grpThemeStyleEditor.Controls.Add(this.lblThemeGroupName);
            this.grpThemeStyleEditor.Controls.Add(this.lblGroupColour);
            this.grpThemeStyleEditor.Controls.Add(this.lblThemeFormatting);
            this.grpThemeStyleEditor.Controls.Add(this.chkThemeItalic);
            this.grpThemeStyleEditor.Controls.Add(this.chkThemeBold);
            this.grpThemeStyleEditor.Controls.Add(this.btnSaveTheme);
            this.grpThemeStyleEditor.Controls.Add(this.btnChangeThemeColour);
            this.grpThemeStyleEditor.Controls.Add(this.lblGroupColourValue);
            this.grpThemeStyleEditor.Controls.Add(this.pnlGroupColourPreview);
            this.grpThemeStyleEditor.Location = new System.Drawing.Point(240, 16);
            this.grpThemeStyleEditor.Name = "grpThemeStyleEditor";
            this.grpThemeStyleEditor.Size = new System.Drawing.Size(455, 250);
            this.grpThemeStyleEditor.TabIndex = 1;
            this.grpThemeStyleEditor.TabStop = false;
            this.grpThemeStyleEditor.Text = "Style";
            // 
            // 
            // grpPreview
            // 
            this.grpPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPreview.Controls.Add(this.pnlPreview);
            this.grpPreview.Controls.Add(this.lblPreviewStatus);
            this.grpPreview.Location = new System.Drawing.Point(24, 410);
            this.grpPreview.Name = "grpPreview";
            this.grpPreview.Size = new System.Drawing.Size(711, 270);
            this.grpPreview.TabIndex = 31;
            this.grpPreview.TabStop = false;
            this.grpPreview.Text = "Preview";
            // 
            // pnlPreview
            // 
            this.pnlPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPreview.Location = new System.Drawing.Point(12, 48);
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.Size = new System.Drawing.Size(687, 210);
            this.pnlPreview.TabIndex = 0;
            // 
            // lblPreviewStatus
            // 
            this.lblPreviewStatus.AutoSize = true;
            this.lblPreviewStatus.Location = new System.Drawing.Point(12, 25);
            this.lblPreviewStatus.Name = "lblPreviewStatus";
            this.lblPreviewStatus.Size = new System.Drawing.Size(98, 13);
            this.lblPreviewStatus.TabIndex = 1;
            this.lblPreviewStatus.Text = "Preview not loaded";
            // 
            // btnSaveLanguage
            // 
            this.btnSaveLanguage.Location = new System.Drawing.Point(229, 146);
            this.btnSaveLanguage.Name = "btnSaveLanguage";
            this.btnSaveLanguage.Size = new System.Drawing.Size(75, 23);
            this.btnSaveLanguage.TabIndex = 32;
            this.btnSaveLanguage.Text = "Save";
            this.btnSaveLanguage.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(760, 705);
            this.Controls.Add(this.btnImportConfiguration);
            this.Controls.Add(this.btnExportConfiguration);
            this.Controls.Add(this.grpPreview);
            this.Controls.Add(this.tabSettings);
            this.Controls.Add(this.cbShowTableBorder);
            this.Controls.Add(this.btnFont);
            this.MinimumSize = new System.Drawing.Size(776, 744);
            this.Name = "SettingsForm";
            this.Text = "SettingsForm";
            ((System.ComponentModel.ISupportInitialize)(this.nudGroupId)).EndInit();
            this.grpPreview.ResumeLayout(false);
            this.grpPreview.PerformLayout();
            this.tabLanguageGroups.ResumeLayout(false);
            this.tabLanguageGroups.PerformLayout();
            this.grpThemeManagement.ResumeLayout(false);
            this.grpThemeManagement.PerformLayout();
            this.grpThemeStyleEditor.ResumeLayout(false);
            this.grpThemeStyleEditor.PerformLayout();
            this.tabThemeEditor.ResumeLayout(false);
            this.tabSettings.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FontDialog fontDialog1;
        private System.Windows.Forms.Button btnFont;
        private System.Windows.Forms.CheckBox cbShowTableBorder;
        private System.Windows.Forms.Label lblLanguages;
        private System.Windows.Forms.ListBox lbxLanguages;
        private System.Windows.Forms.Button btnRemoveLanguage;
        private System.Windows.Forms.Label lblAddLanguage;
        private System.Windows.Forms.ComboBox cmbAvailableLanguages;
        private System.Windows.Forms.Button btnAddLanguage;
        private ListBox lbxKeywordGroups;
        private ListBox lbxGroupWords;
        private TextBox txtGroupName;
        private TextBox txtGroupDescription;
        private NumericUpDown nudGroupId;
        private TextBox txtNewGroupWord;
        private Button btnAddGroupWord;
        private Button btnRemoveGroupWord;
        private Label lblGroupName;
        private Label lblGroupDescription;
        private Label lblGroupId;
        private Label lblGroupColour;
        private Label lblThemeGroupName;
        private Label lblThemeUses;
        private Label lblThemeStyleSlot;
        private Label lblThemeSelector;
        private ComboBox cmbThemes;
        private Panel pnlGroupColourPreview;
        private Label lblGroupColourValue;
        private Label lblThemeStyleStatus;
        private Button btnChangeThemeColour;
        private Button btnSaveTheme;
        private Button btnNewTheme;
        private Button btnDuplicateTheme;
        private Button btnRenameTheme;
        private Button btnDeleteTheme;
        private Button btnResetTheme;
        private Button btnExportConfiguration;
        private Button btnImportConfiguration;
        private CheckBox chkThemeBold;
        private CheckBox chkThemeItalic;
        private Label lblThemeFormatting;
        private Label lblThemeStyleTarget;
        private ComboBox cmbThemeStyleTarget;
        private System.Windows.Forms.GroupBox grpPreview;
        private System.Windows.Forms.Panel pnlPreview;
        private System.Windows.Forms.Label lblPreviewStatus;
        private Button btnSaveLanguage;
        private TabControl tabSettings;
        private TabPage tabLanguageGroups;
        private TabPage tabThemeEditor;
        private GroupBox grpThemeManagement;
        private GroupBox grpThemeStyleEditor;
    }
}
