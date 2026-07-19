using System;

namespace NoteHighlightAddin
{
    public class CodeEditorLanguageMapper
    {
        public string GetHighlightingName(string codeType)
        {
            if (string.IsNullOrWhiteSpace(codeType))
            {
                return string.Empty;
            }

            switch (codeType.ToLowerInvariant())
            {
                case "cs":
                    return "C#";

                case "vb":
                    return "VBNET";

                case "js":
                    return "JavaScript";

                case "xml":
                    return "XML";

                case "css":
                    return "CSS";

                case "html":
                    return "HTML";

                case "php":
                    return "PHP";

                case "java":
                    return "Java";

                case "c":
                    return "C++.NET";

                default:
                    return string.Empty;
            }
        }
    }
}