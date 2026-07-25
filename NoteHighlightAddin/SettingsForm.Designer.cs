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
            this.btnTestRoundTrip = new System.Windows.Forms.Button();
            this.lbxKeywordGroups = new System.Windows.Forms.ListBox();
            this.lbxGroupWords = new System.Windows.Forms.ListBox();
            this.txtGroupName = new System.Windows.Forms.TextBox();
            this.txtGroupDescription = new System.Windows.Forms.TextBox();
            this.nudGroupPriority = new System.Windows.Forms.NumericUpDown();
            this.chkGroupVisible = new System.Windows.Forms.CheckBox();
            this.chkGroupBold = new System.Windows.Forms.CheckBox();
            this.chkGroupItalic = new System.Windows.Forms.CheckBox();
            this.txtGroupColour = new System.Windows.Forms.TextBox();
            this.txtNewGroupWord = new System.Windows.Forms.TextBox();
            this.btnAddGroupWord = new System.Windows.Forms.Button();
            this.btnRemoveGroupWord = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudGroupPriority)).BeginInit();
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
            this.btnFont.Location = new System.Drawing.Point(34, 20);
            this.btnFont.Name = "btnFont";
            this.btnFont.Size = new System.Drawing.Size(339, 23);
            this.btnFont.TabIndex = 1;
            this.btnFont.Text = "Font";
            this.btnFont.UseVisualStyleBackColor = true;
            this.btnFont.Click += new System.EventHandler(this.BtnFont_Click);
            // 
            // cbShowTableBorder
            // 
            this.cbShowTableBorder.AutoSize = true;
            this.cbShowTableBorder.Location = new System.Drawing.Point(415, 218);
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
            this.lblLanguages.Location = new System.Drawing.Point(538, 219);
            this.lblLanguages.Name = "lblLanguages";
            this.lblLanguages.Size = new System.Drawing.Size(96, 13);
            this.lblLanguages.TabIndex = 3;
            this.lblLanguages.Text = "Active Languages:";
            // 
            // lbxLanguages
            // 
            this.lbxLanguages.FormattingEnabled = true;
            this.lbxLanguages.Location = new System.Drawing.Point(415, 241);
            this.lbxLanguages.Name = "lbxLanguages";
            this.lbxLanguages.Size = new System.Drawing.Size(213, 95);
            this.lbxLanguages.TabIndex = 4;
            this.lbxLanguages.SelectedIndexChanged += new System.EventHandler(this.lbxLanguages_SelectedIndexChanged);
            // 
            // btnRemoveLanguage
            // 
            this.btnRemoveLanguage.Location = new System.Drawing.Point(253, 218);
            this.btnRemoveLanguage.Name = "btnRemoveLanguage";
            this.btnRemoveLanguage.Size = new System.Drawing.Size(120, 36);
            this.btnRemoveLanguage.TabIndex = 5;
            this.btnRemoveLanguage.Text = "Remove Selected Language";
            this.btnRemoveLanguage.UseVisualStyleBackColor = true;
            this.btnRemoveLanguage.Click += new System.EventHandler(this.BtnRemoveLanguage_Click);
            // 
            // lblAddLanguage
            // 
            this.lblAddLanguage.AutoSize = true;
            this.lblAddLanguage.Location = new System.Drawing.Point(31, 316);
            this.lblAddLanguage.Name = "lblAddLanguage";
            this.lblAddLanguage.Size = new System.Drawing.Size(105, 13);
            this.lblAddLanguage.TabIndex = 6;
            this.lblAddLanguage.Text = "Add New Language:";
            this.lblAddLanguage.Click += new System.EventHandler(this.lblAddLanguage_Click);
            // 
            // cmbAvailableLanguages
            // 
            this.cmbAvailableLanguages.FormattingEnabled = true;
            this.cmbAvailableLanguages.Location = new System.Drawing.Point(34, 332);
            this.cmbAvailableLanguages.Name = "cmbAvailableLanguages";
            this.cmbAvailableLanguages.Size = new System.Drawing.Size(213, 21);
            this.cmbAvailableLanguages.TabIndex = 7;
            // 
            // btnAddLanguage
            // 
            this.btnAddLanguage.Location = new System.Drawing.Point(253, 316);
            this.btnAddLanguage.Name = "btnAddLanguage";
            this.btnAddLanguage.Size = new System.Drawing.Size(120, 37);
            this.btnAddLanguage.TabIndex = 8;
            this.btnAddLanguage.Text = "Add Language to Ribbon";
            this.btnAddLanguage.UseVisualStyleBackColor = true;
            this.btnAddLanguage.Click += new System.EventHandler(this.BtnAddLanguage_Click);
            // 
            // btnTestRoundTrip
            // 
            this.btnTestRoundTrip.Location = new System.Drawing.Point(253, 158);
            this.btnTestRoundTrip.Name = "btnTestRoundTrip";
            this.btnTestRoundTrip.Size = new System.Drawing.Size(75, 23);
            this.btnTestRoundTrip.TabIndex = 9;
            this.btnTestRoundTrip.Text = "Test Round Trip";
            this.btnTestRoundTrip.UseVisualStyleBackColor = true;
            this.btnTestRoundTrip.Click += new System.EventHandler(this.btnTestRoundTrip_Click);
            // 
            // lbxKeywordGroups
            // 
            this.lbxKeywordGroups.FormattingEnabled = true;
            this.lbxKeywordGroups.Location = new System.Drawing.Point(34, 58);
            this.lbxKeywordGroups.Name = "lbxKeywordGroups";
            this.lbxKeywordGroups.Size = new System.Drawing.Size(171, 95);
            this.lbxKeywordGroups.TabIndex = 10;
            this.lbxKeywordGroups.SelectedIndexChanged += new System.EventHandler(this.lbxKeywordGroups_SelectedIndexChanged);
            // 
            // lbxGroupWords
            // 
            this.lbxGroupWords.FormattingEnabled = true;
            this.lbxGroupWords.Location = new System.Drawing.Point(253, 58);
            this.lbxGroupWords.Name = "lbxGroupWords";
            this.lbxGroupWords.Size = new System.Drawing.Size(120, 95);
            this.lbxGroupWords.TabIndex = 11;
            // 
            // txtGroupName
            // 
            this.txtGroupName.Location = new System.Drawing.Point(415, 20);
            this.txtGroupName.Name = "txtGroupName";
            this.txtGroupName.Size = new System.Drawing.Size(100, 20);
            this.txtGroupName.TabIndex = 12;
            this.txtGroupName.Text = "Name";
            this.txtGroupName.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // txtGroupDescription
            // 
            this.txtGroupDescription.Location = new System.Drawing.Point(415, 58);
            this.txtGroupDescription.Name = "txtGroupDescription";
            this.txtGroupDescription.Size = new System.Drawing.Size(100, 20);
            this.txtGroupDescription.TabIndex = 13;
            this.txtGroupDescription.Text = "Description";
            // 
            // nudGroupPriority
            // 
            this.nudGroupPriority.Location = new System.Drawing.Point(415, 94);
            this.nudGroupPriority.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudGroupPriority.Name = "nudGroupPriority";
            this.nudGroupPriority.Size = new System.Drawing.Size(120, 20);
            this.nudGroupPriority.TabIndex = 14;
            // 
            // chkGroupVisible
            // 
            this.chkGroupVisible.AutoSize = true;
            this.chkGroupVisible.Location = new System.Drawing.Point(415, 135);
            this.chkGroupVisible.Name = "chkGroupVisible";
            this.chkGroupVisible.Size = new System.Drawing.Size(56, 17);
            this.chkGroupVisible.TabIndex = 15;
            this.chkGroupVisible.Text = "Visible";
            this.chkGroupVisible.UseVisualStyleBackColor = true;
            // 
            // chkGroupBold
            // 
            this.chkGroupBold.AutoSize = true;
            this.chkGroupBold.Location = new System.Drawing.Point(501, 135);
            this.chkGroupBold.Name = "chkGroupBold";
            this.chkGroupBold.Size = new System.Drawing.Size(47, 17);
            this.chkGroupBold.TabIndex = 16;
            this.chkGroupBold.Text = "Bold";
            this.chkGroupBold.UseVisualStyleBackColor = true;
            // 
            // chkGroupItalic
            // 
            this.chkGroupItalic.AutoSize = true;
            this.chkGroupItalic.Location = new System.Drawing.Point(415, 158);
            this.chkGroupItalic.Name = "chkGroupItalic";
            this.chkGroupItalic.Size = new System.Drawing.Size(48, 17);
            this.chkGroupItalic.TabIndex = 17;
            this.chkGroupItalic.Text = "Italic";
            this.chkGroupItalic.UseVisualStyleBackColor = true;
            // 
            // txtGroupColour
            // 
            this.txtGroupColour.Location = new System.Drawing.Point(415, 182);
            this.txtGroupColour.Name = "txtGroupColour";
            this.txtGroupColour.Size = new System.Drawing.Size(100, 20);
            this.txtGroupColour.TabIndex = 18;
            this.txtGroupColour.Text = "Colour";
            // 
            // txtNewGroupWord
            // 
            this.txtNewGroupWord.Location = new System.Drawing.Point(34, 160);
            this.txtNewGroupWord.Name = "txtNewGroupWord";
            this.txtNewGroupWord.Size = new System.Drawing.Size(171, 20);
            this.txtNewGroupWord.TabIndex = 19;
            // 
            // btnAddGroupWord
            // 
            this.btnAddGroupWord.Location = new System.Drawing.Point(34, 187);
            this.btnAddGroupWord.Name = "btnAddGroupWord";
            this.btnAddGroupWord.Size = new System.Drawing.Size(75, 23);
            this.btnAddGroupWord.TabIndex = 20;
            this.btnAddGroupWord.Text = "Add Word";
            this.btnAddGroupWord.UseVisualStyleBackColor = true;
            // 
            // btnRemoveGroupWord
            // 
            this.btnRemoveGroupWord.Location = new System.Drawing.Point(130, 187);
            this.btnRemoveGroupWord.Name = "btnRemoveGroupWord";
            this.btnRemoveGroupWord.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveGroupWord.TabIndex = 21;
            this.btnRemoveGroupWord.Text = "Remove Word";
            this.btnRemoveGroupWord.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 365);
            this.Controls.Add(this.btnRemoveGroupWord);
            this.Controls.Add(this.btnAddGroupWord);
            this.Controls.Add(this.txtNewGroupWord);
            this.Controls.Add(this.txtGroupColour);
            this.Controls.Add(this.chkGroupItalic);
            this.Controls.Add(this.chkGroupBold);
            this.Controls.Add(this.chkGroupVisible);
            this.Controls.Add(this.nudGroupPriority);
            this.Controls.Add(this.txtGroupDescription);
            this.Controls.Add(this.txtGroupName);
            this.Controls.Add(this.lbxGroupWords);
            this.Controls.Add(this.lbxKeywordGroups);
            this.Controls.Add(this.btnTestRoundTrip);
            this.Controls.Add(this.btnAddLanguage);
            this.Controls.Add(this.cmbAvailableLanguages);
            this.Controls.Add(this.lblAddLanguage);
            this.Controls.Add(this.btnRemoveLanguage);
            this.Controls.Add(this.lbxLanguages);
            this.Controls.Add(this.lblLanguages);
            this.Controls.Add(this.cbShowTableBorder);
            this.Controls.Add(this.btnFont);
            this.Name = "SettingsForm";
            this.Text = "SettingsForm";
            this.Shown += new System.EventHandler(this.SettingsForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.nudGroupPriority)).EndInit();
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
        private Button btnTestRoundTrip;
        private ListBox lbxKeywordGroups;
        private ListBox lbxGroupWords;
        private TextBox txtGroupName;
        private TextBox txtGroupDescription;
        private NumericUpDown nudGroupPriority;
        private CheckBox chkGroupVisible;
        private CheckBox chkGroupBold;
        private CheckBox chkGroupItalic;
        private TextBox txtGroupColour;
        private TextBox txtNewGroupWord;
        private Button btnAddGroupWord;
        private Button btnRemoveGroupWord;
    }
}