using System.Collections.Generic;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Metadata
{
    /// <summary>
    /// Representa los metadatos de edición asociados
    /// a un archivo de definición de lenguaje.
    /// </summary>
    public sealed class LanguageGroupMetadata
    {
        public LanguageGroupMetadata()
        {
            Groups =
                new List<GroupMetadata>();
        }

        public List<GroupMetadata> Groups
        {
            get;
            set;
        }
    }
}