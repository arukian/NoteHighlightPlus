using NoteHighlightAddin.Highlighting.KeywordGroups;
using System.Collections.Generic;

namespace NoteHighlightAddin.Highlighting.Preview.Services.Builders
{
    internal interface IPreviewSampleBuilder
    {
        bool CanHandle(
            string language);

        string Generate(
            EditableLanguageConfiguration configuration,
            KeywordGroupConfiguration selectedGroup,
            IReadOnlyList<string> words);
    }
}