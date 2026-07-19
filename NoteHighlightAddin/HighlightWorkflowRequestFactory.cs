using System.Drawing;

namespace NoteHighlightAddin
{
    public class HighlightWorkflowRequestFactory
    {
        public HighlightWorkflowRequest Create(
            string fileName,
            string content,
            string codeType,
            string highLightStyle,
            bool showLineNumber,
            bool copyToClipboard,
            bool darkMode,
            Color highlightColor)
        {
            return new HighlightWorkflowRequest
            {
                FileName = fileName,
                Content = content,
                CodeType = codeType,
                HighLightStyle = highLightStyle,
                ShowLineNumber = showLineNumber,
                CopyToClipboard = copyToClipboard,
                DarkMode = darkMode,
                HighlightColor = highlightColor
            };
        }
    }
}