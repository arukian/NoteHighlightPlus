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

        /// <summary>
        /// Color resuelto que utiliza la interfaz y el preview.
        /// </summary>
        public string Colour
        {
            get;
            set;
        }

        /// <summary>
        /// Nombre de la variable usada originalmente por Colour.
        /// Es null cuando el color fue escrito como literal.
        /// </summary>
        public string ColourReference
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
