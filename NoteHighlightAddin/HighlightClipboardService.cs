using Helper;
using NoteHighLightForm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NoteHighlightAddin
{
    public class HighlightClipboardService
    {
        private const string SpanEndTag = "</span>";

        public void Copy(
            string outputFileName,
            bool darkMode,
            bool showLineNumber)
        {
            var html = BuildClipboardHtml(
                outputFileName,
                darkMode,
                showLineNumber);

            HtmlFragment.CopyToClipboard(html);

            File.Delete(outputFileName);
        }

        private string BuildClipboardHtml(
            string outputFileName,
            bool darkMode,
            bool showLineNumber)
        {
            var result = new StringBuilder();

            using (var stream = new FileStream(
                outputFileName,
                FileMode.Open,
                FileAccess.Read))
            using (var reader = new StreamReader(
                stream,
                new UTF8Encoding(false)))
            {
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();

                    line = RemoveUtf8ByteOrderMark(line);
                    line = RemoveDarkModeBackground(line, darkMode);

                    if (!line.StartsWith("</pre>"))
                    {
                        line = line
                            .Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;")
                            .Replace("&apos;", "'")
                            + "<br />";
                    }

                    result.AppendLine(
                        PreserveLeadingSpaces(
                            line,
                            showLineNumber));
                }
            }

            return result.ToString();
        }

        private string RemoveUtf8ByteOrderMark(string line)
        {
            string byteOrderMark =
                Encoding.UTF8.GetString(
                    Encoding.UTF8.GetPreamble());

            return line.Replace(byteOrderMark, string.Empty);
        }

        private string RemoveDarkModeBackground(
            string line,
            bool darkMode)
        {
            if (!darkMode || !line.StartsWith("<pre"))
            {
                return line;
            }

            int backgroundIndex =
                line.IndexOf("background-color");

            if (backgroundIndex < 0)
            {
                return line;
            }

            int semicolonIndex =
                line.IndexOf(';', backgroundIndex);

            if (semicolonIndex < 0)
            {
                return line;
            }

            return line.Remove(
                backgroundIndex,
                semicolonIndex - backgroundIndex + 1);
        }

        private string PreserveLeadingSpaces(
            string line,
            bool showLineNumber)
        {
            List<char> characters =
                line.ToCharArray().ToList();

            var result = new StringBuilder();

            int index = 0;

            if (showLineNumber &&
                !line.StartsWith("</pre>"))
            {
                index = AppendLineNumber(
                    line,
                    result);
            }

            for (int i = index; i < characters.Count; i++)
            {
                if (characters[i] == ' ')
                {
                    result.Append("&nbsp;");
                }
                else
                {
                    result.Append(line.Substring(i));
                    break;
                }
            }

            return result.ToString();
        }

        private int AppendLineNumber(
            string line,
            StringBuilder result)
        {
            int index =
                line.IndexOf(SpanEndTag)
                + SpanEndTag.Length;

            string lineNumberHtml =
                line.Substring(0, index);

            int endTextIndex =
                lineNumberHtml.IndexOf(SpanEndTag);

            int startTextIndex =
                lineNumberHtml.LastIndexOf(
                    ">",
                    endTextIndex)
                + 1;

            lineNumberHtml =
                lineNumberHtml.Substring(
                    0,
                    startTextIndex)
                + lineNumberHtml
                    .Substring(
                        startTextIndex,
                        endTextIndex - startTextIndex)
                    .Replace(" ", "&nbsp;")
                + lineNumberHtml.Substring(
                    endTextIndex);

            result.Append(lineNumberHtml);

            return index;
        }
    }
}