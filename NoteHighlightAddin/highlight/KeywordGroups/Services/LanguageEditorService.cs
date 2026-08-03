using GenerateHighlightContent.LanguageDefinitions;
using Infrastructure.Core;
using NoteHighlightAddin.Highlighting.KeywordGroups.Metadata;
using NoteHighlightAddin.Highlighting.KeywordGroups.Readers;
using NoteHighlightAddin.Highlighting.KeywordGroups.Writers;
using System;
using System.IO;
using System.Linq;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Services
{
    public sealed class LanguageEditorService
        : ILanguageEditorService
    {
        private readonly ILanguageDefinitionReader reader;
        private readonly ILanguageDefinitionWriter writer;
        private readonly HighlightLanguageMapper mapper;
        private readonly ILanguageGroupMetadataStore metadataStore;
        private readonly string languagesFolder;

        public LanguageEditorService()
            : this(
                new HighlightLanguageDefinitionReader(),
                new HighlightLanguageDefinitionWriter(),
                new HighlightLanguageMapper(),
                new JsonLanguageGroupMetadataStore(),
                PathManager.LanguagesFolder)
        {
        }

        public LanguageEditorService(
            ILanguageDefinitionReader reader,
            ILanguageDefinitionWriter writer,
            HighlightLanguageMapper mapper,
            ILanguageGroupMetadataStore metadataStore,
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

            this.metadataStore =
                metadataStore ?? throw new ArgumentNullException(
                    nameof(metadataStore));

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

            EditableLanguageConfiguration configuration =
                mapper.ToEditableConfiguration(
                    definition);

            LanguageGroupMetadata metadata =
                metadataStore.Load(
                    filePath);

            ApplyMetadata(
                configuration,
                metadata);

            return configuration;
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

            LanguageGroupMetadata metadata =
                CreateMetadata(
                    configuration);

            metadataStore.Save(
                filePath,
                metadata);
        }

        private static void ApplyMetadata(
            EditableLanguageConfiguration configuration,
            LanguageGroupMetadata metadata)
        {
            if (configuration?.Groups == null ||
                metadata?.Groups == null)
            {
                return;
            }

            foreach (KeywordGroupConfiguration group
                in configuration.Groups)
            {
                if (group == null)
                {
                    continue;
                }

                GroupMetadata groupMetadata =
                    metadata.Groups
                        .FirstOrDefault(
                            item =>
                                item != null &&
                                item.Id == group.Id);

                if (groupMetadata == null)
                {
                    continue;
                }

                group.DisplayName =
                    groupMetadata.DisplayName;

                group.Description =
                    groupMetadata.Description;

                group.Priority =
                    groupMetadata.Priority;

                group.Visible =
                    groupMetadata.Visible;

                group.IsCustom =
                    groupMetadata.IsCustom;
            }
        }

        private static LanguageGroupMetadata CreateMetadata(
            EditableLanguageConfiguration configuration)
        {
            var metadata =
                new LanguageGroupMetadata();

            foreach (KeywordGroupConfiguration group
                in configuration.Groups
                    .Where(group => group != null)
                    .OrderBy(group => group.Priority)
                    .ThenBy(group => group.Id))
            {
                metadata.Groups.Add(
                    new GroupMetadata
                    {
                        Id = group.Id,
                        DisplayName = group.DisplayName,
                        Description = group.Description,
                        Priority = group.Priority,
                        Visible = group.Visible,
                        IsCustom = group.IsCustom
                    });
            }

            return metadata;
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