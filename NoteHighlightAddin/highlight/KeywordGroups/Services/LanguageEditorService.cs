using GenerateHighlightContent.LanguageDefinitions;
using Infrastructure.Core;
using NoteHighlightAddin.Highlighting.KeywordGroups.Readers;
using NoteHighlightAddin.Highlighting.KeywordGroups.Writers;
using System;
using System.IO;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Services
{
    public sealed class LanguageEditorService
        : ILanguageEditorService
    {
        private readonly ILanguageDefinitionReader reader;
        private readonly ILanguageDefinitionWriter writer;
        private readonly HighlightLanguageMapper mapper;
        private readonly string languagesFolder;

        public LanguageEditorService()
            : this(
                new HighlightLanguageDefinitionReader(),
                new HighlightLanguageDefinitionWriter(),
                new HighlightLanguageMapper(),
                PathManager.LanguagesFolder)
        {
        }

        public LanguageEditorService(
            ILanguageDefinitionReader reader,
            ILanguageDefinitionWriter writer,
            HighlightLanguageMapper mapper,
            string languagesFolder)
        {
            this.reader =
                reader ?? throw new ArgumentNullException(
                    nameof(reader));

            this.writer =
                writer ?? throw new ArgumentNullException(
                    nameof(writer));

            this.mapper =
                mapper ?? throw new ArgumentNullException(
                    nameof(mapper));

            if (string.IsNullOrWhiteSpace(languagesFolder))
            {
                throw new ArgumentException(
                    "The languages folder cannot be empty.",
                    nameof(languagesFolder));
            }

            this.languagesFolder =
                languagesFolder;
        }

        public EditableLanguageConfiguration Load(
            string language)
        {
            string filePath =
                GetLanguageFilePath(
                    language);

            return LoadFromFile(
                filePath);
        }

        public EditableLanguageConfiguration LoadFromFile(
            string filePath)
        {
            ValidateSourceFile(
                filePath);

            HighlightLanguageDefinition definition =
                reader.Read(
                    filePath);

            return mapper.ToEditableConfiguration(
                definition);
        }

        public void Save(
            EditableLanguageConfiguration configuration)
        {
            ValidateConfiguration(
                configuration);

            string filePath =
                GetLanguageFilePath(
                    configuration.Language);

            SaveAs(
                configuration,
                filePath);
        }

        public void SaveAs(
            EditableLanguageConfiguration configuration,
            string filePath)
        {
            ValidateConfiguration(
                configuration);

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "The destination file path cannot be empty.",
                    nameof(filePath));
            }

            HighlightLanguageDefinition definition =
                mapper.ToLanguageDefinition(
                    configuration);

            writer.Write(
                definition,
                filePath);
        }

        private string GetLanguageFilePath(
            string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                throw new ArgumentException(
                    "The language name cannot be empty.",
                    nameof(language));
            }

            string normalizedLanguage =
                Path.GetFileNameWithoutExtension(
                    language.Trim());

            if (string.IsNullOrWhiteSpace(normalizedLanguage))
            {
                throw new ArgumentException(
                    "The language name is invalid.",
                    nameof(language));
            }

            return Path.Combine(
                languagesFolder,
                normalizedLanguage + ".lang");
        }

        private static void ValidateSourceFile(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "The source file path cannot be empty.",
                    nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The language definition file was not found.",
                    filePath);
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

            if (string.IsNullOrWhiteSpace(configuration.Language))
            {
                throw new InvalidOperationException(
                    "The editable language configuration has no language name.");
            }

            if (configuration.Groups == null)
            {
                throw new InvalidOperationException(
                    "The editable language configuration has no group collection.");
            }
        }
    }
}