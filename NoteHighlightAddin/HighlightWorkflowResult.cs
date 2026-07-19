using GenerateHighlightContent;

namespace NoteHighlightAddin
{
    public class HighlightWorkflowResult
    {
        public HighLightParameter Parameters { get; set; }

        public string OutputFileName { get; set; }

        public bool CopiedToClipboard { get; set; }
    }
}