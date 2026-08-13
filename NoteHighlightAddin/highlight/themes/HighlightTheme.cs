using System.Collections.Generic;

namespace NoteHighlightAddin.Highlighting.Themes
{
    /// <summary>
    /// Representa un tema de Highlight cargado desde
    /// un archivo .theme.
    /// </summary>
    public sealed class HighlightTheme
    {
        public HighlightTheme()
        {
            Variables =
                new Dictionary<string, string>(
                    System.StringComparer.OrdinalIgnoreCase);

            Categories =
                new List<string>();

            SemanticTokenTypes =
                new List<SemanticTokenStyle>();

            Styles =
                new Dictionary<string, ThemeStyle>(
                    System.StringComparer.OrdinalIgnoreCase);

            StyleAliases =
                new Dictionary<string, string>(
                    System.StringComparer.OrdinalIgnoreCase);

            KeywordStyles =
                new List<ThemeStyle>();
        }

        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Variables de texto declaradas en el tema.
        /// Por ejemplo: blue = "#0080C0".
        /// </summary>
        public IDictionary<string, string> Variables
        {
            get;
            private set;
        }

        /// <summary>
        /// Categorías declaradas por el tema.
        /// Por ejemplo: Categories = {"light"}.
        /// </summary>
        public IList<string> Categories
        {
            get;
            private set;
        }

        /// <summary>
        /// Asignaciones LSP/SemanticTokenTypes del tema.
        /// Conservan la referencia al estilo original,
        /// por ejemplo Keywords[2], Number o Operator.
        /// </summary>
        public IList<SemanticTokenStyle> SemanticTokenTypes
        {
            get;
            private set;
        }


        /// <summary>
        /// Estilos generales como Default, Canvas,
        /// Number, String, Operator, etc.
        /// </summary>
        public IDictionary<string, ThemeStyle> Styles
        {
            get;
            private set;
        }

        /// <summary>
        /// Relación entre un alias de estilo y el estilo
        /// al que apunta. Por ejemplo: LineNum = Default.
        /// </summary>
        public IDictionary<string, string> StyleAliases
        {
            get;
            private set;
        }

        /// <summary>
        /// Estilos Keywords[1], Keywords[2], etc.
        /// El índice cero representa Keywords[1].
        /// </summary>
        public IList<ThemeStyle> KeywordStyles
        {
            get;
            private set;
        }

        public ThemeStyle GetKeywordStyle(
            int groupId)
        {
            if (groupId <= 0 ||
                groupId > KeywordStyles.Count)
            {
                return null;
            }

            return KeywordStyles[groupId - 1];
        }
    }
}