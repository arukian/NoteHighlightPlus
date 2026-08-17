using System;
using System.Drawing;
using System.Windows.Forms;

namespace NoteHighlightAddin
{
    internal sealed class KeyboardShortcutsForm : Form
    {
        public KeyboardShortcutsForm()
        {
            InitializeHelp();
        }


        public static void ShowHelp(
            IWin32Window owner)
        {
            using (var form =
                new KeyboardShortcutsForm())
            {
                form.ShowDialog(
                    owner);
            }
        }


        protected override bool ProcessCmdKey(
            ref Message msg,
            Keys keyData)
        {
            Keys keyCode =
                keyData &
                Keys.KeyCode;

            if (keyCode == Keys.Escape ||
                keyCode == Keys.Enter)
            {
                Close();

                return true;
            }

            return base.ProcessCmdKey(
                ref msg,
                keyData);
        }


        private void InitializeHelp()
        {
            Text =
                "Keyboard Help";

            StartPosition =
                FormStartPosition.CenterParent;

            ClientSize =
                new Size(
                    720,
                    620);

            MinimumSize =
                new Size(
                    650,
                    560);

            FormBorderStyle =
                FormBorderStyle.Sizable;

            MaximizeBox =
                false;

            KeyPreview =
                true;

            UiStyleManager.StyleForm(
                this);

            Label title =
                new Label
                {
                    Text =
                        "Keyboard shortcuts",

                    AutoSize =
                        true,

                    Location =
                        new Point(
                            28,
                            24)
                };

            UiStyleManager.StyleLabel(
                title,
                false);

            title.Font =
                NoteHighlightUiTheme.CreateSectionFont();

            Controls.Add(
                title);

            Label subtitle =
                new Label
                {
                    Text =
                        "Press Enter or Esc at any time to close this window.",

                    AutoSize =
                        true,

                    Location =
                        new Point(
                            29,
                            56)
                };

            UiStyleManager.StyleLabel(
                subtitle,
                true);

            Controls.Add(
                subtitle);

            Panel scroll =
                new Panel
                {
                    Location =
                        new Point(
                            24,
                            92),

                    Size =
                        new Size(
                            ClientSize.Width - 48,
                            ClientSize.Height - 154),

                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Bottom |
                        AnchorStyles.Left |
                        AnchorStyles.Right,

                    AutoScroll =
                        true,

                    BackColor =
                        NoteHighlightUiTheme.WindowBackground
                };

            Controls.Add(
                scroll);

            int y =
                4;

            AddSection(
                scroll,
                ref y,
                "Global",
                "F1     Open this help window\n" +
                "Tab    Next control\n" +
                "Shift + Tab    Previous control\n" +
                "Space / Enter  Activate the focused button or toggle");

            AddSection(
                scroll,
                ref y,
                "MainForm",
                "Theme: ↑ / ↓ change theme\n" +
                "Code Editor: Tab indents, Shift + Tab unindents\n" +
                "Code Editor: F6 leaves the editor\n" +
                "Shift + F6 moves backward\n" +
                "Insert Code: Space / Enter inserts into OneNote");

            AddSection(
                scroll,
                ref y,
                "SettingsForm",
                "Tab / Shift + Tab follow the current settings page\n" +
                "Language & Groups / Theme Editor: ← / → switch tabs\n" +
                "Lists and dropdowns: ↑ / ↓ change selection\n" +
                "Space toggles checkboxes and activates buttons");

            AddSection(
                scroll,
                ref y,
                "Color Picker",
                "Quick / Recent: ← → ↑ ↓ navigate colours\n" +
                "Space / Enter selects a colour\n" +
                "Colour field: ← / → saturation, ↑ / ↓ brightness\n" +
                "Shift + Arrow makes a larger change\n" +
                "Hue: ↑ / ↓ changes hue; Shift changes faster\n" +
                "Enter applies the colour\n" +
                "Esc cancels");

            Button close =
                new Button
                {
                    Text =
                        "Close",

                    Size =
                        new Size(
                            110,
                            36),

                    Location =
                        new Point(
                            ClientSize.Width - 134,
                            ClientSize.Height - 50),

                    Anchor =
                        AnchorStyles.Right |
                        AnchorStyles.Bottom,

                    TabStop =
                        false
                };

            UiStyleManager.StylePrimaryButton(
                close);

            close.Click +=
                delegate
                {
                    Close();
                };

            Controls.Add(
                close);
        }


        private static void AddSection(
            Control parent,
            ref int y,
            string heading,
            string commands)
        {
            Label headingLabel =
                new Label
                {
                    Text =
                        heading,

                    AutoSize =
                        true,

                    Location =
                        new Point(
                            4,
                            y)
                };

            UiStyleManager.StyleLabel(
                headingLabel,
                false);

            headingLabel.Font =
                NoteHighlightUiTheme.CreateSectionFont();

            parent.Controls.Add(
                headingLabel);

            y +=
                headingLabel.Height + 8;

            Label commandsLabel =
                new Label
                {
                    Text =
                        commands,

                    AutoSize =
                        true,

                    MaximumSize =
                        new Size(
                            Math.Max(
                                420,
                                parent.ClientSize.Width - 32),
                            0),

                    Location =
                        new Point(
                            12,
                            y)
                };

            UiStyleManager.StyleLabel(
                commandsLabel,
                true);

            commandsLabel.Font =
                NoteHighlightUiTheme.CreateBodyFont();

            parent.Controls.Add(
                commandsLabel);

            y +=
                commandsLabel.Height + 24;
        }
    }
}
