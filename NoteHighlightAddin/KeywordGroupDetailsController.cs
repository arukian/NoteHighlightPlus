using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.KeywordGroups.ViewModels;
using System;
using System.Windows.Forms;


namespace NoteHighlightAddin
{
    /// <summary>
    /// Synchronizes the selected keyword group with its detail controls.
    /// </summary>
    internal sealed class KeywordGroupDetailsController
    {
        private readonly LanguageEditorViewModel _languageEditor;
        private readonly TextBox _nameEditor;
        private readonly TextBox _descriptionEditor;
        private readonly NumericUpDown _priorityEditor;
        private readonly CheckBox _visibleEditor;
        private readonly CheckBox _boldEditor;
        private readonly CheckBox _italicEditor;
        private readonly TextBox _colourEditor;
        private readonly NumericUpDown _groupIdEditor;
        private readonly Action _refreshSelectedListItem;
        private readonly Action _updateWindowTitle;

        private bool _isLoading;


        public bool IsLoading =>
            _isLoading;


        public KeywordGroupDetailsController(
            LanguageEditorViewModel languageEditor,
            TextBox nameEditor,
            TextBox descriptionEditor,
            NumericUpDown priorityEditor,
            CheckBox visibleEditor,
            CheckBox boldEditor,
            CheckBox italicEditor,
            TextBox colourEditor,
            NumericUpDown groupIdEditor,
            Action refreshSelectedListItem,
            Action updateWindowTitle)
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

            _priorityEditor =
                priorityEditor
                ?? throw new ArgumentNullException(
                    nameof(priorityEditor));

            _visibleEditor =
                visibleEditor
                ?? throw new ArgumentNullException(
                    nameof(visibleEditor));

            _boldEditor =
                boldEditor
                ?? throw new ArgumentNullException(
                    nameof(boldEditor));

            _italicEditor =
                italicEditor
                ?? throw new ArgumentNullException(
                    nameof(italicEditor));

            _colourEditor =
                colourEditor
                ?? throw new ArgumentNullException(
                    nameof(colourEditor));

            _groupIdEditor =
                groupIdEditor
                ?? throw new ArgumentNullException(
                    nameof(groupIdEditor));

            _refreshSelectedListItem =
                refreshSelectedListItem
                ?? throw new ArgumentNullException(
                    nameof(refreshSelectedListItem));

            _updateWindowTitle =
                updateWindowTitle
                ?? throw new ArgumentNullException(
                    nameof(updateWindowTitle));
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

                SetNumericValue(
                    _priorityEditor,
                    group.Priority);

                _visibleEditor.Checked =
                    group.Visible;

                _boldEditor.Checked =
                    group.Bold;

                _italicEditor.Checked =
                    group.Italic;

                _colourEditor.Text =
                    group.Colour
                    ?? string.Empty;
            }
            finally
            {
                _isLoading =
                    false;
            }
        }


        public void ApplyChanges()
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

            group.Visible =
                _visibleEditor.Checked;

            group.Bold =
                _boldEditor.Checked;

            group.Italic =
                _italicEditor.Checked;

            group.Colour =
                GetOptionalText(
                    _colourEditor.Text);

            _languageEditor.MarkAsModified();

            _refreshSelectedListItem();
            _updateWindowTitle();
        }


        private void SetEditorsEnabled(
            bool enabled)
        {
            _nameEditor.Enabled =
                enabled;

            _descriptionEditor.Enabled =
                enabled;

            // Priority is normalized automatically through group ordering.
            _priorityEditor.Enabled =
                false;

            _visibleEditor.Enabled =
                enabled;

            _boldEditor.Enabled =
                enabled;

            _italicEditor.Enabled =
                enabled;

            _colourEditor.Enabled =
                enabled;

            _groupIdEditor.Enabled =
                enabled;
        }


        private void ClearEditors()
        {
            _nameEditor.Clear();
            _descriptionEditor.Clear();

            _priorityEditor.Value =
                GetClampedValue(
                    _priorityEditor,
                    0);

            _visibleEditor.Checked =
                false;

            _boldEditor.Checked =
                false;

            _italicEditor.Checked =
                false;

            _colourEditor.Clear();

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