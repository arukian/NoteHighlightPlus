using System.Collections.Generic;

namespace NoteHighlightAddin.Highlighting.KeywordGroups
{
    /// <summary>
    /// Representa la configuración editable completa de un lenguaje.
    /// </summary>
    public sealed class EditableLanguageConfiguration
    {
        public EditableLanguageConfiguration()
        {
            Groups = new List<KeywordGroupConfiguration>();
        }

        public string Language { get; set; }

        public string Description { get; set; }

        public bool CaseSensitive { get; set; }

        public List<string> Extensions { get; set; } =
            new List<string>();

        public List<KeywordGroupConfiguration> Groups { get; set; }
    }
}