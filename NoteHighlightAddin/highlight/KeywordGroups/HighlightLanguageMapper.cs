using GenerateHighlightContent.LanguageDefinitions;
using System;
using System.Linq;

namespace NoteHighlightAddin.Highlighting.KeywordGroups
{
    /// <summary>
    /// Convierte entre el modelo estructural de un archivo .lang
    /// y el modelo editable utilizado por la aplicación.
    /// </summary>
    public sealed class HighlightLanguageMapper
    {
        public EditableLanguageConfiguration ToEditableConfiguration(
            HighlightLanguageDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            var configuration =
                new EditableLanguageConfiguration
                {
                    Language = definition.Language,
                    Description = definition.Description,
                    CaseSensitive = definition.CaseSensitive
                };

            foreach (string extension in definition.Extensions)
            {
                configuration.Extensions.Add(
                    extension);
            }

            int priority = 0;

            foreach (HighlightKeywordGroup sourceGroup
                in definition.Groups.OrderBy(group => group.Id))
            {
                var targetGroup =
                    new KeywordGroupConfiguration
                    {
                        Id = sourceGroup.Id,
                        DisplayName =
                            $"Group {sourceGroup.Id}",

                        Description =
                            "Group loaded from the language definition.",

                        Priority = priority,
                        Visible = true,
                        IsCustom = false
                    };

                foreach (string word in sourceGroup.Words)
                {
                    targetGroup.Words.Add(
                        word);
                }

                foreach (string regex in sourceGroup.Regex)
                {
                    targetGroup.Regex.Add(
                        regex);
                }

                configuration.Groups.Add(
                    targetGroup);

                priority++;
            }

            return configuration;
        }

        public HighlightLanguageDefinition ToLanguageDefinition(
            EditableLanguageConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(
                    nameof(configuration));
            }

            var definition =
                new HighlightLanguageDefinition
                {
                    Language = configuration.Language,
                    Description = configuration.Description,
                    CaseSensitive = configuration.CaseSensitive
                };

            foreach (string extension in configuration.Extensions)
            {
                definition.Extensions.Add(
                    extension);
            }

            foreach (KeywordGroupConfiguration sourceGroup
                in configuration.Groups
                    .OrderBy(group => group.Priority)
                    .ThenBy(group => group.Id))
            {
                var targetGroup =
                    new HighlightKeywordGroup
                    {
                        Id = sourceGroup.Id
                    };

                foreach (string word in sourceGroup.Words)
                {
                    targetGroup.Words.Add(
                        word);
                }

                foreach (string regex in sourceGroup.Regex)
                {
                    targetGroup.Regex.Add(
                        regex);
                }

                definition.Groups.Add(
                    targetGroup);
            }

            return definition;
        }
    }
}