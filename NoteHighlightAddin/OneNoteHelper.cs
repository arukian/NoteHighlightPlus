using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace NoteHighlightAddin
{
    public static class OneNoteHelper
    {
        public static XElement GetOutline(
            string pageXml,
            XNamespace ns)
        {
            if (string.IsNullOrWhiteSpace(pageXml))
            {
                throw new ArgumentException(
                    "El XML de la página no puede estar vacío.",
                    nameof(pageXml));
            }

            return XDocument
                .Parse(pageXml)
                .Descendants(ns + "Outline")
                .FirstOrDefault(
                    node =>
                        node.Attribute("selected") != null &&
                        (
                            node.Attribute("selected").Value == "all" ||
                            node.Attribute("selected").Value == "partial"
                        ));
        }

        public static string[] GetMousePointPosition(
             string pageXml, XNamespace ns)
        {
            if (string.IsNullOrWhiteSpace(pageXml))
            {
                throw new ArgumentException(
                    "El XML de la página no puede estar vacío.",
                    nameof(pageXml));
            }

            XElement node = XDocument
                .Parse(pageXml)
                .Descendants(ns + "Outline")
                .FirstOrDefault(
                    element =>
                        element.Attribute("selected") != null &&
                        element.Attribute("selected").Value == "partial");

            if (node == null)
            {
                return null;
            }

            XElement position = node
                .Descendants(ns + "Position")
                .FirstOrDefault();

            if (position == null)
            {
                return null;
            }

            XAttribute xAttribute = position.Attribute("x");
            XAttribute yAttribute = position.Attribute("y");

            if (xAttribute == null || yAttribute == null)
            {
                return null;
            }

            return new[]
            {
                xAttribute.Value,
                yAttribute.Value
            };
        }

        public static bool IsSelectedTextInline(
    string pageXml,
    XNamespace ns)
        {
            if (string.IsNullOrWhiteSpace(pageXml))
            {
                throw new ArgumentException(
                    "El XML de la página no puede estar vacío.",
                    nameof(pageXml));
            }

            XElement outline = XDocument
                .Parse(pageXml)
                .Descendants(ns + "Outline")
                .FirstOrDefault(
                    node =>
                        node.Attribute("selected") != null &&
                        (
                            node.Attribute("selected").Value == "all" ||
                            node.Attribute("selected").Value == "partial"
                        ));

            if (outline == null)
            {
                return false;
            }

            XElement table = outline
                .Descendants(ns + "Table")
                .FirstOrDefault(
                    node =>
                        node.Attribute("selected") != null &&
                        (
                            node.Attribute("selected").Value == "all" ||
                            node.Attribute("selected").Value == "partial"
                        ));

            if (table != null)
            {
                return false;
            }

            foreach (XElement oeNode in outline.Descendants(ns + "OE"))
            {
                bool hasSelectedText = oeNode
                    .Descendants(ns + "T")
                    .Any(
                        node =>
                            node.Attribute("selected") != null &&
                            node.Attribute("selected").Value == "all");

                bool hasUnselectedText = oeNode
                    .Descendants(ns + "T")
                    .Any(
                        node =>
                            node.Attribute("selected") == null ||
                            node.Attribute("selected").Value == "none");

                if (hasSelectedText && hasUnselectedText)
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetSelectedText(
    string pageXml,
    XNamespace ns,
    out bool selectedTextFormated)
        {
            if (string.IsNullOrWhiteSpace(pageXml))
            {
                throw new ArgumentException(
                    "El XML de la página no puede estar vacío.",
                    nameof(pageXml));
            }

            XElement outline = XDocument
                .Parse(pageXml)
                .Descendants(ns + "Outline")
                .FirstOrDefault(
                    node =>
                        node.Attribute("selected") != null &&
                        (
                            node.Attribute("selected").Value == "all" ||
                            node.Attribute("selected").Value == "partial"
                        ));

            StringBuilder result = new StringBuilder();

            selectedTextFormated = false;

            if (outline == null)
            {
                return string.Empty;
            }

            XElement table = outline
                .Descendants(ns + "Table")
                .FirstOrDefault(
                    node =>
                        node.Attribute("selected") != null &&
                        (
                            node.Attribute("selected").Value == "all" ||
                            node.Attribute("selected").Value == "partial"
                        ));

            IEnumerable<XElement> textNodes;

            if (table == null)
            {
                textNodes = outline
                    .Descendants(ns + "T")
                    .Where(
                        node =>
                            node.Attribute("selected") != null &&
                            node.Attribute("selected").Value == "all");
            }
            else
            {
                XElement lastCell = table
                    .Descendants(ns + "Cell")
                    .LastOrDefault();

                if (lastCell == null)
                {
                    return string.Empty;
                }

                textNodes = lastCell
                    .Descendants(ns + "T")
                    .Where(
                        node =>
                            node.Attribute("selected") != null &&
                            node.Attribute("selected").Value == "all");

                selectedTextFormated = true;
            }

            int initialTabCount = -1;

            foreach (XElement line in textNodes)
            {
                var htmlDocument =
                    new HtmlAgilityPack.HtmlDocument();

                htmlDocument.LoadHtml(line.Value);

                if (initialTabCount == -1)
                {
                    initialTabCount = line
                        .Ancestors()
                        .Elements(ns + "T")
                        .Count();
                }

                int tabCount =
                    line.Ancestors()
                        .Elements(ns + "T")
                        .Count()
                    - initialTabCount;

                if (tabCount < 0)
                {
                    tabCount = 0;
                }

                string decodedText = HttpUtility.HtmlDecode(
                    htmlDocument.DocumentNode.InnerText);

                result.AppendLine(
                    new string('\t', tabCount) + decodedText);
            }

            return result
                .ToString()
                .TrimEnd('\r', '\n');
        }
    }


}