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
using GenerateHighlightContent;


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
        private WebView2 _previewWebView;
        private readonly IPreviewHtmlService _previewHtmlService;
        private readonly IPreviewSampleCodeService _previewSampleCodeService;
        private readonly Timer _previewRefreshTimer;
        private bool _isGeneratingPreview;
        private bool _previewRefreshPending;

        public SettingsForm()
        {
            InitializeComponent();

            // Connect the form events explicitly. This avoids depending on the WinForms designer event wiring.
            Shown += SettingsForm_Shown;

            _languageEditor =
                new LanguageEditorViewModel(
                    new LanguageEditorService());

            _previewHtmlService =
             new PreviewHtmlService();

            _previewSampleCodeService =
                new PreviewSampleCodeService();

            _previewRefreshTimer =
                new Timer
                {
                    Interval = 250
                };

            _previewRefreshTimer.Tick +=
                PreviewRefreshTimer_Tick;

            _languageEditor.ConfigurationChanged +=
                LanguageEditor_ConfigurationChanged;

            // Connect the word editor events explicitly. This avoids depending
            // on the WinForms designer event wiring.
            btnAddGroupWord.Click -= btnAddGroupWord_Click;

            btnAddGroupWord.Click += btnAddGroupWord_Click;

            btnRemoveGroupWord.Click -= btnRemoveGroupWord_Click;

            btnRemoveGroupWord.Click += btnRemoveGroupWord_Click;

            lbxGroupWords.SelectedIndexChanged -= lbxGroupWords_SelectedIndexChanged;

            lbxGroupWords.SelectedIndexChanged += lbxGroupWords_SelectedIndexChanged;

            txtNewGroupWord.KeyDown -= txtNewGroupWord_KeyDown;

            txtNewGroupWord.KeyDown += txtNewGroupWord_KeyDown;

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

            lbxLanguages.SelectedIndexChanged -=
                lbxLanguages_SelectedIndexChanged;

            lbxLanguages.SelectedIndexChanged +=
                lbxLanguages_SelectedIndexChanged;

            _wordEditorController.UpdateState();
        }

        /// <summary>
        /// Initializes the embedded WebView2 preview.
        /// </summary>
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

                _previewWebView.NavigationCompleted += PreviewWebView_NavigationCompleted;

                lblPreviewStatus.Text =
                    "Preview ready.";

                RequestPreviewRefresh();
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

        /// <summary>
        /// Schedules a preview refresh after a short quiet period.
        /// </summary>
        private void RequestPreviewRefresh()
        {
            if (IsDisposed ||
                Disposing)
            {
                return;
            }

            _previewRefreshPending =
                true;

            _previewRefreshTimer.Stop();
            _previewRefreshTimer.Start();
        }

        private void PreviewRefreshTimer_Tick(
            object sender,
            EventArgs e)
        {
            _previewRefreshTimer.Stop();

            if (!_previewRefreshPending)
            {
                return;
            }

            if (_isGeneratingPreview)
            {
                return;
            }

            _previewRefreshPending =
                false;

            RefreshPreview();
        }

        private void PreviewWebView_NavigationCompleted(
            object sender,
            CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                lblPreviewStatus.Text =
                    "Preview loaded successfully.";

                return;
            }

            lblPreviewStatus.Text =
                "Preview could not be loaded: " +
                e.WebErrorStatus;
        }

        /// <summary>
        /// Generates and displays the current language preview.
        /// </summary>
        private void RefreshPreview()
        {
            if (_isGeneratingPreview)
            {
                _previewRefreshPending =
                    true;

                return;
            }

            try
            {
                if (!_languageEditor.HasConfiguration)
                {
                    lblPreviewStatus.Text =
                        "Select a language first.";

                    _previewRefreshPending =
                        false;

                    return;
                }

                if (_previewWebView == null ||
                    _previewWebView.CoreWebView2 == null)
                {
                    lblPreviewStatus.Text =
                        "Preview is not ready.";

                    _previewRefreshPending =
                        true;

                    return;
                }

                _isGeneratingPreview =
                    true;

                lblPreviewStatus.Text =
                    "Generating preview...";

                HighLightParameter parameter =
                    CreatePreviewParameter();

                string htmlPath =
                    _previewHtmlService.GeneratePreviewHtml(
                        _languageEditor.Configuration,
                        parameter);

                ValidatePreviewHtml(
                    htmlPath);

                Uri htmlUri =
                    new Uri(
                        htmlPath);

                _previewWebView.Source =
                    htmlUri;

                lblPreviewStatus.Text =
                    "Loading preview...";
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
            finally
            {
                _isGeneratingPreview =
                    false;

                if (_previewRefreshPending)
                {
                    _previewRefreshTimer.Stop();
                    _previewRefreshTimer.Start();
                }
            }
        }

        private HighLightParameter CreatePreviewParameter()
        {
            return new HighLightParameter
            {
                FileName =
                    CreatePreviewFileName(),

                Content =
                    _previewSampleCodeService.Generate(
                        _languageEditor.Configuration,
                        _languageEditor.SelectedGroup),

                CodeType =
                    _languageEditor.Configuration.Language,

                HighLightStyle =
                    "shinx",

                ShowLineNumber =
                    true,

                HighlightColor =
                    Color.Transparent,

                Font =
                    fontDialog1.Font.Name,

                FontSize =
                    (int)Math.Round(
                        fontDialog1.Font.Size)
            };
        }

        private string CreatePreviewFileName()
        {
            string language =
                _languageEditor.Configuration.Language;

            if (string.Equals(
                language,
                "python",
                StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    language,
                    "python.lang",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "notehighlight_preview.py";
            }

            return "notehighlight_preview.txt";
        }

        private static void ValidatePreviewHtml(
            string htmlPath)
        {
            if (string.IsNullOrWhiteSpace(
                htmlPath))
            {
                throw new InvalidOperationException(
                    "The preview service returned an empty HTML path.");
            }

            if (!File.Exists(
                htmlPath))
            {
                throw new FileNotFoundException(
                    "The preview HTML file was not generated.",
                    htmlPath);
            }

            FileInfo htmlFile =
                new FileInfo(
                    htmlPath);

            if (htmlFile.Length == 0)
            {
                throw new InvalidOperationException(
                    "The generated preview HTML file is empty.");
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

            RequestPreviewRefresh();
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

            RequestPreviewRefresh();
        }

        private void btnMoveKeywordGroupDown_Click(
            object sender,
            EventArgs e)
        {
            _groupEditorController.MoveSelectedGroupDown();

            RequestPreviewRefresh();
        }

        private void btnRemoveKeywordGroup_Click(
            object sender,
            EventArgs e)
        {
            _groupEditorController.RemoveSelectedGroup();

            RequestPreviewRefresh();
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

        private void LanguageEditor_ConfigurationChanged(
            object sender,
            EventArgs e)
        {
            UpdateWindowTitle();
            RequestPreviewRefresh();
        }

        // Temporary diagnostic helper.

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

            RequestPreviewRefresh();
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

            RequestPreviewRefresh();
        }

        private void ChShowTableBorder_CheckedChanged(
            object sender,
            EventArgs e)
        {
            NoteHighlightForm.Properties.Settings.Default.ShowTableBorder =
                cbShowTableBorder.Checked;

            NoteHighlightForm.Properties.Settings.Default.Save();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _languageEditor.ConfigurationChanged -=
                LanguageEditor_ConfigurationChanged;

            if (_previewWebView != null)
            {
                _previewWebView.NavigationCompleted -=
                    PreviewWebView_NavigationCompleted;
            }

            _previewRefreshTimer.Stop();

            _previewRefreshTimer.Tick -=
                PreviewRefreshTimer_Tick;

            _previewRefreshTimer.Dispose();

            _previewHtmlService.Dispose();

            base.OnFormClosed(
                e);
        }


        private async void SettingsForm_Shown(
            object sender,
            EventArgs e)
        {
            WindowState =
                FormWindowState.Minimized;

            WindowState =
                FormWindowState.Normal;

            SetForegroundWindow(
                Handle);

            await InitializePreviewWebViewAsync();

            _languageRibbonController.RefreshLanguageList();

            RequestPreviewRefresh();
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

            RequestPreviewRefresh();
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

            RequestPreviewRefresh();
        }

        private void btnRemoveGroupWord_Click(
            object sender,
            EventArgs e)
        {
            _wordEditorController.RemoveSelectedWord();

            RequestPreviewRefresh();
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
            _wordEditorController.HandleWordInputKeyDown(e);

            if (e.KeyCode == Keys.Enter)
            {
                RequestPreviewRefresh();
            }
        }

    }
}