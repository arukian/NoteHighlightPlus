using GenerateHighlightContent;
using Infrastructure.Core;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using NoteHighlightAddin.Highlighting.Preview.Services;
using NoteHighlightAddin.Highlighting.Themes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace NoteHighlightAddin
{
    public partial class MainForm : Form
    {
        #region -- Field and Property --

        // Updated to work with the constructor logic

        private readonly string _codeType;
        private readonly string _fileName;
        private readonly bool _darkMode;
        private readonly bool _quickStyle;

        // Creating Expression-bodied property for CodeContent

        private string CodeContent => txtCode.Text;
        private string CodeStyle => cbx_style.Text;
        private bool IsShowLineNumber => cbx_lineNumber.Checked;
        private bool IsClipboard => cbx_Clipboard.Checked;
        private Color BackgroundColor => btnBackground.BackColor;
        public bool DarkMode => _darkMode;

        // Added new methods
        private readonly MainFormSettingsProvider _settingsProvider;
        private readonly MainFormSettingsBinder _settingsBinder;
        private readonly MainFormInitializer _initializer;
        private readonly HighlightWorkflowRequestFactory _requestFactory;
        private readonly HighlightWorkflowService _workflowService;
        private readonly BackgroundColorSelector _backgroundColorSelector;
        private readonly MainFormDisplayCoordinator _displayCoordinator;
        private readonly ThemePreferenceProvider _themePreferenceProvider;
        private readonly IHighlightThemeReader _themeReader;

        // Live preview state.
        private readonly Timer _previewRefreshTimer;
        private readonly List<string> _previewFiles;
        private WebView2 _previewWebView;
        private bool _previewRefreshPending;
        private bool _isGeneratingPreview;
        private bool _previewEventsConnected;
        private bool _hasBackgroundOverride;
        private readonly KeyboardFocusVisualManager _keyboardFocusVisualManager;
        private readonly KeyboardHelpManager _keyboardHelpManager;
        private Label _keyboardHelpLabel;

        public HighLightParameter Parameters { get; private set; }

        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            PreviewHtmlServiceTester.Cleanup();
        }

        #endregion

        #region -- Constructor --

        public MainForm(string codeType, string fileName, string selectedText, bool quickStyle, bool darkMode)
        {
            _codeType = codeType;
            _fileName = fileName;
            _quickStyle = quickStyle;
            _darkMode = darkMode;

            _settingsProvider = new MainFormSettingsProvider();
            _settingsBinder = new MainFormSettingsBinder();
            _themePreferenceProvider = new ThemePreferenceProvider();
            _themeReader = new HighlightThemeReader();
            _backgroundColorSelector = new BackgroundColorSelector();
            var themeBinder = new ThemeComboBoxBinder(new ThemeProvider());
            var editorConfigurator = new CodeEditorConfigurator(new CodeEditorLanguageMapper());
            _initializer = new MainFormInitializer(
                themeBinder,
                editorConfigurator,
                _settingsProvider,
                _settingsBinder,
                _themePreferenceProvider);
            _requestFactory = new HighlightWorkflowRequestFactory();
            var workflowServiceFactory = new HighlightWorkflowServiceFactory(_settingsProvider);
            _workflowService = workflowServiceFactory.Create();
            _displayCoordinator = new MainFormDisplayCoordinator(new WindowForegroundService());

            _previewRefreshTimer = new Timer
            {
                Interval = 450
            };
            _previewRefreshTimer.Tick += PreviewRefreshTimer_Tick;
            _previewFiles = new List<string>();

            InitializeComponent();
            ApplyModernAppearance();
            CreateKeyboardHelpLegend();
            CreateKeyboardHelpButton();

            txtCode.Text = selectedText;
            FormClosed += SettingsForm_FormClosed;

            if (_quickStyle)
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                splitMainContent.Panel2Collapsed = true;
            }

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

        #endregion

        #region -- Event --

        /// <summary>
        /// Form Load
        /// </summary>
        private async void CodeForm_Load(object sender, EventArgs e)
        {
            try
            {
                _initializer.Initialize(
                    txtCode,
                    _codeType,
                    cbx_style,
                    btnBackground,
                    cbx_Clipboard,
                    cbx_lineNumber);

                ResetBackgroundToSelectedTheme();
                UpdateBackgroundDisplay();

                if (!_quickStyle)
                {
                    ConnectPreviewEvents();
                    await InitializePreviewWebViewAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Exception while initializing MainForm: " +
                    ex.Message,
                    "NoteHighlight",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Form Closed
        /// </summary>
        private void CodeForm_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            SaveSetting();
            CleanupLivePreview();
        }

        private HighlightWorkflowRequest BuildWorkflowRequest()
        {
            return _requestFactory.Create(
                _fileName,
                CodeContent,
                _codeType,
                CodeStyle,
                IsShowLineNumber,
                IsClipboard,
                DarkMode,
                BackgroundColor);
        }

        /// <summary>
        /// Generate HighLight File
        /// </summary>
        private void btnCodeHighLight_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CodeStyle))
            {
                MessageBox.Show("Please select code Style!");
                return;
            }

            try
            {

                HighlightWorkflowRequest request = BuildWorkflowRequest();

                HighlightWorkflowResult result = _workflowService.Execute(request);

                ApplyBackgroundOverrideToGeneratedHtml(
                    result.OutputFileName);

                EnsureFinalOutputForAddIn(
                    result.OutputFileName);

                Parameters = result.Parameters;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

                Close();
            }
        }

        #endregion

        private void SaveSetting()
        {
            MainFormSettings settings =
                _settingsBinder.Capture(
                    cbx_style,
                    btnBackground,
                    cbx_Clipboard,
                    cbx_lineNumber);

            _settingsProvider.Save(settings);
            _themePreferenceProvider.SaveThemeName(CodeStyle);
        }

        private void btnBackground_Click(object sender, EventArgs e)
        {
            _backgroundColorSelector.ShowMenu(btnBackground, contextMenuStrip1);
        }

        // Simplified the logic for handling the form's Shown event
        private void MainForm_Shown(object sender, EventArgs e)
        {
            _displayCoordinator.HandleShown(this, _quickStyle, btnCodeHighLight);
        }

        private void PickColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Color previousColor =
                btnBackground.BackColor;

            Color initialColor =
                previousColor == Color.Transparent ||
                previousColor.A == 0
                    ? Color.White
                    : previousColor;

            using (ColorPickerForm picker =
                new ColorPickerForm(
                    initialColor))
            {
                if (picker.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                btnBackground.BackColor =
                    picker.SelectedColor;

                _hasBackgroundOverride =
                    true;
            }

            if (previousColor != btnBackground.BackColor)
            {
                UpdateBackgroundDisplay();
                RequestPreviewRefresh();
            }
        }

        private void TransparentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _backgroundColorSelector.SetTransparent(btnBackground);

            _hasBackgroundOverride =
                true;

            UpdateBackgroundDisplay();
            RequestPreviewRefresh();
        }


        private void CreateKeyboardHelpLegend()
        {
            _keyboardHelpLabel =
                new Label
                {
                    Name =
                        "lblKeyboardHelp",

                    AutoEllipsis =
                        true,

                    Location =
                        new Point(
                            180,
                            24),

                    Size =
                        new Size(
                            Math.Max(
                                220,
                                panel2.ClientSize.Width -
                                370),
                            24),

                    Anchor =
                        AnchorStyles.Left |
                        AnchorStyles.Right |
                        AnchorStyles.Top,

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

            panel2.Controls.Add(
                _keyboardHelpLabel);

            _keyboardHelpLabel.BringToFront();
        }


        private static string GetDefaultKeyboardHelp()
        {
            return
                "Keyboard: Tab = next  •  Shift+Tab = previous  •  Space = activate";
        }


        private string ResolveKeyboardHelp(
            Control control)
        {
            if (txtCode != null &&
                txtCode.ContainsFocus)
            {
                return
                    "Code Editor: Tab = indent  •  Shift+Tab = unindent  •  F6 = leave editor";
            }

            if (control == cbx_style)
            {
                return
                    "Theme: ↑/↓ = choose theme  •  Tab = next control";
            }

            if (control == cbx_Clipboard ||
                control == cbx_lineNumber)
            {
                return
                    "Toggle: Space = change  •  Tab = next  •  Shift+Tab = previous";
            }

            if (control == btnBackground)
            {
                return
                    "Background: Space/Enter = open options  •  Tab = next";
            }

            if (control == btnCodeHighLight)
            {
                return
                    "Insert Code: Space/Enter = insert into OneNote  •  Shift+Tab = previous";
            }

            if (_previewWebView != null &&
                control == _previewWebView)
            {
                return
                    "Preview: Tab = next control  •  Shift+Tab = previous  •  F6 = next";
            }

            return
                GetDefaultKeyboardHelp();
        }


        protected override bool ProcessCmdKey(
            ref Message msg,
            Keys keyData)
        {
            bool shift =
                (keyData & Keys.Shift) ==
                Keys.Shift;

            Keys keyCode =
                keyData &
                Keys.KeyCode;

            if (keyCode == Keys.F1)
            {
                KeyboardShortcutsForm.ShowHelp(
                    this);

                return true;
            }

            if (keyCode == Keys.F6)
            {
                MoveMainKeyboardFocus(
                    !shift);

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

            if (keyCode == Keys.Tab)
            {
                // The editor owns Tab/Shift+Tab for indentation.
                // F6/Shift+F6 is the explicit way to leave it.
                if (txtCode != null &&
                    txtCode.ContainsFocus)
                {
                    return base.ProcessDialogKey(
                        keyData);
                }

                MoveMainKeyboardFocus(
                    !shift);

                return true;
            }

            return base.ProcessDialogKey(
                keyData);
        }


        private void MoveMainKeyboardFocus(
            bool forward)
        {
            Control[] navigationOrder =
            {
                cbx_style,
                cbx_Clipboard,
                cbx_lineNumber,
                txtCode,
                _previewWebView,
                btnCodeHighLight,
                btnBackground
            };

            FocusControlInSequence(
                navigationOrder,
                forward);
        }


        private static bool CanUseKeyboardFocus(
            Control control)
        {
            return
                control != null &&
                !control.IsDisposed &&
                control.Visible &&
                control.Enabled &&
                control.CanSelect;
        }


        private static bool IsCurrentKeyboardFocus(
            Control control)
        {
            return
                control != null &&
                !control.IsDisposed &&
                control.ContainsFocus;
        }


        private void FocusControlInSequence(
            Control[] navigationOrder,
            bool forward)
        {
            if (navigationOrder == null ||
                navigationOrder.Length == 0)
            {
                return;
            }

            int currentIndex =
                -1;

            for (int index = 0;
                index < navigationOrder.Length;
                index++)
            {
                if (IsCurrentKeyboardFocus(
                    navigationOrder[index]))
                {
                    currentIndex =
                        index;

                    break;
                }
            }

            int step =
                forward
                    ? 1
                    : -1;

            int candidateIndex =
                currentIndex;

            for (int attempts = 0;
                attempts < navigationOrder.Length;
                attempts++)
            {
                if (candidateIndex < 0)
                {
                    candidateIndex =
                        forward
                            ? 0
                            : navigationOrder.Length - 1;
                }
                else
                {
                    candidateIndex =
                        (candidateIndex +
                            step +
                            navigationOrder.Length) %
                        navigationOrder.Length;
                }

                Control candidate =
                    navigationOrder[candidateIndex];

                if (!CanUseKeyboardFocus(
                    candidate))
                {
                    continue;
                }

                candidate.Focus();

                return;
            }
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


        private void ApplyModernAppearance()
        {
            UiStyleManager.StyleForm(this);

            UiStyleManager.StylePanel(pnlHeader, false);
            UiStyleManager.StylePanel(pnlOptions, true);
            UiStyleManager.StylePanel(panel3, false);
            UiStyleManager.StylePanel(panel2, true);
            UiStyleManager.StylePanel(pnlEditorCard, true);
            UiStyleManager.StylePanel(pnlEditorHeader, false);
            UiStyleManager.StylePanel(pnlPreviewCard, true);
            UiStyleManager.StylePanel(pnlPreviewHeader, false);
            UiStyleManager.StylePanel(pnlLivePreview, false);

            UiStyleManager.StyleLabel(lblTitle, false);
            lblTitle.Font = new Font(
                NoteHighlightUiTheme.FontFamily,
                15.5f,
                FontStyle.Bold,
                GraphicsUnit.Point);

            UiStyleManager.StyleLabel(lblSubtitle, true);
            UiStyleManager.StyleLabel(lblTheme, true);
            UiStyleManager.StyleSectionLabel(lblEditorTitle);
            UiStyleManager.StyleSectionLabel(lblLivePreviewTitle);
            UiStyleManager.StyleLabel(lblPreviewStatus, true);
            lblPreviewStatus.Font =
                NoteHighlightUiTheme.CreateSmallFont();
            UiStyleManager.StyleLabel(lblBackgroundCaption, true);
            UiStyleManager.StyleLabel(lblBackgroundValue, false);

            UiStyleManager.StyleComboBox(cbx_style);
            cbx_Clipboard.Location =
                new Point(
                    302,
                    27);

            cbx_Clipboard.Size =
                new Size(
                    136,
                    30);

            cbx_Clipboard.Text =
                "Copy to Clipboard";

            UiStyleManager.StyleToggleCheckBox(
                cbx_Clipboard,
                FontStyle.Regular);

            cbx_lineNumber.Location =
                new Point(
                    448,
                    27);

            cbx_lineNumber.Size =
                new Size(
                    116,
                    30);

            cbx_lineNumber.Text =
                "Line numbers";

            UiStyleManager.StyleToggleCheckBox(
                cbx_lineNumber,
                FontStyle.Regular);
            UiStyleManager.StylePrimaryButton(btnCodeHighLight);
            UiStyleManager.StyleSecondaryButton(btnBackground);

            btnCodeHighLight.Text = "Insert Code";

            splitMainContent.BackColor =
                NoteHighlightUiTheme.WindowBackground;

            splitMainContent.Panel1.BackColor =
                NoteHighlightUiTheme.WindowBackground;

            splitMainContent.Panel2.BackColor =
                NoteHighlightUiTheme.WindowBackground;

            pnlEditorCard.BorderStyle =
                BorderStyle.None;

            pnlPreviewCard.BorderStyle =
                BorderStyle.None;

            pnlEditorCard.Paint -=
                CardPanel_Paint;

            pnlEditorCard.Paint +=
                CardPanel_Paint;

            pnlPreviewCard.Paint -=
                CardPanel_Paint;

            pnlPreviewCard.Paint +=
                CardPanel_Paint;

            pnlLivePreview.BackColor =
                NoteHighlightUiTheme.Surface;

            lblPreviewStatus.BackColor =
                NoteHighlightUiTheme.SurfaceRaised;

            contextMenuStrip1.BackColor =
                NoteHighlightUiTheme.SurfaceRaised;

            contextMenuStrip1.ForeColor =
                NoteHighlightUiTheme.TextPrimary;

            contextMenuStrip1.RenderMode =
                ToolStripRenderMode.System;
        }


        private void CardPanel_Paint(
            object sender,
            PaintEventArgs e)
        {
            Panel panel =
                sender as Panel;

            if (panel == null ||
                panel.ClientRectangle.Width <= 0 ||
                panel.ClientRectangle.Height <= 0)
            {
                return;
            }

            Rectangle borderRectangle =
                new Rectangle(
                    0,
                    0,
                    panel.ClientRectangle.Width - 1,
                    panel.ClientRectangle.Height - 1);

            using (Pen borderPen =
                new Pen(
                    NoteHighlightUiTheme.Border))
            {
                e.Graphics.DrawRectangle(
                    borderPen,
                    borderRectangle);
            }
        }


        private void UpdateBackgroundDisplay()
        {
            Color color =
                btnBackground.BackColor;

            bool isTransparent =
                color == Color.Transparent ||
                color.A == 0;

            lblBackgroundValue.Text =
                isTransparent
                    ? "Transparent"
                    : string.Format(
                        "#{0:X2}{1:X2}{2:X2}",
                        color.R,
                        color.G,
                        color.B);

            btnBackground.Text =
                string.Empty;

            btnBackground.FlatAppearance.MouseOverBackColor =
                color;

            btnBackground.FlatAppearance.MouseDownBackColor =
                color;
        }

        private void EnsureFinalOutputForAddIn(
            string generatedFilePath)
        {
            if (string.IsNullOrWhiteSpace(
                generatedFilePath) ||
                !File.Exists(
                    generatedFilePath))
            {
                throw new FileNotFoundException(
                    "The final highlighted HTML file was not generated.",
                    generatedFilePath);
            }

            string expectedFilePath =
                Path.Combine(
                    Path.GetTempPath(),
                    _fileName + ".html");

            string generatedFullPath =
                Path.GetFullPath(
                    generatedFilePath);

            string expectedFullPath =
                Path.GetFullPath(
                    expectedFilePath);

            if (string.Equals(
                generatedFullPath,
                expectedFullPath,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            File.Copy(
                generatedFullPath,
                expectedFullPath,
                true);
        }


        private void ApplyBackgroundOverrideToGeneratedHtml(
            string htmlFilePath)
        {
            if (!_hasBackgroundOverride ||
                string.IsNullOrWhiteSpace(
                    htmlFilePath) ||
                !File.Exists(
                    htmlFilePath))
            {
                return;
            }

            string html =
                File.ReadAllText(
                    htmlFilePath);

            string updatedHtml =
                ApplyBackgroundOverrideToHtml(
                    html,
                    BackgroundColor);

            if (!string.Equals(
                html,
                updatedHtml,
                StringComparison.Ordinal))
            {
                File.WriteAllText(
                    htmlFilePath,
                    updatedHtml);
            }
        }


        private static string ApplyBackgroundOverrideToHtml(
            string html,
            Color backgroundColor)
        {
            if (string.IsNullOrEmpty(
                html))
            {
                return html;
            }

            int preStart =
                html.IndexOf(
                    "<pre",
                    StringComparison.OrdinalIgnoreCase);

            if (preStart < 0)
            {
                return html;
            }

            int preEnd =
                html.IndexOf(
                    '>',
                    preStart);

            if (preEnd < 0)
            {
                return html;
            }

            string preTag =
                html.Substring(
                    preStart,
                    preEnd - preStart + 1);

            string updatedPreTag =
                ReplaceBackgroundColourInPreTag(
                    preTag,
                    backgroundColor);

            if (string.Equals(
                preTag,
                updatedPreTag,
                StringComparison.Ordinal))
            {
                return html;
            }

            return
                html.Substring(
                    0,
                    preStart) +
                updatedPreTag +
                html.Substring(
                    preEnd + 1);
        }


        private static string ReplaceBackgroundColourInPreTag(
            string preTag,
            Color backgroundColor)
        {
            const string propertyName =
                "background-color";

            int propertyIndex =
                preTag.IndexOf(
                    propertyName,
                    StringComparison.OrdinalIgnoreCase);

            bool transparent =
                backgroundColor == Color.Transparent ||
                backgroundColor.A == 0;

            string replacement =
                transparent
                    ? string.Empty
                    : string.Format(
                        "background-color:#{0:X2}{1:X2}{2:X2};",
                        backgroundColor.R,
                        backgroundColor.G,
                        backgroundColor.B);

            if (propertyIndex >= 0)
            {
                int semicolonIndex =
                    preTag.IndexOf(
                        ';',
                        propertyIndex);

                if (semicolonIndex < 0)
                {
                    semicolonIndex =
                        preTag.IndexOf(
                            '"',
                            propertyIndex);

                    if (semicolonIndex < 0)
                    {
                        return preTag;
                    }

                    return
                        preTag.Remove(
                            propertyIndex,
                            semicolonIndex -
                            propertyIndex)
                        .Insert(
                            propertyIndex,
                            replacement);
                }

                return
                    preTag.Remove(
                        propertyIndex,
                        semicolonIndex -
                        propertyIndex + 1)
                    .Insert(
                        propertyIndex,
                        replacement);
            }

            if (transparent)
            {
                return preTag;
            }

            int styleIndex =
                preTag.IndexOf(
                    "style=\"",
                    StringComparison.OrdinalIgnoreCase);

            if (styleIndex >= 0)
            {
                int styleContentStart =
                    styleIndex +
                    "style=\"".Length;

                return
                    preTag.Insert(
                        styleContentStart,
                        replacement);
            }

            return
                preTag.Insert(
                    preTag.Length - 1,
                    " style=\"" +
                    replacement +
                    "\"");
        }


        private void ThemeSelectionChanged(
            object sender,
            EventArgs e)
        {
            ResetBackgroundToSelectedTheme();
            UpdateBackgroundDisplay();
            RequestPreviewRefresh();
        }


        private void ResetBackgroundToSelectedTheme()
        {
            _hasBackgroundOverride =
                false;

            Color themeBackground;

            if (TryGetSelectedThemeBackground(
                out themeBackground))
            {
                btnBackground.BackColor =
                    themeBackground;
            }
        }


        private bool TryGetSelectedThemeBackground(
            out Color backgroundColor)
        {
            backgroundColor =
                btnBackground.BackColor;

            if (string.IsNullOrWhiteSpace(
                CodeStyle))
            {
                return false;
            }

            try
            {
                string themePath =
                    Path.Combine(
                        PathManager.ThemesFolder,
                        CodeStyle + ".theme");

                if (!File.Exists(
                    themePath))
                {
                    return false;
                }

                HighlightTheme theme =
                    _themeReader.Read(
                        themePath);

                if (theme == null ||
                    theme.Styles == null)
                {
                    return false;
                }

                ThemeStyle canvasStyle;

                if (!theme.Styles.TryGetValue(
                    "Canvas",
                    out canvasStyle) ||
                    canvasStyle == null ||
                    string.IsNullOrWhiteSpace(
                        canvasStyle.Colour))
                {
                    return false;
                }

                return TryParseThemeBackgroundColour(
                    canvasStyle.Colour,
                    out backgroundColor);
            }
            catch
            {
                // A malformed theme should not prevent MainForm from opening.
                // highlight.exe will still use its normal theme handling.
                return false;
            }
        }


        private static bool TryParseThemeBackgroundColour(
            string value,
            out Color color)
        {
            color =
                Color.Transparent;

            if (string.IsNullOrWhiteSpace(
                value))
            {
                return false;
            }

            string normalized =
                value.Trim();

            if (string.Equals(
                normalized,
                "none",
                StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    normalized,
                    "transparent",
                    StringComparison.OrdinalIgnoreCase))
            {
                color =
                    Color.Transparent;

                return true;
            }

            if (!normalized.StartsWith(
                "#",
                StringComparison.Ordinal))
            {
                normalized =
                    "#" +
                    normalized;
            }

            try
            {
                color =
                    ColorTranslator.FromHtml(
                        normalized);

                return true;
            }
            catch
            {
                return false;
            }
        }


        #region -- Live Preview --

        private void ConnectPreviewEvents()
        {
            if (_previewEventsConnected)
            {
                return;
            }

            txtCode.TextChanged += PreviewInputChanged;
            cbx_style.SelectedIndexChanged += PreviewInputChanged;
            cbx_style.SelectedIndexChanged += ThemeSelectionChanged;
            cbx_lineNumber.CheckedChanged += PreviewInputChanged;

            _previewEventsConnected = true;
        }

        private void DisconnectPreviewEvents()
        {
            if (!_previewEventsConnected)
            {
                return;
            }

            txtCode.TextChanged -= PreviewInputChanged;
            cbx_style.SelectedIndexChanged -= PreviewInputChanged;
            cbx_style.SelectedIndexChanged -= ThemeSelectionChanged;
            cbx_lineNumber.CheckedChanged -= PreviewInputChanged;

            _previewEventsConnected = false;
        }

        private void PreviewInputChanged(object sender, EventArgs e)
        {
            RequestPreviewRefresh();
        }

        private async Task InitializePreviewWebViewAsync()
        {
            if (_previewWebView != null || IsDisposed || Disposing)
            {
                return;
            }

            _previewWebView = new WebView2
            {
                Dock = DockStyle.Fill,
                Name = "mainFormPreviewWebView"
            };

            pnlLivePreview.Controls.Add(_previewWebView);
            lblPreviewStatus.Text = "Initializing preview...";

            try
            {
                string userDataFolder =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        "NoteHighlightPlus",
                        "WebView2");

                Directory.CreateDirectory(userDataFolder);

                CoreWebView2Environment environment =
                    await CoreWebView2Environment.CreateAsync(
                        browserExecutableFolder: null,
                        userDataFolder: userDataFolder,
                        options: null);

                if (IsDisposed || Disposing || _previewWebView == null)
                {
                    return;
                }

                await _previewWebView.EnsureCoreWebView2Async(environment);

                _previewWebView.NavigationCompleted +=
                    PreviewWebView_NavigationCompleted;

                lblPreviewStatus.Text = "Preview ready.";
                RequestPreviewRefresh();
            }
            catch (Exception exception)
            {
                lblPreviewStatus.Text = "WebView2 initialization failed.";

                MessageBox.Show(
                    this,
                    exception.ToString(),
                    "Live Preview error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void RequestPreviewRefresh()
        {
            if (_quickStyle || IsDisposed || Disposing)
            {
                return;
            }

            _previewRefreshPending = true;
            _previewRefreshTimer.Stop();
            _previewRefreshTimer.Start();
        }

        private void PreviewRefreshTimer_Tick(object sender, EventArgs e)
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

            _previewRefreshPending = false;
            RefreshLivePreview();
        }

        private void RefreshLivePreview()
        {
            if (_isGeneratingPreview)
            {
                _previewRefreshPending = true;
                return;
            }

            if (_previewWebView == null ||
                _previewWebView.CoreWebView2 == null)
            {
                _previewRefreshPending = true;
                lblPreviewStatus.Text = "Preview is not ready.";
                return;
            }

            if (string.IsNullOrWhiteSpace(CodeStyle))
            {
                lblPreviewStatus.Text = "Select a style to preview.";
                return;
            }

            try
            {
                _isGeneratingPreview = true;
                lblPreviewStatus.Text = "Generating preview...";

                HighlightWorkflowRequest request =
                    CreatePreviewWorkflowRequest();

                HighlightWorkflowResult result =
                    _workflowService.Execute(request);

                ApplyBackgroundOverrideToGeneratedHtml(
                    result.OutputFileName);

                if (string.IsNullOrWhiteSpace(result.OutputFileName) ||
                    !File.Exists(result.OutputFileName))
                {
                    throw new FileNotFoundException(
                        "The live preview HTML file was not generated.",
                        result.OutputFileName);
                }

                _previewFiles.Add(result.OutputFileName);

                _previewWebView.Source =
                    new Uri(result.OutputFileName);

                lblPreviewStatus.Text = "Loading preview...";
            }
            catch (Exception exception)
            {
                lblPreviewStatus.Text = "Preview generation failed.";

                MessageBox.Show(
                    this,
                    exception.ToString(),
                    "Live Preview error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isGeneratingPreview = false;

                if (_previewRefreshPending)
                {
                    _previewRefreshTimer.Stop();
                    _previewRefreshTimer.Start();
                }
            }
        }

        private HighlightWorkflowRequest CreatePreviewWorkflowRequest()
        {
            string extension = Path.GetExtension(_fileName);

            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".txt";
            }

            string previewFileName =
                "notehighlight_main_preview_" +
                Guid.NewGuid().ToString("N") +
                extension;

            return _requestFactory.Create(
                previewFileName,
                CodeContent,
                _codeType,
                CodeStyle,
                IsShowLineNumber,
                false,
                DarkMode,
                BackgroundColor);
        }

        private void PreviewWebView_NavigationCompleted(
            object sender,
            CoreWebView2NavigationCompletedEventArgs e)
        {
            lblPreviewStatus.Text = e.IsSuccess
                ? "Preview updated."
                : "Preview could not be loaded: " + e.WebErrorStatus;
        }

        private void CleanupLivePreview()
        {
            _previewRefreshTimer.Stop();
            _previewRefreshTimer.Tick -= PreviewRefreshTimer_Tick;

            DisconnectPreviewEvents();

            if (_previewWebView != null)
            {
                _previewWebView.NavigationCompleted -=
                    PreviewWebView_NavigationCompleted;

                _previewWebView.Dispose();
                _previewWebView = null;
            }

            foreach (string previewFile in _previewFiles)
            {
                TryDeletePreviewFile(previewFile);
            }

            _previewFiles.Clear();
        }

        private static void TryDeletePreviewFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // WebView2 may briefly keep the last preview file open.
                // A stale temp preview is harmless and can be removed later.
            }
        }

        #endregion
    }
}
