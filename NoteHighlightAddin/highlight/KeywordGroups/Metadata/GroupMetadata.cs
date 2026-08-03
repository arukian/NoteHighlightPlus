using System.Runtime.Serialization;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Metadata
{
    /// <summary>
    /// Contiene las propiedades de un grupo que pertenecen
    /// exclusivamente al editor y no al formato .lang.
    /// </summary>
    [DataContract]
    public sealed class GroupMetadata
    {
        [DataMember(Name = "id", Order = 1)]
        public int Id
        {
            get;
            set;
        }

        [DataMember(Name = "displayName", Order = 2)]
        public string DisplayName
        {
            get;
            set;
        }

        [DataMember(Name = "description", Order = 3)]
        public string Description
        {
            get;
            set;
        }

        [DataMember(Name = "priority", Order = 4)]
        public int Priority
        {
            get;
            set;
        }

        [DataMember(Name = "visible", Order = 5)]
        public bool Visible
        {
            get;
            set;
        }

        [DataMember(Name = "isCustom", Order = 6)]
        public bool IsCustom
        {
            get;
            set;
        }
    }
}