using System;
using System.Drawing;
using System.Windows.Forms;

namespace NoteHighlightAddin
{
    /// <summary>
    /// Centralized WinForms styling helpers used by the modern NoteHighlight+
    /// interface. Keeping styling here prevents MainForm and SettingsForm from
    /// gradually developing different visual rules.
    /// </summary>
    internal static class UiStyleManager
    {
        public static void StyleForm(Form form)
        {
            if (form == null)
            {
                throw new ArgumentNullException(nameof(form));
            }

            form.BackColor =
                NoteHighlightUiTheme.WindowBackground;

            form.ForeColor =
                NoteHighlightUiTheme.TextPrimary;

            form.Font =
                NoteHighlightUiTheme.CreateBodyFont();
        }

        public static void StylePanel(
            Panel panel,
            bool raised)
        {
            if (panel == null)
            {
                throw new ArgumentNullException(nameof(panel));
            }

            panel.BackColor =
                raised
                    ? NoteHighlightUiTheme.SurfaceRaised
                    : NoteHighlightUiTheme.Surface;
        }

        public static void StylePrimaryButton(Button button)
        {
            StyleButton(
                button,
                NoteHighlightUiTheme.Accent,
                NoteHighlightUiTheme.TextPrimary,
                NoteHighlightUiTheme.Accent,
                NoteHighlightUiTheme.AccentHover,
                NoteHighlightUiTheme.AccentPressed);
        }

        public static void StyleSecondaryButton(Button button)
        {
            StyleButton(
                button,
                NoteHighlightUiTheme.SurfaceRaised,
                NoteHighlightUiTheme.TextPrimary,
                NoteHighlightUiTheme.BorderStrong,
                NoteHighlightUiTheme.SurfaceHover,
                NoteHighlightUiTheme.AccentPressed);
        }

        public static void StyleDangerButton(Button button)
        {
            StyleButton(
                button,
                NoteHighlightUiTheme.SurfaceRaised,
                NoteHighlightUiTheme.Danger,
                NoteHighlightUiTheme.BorderStrong,
                NoteHighlightUiTheme.SurfaceHover,
                NoteHighlightUiTheme.AccentPressed);
        }

        public static void StyleTextBox(TextBox textBox)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException(nameof(textBox));
            }

            textBox.BackColor =
                NoteHighlightUiTheme.SurfaceRaised;

            textBox.ForeColor =
                NoteHighlightUiTheme.TextPrimary;

            textBox.BorderStyle =
                BorderStyle.FixedSingle;
        }

        public static void StyleComboBox(ComboBox comboBox)
        {
            if (comboBox == null)
            {
                throw new ArgumentNullException(nameof(comboBox));
            }

            comboBox.BackColor =
                NoteHighlightUiTheme.SurfaceRaised;

            comboBox.ForeColor =
                NoteHighlightUiTheme.TextPrimary;

            comboBox.FlatStyle =
                FlatStyle.Flat;
        }

        public static void StyleListBox(ListBox listBox)
        {
            if (listBox == null)
            {
                throw new ArgumentNullException(nameof(listBox));
            }

            listBox.BackColor =
                NoteHighlightUiTheme.SurfaceRaised;

            listBox.ForeColor =
                NoteHighlightUiTheme.TextPrimary;

            listBox.BorderStyle =
                BorderStyle.FixedSingle;
        }

        public static void StyleNumericUpDown(
            NumericUpDown numericUpDown)
        {
            if (numericUpDown == null)
            {
                throw new ArgumentNullException(nameof(numericUpDown));
            }

            numericUpDown.BackColor =
                NoteHighlightUiTheme.SurfaceRaised;

            numericUpDown.ForeColor =
                NoteHighlightUiTheme.TextPrimary;

            numericUpDown.BorderStyle =
                BorderStyle.FixedSingle;
        }

        public static void StyleCheckBox(CheckBox checkBox)
        {
            if (checkBox == null)
            {
                throw new ArgumentNullException(nameof(checkBox));
            }

            checkBox.BackColor =
                Color.Transparent;

            checkBox.ForeColor =
                NoteHighlightUiTheme.TextPrimary;

            checkBox.FlatStyle =
                FlatStyle.Flat;

            checkBox.FlatAppearance.BorderColor =
                NoteHighlightUiTheme.BorderStrong;
        }

        public static void StyleToggleCheckBox(
            CheckBox checkBox,
            FontStyle fontStyle)
        {
            if (checkBox == null)
            {
                throw new ArgumentNullException(nameof(checkBox));
            }

            checkBox.Appearance =
                Appearance.Button;

            checkBox.AutoSize =
                false;

            checkBox.TextAlign =
                ContentAlignment.MiddleCenter;

            checkBox.FlatStyle =
                FlatStyle.Flat;

            checkBox.Font =
                new Font(
                    NoteHighlightUiTheme.FontFamily,
                    NoteHighlightUiTheme.BodyFontSize,
                    fontStyle,
                    GraphicsUnit.Point);

            checkBox.CheckedChanged -=
                ToggleCheckBox_StateChanged;

            checkBox.CheckedChanged +=
                ToggleCheckBox_StateChanged;

            checkBox.EnabledChanged -=
                ToggleCheckBox_StateChanged;

            checkBox.EnabledChanged +=
                ToggleCheckBox_StateChanged;

            ApplyToggleCheckBoxState(
                checkBox);
        }

        private static void ToggleCheckBox_StateChanged(
            object sender,
            EventArgs e)
        {
            CheckBox checkBox =
                sender as CheckBox;

            if (checkBox != null)
            {
                ApplyToggleCheckBoxState(
                    checkBox);
            }
        }

        private static void ApplyToggleCheckBoxState(
            CheckBox checkBox)
        {
            if (!checkBox.Enabled)
            {
                checkBox.BackColor =
                    NoteHighlightUiTheme.DisabledBackground;

                checkBox.ForeColor =
                    NoteHighlightUiTheme.DisabledText;

                checkBox.FlatAppearance.BorderColor =
                    NoteHighlightUiTheme.Border;

                return;
            }

            if (checkBox.Checked)
            {
                checkBox.BackColor =
                    NoteHighlightUiTheme.Accent;

                checkBox.ForeColor =
                    NoteHighlightUiTheme.TextPrimary;

                checkBox.FlatAppearance.BorderColor =
                    NoteHighlightUiTheme.AccentHover;

                checkBox.FlatAppearance.MouseOverBackColor =
                    NoteHighlightUiTheme.AccentHover;

                checkBox.FlatAppearance.MouseDownBackColor =
                    NoteHighlightUiTheme.AccentPressed;
            }
            else
            {
                checkBox.BackColor =
                    NoteHighlightUiTheme.SurfaceRaised;

                checkBox.ForeColor =
                    NoteHighlightUiTheme.TextPrimary;

                checkBox.FlatAppearance.BorderColor =
                    NoteHighlightUiTheme.BorderStrong;

                checkBox.FlatAppearance.MouseOverBackColor =
                    NoteHighlightUiTheme.SurfaceHover;

                checkBox.FlatAppearance.MouseDownBackColor =
                    NoteHighlightUiTheme.AccentPressed;
            }
        }

        public static void StyleLabel(
            Label label,
            bool secondary)
        {
            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            label.BackColor =
                Color.Transparent;

            label.ForeColor =
                secondary
                    ? NoteHighlightUiTheme.TextSecondary
                    : NoteHighlightUiTheme.TextPrimary;
        }

        public static void StyleSectionLabel(Label label)
        {
            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            StyleLabel(
                label,
                false);

            label.Font =
                NoteHighlightUiTheme.CreateSectionFont();
        }

        public static void StyleGroupBox(GroupBox groupBox)
        {
            if (groupBox == null)
            {
                throw new ArgumentNullException(nameof(groupBox));
            }

            groupBox.BackColor =
                NoteHighlightUiTheme.Surface;

            groupBox.ForeColor =
                NoteHighlightUiTheme.TextPrimary;

            groupBox.FlatStyle =
                FlatStyle.Flat;
        }

        public static void StyleTabControl(TabControl tabControl)
        {
            if (tabControl == null)
            {
                throw new ArgumentNullException(nameof(tabControl));
            }

            tabControl.Font =
                NoteHighlightUiTheme.CreateBodyFont();

            tabControl.DrawMode =
                TabDrawMode.OwnerDrawFixed;

            tabControl.SizeMode =
                TabSizeMode.Fixed;

            tabControl.ItemSize =
                new Size(
                    150,
                    30);

            tabControl.Appearance =
                TabAppearance.FlatButtons;

            tabControl.Padding =
                new Point(
                    0,
                    0);

            tabControl.DrawItem -=
                TabControl_DrawItem;

            tabControl.DrawItem +=
                TabControl_DrawItem;
        }

        private static void TabControl_DrawItem(
            object sender,
            DrawItemEventArgs e)
        {
            TabControl tabControl =
                sender as TabControl;

            if (tabControl == null ||
                e.Index < 0 ||
                e.Index >= tabControl.TabPages.Count)
            {
                return;
            }

            bool selected =
                e.Index == tabControl.SelectedIndex;

            Rectangle bounds =
                e.Bounds;

            Color background =
                selected
                    ? NoteHighlightUiTheme.SurfaceRaised
                    : NoteHighlightUiTheme.WindowBackground;

            Color foreground =
                selected
                    ? NoteHighlightUiTheme.TextPrimary
                    : NoteHighlightUiTheme.TextSecondary;

            using (SolidBrush backgroundBrush =
                new SolidBrush(background))
            {
                e.Graphics.FillRectangle(
                    backgroundBrush,
                    bounds);
            }

            if (selected)
            {
                using (SolidBrush accentBrush =
                    new SolidBrush(NoteHighlightUiTheme.Accent))
                {
                    e.Graphics.FillRectangle(
                        accentBrush,
                        new Rectangle(
                            bounds.Left,
                            bounds.Bottom - 3,
                            bounds.Width,
                            3));
                }
            }

            TextRenderer.DrawText(
                e.Graphics,
                tabControl.TabPages[e.Index].Text,
                NoteHighlightUiTheme.CreateBodyFont(),
                bounds,
                foreground,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
        }

        public static void StyleTabPage(TabPage tabPage)
        {
            if (tabPage == null)
            {
                throw new ArgumentNullException(nameof(tabPage));
            }

            tabPage.BackColor =
                NoteHighlightUiTheme.WindowBackground;

            tabPage.ForeColor =
                NoteHighlightUiTheme.TextPrimary;
        }

        private static void StyleButton(
            Button button,
            Color background,
            Color foreground,
            Color border,
            Color hover,
            Color pressed)
        {
            if (button == null)
            {
                throw new ArgumentNullException(nameof(button));
            }

            button.BackColor =
                background;

            button.ForeColor =
                foreground;

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                1;

            button.FlatAppearance.BorderColor =
                border;

            button.FlatAppearance.MouseOverBackColor =
                hover;

            button.FlatAppearance.MouseDownBackColor =
                pressed;

            button.UseVisualStyleBackColor =
                false;
        }
    }
}
