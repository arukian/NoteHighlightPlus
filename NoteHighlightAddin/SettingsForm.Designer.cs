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
            this.pnlGroupColourPreview = new System.Windows.Forms.Panel();
            this.lblGroupColourValue = new System.Windows.Forms.Label();
            this.lblThemeStyleStatus = new System.Windows.Forms.Label();
            this.btnChangeThemeColour = new System.Windows.Forms.Button();
            this.btnSaveTheme = new System.Windows.Forms.Button();
            this.chkThemeBold = new System.Windows.Forms.CheckBox();
            this.chkThemeItalic = new System.Windows.Forms.CheckBox();
            this.lblThemeFormatting = new System.Windows.Forms.Label();
            this.grpPreview = new System.Windows.Forms.GroupBox();
            this.pnlPreview = new System.Windows.Forms.Panel();
            this.lblPreviewStatus = new System.Windows.Forms.Label();
            this.btnSaveLanguage = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudGroupId)).BeginInit();
            this.grpPreview.SuspendLayout();
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
            this.lblLanguages.Location = new System.Drawing.Point(582, 58);
            this.lblLanguages.Name = "lblLanguages";
            this.lblLanguages.Size = new System.Drawing.Size(96, 13);
            this.lblLanguages.TabIndex = 3;
            this.lblLanguages.Text = "Active Languages:";
            // 
            // lbxLanguages
            // 
            this.lbxLanguages.FormattingEnabled = true;
            this.lbxLanguages.Location = new System.Drawing.Point(585, 76);
            this.lbxLanguages.Name = "lbxLanguages";
            this.lbxLanguages.Size = new System.Drawing.Size(150, 147);
            this.lbxLanguages.TabIndex = 4;
            this.lbxLanguages.SelectedIndexChanged += new System.EventHandler(this.lbxLanguages_SelectedIndexChanged);
            // 
            // btnRemoveLanguage
            // 
            this.btnRemoveLanguage.Location = new System.Drawing.Point(585, 271);
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
            this.lblAddLanguage.Location = new System.Drawing.Point(582, 229);
            this.lblAddLanguage.Name = "lblAddLanguage";
            this.lblAddLanguage.Size = new System.Drawing.Size(105, 13);
            this.lblAddLanguage.TabIndex = 6;
            this.lblAddLanguage.Text = "Add New Language:";
            this.lblAddLanguage.Click += new System.EventHandler(this.lblAddLanguage_Click);
            // 
            // cmbAvailableLanguages
            // 
            this.cmbAvailableLanguages.FormattingEnabled = true;
            this.cmbAvailableLanguages.Location = new System.Drawing.Point(585, 245);
            this.cmbAvailableLanguages.Name = "cmbAvailableLanguages";
            this.cmbAvailableLanguages.Size = new System.Drawing.Size(150, 21);
            this.cmbAvailableLanguages.TabIndex = 7;
            // 
            // btnAddLanguage
            // 
            this.btnAddLanguage.Location = new System.Drawing.Point(585, 313);
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
            this.lbxKeywordGroups.Location = new System.Drawing.Point(24, 55);
            this.lbxKeywordGroups.Name = "lbxKeywordGroups";
            this.lbxKeywordGroups.Size = new System.Drawing.Size(171, 95);
            this.lbxKeywordGroups.TabIndex = 10;
            this.lbxKeywordGroups.SelectedIndexChanged += new System.EventHandler(this.lbxKeywordGroups_SelectedIndexChanged);
            // 
            // lbxGroupWords
            // 
            this.lbxGroupWords.FormattingEnabled = true;
            this.lbxGroupWords.Location = new System.Drawing.Point(243, 55);
            this.lbxGroupWords.Name = "lbxGroupWords";
            this.lbxGroupWords.Size = new System.Drawing.Size(119, 95);
            this.lbxGroupWords.TabIndex = 11;
            // 
            // txtGroupName
            // 
            this.txtGroupName.Location = new System.Drawing.Point(405, 42);
            this.txtGroupName.Name = "txtGroupName";
            this.txtGroupName.Size = new System.Drawing.Size(145, 20);
            this.txtGroupName.TabIndex = 12;
            this.txtGroupName.TextChanged += new System.EventHandler(this.txtGroupName_TextChanged);
            // 
            // txtGroupDescription
            // 
            this.txtGroupDescription.Location = new System.Drawing.Point(405, 102);
            this.txtGroupDescription.Name = "txtGroupDescription";
            this.txtGroupDescription.Size = new System.Drawing.Size(145, 20);
            this.txtGroupDescription.TabIndex = 13;
            this.txtGroupDescription.TextChanged += new System.EventHandler(this.txtGroupDescription_TextChanged);
            // 
            // nudGroupId
            // 
            this.nudGroupId.Enabled = false;
            this.nudGroupId.Location = new System.Drawing.Point(405, 160);
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
            this.txtNewGroupWord.Location = new System.Drawing.Point(24, 160);
            this.txtNewGroupWord.Name = "txtNewGroupWord";
            this.txtNewGroupWord.Size = new System.Drawing.Size(171, 20);
            this.txtNewGroupWord.TabIndex = 19;
            // 
            // btnAddGroupWord
            // 
            this.btnAddGroupWord.Location = new System.Drawing.Point(24, 187);
            this.btnAddGroupWord.Name = "btnAddGroupWord";
            this.btnAddGroupWord.Size = new System.Drawing.Size(75, 23);
            this.btnAddGroupWord.TabIndex = 20;
            this.btnAddGroupWord.Text = "Add Word";
            this.btnAddGroupWord.UseVisualStyleBackColor = true;
            // 
            // btnRemoveGroupWord
            // 
            this.btnRemoveGroupWord.Location = new System.Drawing.Point(120, 187);
            this.btnRemoveGroupWord.Name = "btnRemoveGroupWord";
            this.btnRemoveGroupWord.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveGroupWord.TabIndex = 21;
            this.btnRemoveGroupWord.Text = "Remove Word";
            this.btnRemoveGroupWord.UseVisualStyleBackColor = true;
            // 
            // lblGroupName
            // 
            this.lblGroupName.AutoSize = true;
            this.lblGroupName.Location = new System.Drawing.Point(402, 18);
            this.lblGroupName.Name = "lblGroupName";
            this.lblGroupName.Size = new System.Drawing.Size(70, 13);
            this.lblGroupName.TabIndex = 22;
            this.lblGroupName.Text = "Group Name:";
            // 
            // lblGroupDescription
            // 
            this.lblGroupDescription.AutoSize = true;
            this.lblGroupDescription.Location = new System.Drawing.Point(402, 76);
            this.lblGroupDescription.Name = "lblGroupDescription";
            this.lblGroupDescription.Size = new System.Drawing.Size(63, 13);
            this.lblGroupDescription.TabIndex = 23;
            this.lblGroupDescription.Text = "Description:";
            // 
            // lblGroupId
            // 
            this.lblGroupId.AutoSize = true;
            this.lblGroupId.Location = new System.Drawing.Point(402, 137);
            this.lblGroupId.Name = "lblGroupId";
            this.lblGroupId.Size = new System.Drawing.Size(53, 13);
            this.lblGroupId.TabIndex = 24;
            this.lblGroupId.Text = "Group ID:";
            // 
            // lblGroupColour
            // 
            this.lblGroupColour.AutoSize = true;
            this.lblGroupColour.Location = new System.Drawing.Point(402, 188);
            this.lblGroupColour.Name = "lblGroupColour";
            this.lblGroupColour.Size = new System.Drawing.Size(40, 13);
            this.lblGroupColour.TabIndex = 25;
            this.lblGroupColour.Text = "Style:";
            this.lblGroupColour.Visible = false;
            // 
            // lblThemeGroupName
            // 
            this.lblThemeGroupName.AutoEllipsis = true;
            this.lblThemeGroupName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblThemeGroupName.Location = new System.Drawing.Point(405, 190);
            this.lblThemeGroupName.Name = "lblThemeGroupName";
            this.lblThemeGroupName.Size = new System.Drawing.Size(170, 17);
            this.lblThemeGroupName.TabIndex = 35;
            this.lblThemeGroupName.Text = "(no group selected)";
            // 
            // lblThemeUses
            // 
            this.lblThemeUses.AutoSize = true;
            this.lblThemeUses.Location = new System.Drawing.Point(405, 211);
            this.lblThemeUses.Name = "lblThemeUses";
            this.lblThemeUses.Size = new System.Drawing.Size(34, 13);
            this.lblThemeUses.TabIndex = 36;
            this.lblThemeUses.Text = "Uses:";
            // 
            // lblThemeStyleSlot
            // 
            this.lblThemeStyleSlot.AutoSize = true;
            this.lblThemeStyleSlot.Location = new System.Drawing.Point(443, 211);
            this.lblThemeStyleSlot.Name = "lblThemeStyleSlot";
            this.lblThemeStyleSlot.Size = new System.Drawing.Size(10, 13);
            this.lblThemeStyleSlot.TabIndex = 37;
            this.lblThemeStyleSlot.Text = "-";
            // 
            // btnChangeThemeColour
            // 
            this.btnChangeThemeColour.Enabled = false;
            this.btnChangeThemeColour.Location = new System.Drawing.Point(494, 232);
            this.btnChangeThemeColour.Name = "btnChangeThemeColour";
            this.btnChangeThemeColour.Size = new System.Drawing.Size(81, 23);
            this.btnChangeThemeColour.TabIndex = 27;
            this.btnChangeThemeColour.Text = "Colour...";
            this.btnChangeThemeColour.UseVisualStyleBackColor = true;
            // 
            // btnSaveTheme
            // 
            this.btnSaveTheme.Enabled = false;
            this.btnSaveTheme.Location = new System.Drawing.Point(505, 296);
            this.btnSaveTheme.Name = "btnSaveTheme";
            this.btnSaveTheme.Size = new System.Drawing.Size(70, 23);
            this.btnSaveTheme.TabIndex = 33;
            this.btnSaveTheme.Text = "Save Theme";
            this.btnSaveTheme.UseVisualStyleBackColor = true;
            // 
            // chkThemeBold
            // 
            this.chkThemeBold.AutoSize = true;
            this.chkThemeBold.Enabled = false;
            this.chkThemeBold.Location = new System.Drawing.Point(405, 300);
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
            this.chkThemeItalic.Location = new System.Drawing.Point(456, 300);
            this.chkThemeItalic.Name = "chkThemeItalic";
            this.chkThemeItalic.Size = new System.Drawing.Size(48, 17);
            this.chkThemeItalic.TabIndex = 32;
            this.chkThemeItalic.Text = "Italic";
            this.chkThemeItalic.UseVisualStyleBackColor = true;
            // 
            // pnlGroupColourPreview
            // 
            this.pnlGroupColourPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGroupColourPreview.Location = new System.Drawing.Point(405, 233);
            this.pnlGroupColourPreview.Name = "pnlGroupColourPreview";
            this.pnlGroupColourPreview.Size = new System.Drawing.Size(48, 25);
            this.pnlGroupColourPreview.TabIndex = 28;
            // 
            // lblGroupColourValue
            // 
            this.lblGroupColourValue.AutoSize = true;
            this.lblGroupColourValue.Location = new System.Drawing.Point(461, 239);
            this.lblGroupColourValue.Name = "lblGroupColourValue";
            this.lblGroupColourValue.Size = new System.Drawing.Size(101, 13);
            this.lblGroupColourValue.TabIndex = 29;
            this.lblGroupColourValue.Text = "(no group selected)";
            // 
            // lblThemeStyleStatus
            // 
            this.lblThemeStyleStatus.AutoSize = false;
            this.lblThemeStyleStatus.Location = new System.Drawing.Point(405, 264);
            this.lblThemeStyleStatus.Name = "lblThemeStyleStatus";
            this.lblThemeStyleStatus.Size = new System.Drawing.Size(170, 17);
            this.lblThemeStyleStatus.TabIndex = 30;
            this.lblThemeStyleStatus.Text = "Theme style not loaded.";
            // 
            // lblThemeFormatting
            // 
            this.lblThemeFormatting.AutoSize = true;
            this.lblThemeFormatting.Location = new System.Drawing.Point(402, 283);
            this.lblThemeFormatting.Name = "lblThemeFormatting";
            this.lblThemeFormatting.Size = new System.Drawing.Size(73, 13);
            this.lblThemeFormatting.TabIndex = 34;
            this.lblThemeFormatting.Text = "Formatting:";
            // 
            // grpPreview
            // 
            this.grpPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPreview.Controls.Add(this.pnlPreview);
            this.grpPreview.Controls.Add(this.lblPreviewStatus);
            this.grpPreview.Location = new System.Drawing.Point(24, 325);
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
            this.btnSaveLanguage.Location = new System.Drawing.Point(243, 187);
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
            this.ClientSize = new System.Drawing.Size(760, 620);
            this.Controls.Add(this.btnSaveLanguage);
            this.Controls.Add(this.grpPreview);
            this.Controls.Add(this.lblThemeStyleSlot);
            this.Controls.Add(this.lblThemeUses);
            this.Controls.Add(this.lblThemeGroupName);
            this.Controls.Add(this.lblThemeFormatting);
            this.Controls.Add(this.chkThemeItalic);
            this.Controls.Add(this.chkThemeBold);
            this.Controls.Add(this.btnSaveTheme);
            this.Controls.Add(this.lblThemeStyleStatus);
            this.Controls.Add(this.btnChangeThemeColour);
            this.Controls.Add(this.lblGroupColourValue);
            this.Controls.Add(this.pnlGroupColourPreview);
            this.Controls.Add(this.lblGroupColour);
            this.Controls.Add(this.lblGroupId);
            this.Controls.Add(this.lblGroupDescription);
            this.Controls.Add(this.lblGroupName);
            this.Controls.Add(this.btnRemoveGroupWord);
            this.Controls.Add(this.btnAddGroupWord);
            this.Controls.Add(this.txtNewGroupWord);
            this.Controls.Add(this.nudGroupId);
            this.Controls.Add(this.txtGroupDescription);
            this.Controls.Add(this.txtGroupName);
            this.Controls.Add(this.lbxGroupWords);
            this.Controls.Add(this.lbxKeywordGroups);
            this.Controls.Add(this.btnAddLanguage);
            this.Controls.Add(this.cmbAvailableLanguages);
            this.Controls.Add(this.lblAddLanguage);
            this.Controls.Add(this.btnRemoveLanguage);
            this.Controls.Add(this.lbxLanguages);
            this.Controls.Add(this.lblLanguages);
            this.Controls.Add(this.cbShowTableBorder);
            this.Controls.Add(this.btnFont);
            this.MinimumSize = new System.Drawing.Size(776, 659);
            this.Name = "SettingsForm";
            this.Text = "SettingsForm";
            ((System.ComponentModel.ISupportInitialize)(this.nudGroupId)).EndInit();
            this.grpPreview.ResumeLayout(false);
            this.grpPreview.PerformLayout();
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
        private Panel pnlGroupColourPreview;
        private Label lblGroupColourValue;
        private Label lblThemeStyleStatus;
        private Button btnChangeThemeColour;
        private Button btnSaveTheme;
        private CheckBox chkThemeBold;
        private CheckBox chkThemeItalic;
        private Label lblThemeFormatting;
        private System.Windows.Forms.GroupBox grpPreview;
        private System.Windows.Forms.Panel pnlPreview;
        private System.Windows.Forms.Label lblPreviewStatus;
        private Button btnSaveLanguage;
    }
}