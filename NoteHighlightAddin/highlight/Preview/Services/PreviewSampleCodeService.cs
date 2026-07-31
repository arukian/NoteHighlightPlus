using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.Preview.Services.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoteHighlightAddin.Highlighting.Preview.Services
{
    internal sealed class PreviewSampleCodeService
        : IPreviewSampleCodeService
    {
        private const int MaximumPreviewWords =
            20;

        private readonly IReadOnlyList<IPreviewSampleBuilder>
            _builders;

        public PreviewSampleCodeService()
        {
            _builders =
                new List<IPreviewSampleBuilder>
                {
                    new PythonPreviewSampleBuilder(),
                    new JavaScriptPreviewSampleBuilder(),
                    new GenericPreviewSampleBuilder()
                };
        }

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

            IPreviewSampleBuilder builder =
                _builders.FirstOrDefault(
                    item =>
                        item.CanHandle(
                            language));

            if (builder == null)
            {
                throw new InvalidOperationException(
                    "No preview sample builder is available.");
            }

            return builder.Generate(
                configuration,
                selectedGroup,
                words);
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
                .Select(word =>
                    word.Trim())
                .Distinct(
                    StringComparer.Ordinal)
                .Take(
                    MaximumPreviewWords)
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
                language
                    .Trim()
                    .ToLowerInvariant();

            if (normalized == "py")
            {
                return "python";
            }

            if (normalized == "python3")
            {
                return "python";
            }

            if (normalized == "js")
            {
                return "javascript";
            }

            return normalized;
        }
    }
}