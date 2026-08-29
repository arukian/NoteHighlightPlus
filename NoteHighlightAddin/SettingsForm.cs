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
        private Button _btnLanguageGroupsTab;
        private Button _btnThemeEditorTab;
        private readonly LanguageEditorViewModel _languageEditor;
        private readonly KeyboardFocusVisualManager _keyboardFocusVisualManager;
        private readonly KeyboardHelpManager _keyboardHelpManager;
        private Label _keyboardHelpLabel;
        private int _settingsKeyboardNavigationIndex = -1;
        private Control[] _settingsKeyboardNavigationRoute;
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
        private readonly ThemePreferenceProvider _themePreferenceProvider;  
        private readonly LanguagePreferenceProvider _languagePreferenceProvider;
        private readonly ThemeResetService _themeResetService;
        private readonly ConfigurationExportService _configurationExportService;
        private readonly ConfigurationImportService _configurationImportService;
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

            _themePreferenceProvider =
                new ThemePreferenceProvider();

            _languagePreferenceProvider =
                new LanguagePreferenceProvider();

            _themeResetService =
                new ThemeResetService();

            _configurationExportService =
                new ConfigurationExportService();

            _configurationImportService =
                new ConfigurationImportService();

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

            btnResetTheme.Click -=
                btnResetTheme_Click;

            btnResetTheme.Click +=
                btnResetTheme_Click;

            btnExportConfiguration.Click -=
                btnExportConfiguration_Click;

            btnExportConfiguration.Click +=
                btnExportConfiguration_Click;

            btnImportConfiguration.Click -=
                btnImportConfiguration_Click;

            btnImportConfiguration.Click +=
                btnImportConfiguration_Click;

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

            ApplyModernUi();
            CreateKeyboardHelpLegend();
            CreateKeyboardHelpButton();

            _keyboardFocusVisualManager =
                new KeyboardFocusVisualManager(
                    this);

            _keyboardHelpManager =
                new KeyboardHelpManager(
                    this,
                    _keyboardHelpLabel,
                    ResolveKeyboardHelp,
                    GetDefaultKeyboardHelp());
        }


        private void CreateKeyboardHelpLegend()
        {
            // Reserve a slim row at the bottom of Preview for keyboard help.
            pnlPreview.Height =
                Math.Max(
                    100,
                    pnlPreview.Height - 30);

            _keyboardHelpLabel =
                new Label
                {
                    Name =
                        "lblKeyboardHelp",

                    AutoEllipsis =
                        true,

                    Location =
                        new Point(
                            14,
                            grpPreview.ClientSize.Height -
                            32),

                    Size =
                        new Size(
                            Math.Max(
                                120,
                                grpPreview.ClientSize.Width -
                                28),
                            22),

                    Anchor =
                        AnchorStyles.Left |
                        AnchorStyles.Right |
                        AnchorStyles.Bottom,

                    TextAlign =
                        ContentAlignment.MiddleLeft,

                    TabStop =
                        false,

                    Text =
                        GetDefaultKeyboardHelp()
                };

            UiStyleManager.StyleLabel(
                _keyboardHelpLabel,
                true);

            _keyboardHelpLabel.Font =
                NoteHighlightUiTheme.CreateSmallFont();

            grpPreview.Controls.Add(
                _keyboardHelpLabel);

            _keyboardHelpLabel.BringToFront();
        }


        private static string GetDefaultKeyboardHelp()
        {
            return
                "Keyboard: Tab = next control  •  Shift+Tab = previous  •  Space = activate";
        }


        private string ResolveKeyboardHelp(
            Control control)
        {
            if (control == _btnLanguageGroupsTab ||
                control == _btnThemeEditorTab)
            {
                return
                    "Tabs: ←/→ = switch tab  •  Space/Enter = open  •  Tab = continue";
            }

            if (_previewWebView != null &&
                control == _previewWebView)
            {
                return
                    "Preview: Tab = continue to the next control";
            }

            if (control is ListBox)
            {
                return
                    "List: ↑/↓ = move selection  •  Tab = next control";
            }

            if (control is ComboBox)
            {
                return
                    "Dropdown: ↑/↓ = choose item  •  Tab = next control";
            }

            if (control is NumericUpDown)
            {
                return
                    "Number: type a value or use ↑/↓  •  Tab = next";
            }

            if (control is TextBoxBase)
            {
                return
                    "Field: type/edit value  •  Tab = next  •  Shift+Tab = previous";
            }

            if (control is CheckBox)
            {
                return
                    "Toggle: Space = change  •  Tab = next  •  Shift+Tab = previous";
            }

            if (control is Button)
            {
                return
                    "Button: Space/Enter = activate  •  Tab = next  •  Shift+Tab = previous";
            }

            return
                GetDefaultKeyboardHelp();
        }


        private void CreateKeyboardHelpButton()
        {
            Button keyboardHelpButton =
                new Button
                {
                    Text =
                        "?",

                    Size =
                        new Size(
                            34,
                            30),

                    Location =
                        new Point(
                            ClientSize.Width - 46,
                            10),

                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Right,

                    TabStop =
                        false
                };

            UiStyleManager.StyleSecondaryButton(
                keyboardHelpButton);

            keyboardHelpButton.Click +=
                delegate
                {
                    KeyboardShortcutsForm.ShowHelp(
                        this);
                };

            Controls.Add(
                keyboardHelpButton);

            keyboardHelpButton.BringToFront();
        }


        private void ApplyModernUi()
        {
            Text =
                "NoteHighlight+ Settings";

            BackColor =
                NoteHighlightUiTheme.WindowBackground;

            ForeColor =
                NoteHighlightUiTheme.TextPrimary;

            Font =
                NoteHighlightUiTheme.CreateBodyFont();

            MinimumSize =
                new Size(
                    980,
                    790);

            ClientSize =
                new Size(
                    980,
                    820);

            UiStyleManager.StyleForm(
                this);

            CreateSettingsHeader();

            btnFont.Location =
                new Point(
                    24,
                    78);

            btnFont.Size =
                new Size(
                    330,
                    30);

            UiStyleManager.StyleSecondaryButton(
                btnFont);

            cbShowTableBorder.Location =
                new Point(
                    376,
                    78);

            cbShowTableBorder.Size =
                new Size(
                    150,
                    30);

            cbShowTableBorder.Text =
                "Show Table Border";

            UiStyleManager.StyleToggleCheckBox(
                cbShowTableBorder,
                FontStyle.Regular);

            btnImportConfiguration.Location =
                new Point(
                    738,
                    78);

            btnImportConfiguration.Size =
                new Size(
                    100,
                    30);

            btnImportConfiguration.Text =
                "Import...";

            UiStyleManager.StyleSecondaryButton(
                btnImportConfiguration);

            btnExportConfiguration.Location =
                new Point(
                    844,
                    78);

            btnExportConfiguration.Size =
                new Size(
                    112,
                    30);

            btnExportConfiguration.Text =
                "Export...";

            UiStyleManager.StyleSecondaryButton(
                btnExportConfiguration);

            tabSettings.Location =
                new Point(
                    20,
                    154);

            tabSettings.Size =
                new Size(
                    936,
                    358);

            tabSettings.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            // Keep the existing TabPages and selection logic, but hide the
            // native WinForms headers. Custom buttons above the control are
            // used as the visible tabs so Windows cannot draw light borders.
            tabSettings.Appearance =
                TabAppearance.FlatButtons;

            tabSettings.SizeMode =
                TabSizeMode.Fixed;

            tabSettings.ItemSize =
                new Size(
                    0,
                    1);

            tabSettings.DrawMode =
                TabDrawMode.Normal;

            CreateCustomSettingsTabs();

            tabLanguageGroups.BackColor =
                NoteHighlightUiTheme.Surface;

            tabThemeEditor.BackColor =
                NoteHighlightUiTheme.Surface;

            ApplyLanguageGroupsLayout();
            ApplyThemeEditorLayout();
            ApplyPreviewLayout();
            WireModernStateRefresh();
            RefreshModernControlStates();

            Invalidate(
                true);
        }


        private void CreateSettingsHeader()
        {
            Label title =
                new Label
                {
                    AutoSize =
                        true,

                    Text =
                        "NoteHighlight+ Settings",

                    Location =
                        new Point(
                            24,
                            18),

                    ForeColor =
                        NoteHighlightUiTheme.TextPrimary,

                    BackColor =
                        Color.Transparent,

                    Font =
                        new Font(NoteHighlightUiTheme.FontFamily, 16.0f, FontStyle.Bold, GraphicsUnit.Point)
                };

            Label subtitle =
                new Label
                {
                    AutoSize =
                        true,

                    Text =
                        "Manage languages, keyword groups, themes and application preferences.",

                    Location =
                        new Point(
                            25,
                            49),

                    ForeColor =
                        NoteHighlightUiTheme.TextSecondary,

                    BackColor =
                        Color.Transparent,

                    Font =
                        NoteHighlightUiTheme.CreateBodyFont()
                };

            Controls.Add(
                title);

            Controls.Add(
                subtitle);

            title.BringToFront();
            subtitle.BringToFront();
        }


        private void ApplyLanguageGroupsLayout()
        {
            GroupBox keywordEditor =
                CreateSettingsGroupBox(
                    "Keyword Groups & Words",
                    new Rectangle(
                        14,
                        14,
                        486,
                        330));

            GroupBox groupDetails =
                CreateSettingsGroupBox(
                    "Group Details",
                    new Rectangle(
                        514,
                        14,
                        190,
                        248));

            GroupBox languages =
                CreateSettingsGroupBox(
                    "Languages",
                    new Rectangle(
                        718,
                        14,
                        190,
                        330));

            tabLanguageGroups.Controls.Add(
                keywordEditor);

            tabLanguageGroups.Controls.Add(
                groupDetails);

            tabLanguageGroups.Controls.Add(
                languages);

            ReparentControl(
                lbxKeywordGroups,
                keywordEditor,
                new Rectangle(
                    14,
                    28,
                    180,
                    120));

            ReparentControl(
                lbxGroupWords,
                keywordEditor,
                new Rectangle(
                    252,
                    28,
                    218,
                    120));

            ReparentControl(
                _btnMoveKeywordGroupUp,
                keywordEditor,
                new Rectangle(
                    202,
                    28,
                    38,
                    28));

            ReparentControl(
                _btnMoveKeywordGroupDown,
                keywordEditor,
                new Rectangle(
                    202,
                    62,
                    38,
                    28));

            ReparentControl(
                _btnAddKeywordGroup,
                keywordEditor,
                new Rectangle(
                    14,
                    158,
                    86,
                    30));

            ReparentControl(
                _btnRemoveKeywordGroup,
                keywordEditor,
                new Rectangle(
                    108,
                    158,
                    86,
                    30));

            ReparentControl(
                txtNewGroupWord,
                keywordEditor,
                new Rectangle(
                    252,
                    158,
                    218,
                    24));

            ReparentControl(
                btnAddGroupWord,
                keywordEditor,
                new Rectangle(
                    252,
                    192,
                    103,
                    30));

            ReparentControl(
                btnRemoveGroupWord,
                keywordEditor,
                new Rectangle(
                    367,
                    192,
                    103,
                    30));

            ReparentControl(
                _btnEditGroupRegex,
                keywordEditor,
                new Rectangle(
                    252,
                    232,
                    218,
                    30));

            UiStyleManager.StyleListBox(
                lbxKeywordGroups);

            UiStyleManager.StyleListBox(
                lbxGroupWords);

            UiStyleManager.StyleTextBox(
                txtNewGroupWord);

            UiStyleManager.StyleSecondaryButton(
                _btnAddKeywordGroup);

            UiStyleManager.StyleSecondaryButton(
                _btnRemoveKeywordGroup);

            UiStyleManager.StyleSecondaryButton(
                _btnMoveKeywordGroupUp);

            UiStyleManager.StyleSecondaryButton(
                _btnMoveKeywordGroupDown);

            UiStyleManager.StyleSecondaryButton(
                btnAddGroupWord);

            UiStyleManager.StyleSecondaryButton(
                btnRemoveGroupWord);

            UiStyleManager.StyleSecondaryButton(
                _btnEditGroupRegex);

            ReparentLabelAndField(
                lblGroupName,
                txtGroupName,
                groupDetails,
                28);

            ReparentLabelAndField(
                lblGroupDescription,
                txtGroupDescription,
                groupDetails,
                91);

            lblGroupId.Parent =
                groupDetails;

            lblGroupId.Location =
                new Point(
                    14,
                    154);

            UiStyleManager.StyleLabel(
                lblGroupId,
                true);

            nudGroupId.Parent =
                groupDetails;

            nudGroupId.Location =
                new Point(
                    14,
                    175);

            nudGroupId.Size =
                new Size(
                    160,
                    24);

            UiStyleManager.StyleNumericUpDown(
                nudGroupId);

            btnSaveLanguage.Parent =
                groupDetails;

            btnSaveLanguage.Location =
                new Point(
                    14,
                    207);

            btnSaveLanguage.Size =
                new Size(
                    160,
                    30);

            btnSaveLanguage.Text =
                "Save Language";

            UiStyleManager.StylePrimaryButton(
                btnSaveLanguage);

            lblLanguages.Parent =
                languages;

            lblLanguages.Location =
                new Point(
                    14,
                    28);

            lblLanguages.Text =
                "Active languages";

            UiStyleManager.StyleLabel(
                lblLanguages,
                true);

            ReparentControl(
                lbxLanguages,
                languages,
                new Rectangle(
                    14,
                    50,
                    160,
                    116));

            UiStyleManager.StyleListBox(
                lbxLanguages);

            lblAddLanguage.Parent =
                languages;

            lblAddLanguage.Location =
                new Point(
                    14,
                    176);

            lblAddLanguage.Text =
                "Add language";

            UiStyleManager.StyleLabel(
                lblAddLanguage,
                true);

            ReparentControl(
                cmbAvailableLanguages,
                languages,
                new Rectangle(
                    14,
                    198,
                    160,
                    24));

            UiStyleManager.StyleComboBox(
                cmbAvailableLanguages);

            ReparentControl(
                btnRemoveLanguage,
                languages,
                new Rectangle(
                    14,
                    232,
                    160,
                    36));

            ReparentControl(
                btnAddLanguage,
                languages,
                new Rectangle(
                    14,
                    278,
                    160,
                    36));

            btnRemoveLanguage.Text =
                "Remove Selected";

            btnAddLanguage.Text =
                "Add to Ribbon";

            UiStyleManager.StyleSecondaryButton(
                btnRemoveLanguage);

            UiStyleManager.StylePrimaryButton(
                btnAddLanguage);

            keywordEditor.SendToBack();
            groupDetails.SendToBack();
            languages.SendToBack();
        }


        private void ApplyThemeEditorLayout()
        {
            grpThemeManagement.Location =
                new Point(
                    16,
                    16);

            grpThemeManagement.Size =
                new Size(
                    250,
                    302);

            grpThemeStyleEditor.Location =
                new Point(
                    282,
                    16);

            grpThemeStyleEditor.Size =
                new Size(
                    626,
                    302);

            UiStyleManager.StyleGroupBox(
                grpThemeManagement);

            UiStyleManager.StyleGroupBox(
                grpThemeStyleEditor);

            AttachModernGroupBoxBorder(
                grpThemeManagement);

            AttachModernGroupBoxBorder(
                grpThemeStyleEditor);

            lblThemeSelector.Location =
                new Point(
                    16,
                    30);

            UiStyleManager.StyleLabel(
                lblThemeSelector,
                true);

            cmbThemes.Location =
                new Point(
                    16,
                    52);

            cmbThemes.Size =
                new Size(
                    216,
                    24);

            UiStyleManager.StyleComboBox(
                cmbThemes);

            SetControlBounds(
                btnNewTheme,
                16,
                90,
                102,
                30);

            SetControlBounds(
                btnDuplicateTheme,
                130,
                90,
                102,
                30);

            SetControlBounds(
                btnRenameTheme,
                16,
                130,
                102,
                30);

            SetControlBounds(
                btnDeleteTheme,
                130,
                130,
                102,
                30);

            SetControlBounds(
                btnResetTheme,
                16,
                176,
                216,
                30);

            UiStyleManager.StyleSecondaryButton(
                btnNewTheme);

            UiStyleManager.StyleSecondaryButton(
                btnDuplicateTheme);

            UiStyleManager.StyleSecondaryButton(
                btnRenameTheme);

            UiStyleManager.StyleDangerButton(
                btnDeleteTheme);

            UiStyleManager.StyleSecondaryButton(
                btnResetTheme);

            lblThemeStyleTarget.Location =
                new Point(
                    18,
                    30);

            UiStyleManager.StyleLabel(
                lblThemeStyleTarget,
                true);

            cmbThemeStyleTarget.Location =
                new Point(
                    18,
                    52);

            cmbThemeStyleTarget.Size =
                new Size(
                    588,
                    24);

            UiStyleManager.StyleComboBox(
                cmbThemeStyleTarget);

            lblThemeGroupName.Location =
                new Point(
                    18,
                    88);

            lblThemeUses.Location =
                new Point(
                    18,
                    110);

            lblThemeStyleSlot.Location =
                new Point(
                    18,
                    132);

            UiStyleManager.StyleLabel(
                lblThemeGroupName,
                false);

            UiStyleManager.StyleLabel(
                lblThemeUses,
                true);

            UiStyleManager.StyleLabel(
                lblThemeStyleSlot,
                true);

            lblGroupColour.Location =
                new Point(
                    18,
                    164);

            UiStyleManager.StyleLabel(
                lblGroupColour,
                true);

            pnlGroupColourPreview.Location =
                new Point(
                    18,
                    186);

            pnlGroupColourPreview.Size =
                new Size(
                    46,
                    26);

            lblGroupColourValue.Location =
                new Point(
                    76,
                    192);

            UiStyleManager.StyleLabel(
                lblGroupColourValue,
                false);

            btnChangeThemeColour.Location =
                new Point(
                    196,
                    184);

            btnChangeThemeColour.Size =
                new Size(
                    102,
                    30);

            UiStyleManager.StyleSecondaryButton(
                btnChangeThemeColour);

            lblThemeFormatting.Location =
                new Point(
                    18,
                    232);

            UiStyleManager.StyleLabel(
                lblThemeFormatting,
                true);

            chkThemeBold.Location =
                new Point(
                    18,
                    250);

            chkThemeBold.Size =
                new Size(
                    54,
                    30);

            chkThemeBold.Text =
                "B";

            chkThemeItalic.Location =
                new Point(
                    80,
                    250);

            chkThemeItalic.Size =
                new Size(
                    54,
                    30);

            chkThemeItalic.Text =
                "I";

            UiStyleManager.StyleToggleCheckBox(
                chkThemeBold,
                FontStyle.Bold);

            UiStyleManager.StyleToggleCheckBox(
                chkThemeItalic,
                FontStyle.Italic);

            btnSaveTheme.Location =
                new Point(
                    486,
                    248);

            btnSaveTheme.Size =
                new Size(
                    120,
                    34);

            UiStyleManager.StylePrimaryButton(
                btnSaveTheme);

            lblThemeStyleStatus.Location =
                new Point(
                    314,
                    190);

            UiStyleManager.StyleLabel(
                lblThemeStyleStatus,
                true);
        }


        private void ApplyPreviewLayout()
        {
            grpPreview.Location =
                new Point(
                    20,
                    528);

            grpPreview.Size =
                new Size(
                    936,
                    266);

            grpPreview.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            UiStyleManager.StyleGroupBox(
                grpPreview);

            AttachModernGroupBoxBorder(
                grpPreview);

            lblPreviewStatus.Location =
                new Point(
                    14,
                    27);

            lblPreviewStatus.Font =
                NoteHighlightUiTheme.CreateSmallFont();

            UiStyleManager.StyleLabel(
                lblPreviewStatus,
                true);

            pnlPreview.Location =
                new Point(
                    14,
                    50);

            pnlPreview.Size =
                new Size(
                    908,
                    198);

            pnlPreview.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            pnlPreview.BorderStyle =
                BorderStyle.None;

            pnlPreview.BackColor =
                NoteHighlightUiTheme.SurfaceRaised;

            pnlPreview.Paint -=
                ModernPanelBorder_Paint;

            pnlPreview.Paint +=
                ModernPanelBorder_Paint;
        }


        private void WireModernStateRefresh()
        {
            Control[] controls =
            {
                _btnRemoveKeywordGroup,
                _btnMoveKeywordGroupUp,
                _btnMoveKeywordGroupDown,
                _btnEditGroupRegex,
                btnAddGroupWord,
                btnRemoveGroupWord,
                btnSaveLanguage,
                btnRemoveLanguage,
                btnAddLanguage,
                btnNewTheme,
                btnDuplicateTheme,
                btnRenameTheme,
                btnDeleteTheme,
                btnResetTheme,
                btnChangeThemeColour,
                btnSaveTheme
            };

            foreach (Control control
                in controls)
            {
                if (control == null)
                {
                    continue;
                }

                control.EnabledChanged -=
                    ModernControl_EnabledChanged;

                control.EnabledChanged +=
                    ModernControl_EnabledChanged;
            }
        }


        private void ModernControl_EnabledChanged(
            object sender,
            EventArgs e)
        {
            RefreshModernControlStates();
        }


        private void RefreshModernControlStates()
        {
            ApplyModernButtonState(
                _btnRemoveKeywordGroup,
                false);

            ApplyModernButtonState(
                _btnMoveKeywordGroupUp,
                false);

            ApplyModernButtonState(
                _btnMoveKeywordGroupDown,
                false);

            ApplyModernButtonState(
                _btnEditGroupRegex,
                false);

            ApplyModernButtonState(
                btnAddGroupWord,
                false);

            ApplyModernButtonState(
                btnRemoveGroupWord,
                false);

            ApplyModernButtonState(
                btnSaveLanguage,
                true);

            ApplyModernButtonState(
                btnRemoveLanguage,
                false);

            ApplyModernButtonState(
                btnAddLanguage,
                true);

            ApplyModernButtonState(
                btnNewTheme,
                false);

            ApplyModernButtonState(
                btnDuplicateTheme,
                false);

            ApplyModernButtonState(
                btnRenameTheme,
                false);

            ApplyModernButtonState(
                btnDeleteTheme,
                false,
                true);

            ApplyModernButtonState(
                btnResetTheme,
                false);

            ApplyModernButtonState(
                btnChangeThemeColour,
                false);

            ApplyModernButtonState(
                btnSaveTheme,
                true);
        }


        private static void ApplyModernButtonState(
            Button button,
            bool primary,
            bool danger = false)
        {
            if (button == null)
            {
                return;
            }

            if (!button.Enabled)
            {
                button.BackColor =
                    NoteHighlightUiTheme.DisabledBackground;

                button.ForeColor =
                    NoteHighlightUiTheme.DisabledText;

                button.FlatAppearance.BorderColor =
                    NoteHighlightUiTheme.Border;

                return;
            }

            if (danger)
            {
                UiStyleManager.StyleDangerButton(
                    button);
            }
            else if (primary)
            {
                UiStyleManager.StylePrimaryButton(
                    button);
            }
            else
            {
                UiStyleManager.StyleSecondaryButton(
                    button);
            }
        }


        private void CreateTabContentBorderMask()
        {
            int contentTop =
                tabSettings.Top +
                tabSettings.ItemSize.Height;

            Panel leftMask =
                CreateTabBorderMaskPanel(
                    new Rectangle(
                        tabSettings.Left,
                        contentTop,
                        4,
                        tabSettings.Height -
                        tabSettings.ItemSize.Height));

            Panel rightMask =
                CreateTabBorderMaskPanel(
                    new Rectangle(
                        tabSettings.Right - 4,
                        contentTop,
                        4,
                        tabSettings.Height -
                        tabSettings.ItemSize.Height));

            Panel bottomMask =
                CreateTabBorderMaskPanel(
                    new Rectangle(
                        tabSettings.Left,
                        tabSettings.Bottom - 4,
                        tabSettings.Width,
                        4));

            Panel topMask =
                CreateTabBorderMaskPanel(
                    new Rectangle(
                        tabSettings.Left + 300,
                        contentTop,
                        Math.Max(
                            0,
                            tabSettings.Width - 300),
                        4));

            Panel headerSeamMask =
                CreateTabBorderMaskPanel(
                    new Rectangle(
                        tabSettings.Left,
                        contentTop - 2,
                        tabSettings.Width,
                        4));

            leftMask.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left;

            rightMask.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Right;

            bottomMask.Anchor =
                AnchorStyles.Left |
                AnchorStyles.Right |
                AnchorStyles.Top;

            topMask.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            headerSeamMask.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            headerSeamMask.BringToFront();
        }


        private Panel CreateTabBorderMaskPanel(
            Rectangle bounds)
        {
            Panel panel =
                new Panel
                {
                    BackColor =
                        NoteHighlightUiTheme.WindowBackground,

                    Bounds =
                        bounds,

                    TabStop =
                        false
                };

            Controls.Add(
                panel);

            panel.BringToFront();

            return panel;
        }


        private void CreateCustomSettingsTabs()
        {
            _btnLanguageGroupsTab =
                new Button
                {
                    Name =
                        "btnLanguageGroupsTab",

                    Text =
                        "Language & Groups",

                    Location =
                        new Point(
                            20,
                            122),

                    Size =
                        new Size(
                            154,
                            32),

                    TabStop =
                        true
                };

            _btnThemeEditorTab =
                new Button
                {
                    Name =
                        "btnThemeEditorTab",

                    Text =
                        "Theme Editor",

                    Location =
                        new Point(
                            180,
                            122),

                    Size =
                        new Size(
                            140,
                            32),

                    TabStop =
                        true
                };

            Controls.Add(
                _btnLanguageGroupsTab);

            Controls.Add(
                _btnThemeEditorTab);

            _btnLanguageGroupsTab.Click -=
                CustomSettingsTab_Click;

            _btnLanguageGroupsTab.Click +=
                CustomSettingsTab_Click;

            _btnThemeEditorTab.Click -=
                CustomSettingsTab_Click;

            _btnThemeEditorTab.Click +=
                CustomSettingsTab_Click;

            tabSettings.SelectedIndexChanged -=
                CustomSettingsTabSelectionChanged;

            tabSettings.SelectedIndexChanged +=
                CustomSettingsTabSelectionChanged;

            _btnLanguageGroupsTab.BringToFront();
            _btnThemeEditorTab.BringToFront();

            RefreshCustomSettingsTabs();
        }


        private void CustomSettingsTab_Click(
            object sender,
            EventArgs e)
        {
            if (sender == _btnLanguageGroupsTab)
            {
                tabSettings.SelectedTab =
                    tabLanguageGroups;
            }
            else if (sender == _btnThemeEditorTab)
            {
                tabSettings.SelectedTab =
                    tabThemeEditor;
            }
        }


        private void CustomSettingsTabSelectionChanged(
            object sender,
            EventArgs e)
        {
            RefreshCustomSettingsTabs();
        }


        private void RefreshCustomSettingsTabs()
        {
            ApplyCustomTabButtonState(
                _btnLanguageGroupsTab,
                tabSettings.SelectedTab ==
                    tabLanguageGroups);

            ApplyCustomTabButtonState(
                _btnThemeEditorTab,
                tabSettings.SelectedTab ==
                    tabThemeEditor);
        }


        private static void ApplyCustomTabButtonState(
            Button button,
            bool selected)
        {
            if (button == null)
            {
                return;
            }

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                1;

            button.Font =
                NoteHighlightUiTheme.CreateBodyFont();

            button.ForeColor =
                NoteHighlightUiTheme.TextPrimary;

            if (selected)
            {
                button.BackColor =
                    NoteHighlightUiTheme.SurfaceRaised;

                button.FlatAppearance.BorderColor =
                    NoteHighlightUiTheme.Accent;

                button.FlatAppearance.MouseOverBackColor =
                    NoteHighlightUiTheme.SurfaceHover;

                button.FlatAppearance.MouseDownBackColor =
                    NoteHighlightUiTheme.AccentPressed;
            }
            else
            {
                button.BackColor =
                    NoteHighlightUiTheme.WindowBackground;

                button.FlatAppearance.BorderColor =
                    NoteHighlightUiTheme.Border;

                button.FlatAppearance.MouseOverBackColor =
                    NoteHighlightUiTheme.SurfaceHover;

                button.FlatAppearance.MouseDownBackColor =
                    NoteHighlightUiTheme.AccentPressed;
            }

            button.UseVisualStyleBackColor =
                false;
        }


        private void CreateTabHeaderBackground()
        {
            Panel tabHeaderBackground =
                new Panel
                {
                    Name =
                        "pnlTabHeaderBackground",

                    BackColor =
                        NoteHighlightUiTheme.WindowBackground,

                    Location =
                        new Point(
                            tabSettings.Left + 302,
                            tabSettings.Top),

                    Size =
                        new Size(
                            Math.Max(
                                0,
                                tabSettings.Width - 302),
                            31),

                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Left |
                        AnchorStyles.Right
                };

            Controls.Add(
                tabHeaderBackground);

            tabHeaderBackground.BringToFront();
        }


        private void AttachModernGroupBoxBorder(
            GroupBox groupBox)
        {
            if (groupBox == null)
            {
                return;
            }

            groupBox.Paint -=
                ModernGroupBox_Paint;

            groupBox.Paint +=
                ModernGroupBox_Paint;
        }


        private void ModernGroupBox_Paint(
            object sender,
            PaintEventArgs e)
        {
            GroupBox groupBox =
                sender as GroupBox;

            if (groupBox == null ||
                groupBox.Width < 4 ||
                groupBox.Height < 4)
            {
                return;
            }

            Color background =
                groupBox.BackColor;

            string caption =
                groupBox.Text ?? string.Empty;

            Size textSize =
                TextRenderer.MeasureText(
                    caption,
                    groupBox.Font);

            int captionLeft =
                10;

            int borderTop =
                Math.Max(
                    8,
                    textSize.Height / 2);

            using (SolidBrush eraseBrush =
                new SolidBrush(
                    background))
            {
                e.Graphics.FillRectangle(
                    eraseBrush,
                    0,
                    0,
                    groupBox.Width,
                    borderTop + 2);

                e.Graphics.FillRectangle(
                    eraseBrush,
                    0,
                    borderTop,
                    3,
                    groupBox.Height - borderTop);

                e.Graphics.FillRectangle(
                    eraseBrush,
                    groupBox.Width - 3,
                    borderTop,
                    3,
                    groupBox.Height - borderTop);

                e.Graphics.FillRectangle(
                    eraseBrush,
                    0,
                    groupBox.Height - 3,
                    groupBox.Width,
                    3);
            }

            using (Pen borderPen =
                new Pen(
                    NoteHighlightUiTheme.Border))
            {
                e.Graphics.DrawLine(
                    borderPen,
                    1,
                    borderTop,
                    captionLeft - 3,
                    borderTop);

                e.Graphics.DrawLine(
                    borderPen,
                    captionLeft + textSize.Width + 3,
                    borderTop,
                    groupBox.Width - 2,
                    borderTop);

                e.Graphics.DrawLine(
                    borderPen,
                    1,
                    borderTop,
                    1,
                    groupBox.Height - 2);

                e.Graphics.DrawLine(
                    borderPen,
                    groupBox.Width - 2,
                    borderTop,
                    groupBox.Width - 2,
                    groupBox.Height - 2);

                e.Graphics.DrawLine(
                    borderPen,
                    1,
                    groupBox.Height - 2,
                    groupBox.Width - 2,
                    groupBox.Height - 2);
            }

            TextRenderer.DrawText(
                e.Graphics,
                caption,
                groupBox.Font,
                new Point(
                    captionLeft,
                    0),
                NoteHighlightUiTheme.TextPrimary,
                TextFormatFlags.NoPadding);
        }


        private void ModernPanelBorder_Paint(
            object sender,
            PaintEventArgs e)
        {
            Panel panel =
                sender as Panel;

            if (panel == null ||
                panel.Width < 2 ||
                panel.Height < 2)
            {
                return;
            }

            using (Pen borderPen =
                new Pen(
                    NoteHighlightUiTheme.Border))
            {
                e.Graphics.DrawRectangle(
                    borderPen,
                    0,
                    0,
                    panel.Width - 1,
                    panel.Height - 1);
            }
        }


        private GroupBox CreateSettingsGroupBox(
            string text,
            Rectangle bounds)
        {
            GroupBox groupBox =
                new GroupBox
                {
                    Text =
                        text,

                    Location =
                        bounds.Location,

                    Size =
                        bounds.Size,

                    BackColor =
                        NoteHighlightUiTheme.Surface,

                    ForeColor =
                        NoteHighlightUiTheme.TextPrimary,

                    Font =
                        NoteHighlightUiTheme.CreateBodyFont()
                };

            UiStyleManager.StyleGroupBox(
                groupBox);

            AttachModernGroupBoxBorder(
                groupBox);

            return groupBox;
        }


        private static void ReparentControl(
            Control control,
            Control parent,
            Rectangle bounds)
        {
            control.Parent =
                parent;

            control.Location =
                bounds.Location;

            control.Size =
                bounds.Size;

            control.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left;
        }


        private static void SetControlBounds(
            Control control,
            int left,
            int top,
            int width,
            int height)
        {
            control.Location =
                new Point(
                    left,
                    top);

            control.Size =
                new Size(
                    width,
                    height);
        }


        private static void ReparentLabelAndField(
            Label label,
            TextBox field,
            Control parent,
            int top)
        {
            label.Parent =
                parent;

            label.Location =
                new Point(
                    14,
                    top);

            UiStyleManager.StyleLabel(
                label,
                true);

            field.Parent =
                parent;

            field.Location =
                new Point(
                    14,
                    top + 22);

            field.Size =
                new Size(
                    160,
                    24);

            UiStyleManager.StyleTextBox(
                field);
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

            LanguageInfo selectedLanguage = lbxLanguages.SelectedItem as LanguageInfo;

            if (selectedLanguage != null)
            {
                _languagePreferenceProvider
                    .SaveLanguageTag(
                        selectedLanguage.Tag);
            }

            _isFormClosingConfirmed =
                true;
        }

        protected override bool ProcessCmdKey(
            ref Message msg,
            Keys keyData)
        {
            if ((keyData & Keys.KeyCode) ==
                Keys.F1)
            {
                KeyboardShortcutsForm.ShowHelp(
                    this);

                return true;
            }

            return base.ProcessCmdKey(
                ref msg,
                keyData);
        }


        protected override bool ProcessDialogKey(
            Keys keyData)
        {
            bool shift =
                (keyData & Keys.Shift) ==
                Keys.Shift;

            Keys keyCode =
                keyData &
                Keys.KeyCode;

            if (IsCustomTabFocused() &&
                (keyCode == Keys.Left ||
                 keyCode == Keys.Right))
            {
                SwitchFocusedSettingsTab();

                return true;
            }

            if (keyCode == Keys.Tab)
            {
                MoveSettingsKeyboardFocus(
                    !shift);

                return true;
            }

            return base.ProcessDialogKey(
                keyData);
        }


        private bool IsCustomTabFocused()
        {
            return
                (_btnLanguageGroupsTab != null &&
                 _btnLanguageGroupsTab.ContainsFocus) ||
                (_btnThemeEditorTab != null &&
                 _btnThemeEditorTab.ContainsFocus);
        }


        private void SwitchFocusedSettingsTab()
        {
            if (_btnLanguageGroupsTab == null ||
                _btnThemeEditorTab == null)
            {
                return;
            }

            // There are only two custom tabs, so either arrow moves
            // to the other tab. This keeps the behaviour predictable in
            // both left-to-right and right-to-left navigation.
            bool openThemeEditor =
                _btnLanguageGroupsTab.ContainsFocus;

            if (openThemeEditor)
            {
                tabSettings.SelectedTab =
                    tabThemeEditor;

                _btnThemeEditorTab.Focus();
            }
            else
            {
                tabSettings.SelectedTab =
                    tabLanguageGroups;

                _btnLanguageGroupsTab.Focus();
            }

            RefreshCustomSettingsTabs();
            ResetSettingsKeyboardNavigation();
        }


        private void MoveSettingsKeyboardFocus(
            bool forward)
        {
            Control[] route =
                GetSettingsKeyboardNavigationOrder();

            EnsureSettingsKeyboardRoute(
                route);

            MoveAlongSettingsKeyboardRoute(
                forward);
        }


        private void EnsureSettingsKeyboardRoute(
            Control[] route)
        {
            if (route == null ||
                route.Length == 0)
            {
                _settingsKeyboardNavigationRoute =
                    null;

                _settingsKeyboardNavigationIndex =
                    -1;

                return;
            }

            bool sameRoute =
                _settingsKeyboardNavigationRoute != null &&
                _settingsKeyboardNavigationRoute.Length ==
                    route.Length;

            if (sameRoute)
            {
                for (int index = 0;
                    index < route.Length;
                    index++)
                {
                    if (!ReferenceEquals(
                        _settingsKeyboardNavigationRoute[index],
                        route[index]))
                    {
                        sameRoute =
                            false;

                        break;
                    }
                }
            }

            if (!sameRoute)
            {
                _settingsKeyboardNavigationRoute =
                    route;

                _settingsKeyboardNavigationIndex =
                    FindCurrentSettingsRouteIndex(
                        route);
            }
            else
            {
                int detectedIndex =
                    FindCurrentSettingsRouteIndex(
                        route);

                if (detectedIndex >= 0)
                {
                    _settingsKeyboardNavigationIndex =
                        detectedIndex;
                }
            }
        }


        private int FindCurrentSettingsRouteIndex(
            Control[] route)
        {
            if (route == null)
            {
                return -1;
            }

            for (int index = 0;
                index < route.Length;
                index++)
            {
                Control candidate =
                    route[index];

                if (candidate != null &&
                    !candidate.IsDisposed &&
                    candidate.ContainsFocus)
                {
                    return index;
                }
            }

            return -1;
        }


        private void MoveAlongSettingsKeyboardRoute(
            bool forward)
        {
            Control[] route =
                _settingsKeyboardNavigationRoute;

            if (route == null ||
                route.Length == 0)
            {
                return;
            }

            int step =
                forward
                    ? 1
                    : -1;

            int index =
                _settingsKeyboardNavigationIndex;

            for (int attempts = 0;
                attempts < route.Length;
                attempts++)
            {
                if (index < 0)
                {
                    index =
                        forward
                            ? 0
                            : route.Length - 1;
                }
                else
                {
                    index =
                        (index +
                            step +
                            route.Length) %
                        route.Length;
                }

                Control candidate =
                    route[index];

                if (!CanUseSettingsKeyboardFocus(
                    candidate))
                {
                    continue;
                }

                _settingsKeyboardNavigationIndex =
                    index;

                candidate.Select();
                candidate.Focus();

                return;
            }
        }


        private void ResetSettingsKeyboardNavigation()
        {
            _settingsKeyboardNavigationRoute =
                null;

            _settingsKeyboardNavigationIndex =
                -1;
        }


        private Control[] GetSettingsKeyboardNavigationOrder()
        {
            if (tabSettings.SelectedTab ==
                tabThemeEditor)
            {
                return
                    GetThemeEditorKeyboardNavigationOrder();
            }

            bool hasSelectedLanguage =
                lbxLanguages != null &&
                lbxLanguages.SelectedIndex >= 0 &&
                _languageEditor.HasConfiguration;

            if (hasSelectedLanguage)
            {
                return
                    GetLanguageKeyboardNavigationOrder();
            }

            return
                GetEmptyLanguageKeyboardNavigationOrder();
        }


        private Control[] GetEmptyLanguageKeyboardNavigationOrder()
        {
            return new Control[]
            {
                btnFont,
                cbShowTableBorder,
                _previewWebView,
                _btnLanguageGroupsTab,
                lbxKeywordGroups,
                lbxGroupWords,
                _btnAddKeywordGroup,
                txtGroupName,
                txtGroupDescription,
                lbxLanguages,
                btnRemoveLanguage,
                cmbAvailableLanguages,
                btnAddLanguage,
                btnExportConfiguration,
                btnImportConfiguration
            };
        }


        private Control[] GetLanguageKeyboardNavigationOrder()
        {
            return new Control[]
            {
                btnFont,
                cbShowTableBorder,
                _previewWebView,
                _btnLanguageGroupsTab,
                lbxKeywordGroups,
                lbxGroupWords,
                txtNewGroupWord,
                btnAddGroupWord,
                btnRemoveGroupWord,
                _btnEditGroupRegex,
                _btnAddKeywordGroup,
                _btnRemoveKeywordGroup,
                _btnMoveKeywordGroupUp,
                _btnMoveKeywordGroupDown,
                txtGroupName,
                txtGroupDescription,
                nudGroupId,
                lbxLanguages,
                btnRemoveLanguage,
                cmbAvailableLanguages,
                btnAddLanguage,
                btnExportConfiguration,
                btnImportConfiguration
            };
        }


        private Control[] GetThemeEditorKeyboardNavigationOrder()
        {
            return new Control[]
            {
                btnFont,
                cbShowTableBorder,
                _previewWebView,
                _btnThemeEditorTab,
                cmbThemes,
                btnNewTheme,
                btnDuplicateTheme,
                btnRenameTheme,
                btnDeleteTheme,
                btnResetTheme,
                btnChangeThemeColour,
                chkThemeBold,
                chkThemeItalic,
                cmbThemeStyleTarget,
                btnExportConfiguration,
                btnImportConfiguration
            };
        }


        private static bool CanUseSettingsKeyboardFocus(
            Control control)
        {
            return
                control != null &&
                !control.IsDisposed &&
                control.Visible &&
                control.Enabled &&
                control.CanSelect;
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
                    _themePreferenceProvider.ReadThemeName();

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

                // Capture the theme's reset point the first time it is loaded.
                // The baseline lives outside highlight/themes so normal saves do
                // not overwrite it.
                _themeResetService.EnsureBaseline(
                    selectedThemeName,
                    themePath);

                _hasUnsavedThemeChanges =
                    false;

                UpdateSaveThemeButtonState();

                _themePreferenceProvider.SaveThemeName(
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

            Color currentColour =
                Color.Black;

            TryParseThemeColour(
                style.Colour,
                out currentColour);

            using (ColorPickerForm picker =
                new ColorPickerForm(
                    currentColour))
            {
                if (picker.ShowDialog(
                    this) != DialogResult.OK)
                {
                    return;
                }

                style.Colour =
                    ToThemeColour(
                        picker.SelectedColor);

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

                _themePreferenceProvider.SaveThemeName(
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

                _themePreferenceProvider.SaveThemeName(
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

        private void btnImportConfiguration_Click(
            object sender,
            EventArgs e)
        {
            if (!ConfirmPendingLanguageChanges() ||
                !ConfirmPendingThemeChanges())
            {
                return;
            }

            using (OpenFileDialog dialog =
                new OpenFileDialog())
            {
                dialog.Title =
                    "Import NoteHighlight+ Configuration";

                dialog.Filter =
                    "NoteHighlight+ Backup (*.zip)|*.zip";

                dialog.CheckFileExists =
                    true;

                dialog.Multiselect =
                    false;

                if (dialog.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                try
                {
                    ConfigurationImportPlan plan =
                        _configurationImportService.Analyze(
                            dialog.FileName);

                    bool overwriteExisting =
                        false;

                    if (plan.ExistingFiles > 0)
                    {
                        DialogResult conflictChoice =
                            MessageBox.Show(
                                this,
                                "The backup contains " +
                                plan.TotalFiles +
                                " configuration file(s)." +
                                Environment.NewLine +
                                Environment.NewLine +
                                plan.ExistingFiles +
                                " file(s) already exist." +
                                Environment.NewLine +
                                plan.NewFiles +
                                " file(s) are new." +
                                Environment.NewLine +
                                Environment.NewLine +
                                "Yes = overwrite existing files and import everything" +
                                Environment.NewLine +
                                "No = import only new files" +
                                Environment.NewLine +
                                "Cancel = do not import",
                                "Import Configuration",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button3);

                        if (conflictChoice ==
                            DialogResult.Cancel)
                        {
                            return;
                        }

                        overwriteExisting =
                            conflictChoice == DialogResult.Yes;
                    }
                    else
                    {
                        DialogResult confirmation =
                            MessageBox.Show(
                                this,
                                "Import " +
                                plan.TotalFiles +
                                " configuration file(s) from this backup?",
                                "Import Configuration",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button1);

                        if (confirmation !=
                            DialogResult.Yes)
                        {
                            return;
                        }
                    }

                    ConfigurationImportResult result =
                        _configurationImportService.Import(
                            dialog.FileName,
                            overwriteExisting);

                    ReloadConfigurationAfterImport();

                    string message =
                        "Configuration imported successfully." +
                        Environment.NewLine +
                        Environment.NewLine +
                        "Imported: " +
                        result.ImportedFiles +
                        Environment.NewLine +
                        "Skipped: " +
                        result.SkippedFiles;

                    if (result.RibbonConfigurationImported)
                    {
                        message +=
                            Environment.NewLine +
                            Environment.NewLine +
                            "Restart OneNote to apply imported Ribbon language visibility.";
                    }

                    MessageBox.Show(
                        this,
                        message,
                        "Import Configuration",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(
                        this,
                        "The configuration could not be imported." +
                        Environment.NewLine +
                        Environment.NewLine +
                        exception.Message,
                        "Import Configuration",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void ReloadConfigurationAfterImport()
        {
            _languageRibbonController.RefreshLanguageList();
            _languageRibbonController.LoadSelectedLanguageConfiguration();

            LoadAvailableThemes();

            _previousLanguageIndex =
                lbxLanguages.SelectedIndex;

            UpdateWindowTitle();
            UpdateSaveButtonState();
            UpdateSaveThemeButtonState();

            RequestPreviewRefresh();
        }

        private void btnExportConfiguration_Click(
            object sender,
            EventArgs e)
        {
            using (SaveFileDialog dialog =
                new SaveFileDialog())
            {
                dialog.Title =
                    "Export NoteHighlight+ Configuration";

                dialog.Filter =
                    "NoteHighlight+ Backup (*.zip)|*.zip";

                dialog.DefaultExt =
                    "zip";

                dialog.AddExtension =
                    true;

                dialog.OverwritePrompt =
                    true;

                dialog.FileName =
                    "NoteHighlightPlus-Backup-" +
                    DateTime.Now.ToString("yyyy-MM-dd") +
                    ".zip";

                if (dialog.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                try
                {
                    _configurationExportService.Export(
                        dialog.FileName);

                    MessageBox.Show(
                        this,
                        "Configuration exported successfully.",
                        "Export Configuration",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(
                        this,
                        "The configuration could not be exported." +
                        Environment.NewLine +
                        Environment.NewLine +
                        exception.Message,
                        "Export Configuration",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }



        private void btnResetTheme_Click(
            object sender,
            EventArgs e)
        {
            if (_activeTheme == null ||
                string.IsNullOrWhiteSpace(
                    _activeThemeFilePath))
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

            if (!_themeResetService.CanReset(
                themeName))
            {
                MessageBox.Show(
                    this,
                    "No reset point is available for theme '"
                    + themeName
                    + "'.",
                    "Reset Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DialogResult result =
                MessageBox.Show(
                    this,
                    "Reset theme '"
                    + themeName
                    + "'?"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "This will discard saved and unsaved changes made "
                    + "since its reset point was created.",
                    "Reset Theme",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _themeResetService.RestoreBaseline(
                    themeName,
                    _activeThemeFilePath);

                _hasUnsavedThemeChanges =
                    false;

                ReloadThemesAndSelect(
                    themeName);

                RequestPreviewRefresh();

                MessageBox.Show(
                    this,
                    "Theme '"
                    + themeName
                    + "' was reset successfully.",
                    "Reset Theme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    "The theme could not be reset."
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Reset Theme",
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
                    _themePreferenceProvider.Clear();

                    LoadAvailableThemes();

                    RequestPreviewRefresh();

                    return;
                }

                _themePreferenceProvider.SaveThemeName(
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

            btnResetTheme.Enabled =
                _activeTheme != null &&
                _themeResetService.CanReset(
                    cmbThemes.SelectedItem as string);
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

            RestorePreferredLanguageSelection();

            EnsureSelectedLanguageIsLoaded();

            LoadAvailableThemes();

            _previousLanguageIndex =
                lbxLanguages.SelectedIndex;

            UpdateWindowTitle();
            UpdateSaveButtonState();

            RequestPreviewRefresh();
        }

        private void RestorePreferredLanguageSelection()
        {
            string preferredLanguageTag =
                _languagePreferenceProvider.ReadLanguageTag();

            if (string.IsNullOrWhiteSpace(
                preferredLanguageTag))
            {
                return;
            }

            try
            {
                _isChangingLanguageSelection =
                    true;

                for (int index = 0;
                    index < lbxLanguages.Items.Count;
                    index++)
                {
                    LanguageInfo language =
                        lbxLanguages.Items[index]
                        as LanguageInfo;

                    if (language == null)
                    {
                        continue;
                    }

                    if (!string.Equals(
                        language.Tag,
                        preferredLanguageTag,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    lbxLanguages.SelectedIndex =
                        index;

                    return;
                }
            }
            finally
            {
                _isChangingLanguageSelection =
                    false;
            }
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

                LanguageInfo selectedLanguage = lbxLanguages.SelectedItem as LanguageInfo;

                if (selectedLanguage != null)
                {
                    _languagePreferenceProvider
                        .SaveLanguageTag(
                            selectedLanguage.Tag);
                }

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