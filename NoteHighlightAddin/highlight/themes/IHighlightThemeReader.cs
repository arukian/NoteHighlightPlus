namespace NoteHighlightAddin.Highlighting.Themes
{
    public interface IHighlightThemeReader
    {
        HighlightTheme Read(
            string filePath);
    }
}