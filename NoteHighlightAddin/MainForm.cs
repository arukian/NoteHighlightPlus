using GenerateHighlightContent;
using ICSharpCode.TextEditor.Document;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace NoteHighlightAddin
{
    public partial class MainForm : Form
    {
        #region -- Field and Property --

        //檔案類型
        private string _codeType;

        //檔案名稱
        private string _fileName;

        private bool _darkMode;

        //要HighLight的Code
        private string CodeContent { get { return this.txtCode.Text; } }

        // Added to use the ThemeProvider.cs 
        private readonly ThemeProvider _themeProvider;

        private readonly HighLightParameterFactory _parameterFactory;

        private readonly CodeEditorLanguageMapper _languageMapper;

        private readonly MainFormSettingsProvider _settingsProvider;

        private readonly HighlightGenerationService _highlightGenerationService;

        private readonly HighlightClipboardService _clipboardService;

        //HighLight的樣式
        private string CodeStyle { get { return this.cbx_style.Text; } }

        //是否要行號
        private bool IsShowLineNumber { get { return this.cbx_lineNumber.Checked; } }

        //是否存到剪貼簿
        private bool IsClipboard { get { return this.cbx_Clipboard.Checked; } }

        private Color BackgroundColor { get { return this.btnBackground.BackColor; } }

        public HighLightParameter Parameters { get; private set; }

        private bool _quickStyle;

        public bool DarkMode { get { return _darkMode; } }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion

        #region -- Constructor --

        public MainForm(
         string codeType,
         string fileName,
         string selectedText,
         bool quickStyle,
         bool darkMode)
        {
            _codeType = codeType;
            _fileName = fileName;
            _quickStyle = quickStyle;
            _darkMode = darkMode;

            _themeProvider = new ThemeProvider();
            _parameterFactory = new HighLightParameterFactory();
            _languageMapper = new CodeEditorLanguageMapper();
            _settingsProvider = new MainFormSettingsProvider();
            _highlightGenerationService = new HighlightGenerationService();
            _clipboardService = new HighlightClipboardService();

            InitializeComponent();
            LoadThemes();

            txtCode.Text = selectedText;

            if (_quickStyle)
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
            }
        }

        private void LoadThemes()
        {
            try
            {
                cbx_style.Items.Clear();

                foreach (string themeName in _themeProvider.GetThemeNames())
                {
                    cbx_style.Items.Add(themeName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Exception from MainForm.LoadThemes: " + ex.Message,
                    "NoteHighlight",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region -- Event --

        /// <summary>
        /// Form Load
        /// </summary>
        private void CodeForm_Load(object sender, EventArgs e)
        {
            txtCode.Document.HighlightingStrategy =
                HighlightingStrategyFactory.CreateHighlightingStrategy(
                    _languageMapper.GetHighlightingName(_codeType));

            txtCode.Encoding = Encoding.UTF8;

            MainFormSettings settings = _settingsProvider.Load();

            if (settings.HighLightStyle >= 0 &&
                settings.HighLightStyle < cbx_style.Items.Count)
            {
                cbx_style.SelectedIndex = settings.HighLightStyle;
            }

            btnBackground.BackColor = settings.BackgroundColor;
            cbx_Clipboard.Checked = settings.SaveOnClipboard;
            cbx_lineNumber.Checked = settings.ShowLineNumber;
        }

        /// <summary>
        /// Form Closed
        /// </summary>
        private void CodeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSetting();
        }

        // Added new function 

        private HighlightWorkflowRequest BuildWorkflowRequest()
        {
            return new HighlightWorkflowRequest
            {
                FileName = _fileName,
                Content = CodeContent,
                CodeType = _codeType,
                HighLightStyle = CodeStyle,
                ShowLineNumber = IsShowLineNumber,
                CopyToClipboard = IsClipboard,
                DarkMode = DarkMode,
                HighlightColor = BackgroundColor
            };
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

            string outputFileName = string.Empty;

            HighlightWorkflowRequest request =
                BuildWorkflowRequest();

            MainFormSettings settings =
                _settingsProvider.Load();

            Parameters = _parameterFactory.Create(
                request.FileName,
                request.Content,
                request.CodeType,
                request.HighLightStyle,
                request.ShowLineNumber,
                request.HighlightColor,
                settings.Font,
                settings.FontSize);

            try
            {
                outputFileName =
                    _highlightGenerationService.Generate(Parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Dispose();
                Close();
                return;
            }

            if (request.CopyToClipboard &&
                !string.IsNullOrEmpty(outputFileName))
            {
                _clipboardService.Copy(
                    outputFileName,
                    request.DarkMode,
                    request.ShowLineNumber);
            }

            SaveSetting();

            Dispose();
            Close();
        }

        #endregion

        private void SaveSetting()
        {
            var settings = new MainFormSettings
            {
                ShowLineNumber = cbx_lineNumber.Checked,
                SaveOnClipboard = cbx_Clipboard.Checked,
                HighLightStyle = cbx_style.SelectedIndex,
                BackgroundColor = btnBackground.BackColor
            };

            _settingsProvider.Save(settings);
        }

        private void btnBackground_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(btnBackground, new Point(0, btnBackground.Height));

        }

        private void MainForm_Shown(object sender, EventArgs e)
        {

            if (_quickStyle)
            {
                btnCodeHighLight.PerformClick()
;
            }
            else
            {
                // This is necessary in order for SetForegroundWindow to work consistently
                this.WindowState = FormWindowState.Minimized;
                this.WindowState = FormWindowState.Normal;

                SetForegroundWindow(this.Handle);
            }

        }

        private void PickColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnBackground.BackColor = colorDialog1.Color;
            }
        }

        private void TransparentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnBackground.BackColor = Color.Transparent;
        }
    }
}
