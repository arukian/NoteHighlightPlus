using GenerateHighlightContent;
using Infrastructure.Core;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.KeywordGroups.Services;
using NoteHighlightAddin.Highlighting.KeywordGroups.Testing;
using NoteHighlightAddin.Highlighting.KeywordGroups.ViewModels;
using NoteHighlightAddin.Highlighting.Preview.Services;
using NoteHighlightAddin.Highlighting.Themes;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;



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
        private readonly IHighlightThemeReader _themeReader;
        private readonly IHighlightThemeSerializer _themeSerializer;
        private HighlightTheme _activeTheme;
        private string _activeThemeFilePath;
        private bool _hasUnsavedThemeChanges;
        private bool _isChangingThemeSelection;
        private int _previousThemeIndex = -1;
        private bool _isRefreshingThemeStyle;
        private bool _isRefreshingThemeStyleTarget;
        private readonly Timer _previewRefreshTimer;
        private bool _isGeneratingPreview;
        private bool _previewRefreshPending;
        private bool _isFormClosingConfirmed;
        // Adding to protect language change 
        private bool _isChangingLanguageSelection;
        private int _previousLanguageIndex = -1;

        private const string ThemePreferenceFolderName =
            "NoteHighlightPlus";

        private const string ThemePreferenceFileName =
            "last-theme.txt";

        private sealed class ThemeStyleTargetItem
        {
            public string DisplayName { get; set; }
            public string TechnicalName { get; set; }
            public int? KeywordGroupId { get; set; }
            public string GeneralStyleName { get; set; }
            public string AliasName { get; set; }
            public string AliasTarget { get; set; }
            public bool IsHeader { get; set; }

            public bool IsAlias
            {
                get
                {
                    return !string.IsNullOrWhiteSpace(
                        AliasName);
                }
            }

            public override string ToString()
            {
                return DisplayName ?? string.Empty;
            }
        }


        public SettingsForm()
        {
            InitializeComponent();

            // Closing form confirmation before saving the language configuration to prevent accidental loss of unsaved changes

            FormClosing -= SettingsForm_FormClosing;

            FormClosing += SettingsForm_FormClosing;

            // adding save group functionality to save the language configuration

            btnSaveLanguage.Click -= btnSaveLanguage_Click;

            btnSaveLanguage.Click += btnSaveLanguage_Click;

            // Connect the form events explicitly. This avoids depending on the WinForms designer event wiring.
            Shown += SettingsForm_Shown;

            _languageEditor =
                new LanguageEditorViewModel(
                    new LanguageEditorService());

            _previewHtmlService =
             new PreviewHtmlService();

            _previewSampleCodeService =
                new PreviewSampleCodeService();

            _themeReader =
                new HighlightThemeReader();

            _themeSerializer =
                new HighlightThemeSerializer();

            cmbThemes.SelectedIndexChanged -=
                cmbThemes_SelectedIndexChanged;

            cmbThemes.SelectedIndexChanged +=
                cmbThemes_SelectedIndexChanged;

            cmbThemeStyleTarget.SelectedIndexChanged -=
                cmbThemeStyleTarget_SelectedIndexChanged;

            cmbThemeStyleTarget.SelectedIndexChanged +=
                cmbThemeStyleTarget_SelectedIndexChanged;

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

            txtGroupName.Leave -= txtGroupName_Leave;

            txtGroupName.Leave += txtGroupName_Leave;

            txtGroupDescription.Leave -= txtGroupDescription_Leave;

            txtGroupDescription.Leave += txtGroupDescription_Leave;

            btnChangeThemeColour.Click -=
                btnChangeThemeColour_Click;

            btnChangeThemeColour.Click +=
                btnChangeThemeColour_Click;

            btnSaveTheme.Click -=
                btnSaveTheme_Click;

            btnSaveTheme.Click +=
                btnSaveTheme_Click;

            btnNewTheme.Click -=
                btnNewTheme_Click;

            btnNewTheme.Click +=
                btnNewTheme_Click;

            btnDuplicateTheme.Click -=
                btnDuplicateTheme_Click;

            btnDuplicateTheme.Click +=
                btnDuplicateTheme_Click;

            btnRenameTheme.Click -=
                btnRenameTheme_Click;

            btnRenameTheme.Click +=
                btnRenameTheme_Click;

            btnDeleteTheme.Click -=
                btnDeleteTheme_Click;

            btnDeleteTheme.Click +=
                btnDeleteTheme_Click;

            chkThemeBold.CheckedChanged -=
                chkThemeBold_CheckedChanged;

            chkThemeBold.CheckedChanged +=
                chkThemeBold_CheckedChanged;

            chkThemeItalic.CheckedChanged -=
                chkThemeItalic_CheckedChanged;

            chkThemeItalic.CheckedChanged +=
                chkThemeItalic_CheckedChanged;

            InitializeGroupManagementControls();

            KeyPreview = true;

            KeyDown -=
                SettingsForm_KeyDown;

            KeyDown +=
                SettingsForm_KeyDown;

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

            UpdateWindowTitle();
            UpdateSaveButtonState();
            ClearThemeStylePreview();
        }

        // adding call to save functionality to save the language configuration when the save button is clicked
        private void btnSaveLanguage_Click(object sender, EventArgs e)
        {
            SaveCurrentLanguage();
        }

        private bool TrySaveCurrentLanguage()
        {
            if (!_languageEditor.HasConfiguration)
            {
                return false;
            }

            try
            {
                _groupDetailsController.ApplyChanges(
                    false);

                _languageEditor.Save();

                _groupSelectionController
                    .RefreshSelectedListItem();

                UpdateWindowTitle();
                UpdateSaveButtonState();

                return true;
            }
            catch (UnauthorizedAccessException exception)
            {
                MessageBox.Show(
                    this,
                    "The language files could not be saved because access was denied."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Save Language",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
            catch (IOException exception)
            {
                MessageBox.Show(
                    this,
                    "An error occurred while writing the language files."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Save Language",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    "The language configuration could not be saved."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Save Language",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }

        private void SaveCurrentLanguage()
        {
            if (!_languageEditor.HasConfiguration)
            {
                MessageBox.Show(
                    this,
                    "Select a language before saving.",
                    "Save Language",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (!TrySaveCurrentLanguage())
            {
                return;
            }

            MessageBox.Show(
                this,
                "The language configuration was saved successfully.",
                "Save Language",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // adding confirmation for group saving when the form is closing to prevent accidental loss of unsaved changes

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isFormClosingConfirmed)
            {
                return;
            }

            if (!ConfirmPendingLanguageChanges())
            {
                e.Cancel = true;
                return;
            }

            if (!ConfirmPendingThemeChanges())
            {
                e.Cancel = true;
                return;
            }

            _isFormClosingConfirmed =
                true;
        }

        private void SettingsForm_KeyDown(
    object sender,
    KeyEventArgs e)
        {
            if (!e.Control ||
                e.KeyCode != Keys.S)
            {
                return;
            }

            if (!btnSaveLanguage.Enabled)
            {
                return;
            }

            SaveCurrentLanguage();

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void UpdateSaveButtonState()
        {
            btnSaveLanguage.Enabled =
                _languageEditor.HasConfiguration &&
                _languageEditor.HasUnsavedChanges;
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
                string userDataFolder =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        "NoteHighlightPlus",
                        "WebView2");

                Directory.CreateDirectory(
                    userDataFolder);

                CoreWebView2Environment environment =
                    await CoreWebView2Environment.CreateAsync(
                        browserExecutableFolder: null,
                        userDataFolder: userDataFolder,
                        options: null);

                await _previewWebView.EnsureCoreWebView2Async(
                    environment);

                _previewWebView.NavigationCompleted -=
                    PreviewWebView_NavigationCompleted;

                _previewWebView.NavigationCompleted +=
                    PreviewWebView_NavigationCompleted;

                lblPreviewStatus.Text =
                    "Preview ready.";

                RequestPreviewRefresh();
            }
            catch (Exception exception)
            {
                lblPreviewStatus.Text =
                    "WebView2 initialization failed.";

                MessageBox.Show(
                    this,
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

            string previewThemePath =
                null;

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

                if (_activeTheme != null)
                {
                    string previewThemeName =
                        "notehighlight_theme_preview_"
                        + Guid.NewGuid().ToString(
                            "N");

                    previewThemePath =
                        Path.Combine(
                            PathManager.ThemesFolder,
                            previewThemeName + ".theme");

                    _themeSerializer.Serialize(
                        _activeTheme,
                        previewThemePath);

                    parameter.HighLightStyle =
                        previewThemeName;
                }

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
                DeletePreviewThemeFile(
                    previewThemePath);

                _isGeneratingPreview =
                    false;

                if (_previewRefreshPending)
                {
                    _previewRefreshTimer.Stop();
                    _previewRefreshTimer.Start();
                }
            }
        }

        // adding confirmation for group saving 

        private bool ConfirmPendingLanguageChanges()
        {
            if (!_languageEditor.HasConfiguration ||
                !_languageEditor.HasUnsavedChanges)
            {
                return true;
            }

            string languageName =
                _languageEditor.Configuration?.Language;

            if (string.IsNullOrWhiteSpace(languageName))
            {
                languageName =
                    "the current language";
            }

            DialogResult result =
                MessageBox.Show(
                    this,
                    "Save changes to " +
                    languageName +
                    "?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);

            if (result == DialogResult.Cancel)
            {
                return false;
            }

            if (result == DialogResult.No)
            {
                return true;
            }

            return TrySaveCurrentLanguage();
        }

        private void LoadAvailableThemes()
        {
            try
            {
                _isChangingThemeSelection =
                    true;

                cmbThemes.Items.Clear();

                if (!Directory.Exists(
                    PathManager.ThemesFolder))
                {
                    Directory.CreateDirectory(
                        PathManager.ThemesFolder);
                }

                string[] themeFiles =
                    Directory.GetFiles(
                        PathManager.ThemesFolder,
                        "*.theme",
                        SearchOption.TopDirectoryOnly);

                foreach (string themeFile
                    in themeFiles
                        .OrderBy(
                            path =>
                                Path.GetFileNameWithoutExtension(
                                    path),
                            StringComparer.OrdinalIgnoreCase))
                {
                    cmbThemes.Items.Add(
                        Path.GetFileNameWithoutExtension(
                            themeFile));
                }

                if (cmbThemes.Items.Count == 0)
                {
                    _activeTheme =
                        null;

                    _activeThemeFilePath =
                        null;

                    _previousThemeIndex =
                        -1;

                    cmbThemes.Enabled =
                        false;

                    ClearThemeStylePreview();

                    lblThemeStyleStatus.Text =
                        "No .theme files were found.";

                    return;
                }

                cmbThemes.Enabled =
                    true;

                string preferredTheme =
                    ReadPreferredThemeName();

                int preferredThemeIndex =
                    FindThemeIndex(
                        preferredTheme);

                cmbThemes.SelectedIndex =
                    preferredThemeIndex >= 0
                        ? preferredThemeIndex
                        : 0;

                _previousThemeIndex =
                    cmbThemes.SelectedIndex;
            }
            finally
            {
                _isChangingThemeSelection =
                    false;
            }

            LoadSelectedTheme();
        }


        private int FindThemeIndex(
            string themeName)
        {
            if (string.IsNullOrWhiteSpace(
                themeName))
            {
                return -1;
            }

            for (int index = 0;
                index < cmbThemes.Items.Count;
                index++)
            {
                string item =
                    cmbThemes.Items[index] as string;

                if (string.Equals(
                    item,
                    themeName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }


        private static string GetThemePreferenceFilePath()
        {
            string preferenceFolder =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData),
                    ThemePreferenceFolderName);

            return Path.Combine(
                preferenceFolder,
                ThemePreferenceFileName);
        }


        private static string ReadPreferredThemeName()
        {
            try
            {
                string preferenceFile =
                    GetThemePreferenceFilePath();

                if (!File.Exists(
                    preferenceFile))
                {
                    return null;
                }

                string themeName =
                    File.ReadAllText(
                        preferenceFile)
                        .Trim();

                return string.IsNullOrWhiteSpace(
                    themeName)
                        ? null
                        : themeName;
            }
            catch
            {
                return null;
            }
        }


        private static void SavePreferredThemeName(
            string themeName)
        {
            if (string.IsNullOrWhiteSpace(
                themeName))
            {
                return;
            }

            try
            {
                string preferenceFile =
                    GetThemePreferenceFilePath();

                string preferenceFolder =
                    Path.GetDirectoryName(
                        preferenceFile);

                if (!Directory.Exists(
                    preferenceFolder))
                {
                    Directory.CreateDirectory(
                        preferenceFolder);
                }

                File.WriteAllText(
                    preferenceFile,
                    themeName.Trim());
            }
            catch
            {
                // Optional preference persistence only.
            }
        }


        private void cmbThemes_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (_isChangingThemeSelection)
            {
                return;
            }

            int requestedIndex =
                cmbThemes.SelectedIndex;

            if (requestedIndex < 0 ||
                requestedIndex == _previousThemeIndex)
            {
                return;
            }

            if (!ConfirmPendingThemeChanges())
            {
                try
                {
                    _isChangingThemeSelection =
                        true;

                    cmbThemes.SelectedIndex =
                        _previousThemeIndex;
                }
                finally
                {
                    _isChangingThemeSelection =
                        false;
                }

                return;
            }

            LoadSelectedTheme();

            _previousThemeIndex =
                cmbThemes.SelectedIndex;

            RequestPreviewRefresh();
        }


        private void LoadSelectedTheme()
        {
            string selectedThemeName =
                cmbThemes.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(
                selectedThemeName))
            {
                _activeTheme =
                    null;

                _activeThemeFilePath =
                    null;

                ClearThemeStylePreview();

                return;
            }

            string themePath =
                Path.Combine(
                    PathManager.ThemesFolder,
                    selectedThemeName + ".theme");

            try
            {
                HighlightTheme loadedTheme =
                    _themeReader.Read(
                        themePath);

                _activeTheme =
                    loadedTheme;

                _activeThemeFilePath =
                    themePath;

                _hasUnsavedThemeChanges =
                    false;

                UpdateSaveThemeButtonState();

                SavePreferredThemeName(
                    selectedThemeName);

                RefreshThemeStyleTargetList(
                    false);

                RefreshSelectedThemeStyle();
            }
            catch (Exception exception)
            {
                _activeTheme =
                    null;

                _activeThemeFilePath =
                    null;

                _hasUnsavedThemeChanges =
                    false;

                UpdateSaveThemeButtonState();

                ClearThemeStyleTargetList();

                ClearThemeStylePreview();

                lblThemeStyleStatus.Text =
                    "Theme could not be loaded: " +
                    exception.Message;
            }
        }


        private void cmbThemeStyleTarget_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (_isRefreshingThemeStyleTarget)
            {
                return;
            }

            ThemeStyleTargetItem selectedItem =
                cmbThemeStyleTarget.SelectedItem as ThemeStyleTargetItem;

            if (selectedItem != null && selectedItem.IsHeader)
            {
                SelectFirstEditableThemeStyleTarget();
                return;
            }

            RefreshSelectedThemeStyle();
            RequestPreviewRefresh();
        }


        private void RefreshThemeStyleTargetList(
            bool preserveSelection)
        {
            string previousTechnicalName = null;
            ThemeStyleTargetItem previousItem =
                cmbThemeStyleTarget.SelectedItem as ThemeStyleTargetItem;

            if (preserveSelection &&
                previousItem != null &&
                !previousItem.IsHeader)
            {
                previousTechnicalName = previousItem.TechnicalName;
            }

            try
            {
                _isRefreshingThemeStyleTarget = true;
                cmbThemeStyleTarget.Items.Clear();

                if (_activeTheme == null)
                {
                    cmbThemeStyleTarget.Enabled = false;
                    return;
                }

                cmbThemeStyleTarget.Enabled = true;
                AddThemeStyleHeader("General");

                foreach (string styleName
                    in _activeTheme.Styles.Keys
                        .Where(name =>
                            _activeTheme.StyleAliases == null ||
                            !_activeTheme.StyleAliases.ContainsKey(name))
                        .OrderBy(GetGeneralStyleSortOrder)
                        .ThenBy(
                            name => GetFriendlyGeneralStyleName(name),
                            StringComparer.OrdinalIgnoreCase))
                {
                    cmbThemeStyleTarget.Items.Add(
                        new ThemeStyleTargetItem
                        {
                            DisplayName =
                                "    " + GetFriendlyGeneralStyleName(styleName),
                            TechnicalName = styleName,
                            GeneralStyleName = styleName
                        });
                }

                if (_activeTheme.StyleAliases != null &&
                    _activeTheme.StyleAliases.Count > 0)
                {
                    AddThemeStyleHeader(
                        "Aliases");

                    foreach (var alias
                        in _activeTheme.StyleAliases
                            .OrderBy(
                                entry =>
                                    GetFriendlyGeneralStyleName(
                                        entry.Key),
                                StringComparer.OrdinalIgnoreCase))
                    {
                        cmbThemeStyleTarget.Items.Add(
                            new ThemeStyleTargetItem
                            {
                                DisplayName =
                                    "    "
                                    + GetFriendlyGeneralStyleName(
                                        alias.Key)
                                    + "  →  "
                                    + GetFriendlyGeneralStyleName(
                                        alias.Value),

                                TechnicalName =
                                    alias.Key,

                                AliasName =
                                    alias.Key,

                                AliasTarget =
                                    alias.Value
                            });
                    }
                }

                if (_languageEditor.Configuration != null &&
                    _languageEditor.Configuration.Groups != null &&
                    _languageEditor.Configuration.Groups.Count > 0)
                {
                    AddThemeStyleHeader("Keyword Groups");

                    foreach (KeywordGroupConfiguration group
                        in _languageEditor.Configuration.Groups
                            .Where(item => item != null)
                            .OrderBy(item => item.Id))
                    {
                        string groupName =
                            string.IsNullOrWhiteSpace(group.DisplayName)
                                ? "Group " + group.Id
                                : group.DisplayName;

                        cmbThemeStyleTarget.Items.Add(
                            new ThemeStyleTargetItem
                            {
                                DisplayName =
                                    "    " + groupName +
                                    " (Keywords" + group.Id + ")",
                                TechnicalName =
                                    "Keywords[" + group.Id + "]",
                                KeywordGroupId = group.Id
                            });
                    }
                }

                int selectedIndex =
                    FindThemeStyleTargetIndex(previousTechnicalName);

                if (selectedIndex < 0)
                {
                    selectedIndex = FindCurrentKeywordTargetIndex();
                }

                if (selectedIndex < 0)
                {
                    selectedIndex =
                        FindFirstEditableThemeStyleTargetIndex();
                }

                cmbThemeStyleTarget.SelectedIndex = selectedIndex;
            }
            finally
            {
                _isRefreshingThemeStyleTarget = false;
            }
        }


        private void AddThemeStyleHeader(
            string title)
        {
            cmbThemeStyleTarget.Items.Add(
                new ThemeStyleTargetItem
                {
                    DisplayName = "— " + title + " —",
                    IsHeader = true
                });
        }


        private int FindThemeStyleTargetIndex(
            string technicalName)
        {
            if (string.IsNullOrWhiteSpace(technicalName))
            {
                return -1;
            }

            for (int index = 0;
                index < cmbThemeStyleTarget.Items.Count;
                index++)
            {
                ThemeStyleTargetItem item =
                    cmbThemeStyleTarget.Items[index] as ThemeStyleTargetItem;

                if (item == null || item.IsHeader)
                {
                    continue;
                }

                if (string.Equals(
                    item.TechnicalName,
                    technicalName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }


        private int FindCurrentKeywordTargetIndex()
        {
            KeywordGroupConfiguration selectedGroup =
                _languageEditor.SelectedGroup;

            if (selectedGroup == null)
            {
                return -1;
            }

            return FindThemeStyleTargetIndex(
                "Keywords[" + selectedGroup.Id + "]");
        }


        private int FindFirstEditableThemeStyleTargetIndex()
        {
            for (int index = 0;
                index < cmbThemeStyleTarget.Items.Count;
                index++)
            {
                ThemeStyleTargetItem item =
                    cmbThemeStyleTarget.Items[index] as ThemeStyleTargetItem;

                if (item != null && !item.IsHeader)
                {
                    return index;
                }
            }

            return -1;
        }


        private void SelectFirstEditableThemeStyleTarget()
        {
            int index = FindFirstEditableThemeStyleTargetIndex();

            if (index < 0)
            {
                return;
            }

            try
            {
                _isRefreshingThemeStyleTarget = true;
                cmbThemeStyleTarget.SelectedIndex = index;
            }
            finally
            {
                _isRefreshingThemeStyleTarget = false;
            }

            RefreshSelectedThemeStyle();
        }


        private void ClearThemeStyleTargetList()
        {
            try
            {
                _isRefreshingThemeStyleTarget = true;
                cmbThemeStyleTarget.Items.Clear();
                cmbThemeStyleTarget.Enabled = false;
            }
            finally
            {
                _isRefreshingThemeStyleTarget = false;
            }
        }


        private ThemeStyle GetSelectedThemeStyle(
            out string displayName,
            out string technicalName,
            out bool isKeywordStyle)
        {
            displayName = null;
            technicalName = null;
            isKeywordStyle = false;

            if (_activeTheme == null)
            {
                return null;
            }

            ThemeStyleTargetItem target =
                cmbThemeStyleTarget.SelectedItem as ThemeStyleTargetItem;

            if (target == null || target.IsHeader)
            {
                return null;
            }

            technicalName = target.TechnicalName;

            if (target.KeywordGroupId.HasValue)
            {
                int groupId = target.KeywordGroupId.Value;

                KeywordGroupConfiguration group =
                    _languageEditor.Configuration == null
                        ? null
                        : _languageEditor.Configuration.Groups
                            .FirstOrDefault(item =>
                                item != null && item.Id == groupId);

                displayName =
                    group == null ||
                    string.IsNullOrWhiteSpace(group.DisplayName)
                        ? "Group " + groupId
                        : group.DisplayName;

                isKeywordStyle = true;
                return _activeTheme.GetKeywordStyle(groupId);
            }

            if (target.IsAlias)
            {
                string resolvedTargetName;

                ThemeStyle aliasTargetStyle =
                    ResolveAliasTargetStyle(
                        target.AliasName,
                        out resolvedTargetName);

                displayName =
                    GetFriendlyGeneralStyleName(
                        target.AliasName);

                technicalName =
                    target.AliasName
                    + " → "
                    + (resolvedTargetName
                        ?? target.AliasTarget
                        ?? "(unresolved)");

                return aliasTargetStyle;
            }

            displayName =
                GetFriendlyGeneralStyleName(
                    target.GeneralStyleName);

            ThemeStyle style;

            if (!_activeTheme.Styles.TryGetValue(
                target.GeneralStyleName,
                out style))
            {
                return null;
            }

            return style;
        }


        private ThemeStyle ResolveAliasTargetStyle(
            string aliasName,
            out string resolvedTargetName)
        {
            resolvedTargetName =
                null;

            if (_activeTheme == null ||
                _activeTheme.StyleAliases == null ||
                string.IsNullOrWhiteSpace(
                    aliasName))
            {
                return null;
            }

            string currentName =
                aliasName;

            var visitedNames =
                new System.Collections.Generic.HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            while (!string.IsNullOrWhiteSpace(
                currentName))
            {
                if (!visitedNames.Add(
                    currentName))
                {
                    return null;
                }

                string nextTarget;

                if (_activeTheme.StyleAliases.TryGetValue(
                    currentName,
                    out nextTarget))
                {
                    currentName =
                        nextTarget;

                    continue;
                }

                ThemeStyle resolvedStyle;

                if (_activeTheme.Styles.TryGetValue(
                    currentName,
                    out resolvedStyle))
                {
                    resolvedTargetName =
                        currentName;

                    return resolvedStyle;
                }

                return null;
            }

            return null;
        }


        private static string GetFriendlyGeneralStyleName(
            string styleName)
        {
            if (string.IsNullOrWhiteSpace(styleName))
            {
                return "Unnamed Style";
            }

            switch (styleName.ToLowerInvariant())
            {
                case "default": return "Default Text";
                case "canvas": return "Background";
                case "number": return "Numbers";
                case "escape": return "Escape Characters";
                case "string": return "Strings";
                case "blockcomment": return "Block Comments";
                case "linecomment": return "Line Comments";
                case "preprocessor": return "Preprocessor";
                case "operator": return "Operators";
                case "interpolation": return "Interpolation";
                case "linenum": return "Line Numbers";
                default: return SplitPascalCase(styleName);
            }
        }


        private static int GetGeneralStyleSortOrder(
            string styleName)
        {
            if (string.IsNullOrWhiteSpace(styleName))
            {
                return 1000;
            }

            switch (styleName.ToLowerInvariant())
            {
                case "default": return 0;
                case "canvas": return 10;
                case "number": return 20;
                case "string": return 30;
                case "escape": return 40;
                case "blockcomment": return 50;
                case "linecomment": return 60;
                case "preprocessor": return 70;
                case "operator": return 80;
                case "interpolation": return 90;
                case "linenum": return 100;
                default: return 500;
            }
        }


        private static string SplitPascalCase(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var builder = new System.Text.StringBuilder();

            for (int index = 0; index < value.Length; index++)
            {
                char current = value[index];

                if (index > 0 &&
                    char.IsUpper(current) &&
                    (char.IsLower(value[index - 1]) ||
                     char.IsDigit(value[index - 1])))
                {
                    builder.Append(' ');
                }

                builder.Append(current);
            }

            return builder.ToString();
        }


        private void RefreshSelectedThemeStyle()
        {
            string displayName;
            string technicalName;
            bool isKeywordStyle;

            ThemeStyle style =
                GetSelectedThemeStyle(
                    out displayName,
                    out technicalName,
                    out isKeywordStyle);

            if (_activeTheme == null)
            {
                ClearThemeStylePreview();

                return;
            }

            if (string.IsNullOrWhiteSpace(
                technicalName))
            {
                ClearThemeStylePreview();

                return;
            }

            lblThemeGroupName.Text =
                displayName;

            ThemeStyleTargetItem selectedTarget =
                cmbThemeStyleTarget.SelectedItem
                as ThemeStyleTargetItem;

            if (isKeywordStyle)
            {
                lblThemeUses.Text =
                    "Uses:";

                lblThemeStyleSlot.Text =
                    technicalName.Replace(
                        "[",
                        string.Empty)
                    .Replace(
                        "]",
                        string.Empty);
            }
            else if (selectedTarget != null &&
                     selectedTarget.IsAlias)
            {
                lblThemeUses.Text =
                    "Alias:";

                lblThemeStyleSlot.Text =
                    technicalName;
            }
            else
            {
                lblThemeUses.Text =
                    "Type:";

                lblThemeStyleSlot.Text =
                    "General style";
            }

            if (style == null)
            {
                pnlGroupColourPreview.BackColor =
                    SystemColors.Control;

                lblGroupColourValue.Text =
                    "(style not defined)";

                lblThemeStyleStatus.Text =
                    "Theme: " +
                    _activeTheme.Name +
                    " | " +
                    technicalName;

                btnChangeThemeColour.Enabled =
                    false;

                SetThemeStyleEditorState(
                    null);

                return;
            }

            btnChangeThemeColour.Enabled =
                true;

            SetThemeStyleEditorState(
                style);

            Color parsedColour;

            if (TryParseThemeColour(
                style.Colour,
                out parsedColour))
            {
                pnlGroupColourPreview.BackColor =
                    parsedColour;
            }
            else
            {
                pnlGroupColourPreview.BackColor =
                    SystemColors.Control;
            }

            lblGroupColourValue.Text =
                style.Colour;

            lblThemeStyleStatus.Text =
                "Theme: " +
                _activeTheme.Name;
        }


        private void ClearThemeStylePreview()
        {
            pnlGroupColourPreview.BackColor =
                SystemColors.Control;

            lblGroupColourValue.Text =
                "(no group selected)";

            lblThemeStyleStatus.Text =
                "Theme style not loaded.";

            lblThemeGroupName.Text =
                "(no group selected)";

            lblThemeStyleSlot.Text =
                "-";

            btnChangeThemeColour.Enabled =
                false;

            SetThemeStyleEditorState(
                null);
        }

        private void SetThemeStyleEditorState(
            ThemeStyle style)
        {
            try
            {
                _isRefreshingThemeStyle =
                    true;

                bool hasStyle =
                    style != null;

                chkThemeBold.Enabled =
                    hasStyle;

                chkThemeItalic.Enabled =
                    hasStyle;

                chkThemeBold.Checked =
                    hasStyle &&
                    style.Bold;

                chkThemeItalic.Checked =
                    hasStyle &&
                    style.Italic;
            }
            finally
            {
                _isRefreshingThemeStyle =
                    false;
            }
        }

        private void chkThemeBold_CheckedChanged(
            object sender,
            EventArgs e)
        {
            if (_isRefreshingThemeStyle)
            {
                return;
            }

            ApplyThemeFontStyleChanges();
        }

        private void chkThemeItalic_CheckedChanged(
            object sender,
            EventArgs e)
        {
            if (_isRefreshingThemeStyle)
            {
                return;
            }

            ApplyThemeFontStyleChanges();
        }

        private void ApplyThemeFontStyleChanges()
        {
            if (_activeTheme == null)
            {
                return;
            }

            string displayName;
            string technicalName;
            bool isKeywordStyle;

            ThemeStyle style =
                GetSelectedThemeStyle(
                    out displayName,
                    out technicalName,
                    out isKeywordStyle);

            if (style == null)
            {
                return;
            }

            bool newBold =
                chkThemeBold.Checked;

            bool newItalic =
                chkThemeItalic.Checked;

            if (style.Bold == newBold &&
                style.Italic == newItalic)
            {
                return;
            }

            style.Bold =
                newBold;

            style.Italic =
                newItalic;

            _hasUnsavedThemeChanges =
                true;

            UpdateSaveThemeButtonState();

            RefreshSelectedThemeStyle();

            RequestPreviewRefresh();
        }


        private void btnChangeThemeColour_Click(
            object sender,
            EventArgs e)
        {
            if (_activeTheme == null)
            {
                return;
            }

            string displayName;
            string technicalName;
            bool isKeywordStyle;

            ThemeStyle style =
                GetSelectedThemeStyle(
                    out displayName,
                    out technicalName,
                    out isKeywordStyle);

            if (style == null)
            {
                return;
            }

            Color currentColour;

            using (var colourDialog =
                new ColorDialog())
            {
                colourDialog.AnyColor =
                    true;

                colourDialog.FullOpen =
                    true;

                if (TryParseThemeColour(
                    style.Colour,
                    out currentColour))
                {
                    colourDialog.Color =
                        currentColour;
                }

                if (colourDialog.ShowDialog(
                    this) != DialogResult.OK)
                {
                    return;
                }

                style.Colour =
                    ToThemeColour(
                        colourDialog.Color);

                // A direct edit should affect only this style.
                // If the original colour came from a shared variable,
                // convert this style to a literal colour instead of
                // changing the shared variable.
                style.ColourReference =
                    null;
            }

            _hasUnsavedThemeChanges =
                true;

            UpdateSaveThemeButtonState();

            RefreshSelectedThemeStyle();

            RequestPreviewRefresh();
        }

        private void btnNewTheme_Click(
            object sender,
            EventArgs e)
        {
            if (_activeTheme == null)
            {
                return;
            }

            if (!ConfirmPendingThemeChanges())
            {
                return;
            }

            string newThemeName;
            bool copyCurrentTheme;

            if (!ShowNewThemeDialog(
                out newThemeName,
                out copyCurrentTheme))
            {
                return;
            }

            string normalizedThemeName;
            string validationMessage;

            if (!TryNormalizeThemeName(
                newThemeName,
                out normalizedThemeName,
                out validationMessage))
            {
                MessageBox.Show(
                    this,
                    validationMessage,
                    "New Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string destinationPath =
                Path.Combine(
                    PathManager.ThemesFolder,
                    normalizedThemeName + ".theme");

            if (File.Exists(
                destinationPath))
            {
                MessageBox.Show(
                    this,
                    "A theme named '"
                    + normalizedThemeName
                    + "' already exists.",
                    "New Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                HighlightTheme newTheme =
                    copyCurrentTheme
                        ? CloneTheme(
                            _activeTheme,
                            normalizedThemeName)
                        : CreateCleanTheme(
                            _activeTheme,
                            normalizedThemeName);

                _themeSerializer.Serialize(
                    newTheme,
                    destinationPath);

                SavePreferredThemeName(
                    normalizedThemeName);

                ReloadThemesAndSelect(
                    normalizedThemeName);

                RequestPreviewRefresh();
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    "The theme could not be created."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "New Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private HighlightTheme CreateCleanTheme(
            HighlightTheme structuralTemplate,
            string newName)
        {
            if (structuralTemplate == null)
            {
                throw new ArgumentNullException(
                    nameof(structuralTemplate));
            }

            var theme =
                new HighlightTheme
                {
                    Name =
                        newName,

                    Description =
                        "Custom NoteHighlight+ theme."
                };

            theme.Categories.Add(
                "light");

            foreach (var styleEntry
                in structuralTemplate.Styles)
            {
                theme.Styles.Add(
                    styleEntry.Key,
                    CreateCleanStyleForSlot(
                        styleEntry.Key));
            }

            foreach (var alias
                in structuralTemplate.StyleAliases)
            {
                theme.StyleAliases.Add(
                    alias.Key,
                    alias.Value);
            }

            int keywordCount =
                Math.Max(
                    structuralTemplate.KeywordStyles.Count,
                    GetRequiredKeywordStyleCount());

            for (int index = 0;
                index < keywordCount;
                index++)
            {
                theme.KeywordStyles.Add(
                    new ThemeStyle
                    {
                        Colour =
                            "#0000FF",

                        Bold =
                            false,

                        Italic =
                            false
                    });
            }

            foreach (SemanticTokenStyle token
                in structuralTemplate.SemanticTokenTypes)
            {
                theme.SemanticTokenTypes.Add(
                    new SemanticTokenStyle
                    {
                        Type =
                            token.Type,

                        StyleReference =
                            token.StyleReference
                    });
            }

            return theme;
        }


        private static ThemeStyle CreateCleanStyleForSlot(
            string styleName)
        {
            string normalizedName =
                styleName
                    ?? string.Empty;

            string colour =
                "#000000";

            bool italic =
                false;

            if (string.Equals(
                normalizedName,
                "Canvas",
                StringComparison.OrdinalIgnoreCase))
            {
                colour =
                    "#FFFFFF";
            }
            else if (normalizedName.IndexOf(
                "Comment",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                colour =
                    "#008000";

                italic =
                    true;
            }
            else if (normalizedName.IndexOf(
                "String",
                StringComparison.OrdinalIgnoreCase) >= 0 ||
                normalizedName.IndexOf(
                    "Escape",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                colour =
                    "#A31515";
            }
            else if (normalizedName.IndexOf(
                "Number",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                colour =
                    "#098658";
            }
            else if (normalizedName.IndexOf(
                "PreProcessor",
                StringComparison.OrdinalIgnoreCase) >= 0)
            {
                colour =
                    "#AF00DB";
            }

            return new ThemeStyle
            {
                Colour =
                    colour,

                Bold =
                    false,

                Italic =
                    italic
            };
        }


        private int GetRequiredKeywordStyleCount()
        {
            if (_languageEditor.Configuration == null ||
                _languageEditor.Configuration.Groups == null ||
                _languageEditor.Configuration.Groups.Count == 0)
            {
                return 6;
            }

            int maximumGroupId =
                _languageEditor.Configuration.Groups
                    .Where(
                        group =>
                            group != null)
                    .Select(
                        group =>
                            group.Id)
                    .DefaultIfEmpty(
                        6)
                    .Max();

            return Math.Max(
                6,
                maximumGroupId);
        }


        private bool ShowNewThemeDialog(
            out string themeName,
            out bool copyCurrentTheme)
        {
            themeName =
                null;

            copyCurrentTheme =
                false;

            using (var dialog =
                new Form())
            {
                dialog.Text =
                    "New Theme";

                dialog.FormBorderStyle =
                    FormBorderStyle.FixedDialog;

                dialog.StartPosition =
                    FormStartPosition.CenterParent;

                dialog.MinimizeBox =
                    false;

                dialog.MaximizeBox =
                    false;

                dialog.ShowInTaskbar =
                    false;

                dialog.ClientSize =
                    new Size(
                        390,
                        185);

                var nameLabel =
                    new Label
                    {
                        AutoSize = true,
                        Left = 12,
                        Top = 14,
                        Text = "Theme name:"
                    };

                var nameTextBox =
                    new TextBox
                    {
                        Left = 12,
                        Top = 35,
                        Width = 366,
                        Text = "my-theme"
                    };

                var baseLabel =
                    new Label
                    {
                        AutoSize = true,
                        Left = 12,
                        Top = 70,
                        Text = "Base:"
                    };

                var cleanThemeRadio =
                    new RadioButton
                    {
                        AutoSize = true,
                        Left = 28,
                        Top = 92,
                        Checked = true,
                        Text = "Default clean theme"
                    };

                var copyThemeRadio =
                    new RadioButton
                    {
                        AutoSize = true,
                        Left = 28,
                        Top = 116,
                        Text = "Copy current theme"
                    };

                var createButton =
                    new Button
                    {
                        Text = "Create",
                        DialogResult = DialogResult.OK,
                        Left = 222,
                        Top = 150,
                        Width = 75
                    };

                var cancelButton =
                    new Button
                    {
                        Text = "Cancel",
                        DialogResult = DialogResult.Cancel,
                        Left = 303,
                        Top = 150,
                        Width = 75
                    };

                dialog.Controls.Add(
                    nameLabel);

                dialog.Controls.Add(
                    nameTextBox);

                dialog.Controls.Add(
                    baseLabel);

                dialog.Controls.Add(
                    cleanThemeRadio);

                dialog.Controls.Add(
                    copyThemeRadio);

                dialog.Controls.Add(
                    createButton);

                dialog.Controls.Add(
                    cancelButton);

                dialog.AcceptButton =
                    createButton;

                dialog.CancelButton =
                    cancelButton;

                dialog.Shown +=
                    (sender, e) =>
                    {
                        nameTextBox.Focus();
                        nameTextBox.SelectAll();
                    };

                if (dialog.ShowDialog(
                    this) != DialogResult.OK)
                {
                    return false;
                }

                themeName =
                    nameTextBox.Text;

                copyCurrentTheme =
                    copyThemeRadio.Checked;

                return true;
            }
        }


        private void btnRenameTheme_Click(
            object sender,
            EventArgs e)
        {
            if (_activeTheme == null ||
                string.IsNullOrWhiteSpace(
                    _activeThemeFilePath))
            {
                return;
            }

            if (!ConfirmPendingThemeChanges())
            {
                return;
            }

            string currentThemeName =
                cmbThemes.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(
                currentThemeName))
            {
                currentThemeName =
                    _activeTheme.Name;
            }

            string requestedName =
                ShowThemeNameDialog(
                    currentThemeName,
                    "Rename Theme",
                    "New theme name:");

            if (requestedName == null)
            {
                return;
            }

            string normalizedThemeName;
            string validationMessage;

            if (!TryNormalizeThemeName(
                requestedName,
                out normalizedThemeName,
                out validationMessage))
            {
                MessageBox.Show(
                    this,
                    validationMessage,
                    "Rename Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (string.Equals(
                currentThemeName,
                normalizedThemeName,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string destinationPath =
                Path.Combine(
                    PathManager.ThemesFolder,
                    normalizedThemeName + ".theme");

            if (File.Exists(
                destinationPath))
            {
                MessageBox.Show(
                    this,
                    "A theme named '"
                    + normalizedThemeName
                    + "' already exists.",
                    "Rename Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                HighlightTheme renamedTheme =
                    CloneTheme(
                        _activeTheme,
                        normalizedThemeName);

                _themeSerializer.Serialize(
                    renamedTheme,
                    destinationPath);

                if (File.Exists(
                    _activeThemeFilePath))
                {
                    File.Delete(
                        _activeThemeFilePath);
                }

                SavePreferredThemeName(
                    normalizedThemeName);

                ReloadThemesAndSelect(
                    normalizedThemeName);

                RequestPreviewRefresh();
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    "The theme could not be renamed."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Rename Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnDeleteTheme_Click(
            object sender,
            EventArgs e)
        {
            if (_activeTheme == null ||
                string.IsNullOrWhiteSpace(
                    _activeThemeFilePath))
            {
                return;
            }

            if (cmbThemes.Items.Count <= 1)
            {
                MessageBox.Show(
                    this,
                    "The last remaining theme cannot be deleted.",
                    "Delete Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (!ConfirmPendingThemeChanges())
            {
                return;
            }

            string themeName =
                cmbThemes.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(
                themeName))
            {
                themeName =
                    _activeTheme.Name;
            }

            DialogResult result =
                MessageBox.Show(
                    this,
                    "Delete theme '"
                    + themeName
                    + "'?"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "This action cannot be undone.",
                    "Delete Theme",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            int deletedIndex =
                cmbThemes.SelectedIndex;

            try
            {
                if (File.Exists(
                    _activeThemeFilePath))
                {
                    File.Delete(
                        _activeThemeFilePath);
                }

                string nextThemeName =
                    GetNextThemeNameAfterDeletion(
                        deletedIndex);

                if (string.IsNullOrWhiteSpace(
                    nextThemeName))
                {
                    ClearPreferredThemeName();

                    LoadAvailableThemes();

                    RequestPreviewRefresh();

                    return;
                }

                SavePreferredThemeName(
                    nextThemeName);

                ReloadThemesAndSelect(
                    nextThemeName);

                RequestPreviewRefresh();
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    "The theme could not be deleted."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Delete Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private string GetNextThemeNameAfterDeletion(
            int deletedIndex)
        {
            var remainingThemes =
                Directory.GetFiles(
                    PathManager.ThemesFolder,
                    "*.theme",
                    SearchOption.TopDirectoryOnly)
                .Select(
                    path =>
                        Path.GetFileNameWithoutExtension(
                            path))
                .OrderBy(
                    name => name,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (remainingThemes.Count == 0)
            {
                return null;
            }

            int nextIndex =
                deletedIndex;

            if (nextIndex < 0)
            {
                nextIndex = 0;
            }

            if (nextIndex >= remainingThemes.Count)
            {
                nextIndex =
                    remainingThemes.Count - 1;
            }

            return remainingThemes[nextIndex];
        }


        private static void ClearPreferredThemeName()
        {
            try
            {
                string preferenceFile =
                    GetThemePreferenceFilePath();

                if (File.Exists(
                    preferenceFile))
                {
                    File.Delete(
                        preferenceFile);
                }
            }
            catch
            {
                // Preference cleanup is optional.
            }
        }


        private void btnDuplicateTheme_Click(
            object sender,
            EventArgs e)
        {
            if (_activeTheme == null ||
                string.IsNullOrWhiteSpace(
                    _activeThemeFilePath))
            {
                return;
            }

            if (!ConfirmPendingThemeChanges())
            {
                return;
            }

            string sourceThemeName =
                cmbThemes.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(
                sourceThemeName))
            {
                sourceThemeName =
                    _activeTheme.Name;
            }

            string requestedName =
                ShowThemeNameDialog(
                    sourceThemeName + "-copy",
                    "Duplicate Theme",
                    "New theme name:");

            if (requestedName == null)
            {
                return;
            }

            string normalizedThemeName;
            string validationMessage;

            if (!TryNormalizeThemeName(
                requestedName,
                out normalizedThemeName,
                out validationMessage))
            {
                MessageBox.Show(
                    this,
                    validationMessage,
                    "Duplicate Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string destinationPath =
                Path.Combine(
                    PathManager.ThemesFolder,
                    normalizedThemeName + ".theme");

            if (File.Exists(
                destinationPath))
            {
                MessageBox.Show(
                    this,
                    "A theme named '"
                    + normalizedThemeName
                    + "' already exists.",
                    "Duplicate Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                HighlightTheme duplicate =
                    CloneTheme(
                        _activeTheme,
                        normalizedThemeName);

                _themeSerializer.Serialize(
                    duplicate,
                    destinationPath);

                ReloadThemesAndSelect(
                    normalizedThemeName);

                RequestPreviewRefresh();
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    "The theme could not be duplicated."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Duplicate Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void ReloadThemesAndSelect(
            string themeName)
        {
            try
            {
                _isChangingThemeSelection =
                    true;

                cmbThemes.Items.Clear();

                string[] themeFiles =
                    Directory.GetFiles(
                        PathManager.ThemesFolder,
                        "*.theme",
                        SearchOption.TopDirectoryOnly);

                foreach (string themeFile
                    in themeFiles
                        .OrderBy(
                            path =>
                                Path.GetFileNameWithoutExtension(
                                    path),
                            StringComparer.OrdinalIgnoreCase))
                {
                    cmbThemes.Items.Add(
                        Path.GetFileNameWithoutExtension(
                            themeFile));
                }

                int themeIndex =
                    FindThemeIndex(
                        themeName);

                cmbThemes.SelectedIndex =
                    themeIndex >= 0
                        ? themeIndex
                        : 0;

                _previousThemeIndex =
                    cmbThemes.SelectedIndex;
            }
            finally
            {
                _isChangingThemeSelection =
                    false;
            }

            LoadSelectedTheme();
        }


        private static HighlightTheme CloneTheme(
            HighlightTheme source,
            string newName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(
                    nameof(source));
            }

            var clone =
                new HighlightTheme
                {
                    Name =
                        newName,

                    Description =
                        source.Description
                };

            foreach (var variable
                in source.Variables)
            {
                clone.Variables.Add(
                    variable.Key,
                    variable.Value);
            }

            foreach (string category
                in source.Categories)
            {
                clone.Categories.Add(
                    category);
            }

            foreach (var styleEntry
                in source.Styles)
            {
                clone.Styles.Add(
                    styleEntry.Key,
                    CloneThemeStyle(
                        styleEntry.Value));
            }

            foreach (var alias
                in source.StyleAliases)
            {
                clone.StyleAliases.Add(
                    alias.Key,
                    alias.Value);
            }

            foreach (ThemeStyle keywordStyle
                in source.KeywordStyles)
            {
                clone.KeywordStyles.Add(
                    CloneThemeStyle(
                        keywordStyle));
            }

            foreach (SemanticTokenStyle token
                in source.SemanticTokenTypes)
            {
                clone.SemanticTokenTypes.Add(
                    new SemanticTokenStyle
                    {
                        Type =
                            token.Type,

                        StyleReference =
                            token.StyleReference
                    });
            }

            return clone;
        }


        private static ThemeStyle CloneThemeStyle(
            ThemeStyle source)
        {
            if (source == null)
            {
                return new ThemeStyle();
            }

            return new ThemeStyle
            {
                Colour =
                    source.Colour,

                ColourReference =
                    source.ColourReference,

                Bold =
                    source.Bold,

                Italic =
                    source.Italic
            };
        }


        private static bool TryNormalizeThemeName(
            string requestedName,
            out string normalizedName,
            out string validationMessage)
        {
            normalizedName =
                null;

            validationMessage =
                null;

            if (string.IsNullOrWhiteSpace(
                requestedName))
            {
                validationMessage =
                    "Enter a name for the new theme.";

                return false;
            }

            string trimmedName =
                requestedName.Trim();

            if (trimmedName.EndsWith(
                ".theme",
                StringComparison.OrdinalIgnoreCase))
            {
                trimmedName =
                    Path.GetFileNameWithoutExtension(
                        trimmedName);
            }

            if (string.IsNullOrWhiteSpace(
                trimmedName))
            {
                validationMessage =
                    "Enter a valid theme name.";

                return false;
            }

            if (trimmedName.IndexOfAny(
                Path.GetInvalidFileNameChars()) >= 0)
            {
                validationMessage =
                    "The theme name contains characters that are not valid in a file name.";

                return false;
            }

            normalizedName =
                trimmedName;

            return true;
        }


        private string ShowThemeNameDialog(
            string suggestedName,
            string dialogTitle,
            string promptText)
        {
            using (var dialog =
                new Form())
            {
                dialog.Text =
                    string.IsNullOrWhiteSpace(
                        dialogTitle)
                        ? "Theme"
                        : dialogTitle;

                dialog.FormBorderStyle =
                    FormBorderStyle.FixedDialog;

                dialog.StartPosition =
                    FormStartPosition.CenterParent;

                dialog.MinimizeBox =
                    false;

                dialog.MaximizeBox =
                    false;

                dialog.ShowInTaskbar =
                    false;

                dialog.ClientSize =
                    new Size(
                        360,
                        118);

                var label =
                    new Label
                    {
                        AutoSize = true,
                        Left = 12,
                        Top = 14,
                        Text =
                            string.IsNullOrWhiteSpace(
                                promptText)
                                ? "Theme name:"
                                : promptText
                    };

                var textBox =
                    new TextBox
                    {
                        Left = 12,
                        Top = 35,
                        Width = 336,
                        Text = suggestedName ?? string.Empty
                    };

                var okButton =
                    new Button
                    {
                        Text = "OK",
                        DialogResult = DialogResult.OK,
                        Left = 192,
                        Top = 75,
                        Width = 75
                    };

                var cancelButton =
                    new Button
                    {
                        Text = "Cancel",
                        DialogResult = DialogResult.Cancel,
                        Left = 273,
                        Top = 75,
                        Width = 75
                    };

                dialog.Controls.Add(
                    label);

                dialog.Controls.Add(
                    textBox);

                dialog.Controls.Add(
                    okButton);

                dialog.Controls.Add(
                    cancelButton);

                dialog.AcceptButton =
                    okButton;

                dialog.CancelButton =
                    cancelButton;

                dialog.Shown +=
                    (sender, e) =>
                    {
                        textBox.Focus();
                        textBox.SelectAll();
                    };

                return dialog.ShowDialog(
                    this) == DialogResult.OK
                        ? textBox.Text
                        : null;
            }
        }


        private void btnSaveTheme_Click(
            object sender,
            EventArgs e)
        {
            if (!TrySaveCurrentTheme())
            {
                return;
            }

            lblThemeStyleStatus.Text =
                "Theme: "
                + _activeTheme.Name;
        }

        private bool TrySaveCurrentTheme()
        {
            if (_activeTheme == null)
            {
                return false;
            }

            if (!_hasUnsavedThemeChanges)
            {
                return true;
            }

            string themePath =
                !string.IsNullOrWhiteSpace(
                    _activeThemeFilePath)
                    ? _activeThemeFilePath
                    : Path.Combine(
                        PathManager.ThemesFolder,
                        _activeTheme.Name + ".theme");

            try
            {
                _themeSerializer.Serialize(
                    _activeTheme,
                    themePath);

                _hasUnsavedThemeChanges =
                    false;

                UpdateSaveThemeButtonState();

                return true;
            }
            catch (UnauthorizedAccessException exception)
            {
                MessageBox.Show(
                    this,
                    "The theme file could not be saved because access was denied."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Save Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
            catch (IOException exception)
            {
                MessageBox.Show(
                    this,
                    "An error occurred while writing the theme file."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Save Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    "The theme could not be saved."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Save Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }

        private bool ConfirmPendingThemeChanges()
        {
            if (_activeTheme == null ||
                !_hasUnsavedThemeChanges)
            {
                return true;
            }

            string themeName =
                string.IsNullOrWhiteSpace(
                    _activeTheme.Name)
                    ? "the current theme"
                    : _activeTheme.Name;

            DialogResult result =
                MessageBox.Show(
                    this,
                    "Save changes to theme "
                    + themeName
                    + "?",
                    "Unsaved Theme Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);

            if (result == DialogResult.Cancel)
            {
                return false;
            }

            if (result == DialogResult.No)
            {
                return true;
            }

            return TrySaveCurrentTheme();
        }

        private void UpdateSaveThemeButtonState()
        {
            if (btnSaveTheme == null)
            {
                return;
            }

            btnSaveTheme.Enabled =
                _activeTheme != null &&
                _hasUnsavedThemeChanges;

            btnNewTheme.Enabled =
                _activeTheme != null;

            btnDuplicateTheme.Enabled =
                _activeTheme != null;

            btnRenameTheme.Enabled =
                _activeTheme != null;

            btnDeleteTheme.Enabled =
                _activeTheme != null &&
                cmbThemes.Items.Count > 1;
        }


        private static string ToThemeColour(
            Color colour)
        {
            return string.Format(
                "#{0:X2}{1:X2}{2:X2}",
                colour.R,
                colour.G,
                colour.B);
        }

        private static void DeletePreviewThemeFile(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(
                filePath))
            {
                return;
            }

            try
            {
                if (File.Exists(
                    filePath))
                {
                    File.Delete(
                        filePath);
                }
            }
            catch
            {
                // Preview cleanup must never prevent the form
                // from continuing to work.
            }
        }


        private static bool TryParseThemeColour(
            string colourValue,
            out Color colour)
        {
            colour =
                SystemColors.Control;

            if (string.IsNullOrWhiteSpace(
                colourValue))
            {
                return false;
            }

            string normalizedValue =
                colourValue.Trim();

            try
            {
                if (normalizedValue.StartsWith(
                    "#",
                    StringComparison.Ordinal) &&
                    normalizedValue.Length == 9)
                {
                    int alpha =
                        Convert.ToInt32(
                            normalizedValue.Substring(1, 2),
                            16);

                    int red =
                        Convert.ToInt32(
                            normalizedValue.Substring(3, 2),
                            16);

                    int green =
                        Convert.ToInt32(
                            normalizedValue.Substring(5, 2),
                            16);

                    int blue =
                        Convert.ToInt32(
                            normalizedValue.Substring(7, 2),
                            16);

                    colour =
                        Color.FromArgb(
                            alpha,
                            red,
                            green,
                            blue);

                    return true;
                }

                colour =
                    ColorTranslator.FromHtml(
                        normalizedValue);

                return true;
            }
            catch
            {
                return false;
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
                    _activeTheme != null
                        ? _activeTheme.Name
                        : cmbThemes.SelectedItem as string,

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

            RefreshSelectedThemeStyle();
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
            UpdateSaveButtonState();
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
            if (_isChangingLanguageSelection)
            {
                return;
            }

            int requestedLanguageIndex =
                lbxLanguages.SelectedIndex;

            bool isChangingExistingSelection =
                _previousLanguageIndex >= 0 &&
                requestedLanguageIndex != _previousLanguageIndex;

            if (isChangingExistingSelection &&
                !ConfirmPendingLanguageChanges())
            {
                try
                {
                    _isChangingLanguageSelection =
                        true;

                    lbxLanguages.SelectedIndex =
                        _previousLanguageIndex;
                }
                finally
                {
                    _isChangingLanguageSelection =
                        false;
                }

                return;
            }

            try
            {
                _languageRibbonController
                    .LoadSelectedLanguageConfiguration();

                _previousLanguageIndex =
                    lbxLanguages.SelectedIndex;

                UpdateWindowTitle();
                UpdateSaveButtonState();

                RequestPreviewRefresh();
            }
            catch
            {
                try
                {
                    _isChangingLanguageSelection =
                        true;

                    lbxLanguages.SelectedIndex =
                        _previousLanguageIndex;
                }
                finally
                {
                    _isChangingLanguageSelection =
                        false;
                }

                throw;
            }
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


        private async void SettingsForm_Shown(object sender, EventArgs e)
        {

            _isFormClosingConfirmed = false;

            WindowState =
                FormWindowState.Minimized;

            WindowState =
                FormWindowState.Normal;

            SetForegroundWindow(
                Handle);

            await InitializePreviewWebViewAsync();

            _languageRibbonController.RefreshLanguageList();

            EnsureSelectedLanguageIsLoaded();

            LoadAvailableThemes();

            _previousLanguageIndex =
                lbxLanguages.SelectedIndex;

            UpdateWindowTitle();
            UpdateSaveButtonState();

            RequestPreviewRefresh();
        }

        private void EnsureSelectedLanguageIsLoaded()
        {
            if (_languageEditor.HasConfiguration ||
                lbxLanguages.SelectedIndex < 0)
            {
                return;
            }

            try
            {
                _isChangingLanguageSelection =
                    true;

                _languageRibbonController
                    .LoadSelectedLanguageConfiguration();

                _previousLanguageIndex =
                    lbxLanguages.SelectedIndex;

                RefreshSelectedThemeStyle();
            }
            finally
            {
                _isChangingLanguageSelection =
                    false;
            }
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

            ThemeStyleTargetItem currentThemeTarget =
                cmbThemeStyleTarget.SelectedItem as ThemeStyleTargetItem;

            bool preserveThemeTarget =
                currentThemeTarget != null &&
                !currentThemeTarget.IsHeader &&
                !currentThemeTarget.KeywordGroupId.HasValue;

            RefreshThemeStyleTargetList(
                preserveThemeTarget);

            if (!preserveThemeTarget)
            {
                int keywordTargetIndex =
                    FindCurrentKeywordTargetIndex();

                if (keywordTargetIndex >= 0)
                {
                    try
                    {
                        _isRefreshingThemeStyleTarget = true;
                        cmbThemeStyleTarget.SelectedIndex =
                            keywordTargetIndex;
                    }
                    finally
                    {
                        _isRefreshingThemeStyleTarget = false;
                    }
                }
            }

            RefreshSelectedThemeStyle();

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
            _groupDetailsController.ApplyChanges(
                false);

            RefreshThemeStyleTargetList(
                true);
        }

        private void txtGroupDescription_TextChanged(
            object sender,
            EventArgs e)
        {
            _groupDetailsController.ApplyChanges(
                false);
        }

        private void nudGroupPriority_ValueChanged(
            object sender,
            EventArgs e)
        {
            // Priority is managed automatically by Move Up / Move Down.
        }


        // adding events for name update 

        private void txtGroupName_Leave(object sender, EventArgs e)
        {
            _groupSelectionController.RefreshSelectedListItem();
        }

        private void txtGroupDescription_Leave(
            object sender,
            EventArgs e)
        {
            _groupSelectionController.RefreshSelectedListItem();
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