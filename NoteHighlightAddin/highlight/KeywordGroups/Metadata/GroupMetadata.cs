namespace NoteHighlightAddin.Highlighting.KeywordGroups.Metadata
{
    /// <summary>
    /// Contiene las propiedades de un grupo que pertenecen
    /// exclusivamente al editor y no al formato .lang.
    /// </summary>
    public sealed class GroupMetadata
    {
        public int Id
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public int Priority
        {
            get;
            set;
        }

        public bool Visible
        {
            get;
            set;
        }

        public bool IsCustom
        {
            get;
            set;
        }
    }
}