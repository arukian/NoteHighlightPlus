using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.KeywordGroups.ViewModels;
using System;
using System.Windows.Forms;


namespace NoteHighlightAddin
{
    /// <summary>
    /// Coordinates keyword-group operations that require both the
    /// LanguageEditorViewModel and WinForms user interaction.
    /// </summary>
    internal sealed class KeywordGroupEditorController
    {
        private readonly IWin32Window _owner;
        private readonly LanguageEditorViewModel _languageEditor;
        private readonly KeywordGroupSelectionController _selectionController;
        private readonly NumericUpDown _groupIdEditor;
        private readonly Action _updateWindowTitle;
        private readonly Action _focusGroupNameEditor;

        private bool _isChangingGroupId;


        public KeywordGroupEditorController(
            IWin32Window owner,
            LanguageEditorViewModel languageEditor,
            KeywordGroupSelectionController selectionController,
            NumericUpDown groupIdEditor,
            Action updateWindowTitle,
            Action focusGroupNameEditor)
        {
            _owner =
                owner
                ?? throw new ArgumentNullException(
                    nameof(owner));

            _languageEditor =
                languageEditor
                ?? throw new ArgumentNullException(
                    nameof(languageEditor));

            _selectionController =
                selectionController
                ?? throw new ArgumentNullException(
                    nameof(selectionController));

            _groupIdEditor =
                groupIdEditor
                ?? throw new ArgumentNullException(
                    nameof(groupIdEditor));

            _updateWindowTitle =
                updateWindowTitle
                ?? throw new ArgumentNullException(
                    nameof(updateWindowTitle));

            _focusGroupNameEditor =
                focusGroupNameEditor
                ?? throw new ArgumentNullException(
                    nameof(focusGroupNameEditor));
        }


        public void AddGroup()
        {
            KeywordGroupConfiguration newGroup =
                _languageEditor.AddGroup();

            if (newGroup == null)
            {
                MessageBox.Show(
                    _owner,
                    "Load a language configuration before adding a group.",
                    "Keyword Group",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            _selectionController.RefreshGroups(
                newGroup.Id);

            _updateWindowTitle();
            _focusGroupNameEditor();
        }


        public void RemoveSelectedGroup()
        {
            KeywordGroupConfiguration selectedGroup =
                _languageEditor.SelectedGroup;

            if (selectedGroup == null)
            {
                return;
            }

            string groupName =
                string.IsNullOrWhiteSpace(
                    selectedGroup.DisplayName)
                    ? "Group " + selectedGroup.Id
                    : selectedGroup.DisplayName;

            int wordCount =
                selectedGroup.Words?.Count ?? 0;

            int regexCount =
                selectedGroup.Regex?.Count ?? 0;

            DialogResult result =
                MessageBox.Show(
                    _owner,
                    "Remove the selected keyword group?"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Group: "
                    + groupName
                    + Environment.NewLine
                    + "ID: "
                    + selectedGroup.Id
                    + Environment.NewLine
                    + "Words: "
                    + wordCount
                    + Environment.NewLine
                    + "Regex: "
                    + regexCount
                    + Environment.NewLine
                    + Environment.NewLine
                    + "This action cannot be undone.",
                    "Remove Group",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            KeywordGroupConfiguration nextGroup =
                _languageEditor.RemoveSelectedGroup();

            _selectionController.RefreshGroups(
                nextGroup?.Id);

            _updateWindowTitle();
            _selectionController.UpdateState();
        }


        public void MoveSelectedGroupUp()
        {
            if (!_languageEditor.MoveSelectedGroupUp())
            {
                return;
            }

            RefreshMovedGroup();
        }


        public void MoveSelectedGroupDown()
        {
            if (!_languageEditor.MoveSelectedGroupDown())
            {
                return;
            }

            RefreshMovedGroup();
        }


        public void ChangeSelectedGroupId()
        {
            if (_isChangingGroupId)
            {
                return;
            }

            KeywordGroupConfiguration group =
                _languageEditor.SelectedGroup;

            if (group == null)
            {
                return;
            }

            int previousId =
                group.Id;

            int newId =
                Decimal.ToInt32(
                    _groupIdEditor.Value);

            string errorMessage;

            if (_languageEditor.TryChangeSelectedGroupId(
                newId,
                out errorMessage))
            {
                _selectionController.RefreshSelectedListItem();
                _selectionController.UpdateState();
                _updateWindowTitle();

                return;
            }

            RestoreGroupId(
                previousId);

            MessageBox.Show(
                _owner,
                errorMessage,
                "Group ID",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }


        public void EditRegex()
        {
            if (_languageEditor.SelectedGroup == null)
            {
                return;
            }

            using (RegexEditorDialog regexEditor =
                new RegexEditorDialog(
                    _languageEditor))
            {
                regexEditor.ShowDialog(
                    _owner);
            }

            _updateWindowTitle();
            _selectionController.UpdateState();
        }


        private void RefreshMovedGroup()
        {
            KeywordGroupConfiguration selectedGroup =
                _languageEditor.SelectedGroup;

            _selectionController.RefreshGroups(
                selectedGroup?.Id);

            _updateWindowTitle();
        }


        private void RestoreGroupId(
            int groupId)
        {
            decimal value =
                groupId;

            if (value < _groupIdEditor.Minimum)
            {
                value =
                    _groupIdEditor.Minimum;
            }

            if (value > _groupIdEditor.Maximum)
            {
                value =
                    _groupIdEditor.Maximum;
            }

            _isChangingGroupId =
                true;

            try
            {
                _groupIdEditor.Value =
                    value;
            }
            finally
            {
                _isChangingGroupId =
                    false;
            }
        }
    }
}
