namespace NoteHighlightAddin.Highlighting.Themes
{
    /// <summary>
    /// Representa una asignación de SemanticTokenTypes
    /// dentro de un archivo .theme.
    /// </summary>
    public sealed class SemanticTokenStyle
    {
        public string Type
        {
            get;
            set;
        }

        public string StyleReference
        {
            get;
            set;
        }
    }
}