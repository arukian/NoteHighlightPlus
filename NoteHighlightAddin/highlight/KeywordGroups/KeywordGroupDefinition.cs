using System.Collections.Generic;

namespace NoteHighlightAddin.Highlighting.KeywordGroups
{
    public class KeywordGroupDefinition
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> Words { get; set; }

        public string Regex { get; set; }

        public bool IsCustom { get; set; }

        public string Colour { get; set; }

        public bool Bold { get; set; }

        public bool Italic { get; set; }

        public KeywordGroupDefinition()
        {
            Words = new List<string>();
        }
    }
}