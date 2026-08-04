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
            Styles =
                new Dictionary<string, ThemeStyle>(
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
        /// Estilos generales como Default, Canvas,
        /// Number, String, Operator, etc.
        /// </summary>
        public IDictionary<string, ThemeStyle> Styles
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