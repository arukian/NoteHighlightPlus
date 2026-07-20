using System.Collections.Generic;

namespace NoteHighlightAddin.Highlighting.KeywordGroups
{
    /// <summary>
    /// Representa la configuración editable de un grupo de palabras.
    ///
    /// Este modelo es independiente del formato .lang utilizado
    /// por highlight.exe.
    /// </summary>
    public sealed class KeywordGroupConfiguration
    {
        public KeywordGroupConfiguration()
        {
            Words = new List<string>();
            Regex = new List<string>();
        }

        /// <summary>
        /// Identificador utilizado por Highlight.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre visible del grupo dentro del editor.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Descripción opcional del grupo.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Palabras literales pertenecientes al grupo.
        /// </summary>
        public List<string> Words { get; set; }

        /// <summary>
        /// Expresiones regulares pertenecientes al grupo.
        /// </summary>
        public List<string> Regex { get; set; }

        /// <summary>
        /// Color configurable del grupo.
        /// </summary>
        public string Colour { get; set; }

        public bool Bold { get; set; }

        public bool Italic { get; set; }

        /// <summary>
        /// Orden utilizado por el editor.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Indica si el grupo debe mostrarse en el editor.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Indica si el grupo fue creado por el usuario.
        /// </summary>
        public bool IsCustom { get; set; }
    }
}