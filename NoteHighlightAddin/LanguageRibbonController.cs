using Infrastructure.Core;
using NoteHighlightAddin.Highlighting.KeywordGroups.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace NoteHighlightAddin
{
    /// <summary>
    /// Coordinates the languages displayed in the Ribbon and loads the
    /// editable .lang configuration selected in SettingsForm.
    /// </summary>
    internal sealed class LanguageRibbonController
    {
        private readonly IWin32Window _owner;
        private readonly LanguageEditorViewModel _languageEditor;
        private readonly KeywordGroupSelectionController _groupSelectionController;
        private readonly ListBox _visibleLanguages;
        private readonly ComboBox _availableLanguages;
        private readonly LanguageManager _languageManager;


        public LanguageRibbonController(
            IWin32Window owner,
            LanguageEditorViewModel languageEditor,
            KeywordGroupSelectionController groupSelectionController,
            ListBox visibleLanguages,
            ComboBox availableLanguages)
        {
            _owner =
                owner
                ?? throw new ArgumentNullException(
                    nameof(owner));

            _languageEditor =
                languageEditor
                ?? throw new ArgumentNullException(
                    nameof(languageEditor));

            _groupSelectionController =
                groupSelectionController
                ?? throw new ArgumentNullException(
                    nameof(groupSelectionController));

            _visibleLanguages =
                visibleLanguages
                ?? throw new ArgumentNullException(
                    nameof(visibleLanguages));

            _availableLanguages =
                availableLanguages
                ?? throw new ArgumentNullException(
                    nameof(availableLanguages));

            try
            {
                _languageManager =
                    new LanguageManager(
                        PathManager.Ribbon,
                        PathManager.LanguagesFolder);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    _owner,
                    "Error initializing language manager:"
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Language Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        public void RefreshLanguageList()
        {
            if (_languageManager == null)
            {
                return;
            }

            try
            {
                LanguageInfo selectedLanguage =
                    _visibleLanguages.SelectedItem
                    as LanguageInfo;

                _visibleLanguages.Items.Clear();

                List<LanguageInfo> visibleLanguages =
                    _languageManager
                        .GetVisibleLanguages()
                        .ToList();

                foreach (LanguageInfo language in visibleLanguages)
                {
                    _visibleLanguages.Items.Add(
                        language);
                }

                _availableLanguages.Items.Clear();

                List<LanguageInfo> availableLanguages =
                    _languageManager
                        .GetAvailableLanguages()
                        .ToList();

                HashSet<string> visibleTags =
                    new HashSet<string>(
                        visibleLanguages.Select(
                            language => language.Tag),
                        StringComparer.OrdinalIgnoreCase);

                foreach (LanguageInfo language in availableLanguages)
                {
                    if (!visibleTags.Contains(
                        language.Tag))
                    {
                        _availableLanguages.Items.Add(
                            language);
                    }
                }

                RestoreLanguageSelection(
                    selectedLanguage);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    _owner,
                    "Error refreshing language list:"
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Language Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        public void LoadSelectedLanguageConfiguration()
        {
            LanguageInfo selectedLanguage =
                _visibleLanguages.SelectedItem
                as LanguageInfo;

            if (selectedLanguage == null)
            {
                _languageEditor.Clear();
                _groupSelectionController.RefreshGroups();

                return;
            }

            try
            {
                string languageName =
                    GetLanguageDefinitionName(
                        selectedLanguage);

                _languageEditor.Load(
                    languageName);

                _groupSelectionController.RefreshGroups();
            }
            catch (Exception exception)
            {
                _languageEditor.Clear();
                _groupSelectionController.RefreshGroups();

                MessageBox.Show(
                    _owner,
                    "The selected language configuration could not be loaded."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Label: "
                    + selectedLanguage.Label
                    + Environment.NewLine
                    + "Tag: "
                    + selectedLanguage.Tag
                    + Environment.NewLine
                    + "Languages folder: "
                    + PathManager.LanguagesFolder
                    + Environment.NewLine
                    + Environment.NewLine
                    + exception.Message,
                    "Language Configuration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        public void RemoveSelectedLanguage()
        {
            if (_languageManager == null)
            {
                return;
            }

            LanguageInfo selectedLanguage =
                _visibleLanguages.SelectedItem
                as LanguageInfo;

            if (selectedLanguage == null)
            {
                MessageBox.Show(
                    _owner,
                    "Please select a language to remove.",
                    "Remove Language",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    _owner,
                    "Remove '"
                    + selectedLanguage.Label
                    + "' from the ribbon?",
                    "Confirm Removal",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            if (!_languageManager.RemoveLanguage(
                selectedLanguage.Tag))
            {
                MessageBox.Show(
                    _owner,
                    "Failed to remove language.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                _owner,
                "Language removed. Please restart OneNote to see changes.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            RefreshLanguageList();
        }


        public void AddSelectedLanguage()
        {
            if (_languageManager == null)
            {
                return;
            }

            LanguageInfo selectedLanguage =
                _availableLanguages.SelectedItem
                as LanguageInfo;

            if (selectedLanguage == null)
            {
                MessageBox.Show(
                    _owner,
                    "Please select a language to add.",
                    "Add Language",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (!_languageManager.AddLanguage(
                selectedLanguage.Tag,
                selectedLanguage.Label))
            {
                MessageBox.Show(
                    _owner,
                    "Failed to add language.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                _owner,
                "Language added to ribbon. Please restart OneNote to see changes.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            RefreshLanguageList();
        }


        private void RestoreLanguageSelection(
            LanguageInfo previousSelection)
        {
            if (previousSelection == null)
            {
                return;
            }

            for (int index = 0;
                index < _visibleLanguages.Items.Count;
                index++)
            {
                LanguageInfo language =
                    _visibleLanguages.Items[index]
                    as LanguageInfo;

                if (language == null
                    || !string.Equals(
                        language.Tag,
                        previousSelection.Tag,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                _visibleLanguages.SelectedIndex =
                    index;

                return;
            }
        }


        private static string GetLanguageDefinitionName(
            LanguageInfo language)
        {
            if (language == null)
            {
                throw new ArgumentNullException(
                    nameof(language));
            }

            string tag =
                language.Tag?.Trim();

            if (!string.IsNullOrWhiteSpace(
                tag))
            {
                string tagFilePath =
                    Path.Combine(
                        PathManager.LanguagesFolder,
                        Path.GetFileNameWithoutExtension(
                            tag)
                        + ".lang");

                if (File.Exists(
                    tagFilePath))
                {
                    return tag;
                }
            }

            string label =
                language.Label?.Trim();

            if (!string.IsNullOrWhiteSpace(
                label))
            {
                string normalizedLabel =
                    label.ToLowerInvariant();

                string labelFilePath =
                    Path.Combine(
                        PathManager.LanguagesFolder,
                        normalizedLabel
                        + ".lang");

                if (File.Exists(
                    labelFilePath))
                {
                    return normalizedLabel;
                }
            }

            throw new FileNotFoundException(
                "No matching .lang file was found for the selected language.");
        }
    }
}