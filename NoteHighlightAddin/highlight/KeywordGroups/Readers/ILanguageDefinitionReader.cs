using GenerateHighlightContent.LanguageDefinitions;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Readers
{
    public interface ILanguageDefinitionReader
    {
        HighlightLanguageDefinition Read(
            string filePath);
    }
}