namespace NoteHighlightAddin.Highlighting.KeywordGroups.Metadata
{
    /// <summary>
    /// Define las operaciones para cargar y guardar los metadatos
    /// de edición asociados a un lenguaje.
    /// </summary>
    public interface ILanguageGroupMetadataStore
    {
        LanguageGroupMetadata Load(
            string languageFilePath);

        void Save(
            string languageFilePath,
            LanguageGroupMetadata metadata);
    }
}