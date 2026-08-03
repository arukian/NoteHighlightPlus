using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Metadata
{
    /// <summary>
    /// Representa los metadatos de edición asociados
    /// a un archivo de definición de lenguaje.
    /// </summary>
    [DataContract]
    public sealed class LanguageGroupMetadata
    {
        public LanguageGroupMetadata()
        {
            Groups =
                new List<GroupMetadata>();
        }

        [DataMember(Name = "groups", Order = 1)]
        public List<GroupMetadata> Groups
        {
            get;
            set;
        }
    }
}