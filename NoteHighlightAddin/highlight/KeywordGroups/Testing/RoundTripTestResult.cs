using System.Collections.Generic;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Testing
{
    public sealed class RoundTripTestResult
    {
        public RoundTripTestResult()
        {
            Differences = new List<string>();
        }

        public string SourceFilePath { get; set; }

        public string GeneratedFilePath { get; set; }

        public bool IsEquivalent { get; set; }

        public List<string> Differences { get; set; }
    }
}