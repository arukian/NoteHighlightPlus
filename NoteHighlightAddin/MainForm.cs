using GenerateHighlightContent;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using NoteHighlightAddin.Highlighting.Preview.Services;
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

        // Live preview state.
        private readonly Timer _previewRefreshTimer;
        private readonly List<string> _previewFiles;
        private WebView2 _previewWebView;
        private bool _previewRefreshPending;
        private bool _isGeneratingPreview;
        private bool _previewEventsConnected;

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

            txtCode.Text = selectedText;
            FormClosed += SettingsForm_FormClosed;

            if (_quickStyle)
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                splitMainContent.Panel2Collapsed = true;
            }
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
            Color previousColor = btnBackground.BackColor;

            _backgroundColorSelector.PickColor(btnBackground, colorDialog1);

            if (previousColor != btnBackground.BackColor)
            {
                RequestPreviewRefresh();
            }
        }

        private void TransparentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _backgroundColorSelector.SetTransparent(btnBackground);
            RequestPreviewRefresh();
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
