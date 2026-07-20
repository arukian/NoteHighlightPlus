using System.Collections.Generic;

namespace GenerateHighlightContent.LanguageDefinitions
{
    /// <summary>
    /// Representa un grupo de palabras o expresiones definido dentro
    /// de un archivo de lenguaje de Highlight.
    /// </summary>
    public sealed class HighlightKeywordGroup
    {
        public HighlightKeywordGroup()
        {
            Words = new List<string>();
            Regex = new List<string>();
        }

        /// <summary>
        /// Identificador utilizado por Highlight, por ejemplo:
        /// Keywords = { Id = 1, ... }
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Palabras literales pertenecientes al grupo.
        /// </summary>
        public IList<string> Words { get; set; }

        /// <summary>
        /// Expresiones regulares pertenecientes al grupo.
        /// </summary>
        public IList<string> Regex { get; set; }
    }
}