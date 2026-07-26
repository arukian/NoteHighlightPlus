using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.KeywordGroups.ViewModels;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace NoteHighlightAddin
{
    internal sealed class RegexEditorDialog : Form
    {
        private readonly LanguageEditorViewModel _languageEditor;

        private readonly ListBox _regexList;
        private readonly TextBox _regexText;
        private readonly Button _addButton;
        private readonly Button _removeButton;
        private readonly Button _closeButton;


        public RegexEditorDialog(
            LanguageEditorViewModel languageEditor)
        {
            _languageEditor =
                languageEditor
                ?? throw new ArgumentNullException(
                    nameof(languageEditor));

            KeywordGroupConfiguration selectedGroup =
                _languageEditor.SelectedGroup
                ?? throw new InvalidOperationException(
                    "A keyword group must be selected.");

            Text =
                "Regex Editor - "
                + GetGroupDisplayName(
                    selectedGroup);

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.Sizable;

            MinimizeBox = false;
            MaximizeBox = true;
            ShowInTaskbar = false;

            Width = 720;
            Height = 470;

            MinimumSize =
                new Size(
                    560,
                    360);

            Label description =
                new Label
                {
                    AutoSize = true,
                    Left = 12,
                    Top = 12,
                    Text =
                        "Regular expressions in the selected keyword group:"
                };

            _regexList =
                new ListBox
                {
                    Left = 12,
                    Top = 35,
                    Width =
                        ClientSize.Width - 24,
                    Height =
                        ClientSize.Height - 145,
                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Bottom |
                        AnchorStyles.Left |
                        AnchorStyles.Right,
                    HorizontalScrollbar = true
                };

            _regexText =
                new TextBox
                {
                    Left = 12,
                    Top = _regexList.Bottom + 8,
                    Width =
                        ClientSize.Width - 184,
                    Anchor =
                        AnchorStyles.Bottom |
                        AnchorStyles.Left |
                        AnchorStyles.Right
                };

            _addButton =
                new Button
                {
                    Text = "Add Regex",
                    Width = 78,
                    Height =
                        _regexText.Height + 2,
                    Left =
                        _regexText.Right + 6,
                    Top =
                        _regexText.Top - 1,
                    Anchor =
                        AnchorStyles.Bottom |
                        AnchorStyles.Right
                };

            _removeButton =
                new Button
                {
                    Text = "Remove",
                    Width = 78,
                    Height =
                        _regexText.Height + 2,
                    Left =
                        _addButton.Right + 6,
                    Top =
                        _regexText.Top - 1,
                    Anchor =
                        AnchorStyles.Bottom |
                        AnchorStyles.Right,
                    Enabled = false
                };

            Label note =
                new Label
                {
                    AutoSize = true,
                    Left = 12,
                    Top =
                        _regexText.Bottom + 10,
                    Anchor =
                        AnchorStyles.Bottom |
                        AnchorStyles.Left,
                    Text =
                        "Expressions are stored exactly as written for Highlight."
                };

            _closeButton =
                new Button
                {
                    Text = "Close",
                    DialogResult =
                        DialogResult.OK,
                    Width = 86,
                    Height = 27,
                    Left =
                        ClientSize.Width - 98,
                    Top =
                        ClientSize.Height - 39,
                    Anchor =
                        AnchorStyles.Bottom |
                        AnchorStyles.Right
                };

            Controls.Add(
                description);

            Controls.Add(
                _regexList);

            Controls.Add(
                _regexText);

            Controls.Add(
                _addButton);

            Controls.Add(
                _removeButton);

            Controls.Add(
                note);

            Controls.Add(
                _closeButton);

            _addButton.Click +=
                AddButton_Click;

            _removeButton.Click +=
                RemoveButton_Click;

            _regexText.KeyDown +=
                RegexText_KeyDown;

            _regexList.SelectedIndexChanged +=
                RegexList_SelectedIndexChanged;

            AcceptButton =
                _addButton;

            CancelButton =
                _closeButton;

            RefreshRegexList();
        }


        private static string GetGroupDisplayName(
            KeywordGroupConfiguration group)
        {
            if (!string.IsNullOrWhiteSpace(
                group.DisplayName))
            {
                return group.DisplayName;
            }

            return "Group " + group.Id;
        }


        private void AddButton_Click(
            object sender,
            EventArgs e)
        {
            AddCurrentRegex();
        }


        private void RegexText_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            AddCurrentRegex();

            e.SuppressKeyPress = true;
            e.Handled = true;
        }


        private void RegexList_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            UpdateRemoveButtonState();
        }


        private void RemoveButton_Click(
            object sender,
            EventArgs e)
        {
            string selectedRegex =
                _regexList.SelectedItem
                as string;

            if (string.IsNullOrWhiteSpace(
                selectedRegex))
            {
                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    this,
                    "Remove the selected regular expression?"
                    + Environment.NewLine
                    + Environment.NewLine
                    + selectedRegex,
                    "Remove Regex",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (confirmation !=
                DialogResult.Yes)
            {
                return;
            }

            if (!_languageEditor.RemoveRegex(
                selectedRegex))
            {
                return;
            }

            RefreshRegexList();
        }


        private void AddCurrentRegex()
        {
            string regex =
                _regexText.Text.Trim();

            if (string.IsNullOrWhiteSpace(
                regex))
            {
                return;
            }

            bool added =
                _languageEditor.AddRegex(
                    regex);

            if (!added)
            {
                MessageBox.Show(
                    this,
                    "The expression already exists in this group.",
                    "Regex Editor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            _regexText.Clear();

            RefreshRegexList();
            SelectRegex(regex);

            _regexText.Focus();
        }


        private void RefreshRegexList()
        {
            string selectedRegex =
                _regexList.SelectedItem
                as string;

            _regexList.BeginUpdate();

            try
            {
                _regexList.Items.Clear();

                foreach (string regex
                    in _languageEditor.GetSelectedGroupRegex())
                {
                    _regexList.Items.Add(
                        regex);
                }
            }
            finally
            {
                _regexList.EndUpdate();
            }

            SelectRegex(
                selectedRegex);

            UpdateRemoveButtonState();
        }


        private void SelectRegex(
            string regex)
        {
            if (string.IsNullOrEmpty(
                regex))
            {
                return;
            }

            int index =
                _regexList.Items.IndexOf(
                    regex);

            if (index >= 0)
            {
                _regexList.SelectedIndex =
                    index;
            }
        }


        private void UpdateRemoveButtonState()
        {
            _removeButton.Enabled =
                _regexList.SelectedIndex >= 0;
        }
    }
}