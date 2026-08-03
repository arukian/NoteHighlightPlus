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
            this.chkGroupVisible = new System.Windows.Forms.CheckBox();
            this.chkGroupBold = new System.Windows.Forms.CheckBox();
            this.chkGroupItalic = new System.Windows.Forms.CheckBox();
            this.txtNewGroupWord = new System.Windows.Forms.TextBox();
            this.btnAddGroupWord = new System.Windows.Forms.Button();
            this.btnRemoveGroupWord = new System.Windows.Forms.Button();
            this.lblGroupName = new System.Windows.Forms.Label();
            this.lblGroupDescription = new System.Windows.Forms.Label();
            this.lblGroupId = new System.Windows.Forms.Label();
            this.lblGroupColour = new System.Windows.Forms.Label();
            this.cmbGroupColour = new System.Windows.Forms.ComboBox();
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
            // chkGroupVisible
            // 
            this.chkGroupVisible.AutoSize = true;
            this.chkGroupVisible.Location = new System.Drawing.Point(405, 248);
            this.chkGroupVisible.Name = "chkGroupVisible";
            this.chkGroupVisible.Size = new System.Drawing.Size(56, 17);
            this.chkGroupVisible.TabIndex = 15;
            this.chkGroupVisible.Text = "Visible";
            this.chkGroupVisible.UseVisualStyleBackColor = true;
            this.chkGroupVisible.CheckedChanged += new System.EventHandler(this.chkGroupVisible_CheckedChanged);
            // 
            // chkGroupBold
            // 
            this.chkGroupBold.AutoSize = true;
            this.chkGroupBold.Location = new System.Drawing.Point(486, 248);
            this.chkGroupBold.Name = "chkGroupBold";
            this.chkGroupBold.Size = new System.Drawing.Size(47, 17);
            this.chkGroupBold.TabIndex = 16;
            this.chkGroupBold.Text = "Bold";
            this.chkGroupBold.UseVisualStyleBackColor = true;
            this.chkGroupBold.CheckedChanged += new System.EventHandler(this.chkGroupBold_CheckedChanged);
            // 
            // chkGroupItalic
            // 
            this.chkGroupItalic.AutoSize = true;
            this.chkGroupItalic.Location = new System.Drawing.Point(405, 271);
            this.chkGroupItalic.Name = "chkGroupItalic";
            this.chkGroupItalic.Size = new System.Drawing.Size(48, 17);
            this.chkGroupItalic.TabIndex = 17;
            this.chkGroupItalic.Text = "Italic";
            this.chkGroupItalic.UseVisualStyleBackColor = true;
            this.chkGroupItalic.CheckedChanged += new System.EventHandler(this.chkGroupItalic_CheckedChanged);
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
            this.lblGroupColour.Location = new System.Drawing.Point(402, 194);
            this.lblGroupColour.Name = "lblGroupColour";
            this.lblGroupColour.Size = new System.Drawing.Size(40, 13);
            this.lblGroupColour.TabIndex = 25;
            this.lblGroupColour.Text = "Colour:";
            // 
            // cmbGroupColour
            // 
            this.cmbGroupColour.Enabled = false;
            this.cmbGroupColour.FormattingEnabled = true;
            this.cmbGroupColour.Items.AddRange(new object[] {
            "Keywords1",
            "Keywords2",
            "Keywords3",
            "Keywords4",
            "Keywords5",
            "Keywords6"});
            this.cmbGroupColour.Location = new System.Drawing.Point(405, 210);
            this.cmbGroupColour.Name = "cmbGroupColour";
            this.cmbGroupColour.Size = new System.Drawing.Size(145, 21);
            this.cmbGroupColour.TabIndex = 26;
            this.cmbGroupColour.TextChanged += new System.EventHandler(this.cmbGroupColour_TextChanged);
            // 
            // grpPreview
            // 
            this.grpPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPreview.Controls.Add(this.pnlPreview);
            this.grpPreview.Controls.Add(this.lblPreviewStatus);
            this.grpPreview.Location = new System.Drawing.Point(24, 300);
            this.grpPreview.Name = "grpPreview";
            this.grpPreview.Size = new System.Drawing.Size(711, 260);
            this.grpPreview.TabIndex = 30;
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
            this.pnlPreview.Size = new System.Drawing.Size(687, 200);
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
            this.btnSaveLanguage.TabIndex = 31;
            this.btnSaveLanguage.Text = "Save";
            this.btnSaveLanguage.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(760, 585);
            this.Controls.Add(this.btnSaveLanguage);
            this.Controls.Add(this.grpPreview);
            this.Controls.Add(this.cmbGroupColour);
            this.Controls.Add(this.lblGroupColour);
            this.Controls.Add(this.lblGroupId);
            this.Controls.Add(this.lblGroupDescription);
            this.Controls.Add(this.lblGroupName);
            this.Controls.Add(this.btnRemoveGroupWord);
            this.Controls.Add(this.btnAddGroupWord);
            this.Controls.Add(this.txtNewGroupWord);
            this.Controls.Add(this.chkGroupItalic);
            this.Controls.Add(this.chkGroupBold);
            this.Controls.Add(this.chkGroupVisible);
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
            this.MinimumSize = new System.Drawing.Size(776, 624);
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
        private CheckBox chkGroupVisible;
        private CheckBox chkGroupBold;
        private CheckBox chkGroupItalic;
        private TextBox txtNewGroupWord;
        private Button btnAddGroupWord;
        private Button btnRemoveGroupWord;
        private Label lblGroupName;
        private Label lblGroupDescription;
        private Label lblGroupId;
        private Label lblGroupColour;
        private ComboBox cmbGroupColour;
        private System.Windows.Forms.GroupBox grpPreview;
        private System.Windows.Forms.Panel pnlPreview;
        private System.Windows.Forms.Label lblPreviewStatus;
        private Button btnSaveLanguage;
    }
}