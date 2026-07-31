using NoteHighlightAddin.Highlighting.KeywordGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoteHighlightAddin.Highlighting.Preview.Services.Builders
{
    internal sealed class GenericPreviewSampleBuilder
        : IPreviewSampleBuilder
    {
        private const int MaximumDisplayedWords =
            20;

        public bool CanHandle(
            string language)
        {
            return true;
        }

        public string Generate(
            EditableLanguageConfiguration configuration,
            KeywordGroupConfiguration selectedGroup,
            IReadOnlyList<string> words)
        {
            var builder =
                new StringBuilder();

            builder.AppendLine(
                "NoteHighlight+ contextual preview");

            builder.AppendLine(
                "Language: "
                + GetLanguageName(
                    configuration));

            builder.AppendLine(
                "Selected group: "
                + GetGroupDisplayName(
                    selectedGroup));

            builder.AppendLine();

            builder.AppendLine(
                "Words from the selected group:");

            if (words == null ||
                words.Count == 0)
            {
                builder.AppendLine(
                    "No literal words are defined in this group.");

                return builder.ToString();
            }

            foreach (string word in words
                .Where(value =>
                    !string.IsNullOrWhiteSpace(value))
                .Take(MaximumDisplayedWords))
            {
                builder.AppendLine(
                    word.Trim());
            }

            return builder.ToString();
        }

        private static string GetLanguageName(
            EditableLanguageConfiguration configuration)
        {
            if (configuration == null ||
                string.IsNullOrWhiteSpace(
                    configuration.Language))
            {
                return "Unknown";
            }

            return configuration.Language.Trim();
        }

        private static string GetGroupDisplayName(
            KeywordGroupConfiguration selectedGroup)
        {
            if (selectedGroup == null ||
                string.IsNullOrWhiteSpace(
                    selectedGroup.DisplayName))
            {
                return "No group selected";
            }

            return selectedGroup.DisplayName.Trim();
        }
    }
}