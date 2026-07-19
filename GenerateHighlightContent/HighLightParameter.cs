using System.Drawing;

namespace GenerateHighlightContent
{
    public class HighLightParameter
    {
        public string Content { get; set; }

        public string CodeType { get; set; }

        public string HighLightStyle { get; set; }

        public bool ShowLineNumber { get; set; }

        public string FileName { get; set; }

        public Color HighlightColor { get; set; }

        public string Font { get; set; }

        public int FontSize { get; set; }
    }
}