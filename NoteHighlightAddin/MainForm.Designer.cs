namespace NoteHighlightAddin
{
    partial class MainForm
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
            NoteHighlightForm.Properties.Settings settings1 = new NoteHighlightForm.Properties.Settings();
            this.btnCodeHighLight = new System.Windows.Forms.Button();
            this.cbx_style = new System.Windows.Forms.ComboBox();
            this.cbx_Clipboard = new System.Windows.Forms.CheckBox();
            this.cbx_lineNumber = new System.Windows.Forms.CheckBox();
            this.txtCode = new ICSharpCode.TextEditor.TextEditorControl();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlOptions = new System.Windows.Forms.Panel();
            this.lblTheme = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblBackgroundValue = new System.Windows.Forms.Label();
            this.lblBackgroundCaption = new System.Windows.Forms.Label();
            this.btnBackground = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.splitMainContent = new System.Windows.Forms.SplitContainer();
            this.pnlEditorCard = new System.Windows.Forms.Panel();
            this.pnlEditorHeader = new System.Windows.Forms.Panel();
            this.lblEditorTitle = new System.Windows.Forms.Label();
            this.pnlPreviewCard = new System.Windows.Forms.Panel();
            this.pnlLivePreview = new System.Windows.Forms.Panel();
            this.lblPreviewStatus = new System.Windows.Forms.Label();
            this.pnlPreviewHeader = new System.Windows.Forms.Panel();
            this.lblLivePreviewTitle = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip();
            this.pickColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.transparentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlHeader.SuspendLayout();
            this.pnlOptions.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMainContent)).BeginInit();
            this.splitMainContent.Panel1.SuspendLayout();
            this.splitMainContent.Panel2.SuspendLayout();
            this.splitMainContent.SuspendLayout();
            this.pnlEditorCard.SuspendLayout();
            this.pnlEditorHeader.SuspendLayout();
            this.pnlPreviewCard.SuspendLayout();
            this.pnlPreviewHeader.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCodeHighLight
            // 
            this.btnCodeHighLight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCodeHighLight.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnCodeHighLight.Location = new System.Drawing.Point(663, 20);
            this.btnCodeHighLight.Name = "btnCodeHighLight";
            this.btnCodeHighLight.Size = new System.Drawing.Size(126, 34);
            this.btnCodeHighLight.TabIndex = 0;
            this.btnCodeHighLight.Text = "Insert Code";
            this.btnCodeHighLight.UseVisualStyleBackColor = false;
            this.btnCodeHighLight.Click += new System.EventHandler(this.btnCodeHighLight_Click);
            // 
            // cbx_style
            // 
            this.cbx_style.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbx_style.FormattingEnabled = true;
            this.cbx_style.Location = new System.Drawing.Point(24, 30);
            this.cbx_style.Name = "cbx_style";
            this.cbx_style.Size = new System.Drawing.Size(238, 23);
            this.cbx_style.TabIndex = 0;
            // 
            // cbx_Clipboard
            // 
            this.cbx_Clipboard.AutoSize = true;
            settings1.BackgroundColor = System.Drawing.Color.White;
            settings1.HighLightStyle = 0;
            settings1.SaveOnClipboard = false;
            settings1.SettingsKey = "";
            settings1.ShowLineNumber = true;
            settings1.QuickStyle = false;
            this.cbx_Clipboard.Checked = settings1.SaveOnClipboard;
            this.cbx_Clipboard.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "SaveOnClipboard", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.cbx_Clipboard.Location = new System.Drawing.Point(302, 33);
            this.cbx_Clipboard.Name = "cbx_Clipboard";
            this.cbx_Clipboard.Size = new System.Drawing.Size(125, 19);
            this.cbx_Clipboard.TabIndex = 1;
            this.cbx_Clipboard.Text = "Copy to Clipboard";
            this.cbx_Clipboard.UseVisualStyleBackColor = false;
            // 
            // cbx_lineNumber
            // 
            this.cbx_lineNumber.AutoSize = true;
            this.cbx_lineNumber.Checked = settings1.ShowLineNumber;
            this.cbx_lineNumber.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbx_lineNumber.DataBindings.Add(new System.Windows.Forms.Binding("Checked", settings1, "ShowLineNumber", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.cbx_lineNumber.Location = new System.Drawing.Point(457, 33);
            this.cbx_lineNumber.Name = "cbx_lineNumber";
            this.cbx_lineNumber.Size = new System.Drawing.Size(98, 19);
            this.cbx_lineNumber.TabIndex = 2;
            this.cbx_lineNumber.Text = "Line numbers";
            this.cbx_lineNumber.UseVisualStyleBackColor = false;
            // 
            // txtCode
            // 
            this.txtCode.AutoScroll = true;
            this.txtCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCode.IsReadOnly = false;
            this.txtCode.Location = new System.Drawing.Point(12, 36);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(764, 193);
            this.txtCode.TabIndex = 0;
            // 
            // pnlHeader
            // 
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(820, 76);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Location = new System.Drawing.Point(25, 46);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(271, 15);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Preview and highlight code before inserting it into OneNote.";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(23, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(93, 15);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "NoteHighlight+";
            // 
            // pnlOptions
            // 
            this.pnlOptions.Controls.Add(this.lblTheme);
            this.pnlOptions.Controls.Add(this.cbx_lineNumber);
            this.pnlOptions.Controls.Add(this.cbx_Clipboard);
            this.pnlOptions.Controls.Add(this.cbx_style);
            this.pnlOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOptions.Location = new System.Drawing.Point(0, 76);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Size = new System.Drawing.Size(820, 66);
            this.pnlOptions.TabIndex = 1;
            // 
            // lblTheme
            // 
            this.lblTheme.AutoSize = true;
            this.lblTheme.Location = new System.Drawing.Point(22, 9);
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Size = new System.Drawing.Size(43, 15);
            this.lblTheme.TabIndex = 3;
            this.lblTheme.Text = "Theme";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblBackgroundValue);
            this.panel2.Controls.Add(this.lblBackgroundCaption);
            this.panel2.Controls.Add(this.btnBackground);
            this.panel2.Controls.Add(this.btnCodeHighLight);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 662);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(820, 68);
            this.panel2.TabIndex = 3;
            // 
            // lblBackgroundValue
            // 
            this.lblBackgroundValue.AutoSize = true;
            this.lblBackgroundValue.Location = new System.Drawing.Point(82, 35);
            this.lblBackgroundValue.Name = "lblBackgroundValue";
            this.lblBackgroundValue.Size = new System.Drawing.Size(50, 15);
            this.lblBackgroundValue.TabIndex = 4;
            this.lblBackgroundValue.Text = "#FFFFFF";
            // 
            // lblBackgroundCaption
            // 
            this.lblBackgroundCaption.AutoSize = true;
            this.lblBackgroundCaption.Location = new System.Drawing.Point(23, 12);
            this.lblBackgroundCaption.Name = "lblBackgroundCaption";
            this.lblBackgroundCaption.Size = new System.Drawing.Size(69, 15);
            this.lblBackgroundCaption.TabIndex = 3;
            this.lblBackgroundCaption.Text = "Background";
            // 
            // btnBackground
            // 
            this.btnBackground.Location = new System.Drawing.Point(25, 31);
            this.btnBackground.Name = "btnBackground";
            this.btnBackground.Size = new System.Drawing.Size(44, 24);
            this.btnBackground.TabIndex = 1;
            this.btnBackground.UseVisualStyleBackColor = false;
            this.btnBackground.Click += new System.EventHandler(this.btnBackground_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.splitMainContent);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 142);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.panel3.Size = new System.Drawing.Size(820, 520);
            this.panel3.TabIndex = 2;
            // 
            // splitMainContent
            // 
            this.splitMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMainContent.Location = new System.Drawing.Point(16, 12);
            this.splitMainContent.Name = "splitMainContent";
            this.splitMainContent.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMainContent.Panel1
            // 
            this.splitMainContent.Panel1.Controls.Add(this.pnlEditorCard);
            this.splitMainContent.Panel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.splitMainContent.Panel1MinSize = 150;
            // 
            // splitMainContent.Panel2
            // 
            this.splitMainContent.Panel2.Controls.Add(this.pnlPreviewCard);
            this.splitMainContent.Panel2.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.splitMainContent.Panel2MinSize = 150;
            this.splitMainContent.Size = new System.Drawing.Size(788, 496);
            this.splitMainContent.SplitterDistance = 242;
            this.splitMainContent.SplitterWidth = 4;
            this.splitMainContent.TabIndex = 1;
            // 
            // pnlEditorCard
            // 
            this.pnlEditorCard.Controls.Add(this.txtCode);
            this.pnlEditorCard.Controls.Add(this.pnlEditorHeader);
            this.pnlEditorCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEditorCard.Location = new System.Drawing.Point(0, 0);
            this.pnlEditorCard.Name = "pnlEditorCard";
            this.pnlEditorCard.Padding = new System.Windows.Forms.Padding(12, 0, 12, 8);
            this.pnlEditorCard.Size = new System.Drawing.Size(788, 237);
            this.pnlEditorCard.TabIndex = 0;
            // 
            // pnlEditorHeader
            // 
            this.pnlEditorHeader.Controls.Add(this.lblEditorTitle);
            this.pnlEditorHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEditorHeader.Location = new System.Drawing.Point(12, 0);
            this.pnlEditorHeader.Name = "pnlEditorHeader";
            this.pnlEditorHeader.Size = new System.Drawing.Size(764, 36);
            this.pnlEditorHeader.TabIndex = 1;
            // 
            // lblEditorTitle
            // 
            this.lblEditorTitle.AutoSize = true;
            this.lblEditorTitle.Location = new System.Drawing.Point(0, 10);
            this.lblEditorTitle.Name = "lblEditorTitle";
            this.lblEditorTitle.Size = new System.Drawing.Size(37, 15);
            this.lblEditorTitle.TabIndex = 0;
            this.lblEditorTitle.Text = "Editor";
            // 
            // pnlPreviewCard
            // 
            this.pnlPreviewCard.Controls.Add(this.pnlLivePreview);
            this.pnlPreviewCard.Controls.Add(this.lblPreviewStatus);
            this.pnlPreviewCard.Controls.Add(this.pnlPreviewHeader);
            this.pnlPreviewCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPreviewCard.Location = new System.Drawing.Point(0, 5);
            this.pnlPreviewCard.Name = "pnlPreviewCard";
            this.pnlPreviewCard.Padding = new System.Windows.Forms.Padding(12, 0, 12, 8);
            this.pnlPreviewCard.Size = new System.Drawing.Size(788, 245);
            this.pnlPreviewCard.TabIndex = 0;
            // 
            // pnlLivePreview
            // 
            this.pnlLivePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLivePreview.Location = new System.Drawing.Point(12, 36);
            this.pnlLivePreview.Name = "pnlLivePreview";
            this.pnlLivePreview.Size = new System.Drawing.Size(764, 175);
            this.pnlLivePreview.TabIndex = 0;
            // 
            // lblPreviewStatus
            // 
            this.lblPreviewStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblPreviewStatus.Location = new System.Drawing.Point(12, 211);
            this.lblPreviewStatus.Name = "lblPreviewStatus";
            this.lblPreviewStatus.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.lblPreviewStatus.Size = new System.Drawing.Size(764, 26);
            this.lblPreviewStatus.TabIndex = 1;
            this.lblPreviewStatus.Text = "Preview not initialized.";
            this.lblPreviewStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlPreviewHeader
            // 
            this.pnlPreviewHeader.Controls.Add(this.lblLivePreviewTitle);
            this.pnlPreviewHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPreviewHeader.Location = new System.Drawing.Point(12, 0);
            this.pnlPreviewHeader.Name = "pnlPreviewHeader";
            this.pnlPreviewHeader.Size = new System.Drawing.Size(764, 36);
            this.pnlPreviewHeader.TabIndex = 2;
            // 
            // lblLivePreviewTitle
            // 
            this.lblLivePreviewTitle.AutoSize = true;
            this.lblLivePreviewTitle.Location = new System.Drawing.Point(0, 10);
            this.lblLivePreviewTitle.Name = "lblLivePreviewTitle";
            this.lblLivePreviewTitle.Size = new System.Drawing.Size(70, 15);
            this.lblLivePreviewTitle.TabIndex = 0;
            this.lblLivePreviewTitle.Text = "Live Preview";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pickColorToolStripMenuItem,
            this.transparentToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(137, 48);
            // 
            // pickColorToolStripMenuItem
            // 
            this.pickColorToolStripMenuItem.Name = "pickColorToolStripMenuItem";
            this.pickColorToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.pickColorToolStripMenuItem.Text = "Pick color";
            this.pickColorToolStripMenuItem.Click += new System.EventHandler(this.PickColorToolStripMenuItem_Click);
            // 
            // transparentToolStripMenuItem
            // 
            this.transparentToolStripMenuItem.Name = "transparentToolStripMenuItem";
            this.transparentToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.transparentToolStripMenuItem.Text = "Transparent";
            this.transparentToolStripMenuItem.Click += new System.EventHandler(this.TransparentToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AcceptButton = this.btnCodeHighLight;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 730);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.pnlOptions);
            this.Controls.Add(this.pnlHeader);
            this.MinimumSize = new System.Drawing.Size(720, 640);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NoteHighlight+";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CodeForm_FormClosed);
            this.Load += new System.EventHandler(this.CodeForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlOptions.ResumeLayout(false);
            this.pnlOptions.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.splitMainContent.Panel1.ResumeLayout(false);
            this.splitMainContent.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMainContent)).EndInit();
            this.splitMainContent.ResumeLayout(false);
            this.pnlEditorCard.ResumeLayout(false);
            this.pnlEditorHeader.ResumeLayout(false);
            this.pnlEditorHeader.PerformLayout();
            this.pnlPreviewCard.ResumeLayout(false);
            this.pnlPreviewHeader.ResumeLayout(false);
            this.pnlPreviewHeader.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCodeHighLight;
        private System.Windows.Forms.CheckBox cbx_lineNumber;
        private System.Windows.Forms.CheckBox cbx_Clipboard;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ComboBox cbx_style;
        private ICSharpCode.TextEditor.TextEditorControl txtCode;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel pnlOptions;
        private System.Windows.Forms.Label lblTheme;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnBackground;
        private System.Windows.Forms.SplitContainer splitMainContent;
        private System.Windows.Forms.Panel pnlEditorCard;
        private System.Windows.Forms.Panel pnlEditorHeader;
        private System.Windows.Forms.Label lblEditorTitle;
        private System.Windows.Forms.Panel pnlPreviewCard;
        private System.Windows.Forms.Panel pnlPreviewHeader;
        private System.Windows.Forms.Label lblLivePreviewTitle;
        private System.Windows.Forms.Panel pnlLivePreview;
        private System.Windows.Forms.Label lblPreviewStatus;
        private System.Windows.Forms.Label lblBackgroundCaption;
        private System.Windows.Forms.Label lblBackgroundValue;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem pickColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem transparentToolStripMenuItem;
    }
}
