using NoteHighlightAddin.Highlighting.KeywordGroups;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.ViewModels
{
    public sealed class KeywordGroupListItem
    {
        public KeywordGroupListItem(
            KeywordGroupConfiguration group)
        {
            Group = group;
        }

        public KeywordGroupConfiguration Group { get; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Group.DisplayName))
            {
                return $"{Group.Id} - {Group.DisplayName}";
            }

            return $"Group {Group.Id}";
        }
    }
}