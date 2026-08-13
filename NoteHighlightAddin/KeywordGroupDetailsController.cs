using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.KeywordGroups.ViewModels;
using System;
using System.Windows.Forms;


namespace NoteHighlightAddin
{
    /// <summary>
    /// Synchronizes the selected keyword group with the controls that
    /// belong to the language definition itself.
    ///
    /// Visual formatting such as colour, bold and italic is handled
    /// separately by the theme editor.
    /// </summary>
    internal sealed class KeywordGroupDetailsController
    {
        private readonly LanguageEditorViewModel _languageEditor;
        private readonly TextBox _nameEditor;
        private readonly TextBox _descriptionEditor;
        private readonly NumericUpDown _groupIdEditor;
        private readonly Action _refreshSelectedListItem;

        private bool _isLoading;


        public bool IsLoading =>
            _isLoading;


        public KeywordGroupDetailsController(
            LanguageEditorViewModel languageEditor,
            TextBox nameEditor,
            TextBox descriptionEditor,
            NumericUpDown groupIdEditor,
            Action refreshSelectedListItem)
        {
            _languageEditor =
                languageEditor
                ?? throw new ArgumentNullException(
                    nameof(languageEditor));

            _nameEditor =
                nameEditor
                ?? throw new ArgumentNullException(
                    nameof(nameEditor));

            _descriptionEditor =
                descriptionEditor
                ?? throw new ArgumentNullException(
                    nameof(descriptionEditor));

            _groupIdEditor =
                groupIdEditor
                ?? throw new ArgumentNullException(
                    nameof(groupIdEditor));

            _refreshSelectedListItem =
                refreshSelectedListItem
                ?? throw new ArgumentNullException(
                    nameof(refreshSelectedListItem));
        }


        public void Refresh()
        {
            _isLoading =
                true;

            try
            {
                KeywordGroupConfiguration group =
                    _languageEditor.SelectedGroup;

                bool hasGroup =
                    group != null;

                SetEditorsEnabled(
                    hasGroup);

                if (!hasGroup)
                {
                    ClearEditors();

                    return;
                }

                SetNumericValue(
                    _groupIdEditor,
                    group.Id);

                _nameEditor.Text =
                    group.DisplayName
                    ?? string.Empty;

                _descriptionEditor.Text =
                    group.Description
                    ?? string.Empty;
            }
            finally
            {
                _isLoading =
                    false;
            }
        }


        public void ApplyChanges(
            bool refreshSelectedListItem = true)
        {
            if (_isLoading)
            {
                return;
            }

            KeywordGroupConfiguration group =
                _languageEditor.SelectedGroup;

            if (group == null)
            {
                return;
            }

            group.DisplayName =
                _nameEditor.Text.Trim();

            group.Description =
                GetOptionalText(
                    _descriptionEditor.Text);

            _languageEditor.MarkAsModified();

            if (refreshSelectedListItem)
            {
                _refreshSelectedListItem();
            }
        }


        private void SetEditorsEnabled(
            bool enabled)
        {
            _nameEditor.Enabled =
                enabled;

            _descriptionEditor.Enabled =
                enabled;

            _groupIdEditor.Enabled =
                enabled;
        }


        private void ClearEditors()
        {
            _nameEditor.Clear();
            _descriptionEditor.Clear();

            _groupIdEditor.Value =
                _groupIdEditor.Minimum;
        }


        private static string GetOptionalText(
            string value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                    ? null
                    : value.Trim();
        }


        private static void SetNumericValue(
            NumericUpDown editor,
            decimal value)
        {
            editor.Value =
                GetClampedValue(
                    editor,
                    value);
        }


        private static decimal GetClampedValue(
            NumericUpDown editor,
            decimal value)
        {
            if (value < editor.Minimum)
            {
                return editor.Minimum;
            }

            if (value > editor.Maximum)
            {
                return editor.Maximum;
            }

            return value;
        }
    }
}