namespace NoteHighlightAddin.Highlighting.KeywordGroups.Services
{
    public interface ILanguageEditorService
    {
        EditableLanguageConfiguration Load(
            string language);

        EditableLanguageConfiguration LoadFromFile(
            string filePath);

        void Save(
            EditableLanguageConfiguration configuration);

        void SaveAs(
            EditableLanguageConfiguration configuration,
            string filePath);
    }
}