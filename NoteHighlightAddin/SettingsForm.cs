using Infrastructure.Core;
using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.KeywordGroups.Services;
using NoteHighlightAddin.Highlighting.KeywordGroups.Testing;
using NoteHighlightAddin.Highlighting.KeywordGroups.ViewModels;
using NoteHighlightAddin.Highlighting.Preview.Services;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Threading.Tasks;


namespace NoteHighlightAddin
{
    public partial class SettingsForm : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private LanguageRibbonController _languageRibbonController;
        private readonly LanguageEditorViewModel _languageEditor;
        private Button _btnAddKeywordGroup;
        private Button _btnRemoveKeywordGroup;
        private Button _btnMoveKeywordGroupUp;
        private Button _btnMoveKeywordGroupDown;
        private Button _btnEditGroupRegex;
        private KeywordGroupSelectionController _groupSelectionController;
        private KeywordGroupEditorController _groupEditorController;
        private KeywordGroupDetailsController _groupDetailsController;
        private KeywordWordEditorController _wordEditorController;
        private WebView2 _previewWebView; // WebView2 control for preview

        public SettingsForm()
        {
            InitializeComponent();

            // Connect the form events explicitly. This avoids depending on the WinForms designer event wiring.
            Shown += SettingsForm_Shown;
            FormClosed += SettingsForm_FormClosed;

            _languageEditor =
                new LanguageEditorViewModel(
                    new LanguageEditorService());

            _languageEditor.ConfigurationChanged +=
                LanguageEditor_ConfigurationChanged;

            // Connect the word editor events explicitly. This avoids depending
            // on the WinForms designer event wiring.
            btnAddGroupWord.Click +=
                btnAddGroupWord_Click;

            btnRemoveGroupWord.Click +=
                btnRemoveGroupWord_Click;

            lbxGroupWords.SelectedIndexChanged +=
                lbxGroupWords_SelectedIndexChanged;

            txtNewGroupWord.KeyDown +=
                txtNewGroupWord_KeyDown;

            InitializeGroupManagementControls();

            _wordEditorController =
                new KeywordWordEditorController(
                    this,
                    _languageEditor,
                    txtNewGroupWord,
                    lbxGroupWords,
                    btnAddGroupWord,
                    btnRemoveGroupWord,
                    () => _groupSelectionController.RefreshSelection(),
                    UpdateWindowTitle);

            _groupDetailsController =
                new KeywordGroupDetailsController(
                    _languageEditor,
                    txtGroupName,
                    txtGroupDescription,
                    chkGroupVisible,
                    chkGroupBold,
                    chkGroupItalic,
                    cmbGroupColour,
                    nudGroupId,
                    () => _groupSelectionController.RefreshSelectedListItem());

            _groupSelectionController =
                new KeywordGroupSelectionController(
                    _languageEditor,
                    lbxKeywordGroups,
                    lbxGroupWords,
                    _btnAddKeywordGroup,
                    _btnRemoveKeywordGroup,
                    _btnMoveKeywordGroupUp,
                    _btnMoveKeywordGroupDown,
                    _btnEditGroupRegex,
                    _groupDetailsController.Refresh,
                    _wordEditorController.UpdateState);

            _groupEditorController =
                new KeywordGroupEditorController(
                    this,
                    _languageEditor,
                    _groupSelectionController,
                    nudGroupId,
                    UpdateWindowTitle,
                    FocusGroupNameEditor);

            fontDialog1.Font =
                new Font(
                    NoteHighlightForm.Properties.Settings.Default.Font,
                    NoteHighlightForm.Properties.Settings.Default.FontSize);

            btnFont.Text =
                "Font:" +
                fontDialog1.Font.Name +
                ", Size:" +
                fontDialog1.Font.Size;

            btnFont.Font = fontDialog1.Font;

            cbShowTableBorder.Checked =
                NoteHighlightForm.Properties.Settings.Default.ShowTableBorder;

            _languageRibbonController =
                new LanguageRibbonController(
                    this,
                    _languageEditor,
                    _groupSelectionController,
                    lbxLanguages,
                    cmbAvailableLanguages);

            _wordEditorController.UpdateState();
        }

        // Initialize the WebView2 control for preview asynchronously
        private async Task InitializePreviewWebViewAsync()
        {
            if (_previewWebView != null)
            {
                return;
            }

            _previewWebView =
                new WebView2
                {
                    Dock = DockStyle.Fill,
                    Name = "previewWebView"
                };

            pnlPreview.Controls.Add(
                _previewWebView);

            lblPreviewStatus.Text =
                "Initializing preview...";

            try
            {
                await _previewWebView.EnsureCoreWebView2Async();

                _previewWebView.CoreWebView2.NavigationCompleted += PreviewWebView_NavigationCompleted;

                lblPreviewStatus.Text =
                    "Preview ready.";
            }
            catch (Exception exception)
            {
                lblPreviewStatus.Text =
                    "WebView2 initialization failed.";

                MessageBox.Show(
                    exception.ToString(),
                    "WebView2 error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // adding form closed event handler to cleanup the preview service

        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            PreviewHtmlServiceTester.Cleanup();
        }

        // adding preview navigation completed event handler

        private void PreviewWebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            lblPreviewStatus.Text =
                e.IsSuccess
                    ? "Preview loaded successfully."
                    : "Preview could not be loaded.";
        }

        private async void TestPreview()
        {
            try
            {
                if (!_languageEditor.HasConfiguration)
                {
                    lblPreviewStatus.Text =
                        "Select a language first.";

                    return;
                }

                if (_previewWebView == null ||
                    _previewWebView.CoreWebView2 == null)
                {
                    lblPreviewStatus.Text =
                        "Preview is not ready.";

                    return;
                }

                lblPreviewStatus.Text =
                    "Generating preview...";

                string htmlPath =
                    PreviewHtmlServiceTester.GeneratePreview(
                        _languageEditor.Configuration);

                Uri htmlUri =
                    new Uri(htmlPath);

                _previewWebView.CoreWebView2.Navigate(
                    htmlUri.AbsoluteUri);

                lblPreviewStatus.Text =
                    "Loading preview...";

                await Task.CompletedTask;
            }
            catch (Exception exception)
            {
                lblPreviewStatus.Text =
                    "Preview generation failed.";

                MessageBox.Show(
                    exception.ToString(),
                    "Preview error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void InitializeGroupManagementControls()
        {
            int availableWidth =
                lbxKeywordGroups.Width;

            int buttonSpacing = 6;

            int buttonWidth =
                (availableWidth - buttonSpacing) / 2;

            _btnAddKeywordGroup =
                new Button
                {
                    Name = "btnAddKeywordGroup",
                    Text = "Add Group",
                    AutoSize = false,
                    Width = buttonWidth,
                    Height = 27,
                    Left = lbxKeywordGroups.Left,
                    Top = lbxKeywordGroups.Bottom + 6,
                    Anchor =
                        lbxKeywordGroups.Anchor
                };

            _btnRemoveKeywordGroup =
                new Button
                {
                    Name = "btnRemoveKeywordGroup",
                    Text = "Remove Group",
                    AutoSize = false,
                    Width = buttonWidth,
                    Height = 27,
                    Left =
                        lbxKeywordGroups.Left
                        + buttonWidth
                        + buttonSpacing,
                    Top = lbxKeywordGroups.Bottom + 6,
                    Anchor =
                        lbxKeywordGroups.Anchor,
                    Enabled = false
                };

            _btnMoveKeywordGroupUp =
                new Button
                {
                    Name = "btnMoveKeywordGroupUp",
                    Text = "▲",
                    AutoSize = false,
                    Width = 38,
                    Height = 27,
                    Left = lbxKeywordGroups.Right + 6,
                    Top = lbxKeywordGroups.Top,
                    Anchor =
                        lbxKeywordGroups.Anchor,
                    Enabled = false
                };

            _btnMoveKeywordGroupDown =
                new Button
                {
                    Name = "btnMoveKeywordGroupDown",
                    Text = "▼",
                    AutoSize = false,
                    Width = 38,
                    Height = 27,
                    Left = lbxKeywordGroups.Right + 6,
                    Top = lbxKeywordGroups.Top + 33,
                    Anchor =
                        lbxKeywordGroups.Anchor,
                    Enabled = false
                };

            // Place the word input below the group-management buttons.
            // Add Group previously occupied the original position of
            // txtNewGroupWord and visually covered it.
            txtNewGroupWord.Top =
                _btnAddKeywordGroup.Bottom + 6;

            btnAddGroupWord.Top =
                txtNewGroupWord.Bottom + 6;

            btnRemoveGroupWord.Top =
                txtNewGroupWord.Bottom + 6;

            _btnEditGroupRegex =
                new Button
                {
                    Name = "btnEditGroupRegex",
                    Text = "Regex Editor...",
                    AutoSize = false,
                    Height = 27,
                    Left = txtNewGroupWord.Left,
                    Top = btnAddGroupWord.Bottom + 6,
                    Width =
                        btnRemoveGroupWord.Right
                        - txtNewGroupWord.Left,
                    Anchor =
                        txtNewGroupWord.Anchor,
                    Enabled = false
                };

            _btnAddKeywordGroup.Click +=
                btnAddKeywordGroup_Click;

            _btnRemoveKeywordGroup.Click +=
                btnRemoveKeywordGroup_Click;

            _btnMoveKeywordGroupUp.Click +=
                btnMoveKeywordGroupUp_Click;

            _btnMoveKeywordGroupDown.Click +=
                btnMoveKeywordGroupDown_Click;

            _btnEditGroupRegex.Click +=
                btnEditGroupRegex_Click;

            Control wordEditorParent =
                lbxGroupWords.Parent;

            wordEditorParent.Controls.Add(
                _btnEditGroupRegex);

            txtNewGroupWord.BringToFront();
            btnAddGroupWord.BringToFront();
            btnRemoveGroupWord.BringToFront();
            _btnEditGroupRegex.BringToFront();

            Control parent =
                lbxKeywordGroups.Parent;

            parent.Controls.Add(
                _btnAddKeywordGroup);

            parent.Controls.Add(
                _btnRemoveKeywordGroup);

            parent.Controls.Add(
                _btnMoveKeywordGroupUp);

            parent.Controls.Add(
                _btnMoveKeywordGroupDown);

            _btnAddKeywordGroup.BringToFront();
            _btnRemoveKeywordGroup.BringToFront();
            _btnMoveKeywordGroupUp.BringToFront();
            _btnMoveKeywordGroupDown.BringToFront();
        }

        private void btnAddKeywordGroup_Click(
            object sender,
            EventArgs e)
        {
            _groupEditorController.AddGroup();
        }

        private void FocusGroupNameEditor()
        {
            txtGroupName.Focus();
            txtGroupName.SelectAll();
        }

        private void nudGroupId_ValueChanged(
            object sender,
            EventArgs e)
        {
            if (_groupDetailsController.IsLoading)
            {
                return;
            }

            _groupEditorController.ChangeSelectedGroupId();
        }

        private void btnEditGroupRegex_Click(
            object sender,
            EventArgs e)
        {
            _groupEditorController.EditRegex();
        }

        private void btnMoveKeywordGroupUp_Click(
            object sender,
            EventArgs e)
        {
            _groupEditorController.MoveSelectedGroupUp();
        }

        private void btnMoveKeywordGroupDown_Click(
            object sender,
            EventArgs e)
        {
            _groupEditorController.MoveSelectedGroupDown();
        }

        private void btnRemoveKeywordGroup_Click(
            object sender,
            EventArgs e)
        {
            _groupEditorController.RemoveSelectedGroup();
        }

        private void btnTestRoundTrip_Click(
            object sender,
            EventArgs e)
        {
            TestPreview();
        }

        /// <summary>
        /// Executes a read-map-write-read round-trip test for python.lang
        /// without modifying the original language definition file.
        /// </summary>
        private void TestPythonLanguageRoundTrip()
        {
            string pythonLanguagePath =
                Path.Combine(
                    PathManager.LanguagesFolder,
                    "python.lang");

            var tester =
                new HighlightLanguageRoundTripTester();

            try
            {
                RoundTripTestResult result =
                    tester.Test(
                        pythonLanguagePath);

                if (result.IsEquivalent)
                {
                    MessageBox.Show(
                        "Round trip completed successfully."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Generated file:"
                        + Environment.NewLine
                        + result.GeneratedFilePath,
                        "Language Round Trip",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                string differences =
                    result.Differences == null ||
                    result.Differences.Count == 0
                        ? "No detailed differences were reported."
                        : string.Join(
                            Environment.NewLine,
                            result.Differences);

                MessageBox.Show(
                    "The round trip completed, but differences were found."
                    + Environment.NewLine
                    + Environment.NewLine
                    + differences
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Generated file:"
                    + Environment.NewLine
                    + result.GeneratedFilePath,
                    "Language Round Trip",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "The round trip test failed."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception,
                    "Language Round Trip",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Metodo temporal 

        private void TestLanguageEditorService()
        {
            var service =
                new NoteHighlightAddin.Highlighting.KeywordGroups.Services
                    .LanguageEditorService();

            try
            {
                EditableLanguageConfiguration configuration =
                    service.Load(
                        "python");

                string destinationPath =
                    Path.Combine(
                        Path.GetTempPath(),
                        "python.editor-service-test.lang");

                service.SaveAs(
                    configuration,
                    destinationPath);

                MessageBox.Show(
                    "LanguageEditorService test completed successfully."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Language:"
                    + Environment.NewLine
                    + configuration.Language
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Groups loaded:"
                    + Environment.NewLine
                    + configuration.Groups.Count
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Generated file:"
                    + Environment.NewLine
                    + destinationPath,
                    "Language Editor Service",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "LanguageEditorService test failed."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception,
                    "Language Editor Service",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void lbxLanguages_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            _languageRibbonController.LoadSelectedLanguageConfiguration();
        }

        private void BtnFont_Click(
            object sender,
            EventArgs e)
        {
            fontDialog1.Font =
                new Font(
                    NoteHighlightForm.Properties.Settings.Default.Font,
                    NoteHighlightForm.Properties.Settings.Default.FontSize);

            if (fontDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            btnFont.Text =
                "Font:" +
                fontDialog1.Font.Name +
                ", Size:" +
                fontDialog1.Font.Size;

            btnFont.Font =
                fontDialog1.Font;

            NoteHighlightForm.Properties.Settings.Default.Font =
                fontDialog1.Font.Name;

            NoteHighlightForm.Properties.Settings.Default.FontSize =
                (int)Math.Round(
                    fontDialog1.Font.Size);

            NoteHighlightForm.Properties.Settings.Default.Save();
        }

        private void ChShowTableBorder_CheckedChanged(
            object sender,
            EventArgs e)
        {
            NoteHighlightForm.Properties.Settings.Default.ShowTableBorder =
                cbShowTableBorder.Checked;

            NoteHighlightForm.Properties.Settings.Default.Save();
        }

        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            _languageEditor.ConfigurationChanged -=
                LanguageEditor_ConfigurationChanged;

            base.OnFormClosed(
                e);
        }


        private async void SettingsForm_Shown(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            WindowState = FormWindowState.Normal;

            SetForegroundWindow(Handle);

            await InitializePreviewWebViewAsync();

            _languageRibbonController.RefreshLanguageList();
        }

        private void BtnRemoveLanguage_Click(
            object sender,
            EventArgs e)
        {
            _languageRibbonController.RemoveSelectedLanguage();
        }

        private void BtnAddLanguage_Click(
            object sender,
            EventArgs e)
        {
            _languageRibbonController.AddSelectedLanguage();
        }

        private void lblAddLanguage_Click(
            object sender,
            EventArgs e)
        {
        }

        private void lbxKeywordGroups_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            _groupSelectionController.RefreshSelection();
        }

        private void LanguageEditor_ConfigurationChanged(
            object sender,
            EventArgs e)
        {
            UpdateWindowTitle();
        }


        private void UpdateWindowTitle()
        {
            Text =
                _languageEditor.HasUnsavedChanges
                    ? "SettingsForm *"
                    : "SettingsForm";
        }

        private void txtGroupName_TextChanged(
            object sender,
            EventArgs e)
        {
            _groupDetailsController.ApplyChanges();
        }

        private void txtGroupDescription_TextChanged(
            object sender,
            EventArgs e)
        {
            _groupDetailsController.ApplyChanges();
        }

        private void nudGroupPriority_ValueChanged(
            object sender,
            EventArgs e)
        {
            // Priority is managed automatically by Move Up / Move Down.
        }

        private void chkGroupVisible_CheckedChanged(
            object sender,
            EventArgs e)
        {
            _groupDetailsController.ApplyChanges();
        }

        private void chkGroupBold_CheckedChanged(
            object sender,
            EventArgs e)
        {
            _groupDetailsController.ApplyChanges();
        }

        private void chkGroupItalic_CheckedChanged(
            object sender,
            EventArgs e)
        {
            _groupDetailsController.ApplyChanges();
        }

        private void cmbGroupColour_TextChanged(
            object sender,
            EventArgs e)
        {
            _groupDetailsController.ApplyChanges();
        }

        private void btnAddGroupWord_Click(
            object sender,
            EventArgs e)
        {
            _wordEditorController.AddWord();
        }

        private void btnRemoveGroupWord_Click(
            object sender,
            EventArgs e)
        {
            _wordEditorController.RemoveSelectedWord();
        }

        private void lbxGroupWords_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            _wordEditorController.UpdateState();
        }

        private void txtNewGroupWord_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            _wordEditorController.HandleWordInputKeyDown(
                e);
        }


        //adding button
        private void btnRefreshPreview_Click(object sender, EventArgs e)
        {
            TestPreview();
        }




    }
}