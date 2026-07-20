using System;
using System.Collections.Generic;
using System.Linq;

namespace NoteHighlightAddin.Highlighting.KeywordGroups
{
    public class KeywordGroupConfigurationService
    {
        public KeywordGroupConfiguration FindById(
            EditableLanguageConfiguration configuration,
            int id)
        {
            ValidateConfiguration(
                configuration);

            return configuration.Groups
                .FirstOrDefault(
                    group => group.Id == id);
        }

        public KeywordGroupConfiguration FindByName(
            EditableLanguageConfiguration configuration,
            string name)
        {
            ValidateConfiguration(
                configuration);

            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return configuration.Groups.FirstOrDefault(
                group => string.Equals(
                    group.DisplayName,
                    name.Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        public KeywordGroupConfiguration CreateCustomGroup(
            EditableLanguageConfiguration configuration,
            string name,
            string description = null)
        {
            ValidateConfiguration(
                configuration);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "The group name cannot be empty.",
                    nameof(name));
            }

            string normalizedName =
                name.Trim();

            if (FindByName(
                configuration,
                normalizedName) != null)
            {
                throw new InvalidOperationException(
                    $"A keyword group named " +
                    $"'{normalizedName}' already exists.");
            }

            var group =
                new KeywordGroupConfiguration
                {
                    Id =
                        GetNextAvailableId(
                            configuration),

                    DisplayName =
                        normalizedName,

                    Description =
                        description,

                    Priority =
                        GetNextPriority(
                            configuration),

                    Visible = true,
                    IsCustom = true
                };

            configuration.Groups.Add(
                group);

            return group;
        }

        public void AddWord(
            EditableLanguageConfiguration configuration,
            int targetGroupId,
            string word)
        {
            ValidateConfiguration(
                configuration);

            ValidateWord(
                word);

            string normalizedWord =
                word.Trim();

            KeywordGroupConfiguration targetGroup =
                GetListGroup(
                    configuration,
                    targetGroupId);

            RemoveWordFromAllGroups(
                configuration,
                normalizedWord);

            targetGroup.Words.Add(
                normalizedWord);
        }

        public bool RemoveWord(
            EditableLanguageConfiguration configuration,
            int groupId,
            string word)
        {
            ValidateConfiguration(
                configuration);

            ValidateWord(
                word);

            KeywordGroupConfiguration group =
                GetListGroup(
                    configuration,
                    groupId);

            string normalizedWord =
                word.Trim();

            string existingWord =
                group.Words.FirstOrDefault(
                    item => string.Equals(
                        item,
                        normalizedWord,
                        StringComparison.Ordinal));

            if (existingWord == null)
            {
                return false;
            }

            return group.Words.Remove(
                existingWord);
        }

        public void MoveWord(
            EditableLanguageConfiguration configuration,
            int sourceGroupId,
            int targetGroupId,
            string word)
        {
            ValidateConfiguration(
                configuration);

            ValidateWord(
                word);

            if (sourceGroupId == targetGroupId)
            {
                return;
            }

            KeywordGroupConfiguration sourceGroup =
                GetListGroup(
                    configuration,
                    sourceGroupId);

            KeywordGroupConfiguration targetGroup =
                GetListGroup(
                    configuration,
                    targetGroupId);

            string normalizedWord =
                word.Trim();

            string existingWord =
                sourceGroup.Words.FirstOrDefault(
                    item => string.Equals(
                        item,
                        normalizedWord,
                        StringComparison.Ordinal));

            if (existingWord == null)
            {
                throw new InvalidOperationException(
                    $"The word '{normalizedWord}' does not exist " +
                    $"in keyword group {sourceGroupId}.");
            }

            RemoveWordFromAllGroups(
                configuration,
                existingWord);

            targetGroup.Words.Add(
                existingWord);
        }

        public int GetNextAvailableId(
            EditableLanguageConfiguration configuration)
        {
            ValidateConfiguration(
                configuration);

            var usedIds =
                new HashSet<int>(
                    configuration.Groups.Select(
                        group => group.Id));

            int nextId = 1;

            while (usedIds.Contains(nextId))
            {
                nextId++;
            }

            return nextId;
        }

        public int GetNextPriority(
            EditableLanguageConfiguration configuration)
        {
            ValidateConfiguration(
                configuration);

            if (configuration.Groups.Count == 0)
            {
                return 0;
            }

            return configuration.Groups.Max(
                group => group.Priority) + 1;
        }

        private KeywordGroupConfiguration GetListGroup(
            EditableLanguageConfiguration configuration,
            int groupId)
        {
            KeywordGroupConfiguration group =
                FindById(
                    configuration,
                    groupId);

            if (group == null)
            {
                throw new InvalidOperationException(
                    $"Keyword group {groupId} does not exist.");
            }

            if (group.Regex != null &&
                group.Regex.Any(
                    regex =>
                        !string.IsNullOrWhiteSpace(regex)))
            {
                throw new InvalidOperationException(
                    $"Keyword group {groupId} is based on one or more " +
                    "regular expressions and cannot contain " +
                    "individual words.");
            }

            if (group.Words == null)
            {
                group.Words =
                    new List<string>();
            }

            return group;
        }

        private void RemoveWordFromAllGroups(
            EditableLanguageConfiguration configuration,
            string word)
        {
            ValidateConfiguration(
                configuration);

            string normalizedWord =
                word.Trim();

            foreach (KeywordGroupConfiguration group
                in configuration.Groups)
            {
                if (group.Words == null)
                {
                    continue;
                }

                group.Words.RemoveAll(
                    item => string.Equals(
                        item,
                        normalizedWord,
                        StringComparison.Ordinal));
            }
        }

        private static void ValidateWord(
            string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                throw new ArgumentException(
                    "The keyword cannot be empty.",
                    nameof(word));
            }
        }

        private static void ValidateConfiguration(
            EditableLanguageConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(
                    nameof(configuration));
            }

            if (configuration.Groups == null)
            {
                throw new InvalidOperationException(
                    "The language configuration has no group collection.");
            }
        }
    }
}