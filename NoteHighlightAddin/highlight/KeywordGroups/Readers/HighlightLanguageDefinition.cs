using System.Collections.Generic;

namespace GenerateHighlightContent.LanguageDefinitions
{
    /// <summary>
    /// Representa el contenido estructural de un archivo .lang
    /// utilizado por Highlight.
    ///
    /// Este modelo no contiene información visual ni datos propios
    /// del editor.
    /// </summary>
    public sealed class HighlightLanguageDefinition
    {
        public HighlightLanguageDefinition()
        {
            Extensions = new List<string>();
            Groups = new List<HighlightKeywordGroup>();
            OriginalContent = string.Empty;
        }

        /// <summary>
        /// Nombre interno del lenguaje.
        /// Normalmente puede obtenerse del nombre del archivo.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Descripción declarada dentro del archivo .lang.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indica si la comparación de palabras distingue
        /// entre mayúsculas y minúsculas.
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Extensiones asociadas al lenguaje.
        /// </summary>
        public IList<string> Extensions { get; set; }

        /// <summary>
        /// Grupos de keywords y expresiones regulares del lenguaje.
        /// </summary>
        public IList<HighlightKeywordGroup> Groups { get; set; }

        /// <summary>
        /// Contenido original completo del archivo .lang.
        /// Se conserva para que el writer pueda modificar únicamente
        /// las secciones editables sin eliminar reglas de sintaxis.
        /// </summary>
        public string OriginalContent { get; set; }
    }
}