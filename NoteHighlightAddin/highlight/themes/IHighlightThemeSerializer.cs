namespace NoteHighlightAddin.Highlighting.Themes
{
    /// <summary>
    /// Define el contrato para serializar un HighlightTheme
    /// a un archivo .theme.
    /// </summary>
    public interface IHighlightThemeSerializer
    {
        void Serialize(
            HighlightTheme theme,
            string filePath);
    }
}