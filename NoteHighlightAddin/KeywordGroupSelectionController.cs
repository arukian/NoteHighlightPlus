using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.KeywordGroups.ViewModels;
using System;
using System.Windows.Forms;


namespace NoteHighlightAddin
{
    /// <summary>
    /// Synchronizes the keyword-group list, selected group, word list and
    /// enabled state of the group-management controls.
    /// </summary>
    internal sealed class KeywordGroupSelectionController
    {
        private readonly LanguageEditorViewModel _languageEditor;

        private readonly ListBox _groupList;
        private readonly ListBox _wordList;

        private readonly Button _addGroupButton;
        private readonly Button _removeGroupButton;
        private readonly Button _moveUpButton;
        private readonly Button _moveDownButton;
        private readonly Button _regexEditorButton;

        private readonly Action _refreshGroupDetails;
        private readonly Action _updateWordEditorState;


        public KeywordGroupSelectionController(
            LanguageEditorViewModel languageEditor,
            ListBox groupList,
            ListBox wordList,
            Button addGroupButton,
            Button removeGroupButton,
            Button moveUpButton,
            Button moveDownButton,
            Button regexEditorButton,
            Action refreshGroupDetails,
            Action updateWordEditorState)
        {
            _languageEditor =
                languageEditor
                ?? throw new ArgumentNullException(
                    nameof(languageEditor));

            _groupList =
                groupList
                ?? throw new ArgumentNullException(
                    nameof(groupList));

            _wordList =
                wordList
                ?? throw new ArgumentNullException(
                    nameof(wordList));

            _addGroupButton =
                addGroupButton
                ?? throw new ArgumentNullException(
                    nameof(addGroupButton));

            _removeGroupButton =
                removeGroupButton
                ?? throw new ArgumentNullException(
                    nameof(removeGroupButton));

            _moveUpButton =
                moveUpButton
                ?? throw new ArgumentNullException(
                    nameof(moveUpButton));

            _moveDownButton =
                moveDownButton
                ?? throw new ArgumentNullException(
                    nameof(moveDownButton));

            _regexEditorButton =
                regexEditorButton
                ?? throw new ArgumentNullException(
                    nameof(regexEditorButton));

            _refreshGroupDetails =
                refreshGroupDetails
                ?? throw new ArgumentNullException(
                    nameof(refreshGroupDetails));

            _updateWordEditorState =
                updateWordEditorState
                ?? throw new ArgumentNullException(
                    nameof(updateWordEditorState));
        }


        public void RefreshGroups(
            int? groupIdToSelect = null)
        {
            _groupList.BeginUpdate();

            try
            {
                _groupList.Items.Clear();
                _wordList.Items.Clear();

                _languageEditor.SelectGroup(
                    -1);

                _refreshGroupDetails();

                if (!_languageEditor.HasConfiguration)
                {
                    return;
                }

                foreach (KeywordGroupConfiguration group
                    in _languageEditor.GetOrderedGroups())
                {
                    _groupList.Items.Add(
                        new KeywordGroupListItem(
                            group));
                }

                if (_groupList.Items.Count == 0)
                {
                    return;
                }

                _groupList.SelectedIndex =
                    FindGroupIndex(
                        groupIdToSelect);
            }
            finally
            {
                _groupList.EndUpdate();
                UpdateState();
            }
        }


        public void RefreshSelection()
        {
            _wordList.Items.Clear();

            KeywordGroupListItem selectedItem =
                _groupList.SelectedItem
                as KeywordGroupListItem;

            if (selectedItem == null)
            {
                _languageEditor.SelectGroup(
                    -1);

                _refreshGroupDetails();
                _updateWordEditorState();
                UpdateState();

                return;
            }

            _languageEditor.SelectGroup(
                selectedItem.Group.Id);

            foreach (string word
                in _languageEditor.GetSelectedGroupWords())
            {
                _wordList.Items.Add(
                    word);
            }

            _refreshGroupDetails();
            _updateWordEditorState();
            UpdateState();
        }


        public void RefreshSelectedListItem()
        {
            int selectedIndex =
                _groupList.SelectedIndex;

            if (selectedIndex < 0)
            {
                return;
            }

            KeywordGroupListItem selectedItem =
                _groupList.SelectedItem
                as KeywordGroupListItem;

            if (selectedItem == null)
            {
                return;
            }

            _groupList.Items[selectedIndex] =
                new KeywordGroupListItem(
                    selectedItem.Group);

            _groupList.SelectedIndex =
                selectedIndex;
        }


        public void UpdateState()
        {
            _addGroupButton.Enabled =
                _languageEditor.HasConfiguration;

            _removeGroupButton.Enabled =
                _languageEditor.HasSelectedGroup;

            _moveUpButton.Enabled =
                _languageEditor.CanMoveSelectedGroupUp();

            _moveDownButton.Enabled =
                _languageEditor.CanMoveSelectedGroupDown();

            _regexEditorButton.Enabled =
                _languageEditor.HasSelectedGroup;
        }


        private int FindGroupIndex(
            int? groupIdToSelect)
        {
            if (!groupIdToSelect.HasValue)
            {
                return 0;
            }

            for (int index = 0;
                index < _groupList.Items.Count;
                index++)
            {
                KeywordGroupListItem item =
                    _groupList.Items[index]
                    as KeywordGroupListItem;

                if (item?.Group.Id ==
                    groupIdToSelect.Value)
                {
                    return index;
                }
            }

            return 0;
        }
    }
}
