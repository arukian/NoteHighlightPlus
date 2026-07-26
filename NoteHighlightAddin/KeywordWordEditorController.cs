using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.KeywordGroups.ViewModels;
using System;
using System.Windows.Forms;


namespace NoteHighlightAddin
{
    /// <summary>
    /// Coordinates adding, moving and removing words from keyword groups.
    /// </summary>
    internal sealed class KeywordWordEditorController
    {
        private readonly IWin32Window _owner;
        private readonly LanguageEditorViewModel _languageEditor;
        private readonly TextBox _wordInput;
        private readonly ListBox _wordList;
        private readonly Button _addButton;
        private readonly Button _removeButton;
        private readonly Action _refreshSelection;
        private readonly Action _updateWindowTitle;


        public KeywordWordEditorController(
            IWin32Window owner,
            LanguageEditorViewModel languageEditor,
            TextBox wordInput,
            ListBox wordList,
            Button addButton,
            Button removeButton,
            Action refreshSelection,
            Action updateWindowTitle)
        {
            _owner =
                owner
                ?? throw new ArgumentNullException(
                    nameof(owner));

            _languageEditor =
                languageEditor
                ?? throw new ArgumentNullException(
                    nameof(languageEditor));

            _wordInput =
                wordInput
                ?? throw new ArgumentNullException(
                    nameof(wordInput));

            _wordList =
                wordList
                ?? throw new ArgumentNullException(
                    nameof(wordList));

            _addButton =
                addButton
                ?? throw new ArgumentNullException(
                    nameof(addButton));

            _removeButton =
                removeButton
                ?? throw new ArgumentNullException(
                    nameof(removeButton));

            _refreshSelection =
                refreshSelection
                ?? throw new ArgumentNullException(
                    nameof(refreshSelection));

            _updateWindowTitle =
                updateWindowTitle
                ?? throw new ArgumentNullException(
                    nameof(updateWindowTitle));
        }


        public void AddWord()
        {
            if (!_languageEditor.HasSelectedGroup)
            {
                MessageBox.Show(
                    _owner,
                    "Select a keyword group first.",
                    "Keyword Group",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string word =
                _wordInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(
                word))
            {
                return;
            }

            WordLocationResult location =
                _languageEditor.FindWord(
                    word);

            if (location.Exists)
            {
                HandleExistingWord(
                    word,
                    location);

                return;
            }

            if (!_languageEditor.AddWord(
                word))
            {
                return;
            }

            CompleteWordChange(
                word);
        }


        public void RemoveSelectedWord()
        {
            string selectedWord =
                _wordList.SelectedItem
                as string;

            if (string.IsNullOrWhiteSpace(
                selectedWord))
            {
                return;
            }

            DialogResult result =
                MessageBox.Show(
                    _owner,
                    "Remove the selected word?"
                    + Environment.NewLine
                    + Environment.NewLine
                    + selectedWord,
                    "Remove Word",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            if (!_languageEditor.RemoveWord(
                selectedWord))
            {
                return;
            }

            _refreshSelection();
            _updateWindowTitle();
        }


        public void UpdateState()
        {
            bool hasGroup =
                _languageEditor.HasSelectedGroup;

            _wordInput.Enabled =
                hasGroup;

            _addButton.Enabled =
                hasGroup;

            _removeButton.Enabled =
                hasGroup
                && _wordList.SelectedIndex >= 0;
        }


        public void HandleWordInputKeyDown(
            KeyEventArgs e)
        {
            if (e == null
                || e.KeyCode != Keys.Enter)
            {
                return;
            }

            AddWord();

            e.SuppressKeyPress =
                true;

            e.Handled =
                true;
        }


        private void HandleExistingWord(
            string word,
            WordLocationResult location)
        {
            if (ReferenceEquals(
                location.Group,
                _languageEditor.SelectedGroup))
            {
                MessageBox.Show(
                    _owner,
                    "The word already exists in the selected group.",
                    "Keyword Group",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                SelectWordInList(
                    word);

                return;
            }

            string sourceGroupName =
                GetGroupName(
                    location.Group);

            string destinationGroupName =
                GetGroupName(
                    _languageEditor.SelectedGroup);

            DialogResult result =
                MessageBox.Show(
                    _owner,
                    "The word already exists in another group."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Word: "
                    + word
                    + Environment.NewLine
                    + "Current group: "
                    + sourceGroupName
                    + Environment.NewLine
                    + "Destination group: "
                    + destinationGroupName
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Do you want to move it?",
                    "Move Word",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            bool moved =
                _languageEditor.MoveWordToSelectedGroup(
                    word,
                    location.Group);

            if (!moved)
            {
                MessageBox.Show(
                    _owner,
                    "The word could not be moved.",
                    "Move Word",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            CompleteWordChange(
                word);
        }


        private void CompleteWordChange(
            string word)
        {
            _wordInput.Clear();

            _refreshSelection();

            SelectWordInList(
                word);

            _updateWindowTitle();
        }


        private void SelectWordInList(
            string word)
        {
            for (int index = 0;
                index < _wordList.Items.Count;
                index++)
            {
                string currentWord =
                    _wordList.Items[index]
                    as string;

                if (!string.Equals(
                    currentWord,
                    word,
                    StringComparison.Ordinal))
                {
                    continue;
                }

                _wordList.SelectedIndex =
                    index;

                return;
            }
        }


        private static string GetGroupName(
            KeywordGroupConfiguration group)
        {
            if (group == null)
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(
                group.DisplayName)
                    ? "Group " + group.Id
                    : group.DisplayName;
        }
    }
}
