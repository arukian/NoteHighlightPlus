namespace NoteHighlightAddin.Highlighting.Themes
{
    /// <summary>
    /// Representa las propiedades visuales de un estilo
    /// definido dentro de un archivo .theme.
    /// </summary>
    public sealed class ThemeStyle
    {
        public ThemeStyle()
        {
            Colour =
                "#000000";
        }

        public string Name
        {
            get;
            set;
        }

        public string Colour
        {
            get;
            set;
        }

        public bool Bold
        {
            get;
            set;
        }

        public bool Italic
        {
            get;
            set;
        }
    }
}