using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NoteHighlightAddin
{
    /// <summary>
    /// Modern reusable color picker for NoteHighlight+.
    /// Supports visual HSV selection plus synchronized HEX and RGB editing.
    /// </summary>
    internal sealed class ColorPickerForm : Form
    {
        private readonly Color _originalColor;
        private Color _selectedColor;

        private Panel _saturationValuePanel;
        private Panel _huePanel;
        private Panel _currentPreview;
        private Panel _newPreview;
        private TextBox _txtHex;
        private NumericUpDown _nudRed;
        private NumericUpDown _nudGreen;
        private NumericUpDown _nudBlue;
        private Label _lblValidation;
        private FlowLayoutPanel _quickColors;
        private FlowLayoutPanel _recentColors;
        private Button _btnApply;
        private Button _btnCancel;
        private readonly KeyboardFocusVisualManager _keyboardFocusVisualManager;
        private readonly KeyboardHelpManager _keyboardHelpManager;
        private Label _keyboardHelpLabel;

        private double _hue;
        private double _saturation;
        private double _value;

        private bool _updatingControls;
        private bool _draggingSaturationValue;
        private bool _draggingHue;

        private const int MaxRecentColors = 8;

        private static readonly string RecentColorsFilePath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "NoteHighlightPlus",
                "recent-colours.txt");

        private static readonly Color[] QuickColorPalette =
        {
            Color.FromArgb(255, 255, 255),
            Color.FromArgb(220, 220, 220),
            Color.FromArgb(160, 160, 160),
            Color.FromArgb(90, 90, 90),
            Color.FromArgb(35, 35, 35),
            Color.FromArgb(0, 0, 0),

            Color.FromArgb(244, 67, 54),
            Color.FromArgb(255, 152, 0),
            Color.FromArgb(255, 193, 7),
            Color.FromArgb(205, 220, 57),
            Color.FromArgb(76, 175, 80),
            Color.FromArgb(0, 188, 212),

            Color.FromArgb(3, 169, 244),
            Color.FromArgb(33, 150, 243),
            Color.FromArgb(63, 81, 181),
            Color.FromArgb(103, 58, 183),
            Color.FromArgb(156, 39, 176),
            Color.FromArgb(233, 30, 99),

            Color.FromArgb(124, 93, 152),
            Color.FromArgb(197, 142, 192),
            Color.FromArgb(86, 156, 214),
            Color.FromArgb(78, 201, 176),
            Color.FromArgb(206, 145, 120),
            Color.FromArgb(220, 220, 170)
        };


        public ColorPickerForm(
            Color initialColor)
        {
            _originalColor =
                initialColor;

            _selectedColor =
                initialColor;

            RgbToHsv(
                initialColor,
                out _hue,
                out _saturation,
                out _value);

            InitializePicker();
            ApplyTheme();
            SyncControlsFromColor();
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


        public Color SelectedColor =>
            _selectedColor;


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

            if (keyCode == Keys.Escape)
            {
                DialogResult =
                    DialogResult.Cancel;

                Close();

                return true;
            }

            Button focusedSwatch =
                GetFocusedSwatch();

            if (focusedSwatch != null)
            {
                if (keyCode == Keys.Left ||
                    keyCode == Keys.Right ||
                    keyCode == Keys.Up ||
                    keyCode == Keys.Down)
                {
                    MoveFocusedSwatch(
                        focusedSwatch,
                        keyCode);

                    return true;
                }

                if (keyCode == Keys.Enter ||
                    keyCode == Keys.Space)
                {
                    focusedSwatch.PerformClick();

                    return true;
                }
            }

            if (_saturationValuePanel != null &&
                _saturationValuePanel.ContainsFocus &&
                (keyCode == Keys.Left ||
                 keyCode == Keys.Right ||
                 keyCode == Keys.Up ||
                 keyCode == Keys.Down))
            {
                AdjustSaturationValueFromKeyboard(
                    keyCode,
                    shift);

                return true;
            }

            if (_huePanel != null &&
                _huePanel.ContainsFocus &&
                (keyCode == Keys.Up ||
                 keyCode == Keys.Down))
            {
                AdjustHueFromKeyboard(
                    keyCode,
                    shift);

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
                MovePickerKeyboardFocus(
                    !shift);

                return true;
            }

            return base.ProcessDialogKey(
                keyData);
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
                            24,
                            640),

                    Size =
                        new Size(
                            520,
                            24),

                    Anchor =
                        AnchorStyles.Left |
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

            Controls.Add(
                _keyboardHelpLabel);

            _keyboardHelpLabel.BringToFront();
        }


        private static string GetDefaultKeyboardHelp()
        {
            return
                "Keyboard: Tab = next  •  Shift+Tab = previous  •  Enter = Apply  •  Esc = Cancel";
        }


        private string ResolveKeyboardHelp(
            Control control)
        {
            if (IsSwatchInPalette(
                control,
                _quickColors))
            {
                return
                    "Quick colours: ←/→/↑/↓ = navigate  •  Space/Enter = select  •  Tab = next section";
            }

            if (IsSwatchInPalette(
                control,
                _recentColors))
            {
                return
                    "Recent: ←/→/↑/↓ = navigate  •  Space/Enter = select  •  Tab = next section";
            }

            if (control == _saturationValuePanel)
            {
                return
                    "Colour field: ←/→ = saturation  •  ↑/↓ = brightness  •  Shift+Arrow = faster";
            }

            if (control == _huePanel)
            {
                return
                    "Hue: ↑/↓ = change hue  •  Shift+↑/↓ = faster  •  Tab = next section";
            }

            if (control == _txtHex)
            {
                return
                    "HEX: type #RRGGBB  •  Enter = Apply  •  Esc = Cancel";
            }

            if (control == _nudRed ||
                control == _nudGreen ||
                control == _nudBlue)
            {
                return
                    "RGB: type 0–255 or use ↑/↓  •  Enter = Apply  •  Esc = Cancel";
            }

            if (control == _btnApply)
            {
                return
                    "Apply: Space/Enter = accept colour  •  Esc = Cancel";
            }

            if (control == _btnCancel)
            {
                return
                    "Cancel: Space/Enter/Esc = close without applying";
            }

            return
                GetDefaultKeyboardHelp();
        }


        private void InitializePicker()
        {
            SuspendLayout();

            Text =
                "Select Colour";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox =
                false;

            MinimizeBox =
                false;

            ShowInTaskbar =
                false;

            ClientSize =
                new Size(
                    860,
                    700);

            MinimumSize =
                new Size(
                    876,
                    739);

            Font =
                NoteHighlightUiTheme.CreateBodyFont();

            BackColor =
                NoteHighlightUiTheme.WindowBackground;

            ForeColor =
                NoteHighlightUiTheme.TextPrimary;

            Label title =
                CreateLabel(
                    "Select Colour",
                    new Point(24, 20),
                    false);

            title.Font =
                new Font(
                    NoteHighlightUiTheme.FontFamily,
                    16.0f,
                    FontStyle.Bold,
                    GraphicsUnit.Point);

            Label subtitle =
                CreateLabel(
                    "Choose visually or enter an exact HEX / RGB value.",
                    new Point(25, 50),
                    true);

            GroupBox quickGroup =
                CreateGroupBox(
                    "Quick colours",
                    new Rectangle(
                        24,
                        86,
                        204,
                        512));

            _quickColors =
                new FlowLayoutPanel
                {
                    Location =
                        new Point(
                            16,
                            30),

                    Size =
                        new Size(
                            172,
                            214),

                    FlowDirection =
                        FlowDirection.LeftToRight,

                    WrapContents =
                        true,

                    BackColor =
                        NoteHighlightUiTheme.Surface,

                    Margin =
                        Padding.Empty,

                    Padding =
                        Padding.Empty
                };

            quickGroup.Controls.Add(
                _quickColors);

            AddQuickColors();

            Label recentCaption =
                CreateLabel(
                    "Recent",
                    new Point(16, 254),
                    true);

            recentCaption.Parent =
                quickGroup;

            _recentColors =
                new FlowLayoutPanel
                {
                    Location =
                        new Point(
                            16,
                            278),

                    Size =
                        new Size(
                            172,
                            76),

                    FlowDirection =
                        FlowDirection.LeftToRight,

                    WrapContents =
                        true,

                    AutoScroll =
                        false,

                    BackColor =
                        NoteHighlightUiTheme.Surface,

                    Margin =
                        Padding.Empty,

                    Padding =
                        Padding.Empty
                };

            quickGroup.Controls.Add(
                _recentColors);

            LoadRecentColors();

            Label currentCaption =
                CreateLabel(
                    "Current",
                    new Point(16, 374),
                    true);

            currentCaption.Parent =
                quickGroup;

            _currentPreview =
                new Panel
                {
                    Location =
                        new Point(
                            16,
                            398),

                    Size =
                        new Size(
                            78,
                            72),

                    BackColor =
                        _originalColor
                };

            quickGroup.Controls.Add(
                _currentPreview);

            Label newCaption =
                CreateLabel(
                    "New",
                    new Point(110, 374),
                    true);

            newCaption.Parent =
                quickGroup;

            _newPreview =
                new Panel
                {
                    Location =
                        new Point(
                            110,
                            398),

                    Size =
                        new Size(
                            78,
                            72),

                    BackColor =
                        _selectedColor
                };

            quickGroup.Controls.Add(
                _newPreview);

            Label pickerCaption =
                CreateLabel(
                    "Custom colour",
                    new Point(258, 88),
                    false);

            pickerCaption.Font =
                NoteHighlightUiTheme.CreateSectionFont();

            _saturationValuePanel =
                new BufferedPickerPanel
                {
                    Location =
                        new Point(
                            258,
                            118),

                    Size =
                        new Size(
                            360,
                            320),

                    Cursor =
                        Cursors.Cross,

                    TabStop =
                        true,

                    AccessibleName =
                        "Custom colour field"
                };

            _saturationValuePanel.Paint +=
                SaturationValuePanel_Paint;

            _saturationValuePanel.MouseDown +=
                SaturationValuePanel_MouseDown;

            _saturationValuePanel.MouseMove +=
                SaturationValuePanel_MouseMove;

            _saturationValuePanel.MouseUp +=
                SaturationValuePanel_MouseUp;

            _huePanel =
                new BufferedPickerPanel
                {
                    Location =
                        new Point(
                            630,
                            118),

                    Size =
                        new Size(
                            28,
                            320),

                    Cursor =
                        Cursors.Hand,

                    TabStop =
                        true,

                    AccessibleName =
                        "Hue"
                };

            _huePanel.Paint +=
                HuePanel_Paint;

            _huePanel.MouseDown +=
                HuePanel_MouseDown;

            _huePanel.MouseMove +=
                HuePanel_MouseMove;

            _huePanel.MouseUp +=
                HuePanel_MouseUp;

            GroupBox valuesGroup =
                CreateGroupBox(
                    "Values",
                    new Rectangle(
                        682,
                        86,
                        154,
                        512));

            Label hexCaption =
                CreateLabel(
                    "HEX",
                    new Point(16, 32),
                    true);

            hexCaption.Parent =
                valuesGroup;

            _txtHex =
                new TextBox
                {
                    Location =
                        new Point(
                            16,
                            54),

                    Size =
                        new Size(
                            122,
                            24),

                    CharacterCasing =
                        CharacterCasing.Upper
                };

            valuesGroup.Controls.Add(
                _txtHex);

            UiStyleManager.StyleTextBox(
                _txtHex);

            _txtHex.TextChanged +=
                HexTextChanged;

            Label rgbCaption =
                CreateLabel(
                    "RGB",
                    new Point(16, 100),
                    true);

            rgbCaption.Parent =
                valuesGroup;

            _nudRed =
                CreateRgbInput(
                    valuesGroup,
                    "R",
                    126);

            _nudGreen =
                CreateRgbInput(
                    valuesGroup,
                    "G",
                    170);

            _nudBlue =
                CreateRgbInput(
                    valuesGroup,
                    "B",
                    214);

            _nudRed.ValueChanged +=
                RgbValueChanged;

            _nudGreen.ValueChanged +=
                RgbValueChanged;

            _nudBlue.ValueChanged +=
                RgbValueChanged;

            _lblValidation =
                CreateLabel(
                    string.Empty,
                    new Point(16, 266),
                    true);

            _lblValidation.Parent =
                valuesGroup;

            _lblValidation.MaximumSize =
                new Size(
                    122,
                    52);

            _lblValidation.ForeColor =
                NoteHighlightUiTheme.Danger;

            Label hint =
                CreateLabel(
                    "HEX format: #RRGGBB",
                    new Point(16, 320),
                    true);

            hint.Parent =
                valuesGroup;

            _btnApply =
                new Button
                {
                    Text =
                        "Apply",

                    Location =
                        new Point(
                            706,
                            636),

                    Size =
                        new Size(
                            130,
                            38),

                    DialogResult =
                        DialogResult.OK
                };

            UiStyleManager.StylePrimaryButton(
                _btnApply);

            _btnApply.Click +=
                ApplyButton_Click;

            _btnCancel =
                new Button
                {
                    Text =
                        "Cancel",

                    Location =
                        new Point(
                            570,
                            636),

                    Size =
                        new Size(
                            124,
                            38),

                    DialogResult =
                        DialogResult.Cancel
                };

            UiStyleManager.StyleSecondaryButton(
                _btnCancel);

            AcceptButton =
                _btnApply;

            CancelButton =
                _btnCancel;

            Controls.Add(
                title);

            Controls.Add(
                subtitle);

            Controls.Add(
                quickGroup);

            Controls.Add(
                pickerCaption);

            Controls.Add(
                _saturationValuePanel);

            Controls.Add(
                _huePanel);

            Controls.Add(
                valuesGroup);

            Controls.Add(
                _btnCancel);

            Controls.Add(
                _btnApply);

            ResumeLayout(
                false);

            PerformLayout();
        }


        private void ApplyTheme()
        {
            UiStyleManager.StyleForm(
                this);
        }


        private void AddQuickColors()
        {
            foreach (Color color
                in QuickColorPalette)
            {
                Button swatch =
                    new Button
                    {
                        Size =
                            new Size(
                                34,
                                34),

                        Margin =
                            new Padding(
                                4),

                        BackColor =
                            color,

                        FlatStyle =
                            FlatStyle.Flat,

                        TabStop =
                            true,

                        AccessibleName =
                            "Quick colour " +
                            ToHex(
                                color),

                        Tag =
                            color
                    };

                swatch.FlatAppearance.BorderSize =
                    1;

                swatch.FlatAppearance.BorderColor =
                    NoteHighlightUiTheme.BorderStrong;

                swatch.Click +=
                    QuickColor_Click;

                _quickColors.Controls.Add(
                    swatch);
            }
        }


        private NumericUpDown CreateRgbInput(
            Control parent,
            string caption,
            int top)
        {
            Label label =
                CreateLabel(
                    caption,
                    new Point(
                        16,
                        top + 4),
                    false);

            label.Parent =
                parent;

            NumericUpDown input =
                new NumericUpDown
                {
                    Location =
                        new Point(
                            44,
                            top),

                    Size =
                        new Size(
                            94,
                            24),

                    Minimum =
                        0,

                    Maximum =
                        255
                };

            parent.Controls.Add(
                input);

            UiStyleManager.StyleNumericUpDown(
                input);

            return input;
        }


        private static Label CreateLabel(
            string text,
            Point location,
            bool secondary)
        {
            Label label =
                new Label
                {
                    AutoSize =
                        true,

                    Text =
                        text,

                    Location =
                        location,

                    BackColor =
                        Color.Transparent,

                    Font =
                        NoteHighlightUiTheme.CreateBodyFont()
                };

            UiStyleManager.StyleLabel(
                label,
                secondary);

            return label;
        }


        private static GroupBox CreateGroupBox(
            string text,
            Rectangle bounds)
        {
            GroupBox groupBox =
                new GroupBox
                {
                    Text =
                        text,

                    Bounds =
                        bounds,

                    BackColor =
                        NoteHighlightUiTheme.Surface,

                    ForeColor =
                        NoteHighlightUiTheme.TextPrimary
                };

            UiStyleManager.StyleGroupBox(
                groupBox);

            return groupBox;
        }


        private void QuickColor_Click(
            object sender,
            EventArgs e)
        {
            Button swatch =
                sender as Button;

            if (swatch == null ||
                !(swatch.Tag is Color))
            {
                return;
            }

            SetSelectedColor(
                (Color)swatch.Tag,
                true);
        }


        private void HexTextChanged(
            object sender,
            EventArgs e)
        {
            if (_updatingControls)
            {
                return;
            }

            Color parsedColor;

            if (!TryParseHex(
                _txtHex.Text,
                out parsedColor))
            {
                _lblValidation.Text =
                    "Enter a valid HEX value.";

                _btnApply.Enabled =
                    false;

                return;
            }

            _lblValidation.Text =
                string.Empty;

            _btnApply.Enabled =
                true;

            SetSelectedColor(
                parsedColor,
                false);
        }


        private void RgbValueChanged(
            object sender,
            EventArgs e)
        {
            if (_updatingControls)
            {
                return;
            }

            Color color =
                Color.FromArgb(
                    (int)_nudRed.Value,
                    (int)_nudGreen.Value,
                    (int)_nudBlue.Value);

            SetSelectedColor(
                color,
                true);
        }


        private void ApplyButton_Click(
            object sender,
            EventArgs e)
        {
            Color parsedColor;

            if (!TryParseHex(
                _txtHex.Text,
                out parsedColor))
            {
                DialogResult =
                    DialogResult.None;

                _lblValidation.Text =
                    "Enter a valid HEX value.";

                return;
            }

            _selectedColor =
                parsedColor;

            SaveRecentColor(
                _selectedColor);
        }


        private void SetSelectedColor(
            Color color,
            bool updateHex)
        {
            _selectedColor =
                color;

            RgbToHsv(
                color,
                out _hue,
                out _saturation,
                out _value);

            SyncControlsFromColor(
                updateHex);

            _saturationValuePanel.Refresh();
            _huePanel.Refresh();
        }


        private void SetSelectedColorFromHsv()
        {
            _selectedColor =
                HsvToRgb(
                    _hue,
                    _saturation,
                    _value);

            SyncControlsFromColor(
                true);

            _saturationValuePanel.Refresh();
            _huePanel.Refresh();
        }


        private void SyncControlsFromColor(
            bool updateHex = true)
        {
            _updatingControls =
                true;

            try
            {
                _newPreview.BackColor =
                    _selectedColor;

                _nudRed.Value =
                    _selectedColor.R;

                _nudGreen.Value =
                    _selectedColor.G;

                _nudBlue.Value =
                    _selectedColor.B;

                if (updateHex)
                {
                    _txtHex.Text =
                        ToHex(
                            _selectedColor);
                }

                _lblValidation.Text =
                    string.Empty;

                _btnApply.Enabled =
                    true;
            }
            finally
            {
                _updatingControls =
                    false;
            }
        }


        private void SaturationValuePanel_Paint(
            object sender,
            PaintEventArgs e)
        {
            Rectangle rectangle =
                _saturationValuePanel.ClientRectangle;

            if (rectangle.Width <= 1 ||
                rectangle.Height <= 1)
            {
                return;
            }

            Color hueColor =
                HsvToRgb(
                    _hue,
                    1.0,
                    1.0);

            using (SolidBrush hueBrush =
                new SolidBrush(
                    hueColor))
            {
                e.Graphics.FillRectangle(
                    hueBrush,
                    rectangle);
            }

            using (LinearGradientBrush whiteOverlay =
                new LinearGradientBrush(
                    rectangle,
                    Color.White,
                    Color.FromArgb(
                        0,
                        Color.White),
                    LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(
                    whiteOverlay,
                    rectangle);
            }

            using (LinearGradientBrush blackOverlay =
                new LinearGradientBrush(
                    rectangle,
                    Color.FromArgb(
                        0,
                        Color.Black),
                    Color.Black,
                    LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(
                    blackOverlay,
                    rectangle);
            }

            int markerX =
                (int)Math.Round(
                    _saturation *
                    (rectangle.Width - 1));

            int markerY =
                (int)Math.Round(
                    (1.0 - _value) *
                    (rectangle.Height - 1));

            using (Pen outerPen =
                new Pen(
                    Color.Black,
                    3.0f))
            using (Pen innerPen =
                new Pen(
                    Color.White,
                    2.0f))
            {
                e.Graphics.DrawEllipse(
                    outerPen,
                    markerX - 7,
                    markerY - 7,
                    14,
                    14);

                e.Graphics.DrawEllipse(
                    innerPen,
                    markerX - 6,
                    markerY - 6,
                    12,
                    12);
            }
        }


        private void HuePanel_Paint(
            object sender,
            PaintEventArgs e)
        {
            Rectangle rectangle =
                _huePanel.ClientRectangle;

            if (rectangle.Width <= 1 ||
                rectangle.Height <= 1)
            {
                return;
            }

            for (int y = 0;
                y < rectangle.Height;
                y++)
            {
                double hue =
                    rectangle.Height <= 1
                        ? 0.0
                        : 360.0 *
                          y /
                          (rectangle.Height - 1);

                using (Pen pen =
                    new Pen(
                        HsvToRgb(
                            hue,
                            1.0,
                            1.0)))
                {
                    e.Graphics.DrawLine(
                        pen,
                        0,
                        y,
                        rectangle.Width,
                        y);
                }
            }

            int markerY =
                (int)Math.Round(
                    (_hue / 360.0) *
                    (rectangle.Height - 1));

            markerY =
                Math.Max(
                    0,
                    Math.Min(
                        rectangle.Height - 1,
                        markerY));

            using (Pen whitePen =
                new Pen(
                    Color.White,
                    2.0f))
            using (Pen blackPen =
                new Pen(
                    Color.Black,
                    1.0f))
            {
                e.Graphics.DrawRectangle(
                    blackPen,
                    0,
                    markerY - 2,
                    rectangle.Width - 1,
                    4);

                e.Graphics.DrawLine(
                    whitePen,
                    1,
                    markerY,
                    rectangle.Width - 2,
                    markerY);
            }
        }


        private void SaturationValuePanel_MouseDown(
            object sender,
            MouseEventArgs e)
        {
            _saturationValuePanel.Focus();

            _draggingSaturationValue =
                true;

            UpdateSaturationValueFromMouse(
                e.Location);
        }


        private void SaturationValuePanel_MouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (_draggingSaturationValue)
            {
                UpdateSaturationValueFromMouse(
                    e.Location);
            }
        }


        private void SaturationValuePanel_MouseUp(
            object sender,
            MouseEventArgs e)
        {
            _draggingSaturationValue =
                false;
        }


        private void UpdateSaturationValueFromMouse(
            Point location)
        {
            int width =
                Math.Max(
                    1,
                    _saturationValuePanel.Width - 1);

            int height =
                Math.Max(
                    1,
                    _saturationValuePanel.Height - 1);

            int x =
                Math.Max(
                    0,
                    Math.Min(
                        width,
                        location.X));

            int y =
                Math.Max(
                    0,
                    Math.Min(
                        height,
                        location.Y));

            _saturation =
                (double)x /
                width;

            _value =
                1.0 -
                ((double)y /
                 height);

            SetSelectedColorFromHsv();
        }


        private void HuePanel_MouseDown(
            object sender,
            MouseEventArgs e)
        {
            _huePanel.Focus();

            _draggingHue =
                true;

            UpdateHueFromMouse(
                e.Location);
        }


        private void HuePanel_MouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (_draggingHue)
            {
                UpdateHueFromMouse(
                    e.Location);
            }
        }


        private void HuePanel_MouseUp(
            object sender,
            MouseEventArgs e)
        {
            _draggingHue =
                false;
        }


        private void UpdateHueFromMouse(
            Point location)
        {
            int height =
                Math.Max(
                    1,
                    _huePanel.Height - 1);

            int y =
                Math.Max(
                    0,
                    Math.Min(
                        height,
                        location.Y));

            _hue =
                360.0 *
                y /
                height;

            if (_hue >= 360.0)
            {
                _hue =
                    0.0;
            }

            SetSelectedColorFromHsv();
        }


        private Button GetFocusedSwatch()
        {
            Button swatch =
                GetFocusedSwatch(
                    _quickColors);

            if (swatch != null)
            {
                return swatch;
            }

            return
                GetFocusedSwatch(
                    _recentColors);
        }


        private static Button GetFocusedSwatch(
            FlowLayoutPanel palette)
        {
            if (palette == null)
            {
                return null;
            }

            foreach (Button swatch
                in GetKeyboardSwatches(
                    palette))
            {
                if (swatch.ContainsFocus)
                {
                    return swatch;
                }
            }

            return null;
        }


        private static IList<Button> GetKeyboardSwatches(
            FlowLayoutPanel palette)
        {
            var result =
                new List<Button>();

            if (palette == null)
            {
                return result;
            }

            foreach (Button swatch
                in palette.Controls.OfType<Button>())
            {
                if (!swatch.Visible ||
                    !swatch.Enabled)
                {
                    continue;
                }

                // Do not keyboard-navigate to swatches clipped by the
                // compact Quick-colours viewport.
                if (!palette.ClientRectangle.Contains(
                    swatch.Bounds))
                {
                    continue;
                }

                result.Add(
                    swatch);
            }

            return result;
        }


        private void MoveFocusedSwatch(
            Button focusedSwatch,
            Keys keyCode)
        {
            FlowLayoutPanel palette =
                focusedSwatch.Parent as FlowLayoutPanel;

            if (palette == null)
            {
                return;
            }

            IList<Button> swatches =
                GetKeyboardSwatches(
                    palette);

            int currentIndex =
                swatches.IndexOf(
                    focusedSwatch);

            if (currentIndex < 0)
            {
                return;
            }

            const int columns =
                4;

            int row =
                currentIndex /
                columns;

            int column =
                currentIndex %
                columns;

            int targetIndex =
                currentIndex;

            switch (keyCode)
            {
                case Keys.Left:
                    if (column > 0)
                    {
                        targetIndex =
                            currentIndex - 1;
                    }
                    break;

                case Keys.Right:
                    if (column <
                        columns - 1 &&
                        currentIndex + 1 <
                        swatches.Count)
                    {
                        targetIndex =
                            currentIndex + 1;
                    }
                    break;

                case Keys.Up:
                    if (row > 0)
                    {
                        targetIndex =
                            currentIndex -
                            columns;
                    }
                    break;

                case Keys.Down:
                    if (currentIndex +
                        columns <
                        swatches.Count)
                    {
                        targetIndex =
                            currentIndex +
                            columns;
                    }
                    break;
            }

            if (targetIndex ==
                currentIndex)
            {
                return;
            }

            swatches[targetIndex].Focus();
        }


        private void AdjustSaturationValueFromKeyboard(
            Keys keyCode,
            bool faster)
        {
            double step =
                faster
                    ? 0.05
                    : 0.01;

            switch (keyCode)
            {
                case Keys.Left:
                    _saturation -=
                        step;
                    break;

                case Keys.Right:
                    _saturation +=
                        step;
                    break;

                case Keys.Up:
                    _value +=
                        step;
                    break;

                case Keys.Down:
                    _value -=
                        step;
                    break;
            }

            _saturation =
                ClampUnit(
                    _saturation);

            _value =
                ClampUnit(
                    _value);

            SetSelectedColorFromHsv();
        }


        private void AdjustHueFromKeyboard(
            Keys keyCode,
            bool faster)
        {
            double step =
                faster
                    ? 10.0
                    : 1.0;

            if (keyCode ==
                Keys.Up)
            {
                _hue -=
                    step;
            }
            else if (keyCode ==
                Keys.Down)
            {
                _hue +=
                    step;
            }

            while (_hue < 0.0)
            {
                _hue +=
                    360.0;
            }

            while (_hue >= 360.0)
            {
                _hue -=
                    360.0;
            }

            SetSelectedColorFromHsv();
        }


        private static double ClampUnit(
            double value)
        {
            return Math.Max(
                0.0,
                Math.Min(
                    1.0,
                    value));
        }


        private void MovePickerKeyboardFocus(
            bool forward)
        {
            Control[] route =
                GetPickerKeyboardRoute();

            int currentIndex =
                FindCurrentPickerKeyboardIndex(
                    route);

            int step =
                forward
                    ? 1
                    : -1;

            int candidateIndex =
                currentIndex;

            for (int attempts = 0;
                attempts < route.Length;
                attempts++)
            {
                if (candidateIndex < 0)
                {
                    candidateIndex =
                        forward
                            ? 0
                            : route.Length - 1;
                }
                else
                {
                    candidateIndex =
                        (candidateIndex +
                            step +
                            route.Length) %
                        route.Length;
                }

                Control candidate =
                    route[candidateIndex];

                if (!CanUsePickerKeyboardFocus(
                    candidate))
                {
                    continue;
                }

                candidate.Focus();

                return;
            }
        }


        private Control[] GetPickerKeyboardRoute()
        {
            return new Control[]
            {
                GetFirstKeyboardSwatch(
                    _quickColors),

                GetFirstKeyboardSwatch(
                    _recentColors),

                _saturationValuePanel,
                _huePanel,
                _txtHex,
                _nudRed,
                _nudGreen,
                _nudBlue,
                _btnCancel,
                _btnApply
            };
        }


        private int FindCurrentPickerKeyboardIndex(
            Control[] route)
        {
            if (route == null)
            {
                return -1;
            }

            Button focusedSwatch =
                GetFocusedSwatch();

            if (focusedSwatch != null)
            {
                if (focusedSwatch.Parent ==
                    _quickColors)
                {
                    return 0;
                }

                if (focusedSwatch.Parent ==
                    _recentColors)
                {
                    return 1;
                }
            }

            for (int index = 2;
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


        private static Button GetFirstKeyboardSwatch(
            FlowLayoutPanel palette)
        {
            return
                GetKeyboardSwatches(
                    palette)
                .FirstOrDefault();
        }


        private static bool CanUsePickerKeyboardFocus(
            Control control)
        {
            return
                control != null &&
                !control.IsDisposed &&
                control.Visible &&
                control.Enabled &&
                control.CanSelect;
        }


        private static bool IsSwatchInPalette(
            Control control,
            FlowLayoutPanel palette)
        {
            return
                control is Button &&
                palette != null &&
                ReferenceEquals(
                    control.Parent,
                    palette);
        }


        private void LoadRecentColors()
        {
            if (_recentColors == null)
            {
                return;
            }

            _recentColors.Controls.Clear();

            foreach (Color color
                in ReadRecentColors())
            {
                AddRecentColorSwatch(
                    color);
            }
        }


        private void AddRecentColorSwatch(
            Color color)
        {
            Button swatch =
                new Button
                {
                    Size =
                        new Size(
                            34,
                            28),

                    Margin =
                        new Padding(
                            4,
                            3,
                            4,
                            3),

                    BackColor =
                        color,

                    FlatStyle =
                        FlatStyle.Flat,

                    TabStop =
                        true,

                    AccessibleName =
                        "Recent colour " +
                        ToHex(
                            color),

                    Tag =
                        color
                };

            swatch.FlatAppearance.BorderSize =
                1;

            swatch.FlatAppearance.BorderColor =
                NoteHighlightUiTheme.BorderStrong;

            swatch.Click +=
                QuickColor_Click;

            _recentColors.Controls.Add(
                swatch);
        }


        private static IList<Color> ReadRecentColors()
        {
            var colors =
                new List<Color>();

            try
            {
                if (!File.Exists(
                    RecentColorsFilePath))
                {
                    return colors;
                }

                foreach (string line
                    in File.ReadAllLines(
                        RecentColorsFilePath))
                {
                    Color color;

                    if (TryParseHex(
                        line,
                        out color) &&
                        !colors.Contains(
                            color))
                    {
                        colors.Add(
                            color);
                    }

                    if (colors.Count >=
                        MaxRecentColors)
                    {
                        break;
                    }
                }
            }
            catch
            {
                // Recent colours are a convenience feature only.
            }

            return colors;
        }


        private static void SaveRecentColor(
            Color color)
        {
            try
            {
                List<Color> colors =
                    ReadRecentColors()
                        .ToList();

                colors.RemoveAll(
                    item =>
                        item.ToArgb() ==
                        color.ToArgb());

                colors.Insert(
                    0,
                    color);

                if (colors.Count >
                    MaxRecentColors)
                {
                    colors =
                        colors
                            .Take(
                                MaxRecentColors)
                            .ToList();
                }

                string directory =
                    Path.GetDirectoryName(
                        RecentColorsFilePath);

                if (!string.IsNullOrWhiteSpace(
                    directory))
                {
                    Directory.CreateDirectory(
                        directory);
                }

                File.WriteAllLines(
                    RecentColorsFilePath,
                    colors.Select(
                        ToHex));
            }
            catch
            {
                // Failure to remember recent colours should never block Apply.
            }
        }


        private static bool TryParseHex(
            string value,
            out Color color)
        {
            color =
                Color.Black;

            if (string.IsNullOrWhiteSpace(
                value))
            {
                return false;
            }

            string normalized =
                value.Trim();

            if (normalized.StartsWith(
                "#",
                StringComparison.Ordinal))
            {
                normalized =
                    normalized.Substring(1);
            }

            if (normalized.Length == 3)
            {
                normalized =
                    string.Concat(
                        normalized[0],
                        normalized[0],
                        normalized[1],
                        normalized[1],
                        normalized[2],
                        normalized[2]);
            }

            if (normalized.Length != 6)
            {
                return false;
            }

            int red;
            int green;
            int blue;

            if (!int.TryParse(
                    normalized.Substring(
                        0,
                        2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out red) ||
                !int.TryParse(
                    normalized.Substring(
                        2,
                        2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out green) ||
                !int.TryParse(
                    normalized.Substring(
                        4,
                        2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out blue))
            {
                return false;
            }

            color =
                Color.FromArgb(
                    red,
                    green,
                    blue);

            return true;
        }


        private static string ToHex(
            Color color)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "#{0:X2}{1:X2}{2:X2}",
                color.R,
                color.G,
                color.B);
        }


        private static void RgbToHsv(
            Color color,
            out double hue,
            out double saturation,
            out double value)
        {
            double red =
                color.R / 255.0;

            double green =
                color.G / 255.0;

            double blue =
                color.B / 255.0;

            double max =
                Math.Max(
                    red,
                    Math.Max(
                        green,
                        blue));

            double min =
                Math.Min(
                    red,
                    Math.Min(
                        green,
                        blue));

            double delta =
                max - min;

            value =
                max;

            saturation =
                max <= 0.0
                    ? 0.0
                    : delta / max;

            if (delta <= 0.000001)
            {
                hue =
                    0.0;

                return;
            }

            if (Math.Abs(
                max - red) < 0.000001)
            {
                hue =
                    60.0 *
                    (((green - blue) / delta) % 6.0);
            }
            else if (Math.Abs(
                max - green) < 0.000001)
            {
                hue =
                    60.0 *
                    (((blue - red) / delta) + 2.0);
            }
            else
            {
                hue =
                    60.0 *
                    (((red - green) / delta) + 4.0);
            }

            if (hue < 0.0)
            {
                hue +=
                    360.0;
            }
        }


        private static Color HsvToRgb(
            double hue,
            double saturation,
            double value)
        {
            hue =
                ((hue % 360.0) + 360.0) %
                360.0;

            saturation =
                Math.Max(
                    0.0,
                    Math.Min(
                        1.0,
                        saturation));

            value =
                Math.Max(
                    0.0,
                    Math.Min(
                        1.0,
                        value));

            double chroma =
                value * saturation;

            double x =
                chroma *
                (1.0 -
                 Math.Abs(
                    ((hue / 60.0) % 2.0) -
                    1.0));

            double m =
                value - chroma;

            double red;
            double green;
            double blue;

            if (hue < 60.0)
            {
                red = chroma;
                green = x;
                blue = 0.0;
            }
            else if (hue < 120.0)
            {
                red = x;
                green = chroma;
                blue = 0.0;
            }
            else if (hue < 180.0)
            {
                red = 0.0;
                green = chroma;
                blue = x;
            }
            else if (hue < 240.0)
            {
                red = 0.0;
                green = x;
                blue = chroma;
            }
            else if (hue < 300.0)
            {
                red = x;
                green = 0.0;
                blue = chroma;
            }
            else
            {
                red = chroma;
                green = 0.0;
                blue = x;
            }

            return Color.FromArgb(
                ClampByte(
                    (red + m) * 255.0),
                ClampByte(
                    (green + m) * 255.0),
                ClampByte(
                    (blue + m) * 255.0));
        }


        private static int ClampByte(
            double value)
        {
            return Math.Max(
                0,
                Math.Min(
                    255,
                    (int)Math.Round(
                        value)));
        }

        /// <summary>
        /// Prevents the HSV surfaces from showing partial repaint frames while
        /// the mouse is dragged quickly across the picker.
        /// </summary>
        private sealed class BufferedPickerPanel : Panel
        {
            public BufferedPickerPanel()
            {
                DoubleBuffered =
                    true;

                ResizeRedraw =
                    true;

                TabStop =
                    true;

                SetStyle(
                    ControlStyles.Selectable,
                    true);

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer,
                    true);

                UpdateStyles();
            }
        }

    }
}
