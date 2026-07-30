using NoteHighlightAddin.Highlighting.KeywordGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NoteHighlightAddin.Highlighting.Preview.Services
{
    /// <summary>
    /// Generates sample source code using the currently selected
    /// language and keyword group.
    /// </summary>
    [ComVisible(false)]
    public sealed class PreviewSampleCodeService
        : IPreviewSampleCodeService
    {
        private const int MaximumDisplayedWords = 20;

        public string Generate(
            EditableLanguageConfiguration configuration,
            KeywordGroupConfiguration selectedGroup)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(
                    nameof(configuration));
            }

            string language =
                NormalizeLanguage(
                    configuration.Language);

            IReadOnlyList<string> words =
                GetPreviewWords(
                    selectedGroup);

            if (language == "python")
            {
                return GeneratePythonSample(
                    selectedGroup,
                    words);
            }

            return GenerateGenericSample(
                configuration,
                selectedGroup,
                words);
        }

        private static string GeneratePythonSample(
            KeywordGroupConfiguration selectedGroup,
            IReadOnlyList<string> words)
        {
            var builder =
                new StringBuilder();

            builder.AppendLine(
                "# NoteHighlight+ contextual preview");

            AppendPythonGroupInformation(
                builder,
                selectedGroup);

            builder.AppendLine();
            builder.AppendLine(
                "class PreviewExample:");

            builder.AppendLine(
                "    def __init__(self, value):");

            builder.AppendLine(
                "        self.value = value");

            builder.AppendLine();

            builder.AppendLine(
                "    def process(self, items):");

            builder.AppendLine(
                "        for item in items:");

            builder.AppendLine(
                "            if item is not None:");

            builder.AppendLine(
                "                print(item)");

            builder.AppendLine();

            builder.AppendLine(
                "        return self.value");

            builder.AppendLine();

            builder.AppendLine(
                "example = PreviewExample(True)");

            builder.AppendLine(
                "result = example.process([1, 2, 3])");

            AppendPythonSelectedWords(
                builder,
                words);

            return builder.ToString();
        }

        private static void AppendPythonGroupInformation(
            StringBuilder builder,
            KeywordGroupConfiguration selectedGroup)
        {
            if (selectedGroup == null)
            {
                builder.AppendLine(
                    "# Select a keyword group to preview its words.");

                return;
            }

            builder.AppendLine(
                "# Selected group: "
                + CreateSingleLineText(
                    selectedGroup.DisplayName));
        }

        private static void AppendPythonSelectedWords(
            StringBuilder builder,
            IReadOnlyList<string> words)
        {
            builder.AppendLine();
            builder.AppendLine(
                "# Words from the selected group:");

            if (words.Count == 0)
            {
                builder.AppendLine(
                    "# No literal words are defined in this group.");

                return;
            }

            foreach (string word in words)
            {
                builder.AppendLine(
                    "# " + CreateSingleLineText(word));
            }
        }

        private static string GenerateGenericSample(
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
                + CreateSingleLineText(
                    configuration.Language));

            if (selectedGroup != null)
            {
                builder.AppendLine(
                    "Selected group: "
                    + CreateSingleLineText(
                        selectedGroup.DisplayName));
            }

            builder.AppendLine();
            builder.AppendLine(
                "Words from the selected group:");

            if (words.Count == 0)
            {
                builder.AppendLine(
                    "No literal words are defined in this group.");
            }
            else
            {
                foreach (string word in words)
                {
                    builder.AppendLine(
                        CreateSingleLineText(word));
                }
            }

            AppendRegexInformation(
                builder,
                selectedGroup);

            return builder.ToString();
        }

        private static void AppendRegexInformation(
            StringBuilder builder,
            KeywordGroupConfiguration selectedGroup)
        {
            if (selectedGroup == null ||
                selectedGroup.Regex == null ||
                selectedGroup.Regex.Count == 0)
            {
                return;
            }

            builder.AppendLine();
            builder.AppendLine(
                "Regular expressions:");

            foreach (string regex in selectedGroup.Regex
                .Where(value =>
                    !string.IsNullOrWhiteSpace(value))
                .Take(MaximumDisplayedWords))
            {
                builder.AppendLine(
                    CreateSingleLineText(regex));
            }
        }

        private static IReadOnlyList<string> GetPreviewWords(
            KeywordGroupConfiguration selectedGroup)
        {
            if (selectedGroup == null ||
                selectedGroup.Words == null)
            {
                return new List<string>();
            }

            return selectedGroup.Words
                .Where(word =>
                    !string.IsNullOrWhiteSpace(word))
                .Distinct(
                    StringComparer.Ordinal)
                .Take(
                    MaximumDisplayedWords)
                .ToList();
        }

        private static string NormalizeLanguage(
            string language)
        {
            if (string.IsNullOrWhiteSpace(
                language))
            {
                return string.Empty;
            }

            string normalized =
                language.Trim()
                    .ToLowerInvariant();

            if (normalized.EndsWith(
                ".lang",
                StringComparison.Ordinal))
            {
                normalized =
                    normalized.Substring(
                        0,
                        normalized.Length - 5);
            }

            return normalized;
        }

        private static string CreateSingleLineText(
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return string.Empty;
            }

            return value
                .Replace(
                    "\r",
                    " ")
                .Replace(
                    "\n",
                    " ")
                .Trim();
        }
    }
}