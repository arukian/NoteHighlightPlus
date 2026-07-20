using System.Collections.Generic;

namespace NoteHighlightAddin.Highlighting.KeywordGroups
{
    public class LanguageKeywordConfiguration
    {
        public string Language { get; set; }

        public List<KeywordGroupDefinition> Groups { get; set; }

        public LanguageKeywordConfiguration()
        {
            Groups = new List<KeywordGroupDefinition>();
        }
    }
}