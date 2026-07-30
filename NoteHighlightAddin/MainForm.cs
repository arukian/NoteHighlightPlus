using GenerateHighlightContent;
using NoteHighlightAddin.Highlighting.Preview.Services;
using System;
using System.Drawing;
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
            _backgroundColorSelector = new BackgroundColorSelector();
            var themeBinder = new ThemeComboBoxBinder(new ThemeProvider());
            var editorConfigurator = new CodeEditorConfigurator(new CodeEditorLanguageMapper());
            _initializer = new MainFormInitializer(themeBinder,editorConfigurator,_settingsProvider,_settingsBinder);
            _requestFactory = new HighlightWorkflowRequestFactory();
            var workflowServiceFactory = new HighlightWorkflowServiceFactory(_settingsProvider);
            _workflowService = workflowServiceFactory.Create();
            _displayCoordinator = new MainFormDisplayCoordinator(new WindowForegroundService());

            InitializeComponent();

            txtCode.Text = selectedText;
            FormClosed += SettingsForm_FormClosed;

            if (_quickStyle)
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
            }
        }

        #endregion

        #region -- Event --

        /// <summary>
        /// Form Load
        /// </summary>
        private void CodeForm_Load(object sender, EventArgs e)
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
        }

        // Added new function 

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
        }

        private void btnBackground_Click(object sender,EventArgs e)
        {
            _backgroundColorSelector.ShowMenu(btnBackground,contextMenuStrip1);
        }

        // Simplified the logic for handling the form's Shown event
        private void MainForm_Shown(object sender, EventArgs e)
        {
            _displayCoordinator.HandleShown(this, _quickStyle, btnCodeHighLight);
        }

        private void PickColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _backgroundColorSelector.PickColor(btnBackground, colorDialog1);
        }

        private void TransparentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _backgroundColorSelector.SetTransparent(btnBackground);
        }
    }
}