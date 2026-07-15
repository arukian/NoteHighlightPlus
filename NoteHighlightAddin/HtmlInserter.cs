using System.Linq;

namespace NoteHighlightAddin
{
    public class HtmlInserter
    {
        public bool ContainsAsianCharacter(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return text.Any(character =>
                (uint)character >= 0x4E00 &&
                (uint)character <= 0x2FA1F);
        }
    }
}