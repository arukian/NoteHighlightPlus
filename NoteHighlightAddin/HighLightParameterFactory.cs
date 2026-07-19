using GenerateHighlightContent;
using System.Drawing;

namespace NoteHighlightAddin
{
    public class HighLightParameterFactory
    {
        public HighLightParameter Create(
            string fileName,
            string content,
            string codeType,
            string highLightStyle,
            bool showLineNumber,
            Color highlightColor,
            string font,
            int fontSize)
        {
            return new HighLightParameter
            {
                FileName = fileName,
                Content = content,
                CodeType = codeType,
                HighLightStyle = highLightStyle,
                ShowLineNumber = showLineNumber,
                HighlightColor = highlightColor,
                Font = font,
                FontSize = fontSize
            };
        }
    }
}