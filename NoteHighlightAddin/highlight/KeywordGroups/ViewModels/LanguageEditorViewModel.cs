using System;
using System.Collections.Generic;
using System.Linq;
using NoteHighlightAddin.Highlighting.KeywordGroups.Services;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.ViewModels
{
    public sealed class LanguageEditorViewModel
    {
        private readonly ILanguageEditorService languageEditorService;

        public LanguageEditorViewModel(
            ILanguageEditorService languageEditorService)
        {
            this.languageEditorService =
                languageEditorService ??
                throw new ArgumentNullException(
                    nameof(languageEditorService));
        }

        public EditableLanguageConfiguration Configuration
        {
            get;
            private set;
        }

        public KeywordGroupConfiguration SelectedGroup
        {
            get;
            private set;
        }

        public bool HasConfiguration =>
            Configuration != null;

        public bool HasSelectedGroup =>
            SelectedGroup != null;

        public string Language =>
            Configuration?.Language;

        public string Description =>
            Configuration?.Description;

        public IReadOnlyList<KeywordGroupConfiguration>
            GetOrderedGroups()
        {
            if (Configuration?.Groups == null)
            {
                return new List<KeywordGroupConfiguration>();
            }

            return Configuration.Groups
                .Where(group => group != null)
                .OrderBy(group => group.Priority)
                .ThenBy(group => group.Id)
                .ToList();
        }

        public IReadOnlyList<string>
            GetSelectedGroupWords()
        {
            if (SelectedGroup?.Words == null)
            {
                return new List<string>();
            }

            return SelectedGroup.Words
                .Where(word =>
                    !string.IsNullOrWhiteSpace(word))
                .OrderBy(word => word)
                .ToList();
        }

        public IReadOnlyList<string>
            GetSelectedGroupRegex()
        {
            if (SelectedGroup?.Regex == null)
            {
                return new List<string>();
            }

            return SelectedGroup.Regex
                .Where(regex =>
                    !string.IsNullOrWhiteSpace(regex))
                .ToList();
        }

        public void Load(string language)
        {
            Configuration = languageEditorService.Load(
                    language);

            SelectedGroup =
                null;

            HasUnsavedChanges =
                false;
        }

        public void LoadFromFile(string filePath)
        {
            Configuration =
                languageEditorService.LoadFromFile(
                    filePath);

            SelectedGroup =
                null;

            HasUnsavedChanges =
                false;
        }

        public void SelectGroup(
            int groupId)
        {
            SelectedGroup =
                Configuration?.Groups?
                    .FirstOrDefault(
                        group => group.Id == groupId);
        }

        public void Clear()
        {
            Configuration =
                null;

            SelectedGroup =
                null;

            HasUnsavedChanges =
                false;
        }

        public void MarkAsModified()
        {
            if (Configuration == null)
            {
                return;
            }

            HasUnsavedChanges =
                true;
        }

        public void Save()
        {
            if (Configuration == null)
            {
                throw new InvalidOperationException(
                    "There is no language configuration to save.");
            }

            languageEditorService.Save(
                Configuration);

            HasUnsavedChanges = false;
        }

        public void SaveAs(
            string filePath)
        {
            if (Configuration == null)
            {
                throw new InvalidOperationException(
                    "There is no language configuration to save.");
            }

            languageEditorService.SaveAs(
                Configuration,
                filePath);

            HasUnsavedChanges = false;
        }

        public bool HasUnsavedChanges
        {
            get;
            private set;
        }

        public KeywordGroupConfiguration AddGroup()
        {
            if (Configuration == null)
            {
                return null;
            }

            if (Configuration.Groups == null)
            {
                Configuration.Groups =
                    new List<KeywordGroupConfiguration>();
            }

            int newGroupId = 1;

            while (Configuration.Groups.Any(
                group =>
                    group != null &&
                    group.Id == newGroupId))
            {
                newGroupId++;
            }

            int newPriority =
                Configuration.Groups
                    .Where(group => group != null)
                    .Select(group => group.Priority)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

            var newGroup =
                new KeywordGroupConfiguration
                {
                    Id = newGroupId,
                    DisplayName =
                        "Group " + newGroupId,
                    Description = null,
                    Priority = newPriority,
                    Colour = null,
                    Bold = false,
                    Italic = false,
                    Visible = true,
                    IsCustom = true
                };

            Configuration.Groups.Add(
                newGroup);

            SelectedGroup =
                newGroup;

            MarkAsModified();

            return newGroup;
        }

        public KeywordGroupConfiguration RemoveSelectedGroup()
        {
            if (Configuration?.Groups == null ||
                SelectedGroup == null)
            {
                return null;
            }

            List<KeywordGroupConfiguration> orderedGroups =
                GetOrderedGroups().ToList();

            int selectedIndex =
                orderedGroups.IndexOf(
                    SelectedGroup);

            if (selectedIndex < 0)
            {
                return null;
            }

            KeywordGroupConfiguration groupToRemove =
                SelectedGroup;

            Configuration.Groups.Remove(
                groupToRemove);

            KeywordGroupConfiguration nextGroup =
                null;

            if (Configuration.Groups.Count > 0)
            {
                List<KeywordGroupConfiguration> remainingGroups =
                    GetOrderedGroups().ToList();

                int nextIndex =
                    Math.Min(
                        selectedIndex,
                        remainingGroups.Count - 1);

                nextGroup =
                    remainingGroups[nextIndex];
            }

            SelectedGroup =
                nextGroup;

            MarkAsModified();

            return nextGroup;
        }

        public bool AddWord(
    string word)
        {
            if (SelectedGroup == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            string normalizedWord =
                word.Trim();

            if (SelectedGroup.Words == null)
            {
                SelectedGroup.Words =
                    new List<string>();
            }

            bool alreadyExists =
                SelectedGroup.Words.Any(
                    existingWord =>
                        string.Equals(
                            existingWord,
                            normalizedWord,
                            StringComparison.Ordinal));

            if (alreadyExists)
            {
                return false;
            }

            SelectedGroup.Words.Add(
                normalizedWord);

            MarkAsModified();

            return true;
        }

        public bool RemoveWord(
    string word)
        {
            if (SelectedGroup?.Words == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            string existingWord =
                SelectedGroup.Words.FirstOrDefault(
                    currentWord =>
                        string.Equals(
                            currentWord,
                            word,
                            StringComparison.Ordinal));

            if (existingWord == null)
            {
                return false;
            }

            SelectedGroup.Words.Remove(
                existingWord);

            MarkAsModified();

            return true;
        }

        public WordLocationResult FindWord(
    string word)
        {
            var result =
                new WordLocationResult();

            if (Configuration?.Groups == null ||
                string.IsNullOrWhiteSpace(word))
            {
                return result;
            }

            string normalizedWord =
                word.Trim();

            foreach (KeywordGroupConfiguration group
                in Configuration.Groups)
            {
                if (group?.Words == null)
                {
                    continue;
                }

                bool exists =
                    group.Words.Any(
                        currentWord =>
                            string.Equals(
                                currentWord,
                                normalizedWord,
                                StringComparison.Ordinal));

                if (!exists)
                {
                    continue;
                }

                result.Exists =
                    true;

                result.Group =
                    group;

                return result;
            }

            return result;
        }

        public bool MoveWordToSelectedGroup(
    string word,
    KeywordGroupConfiguration sourceGroup)
        {
            if (SelectedGroup == null ||
                sourceGroup == null ||
                string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            if (ReferenceEquals(
                sourceGroup,
                SelectedGroup))
            {
                return false;
            }

            string normalizedWord =
                word.Trim();

            if (sourceGroup.Words == null)
            {
                return false;
            }

            string existingWord =
                sourceGroup.Words.FirstOrDefault(
                    currentWord =>
                        string.Equals(
                            currentWord,
                            normalizedWord,
                            StringComparison.Ordinal));

            if (existingWord == null)
            {
                return false;
            }

            if (SelectedGroup.Words == null)
            {
                SelectedGroup.Words =
                    new List<string>();
            }

            sourceGroup.Words.Remove(
                existingWord);

            SelectedGroup.Words.Add(
                normalizedWord);

            MarkAsModified();

            return true;
        }
    }
}