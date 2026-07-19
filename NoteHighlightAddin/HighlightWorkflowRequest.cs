using System.Drawing;

namespace NoteHighlightAddin
{
    public class HighlightWorkflowRequest
    {
        public string FileName { get; set; }

        public string Content { get; set; }

        public string CodeType { get; set; }

        public string HighLightStyle { get; set; }

        public bool ShowLineNumber { get; set; }

        public bool CopyToClipboard { get; set; }

        public bool DarkMode { get; set; }

        public Color HighlightColor { get; set; }
    }
}