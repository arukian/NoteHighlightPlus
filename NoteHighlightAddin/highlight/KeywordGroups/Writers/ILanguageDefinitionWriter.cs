using GenerateHighlightContent.LanguageDefinitions;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Writers
{
    public interface ILanguageDefinitionWriter
    {
        void Write(
            HighlightLanguageDefinition definition,
            string filePath);

        string Serialize(
            HighlightLanguageDefinition definition);
    }
}