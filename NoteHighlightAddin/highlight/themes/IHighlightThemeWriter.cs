namespace NoteHighlightAddin.Highlighting.Themes
{
    public interface IHighlightThemeWriter
    {
        void UpdateKeywordColour(
            string filePath,
            int groupId,
            string colour);
    }
}