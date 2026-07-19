using System.Drawing;

namespace NoteHighlightAddin
{
    public class MainFormSettings
    {
        public int HighLightStyle { get; set; }

        public Color BackgroundColor { get; set; }

        public bool SaveOnClipboard { get; set; }

        public bool ShowLineNumber { get; set; }

        public string Font { get; set; }

        public int FontSize { get; set; }
    }
}