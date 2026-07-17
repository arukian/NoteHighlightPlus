using System;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using GenerateHighlightContent;
using NoteHighLightForm;

namespace NoteHighlightAddin
{
    public class HtmlInserter
    {
        private readonly XNamespace ns;

        public HtmlInserter(XNamespace xmlNamespace)
        {
            ns = xmlNamespace;
        }

        public XElement CreateOutline(string[] position, XElement children)
        {
            XElement outline = new XElement(ns + "Outline");

            if (position != null && position.Length == 2)
            {
                XElement pos = new XElement(ns + "Position");

                pos.Add(new XAttribute("x", position[0]));
                pos.Add(new XAttribute("y", position[1]));

                outline.Add(pos);

                XElement size = new XElement(ns + "Size");

                size.Add(new XAttribute("width", "1600"));
                size.Add(new XAttribute("height", "200"));

                outline.Add(size);
            }

            outline.Add(children);

            return outline;
        }

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

        public XElement PrepareFormatedContent(string htmlContent, HighLightParameter parameters, HighLightSection config, bool isInline, bool darkMode, bool showTableBorder)
        {
            XElement children = new XElement(ns + "OEChildren");

            XElement table = new XElement(ns + "Table");

            table.Add(
                new XAttribute(
                    "bordersVisible",
                    showTableBorder));

            XElement columns = new XElement(ns + "Columns");

            XElement column1 = new XElement(ns + "Column");

            column1.Add(new XAttribute("index", "0"));
            column1.Add(new XAttribute("width", "40"));

            if (parameters.ShowLineNumber && !isInline)
            {
                columns.Add(column1);
            }

            XElement column2 = new XElement(ns + "Column");

            if (parameters.ShowLineNumber && !isInline)
            {
                column2.Add(new XAttribute("index", "1"));
            }
            else
            {
                column2.Add(new XAttribute("index", "0"));
            }

            column2.Add(new XAttribute("width", "1400"));

            columns.Add(column2);
            table.Add(columns);

            Color color = parameters.HighlightColor;

            string colorString = color.A == 0
                ? "none"
                : string.Format(
                    "#{0:X2}{1:X2}{2:X2}",
                    color.R,
                    color.G,
                    color.B);

            XElement row = new XElement(ns + "Row");

            XElement cell1 = new XElement(ns + "Cell");
            cell1.Add(new XAttribute("shadingColor", colorString));

            XElement cell2 = new XElement(ns + "Cell");
            cell2.Add(new XAttribute("shadingColor", colorString));

            string defaultStyle = "";

            var arrayLine = htmlContent.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None);

            foreach (var it in arrayLine)
            {
                string item = it;

                if (item.StartsWith("<pre"))
                {
                    defaultStyle = item.Substring(
                        0,
                        item.IndexOf(">") + 1);

                    // Sets language to Latin to disable spell check.
                    defaultStyle = defaultStyle.Insert(
                        defaultStyle.Length - 1,
                        " lang=la");

                    if (darkMode)
                    {
                        // Remove background-color so the text has
                        // the correct contrast in dark mode.
                        int backgroundColorIndex =
                            defaultStyle.IndexOf("background-color");

                        defaultStyle = defaultStyle.Remove(
                            backgroundColorIndex,
                            defaultStyle.IndexOf(
                                ';',
                                backgroundColorIndex) -
                            backgroundColorIndex +
                            1);
                    }

                    item = item.Substring(item.IndexOf(">") + 1);
                }

                if (item == "</pre>")
                {
                    continue;
                }

                string itemNr = "";
                string itemLine = "";

                if (parameters.ShowLineNumber)
                {
                    if (item.Contains("</span>"))
                    {
                        int index = item.IndexOf("</span>");

                        itemNr = item.Substring(
                            0,
                            index + "</span>".Length);

                        itemLine = item.Substring(index);
                    }
                    else
                    {
                        itemNr = "";
                        itemLine = item;
                    }

                    string lineNumberHtml;

                    if (string.IsNullOrEmpty(config.LineNrReplaceCh))
                    {
                        lineNumberHtml =
                            defaultStyle +
                            itemNr.Replace("&apos;", "'") +
                            "</pre>";
                    }
                    else
                    {
                        lineNumberHtml =
                            defaultStyle +
                            config.LineNrReplaceCh.PadLeft(5) +
                            "</pre>";
                    }

                    XElement oeElement =
                        new XElement(
                            ns + "OE",
                            new XElement(
                                ns + "T",
                                new XCData(lineNumberHtml)));

                    if (ContainsAsianCharacter(itemLine))
                    {
                        oeElement.Add(
                            new XAttribute(
                                "spaceBefore",
                                config.AsianBeforeSpace));

                        oeElement.Add(
                            new XAttribute(
                                "spaceAfter",
                                config.AsianAfterSpace));
                    }

                    cell1.Add(
                        new XElement(
                            ns + "OEChildren",
                            oeElement));
                }
                else
                {
                    itemLine = item;
                }

                string formattedLine =
                    defaultStyle +
                    itemLine.Replace("&apos;", "'") +
                    "</pre>";

                cell2.Add(
                    new XElement(
                        ns + "OEChildren",
                        new XElement(
                            ns + "OE",
                            new XElement(
                                ns + "T",
                                new XCData(formattedLine)))));
            }

            if (parameters.ShowLineNumber && !isInline)
            {
                row.Add(cell1);
            }

            row.Add(cell2);
            table.Add(row);

            children.Add(
                new XElement(
                    ns + "OE",
                    table));

            return children;
        }

        public XDocument InsertHighLightCode(string htmlContent, string[] position, HighLightParameter parameters, XElement outline, HighLightSection config, bool selectedTextFormated,
            bool isInline, bool darkMode,  bool showTableBorder)
        {
            XElement children = PrepareFormatedContent(
                htmlContent,
                parameters,
                config,
                isInline,
                darkMode,
                showTableBorder);

            if (outline == null)
            {
                XElement newOutline = CreateOutline(position, children);

                return CreatePageDocument(newOutline);
            }

            UpdateExistingOutline(
                outline,
                children,
                selectedTextFormated,
                isInline);

            return outline.Parent.Document;
        }

        private void UpdateExistingOutline(
        XElement outline,
        XElement children,
        bool selectedTextFormated,
        bool isInline)
        {
            // Change outline width.
            XElement size = outline.Element(ns + "Size");

            if (size != null)
            {
                XAttribute width = size.Attribute("width");

                if (width != null)
                {
                    width.Value = "1600";
                }
            }

            if (selectedTextFormated)
            {
                ReplaceFormattedSelection(outline, children);
                return;
            }

            if (isInline)
            {
                ReplaceInlineSelection(outline, children);
                return;
            }

            ReplaceNormalSelection(outline, children);
        }

        private void ReplaceFormattedSelection(
        XElement outline,
        XElement children)
        {
            XElement selectedTable = outline
                .Descendants(ns + "Table")
                .FirstOrDefault(element =>
                    element.Attribute("selected") != null &&
                    (
                        element.Attribute("selected").Value == "all" ||
                        element.Attribute("selected").Value == "partial"
                    ));

            XElement generatedTable = children
                .Descendants(ns + "Table")
                .FirstOrDefault();

            if (selectedTable != null && generatedTable != null)
            {
                selectedTable.ReplaceWith(generatedTable);
            }
        }

        private void ReplaceInlineSelection(
        XElement outline,
        XElement children)
        {
            var selectedOeNodes = outline
                .Descendants(ns + "OE")
                .Where(oeNode =>
                    oeNode
                        .Descendants(ns + "T")
                        .Any(textNode =>
                            textNode.Attribute("selected") != null &&
                            textNode.Attribute("selected").Value == "all"))
                .ToList();

            var generatedOeNodes = children
                .Descendants(ns + "Table")
                .Descendants(ns + "OEChildren")
                .Descendants(ns + "OE")
                .ToList();

            int generatedIndex = 0;

            foreach (XElement oeNode in selectedOeNodes)
            {
                XElement selectedTextNode = oeNode
                    .Descendants(ns + "T")
                    .FirstOrDefault(textNode =>
                        textNode.Attribute("selected") != null &&
                        textNode.Attribute("selected").Value == "all");

                if (selectedTextNode == null)
                {
                    continue;
                }

                if (generatedIndex >= generatedOeNodes.Count)
                {
                    break;
                }

                var generatedTextNodes = generatedOeNodes[generatedIndex]
                    .Descendants(ns + "T")
                    .ToList();

                selectedTextNode.ReplaceWith(generatedTextNodes);

                generatedIndex++;
            }

            outline
                .Descendants(ns + "OE")
                .Where(oeNode =>
                    oeNode
                        .Elements(ns + "T")
                        .Any(textNode =>
                            textNode.Attribute("selected") != null &&
                            textNode.Attribute("selected").Value == "all"))
                .Remove();
        }

        private void ReplaceNormalSelection(
    XElement outline,
    XElement children)
        {
            XElement selectedTextNode = outline
                .Descendants(ns + "T")
                .FirstOrDefault(textNode =>
                    textNode.Attribute("selected") != null &&
                    textNode.Attribute("selected").Value == "all");

            XElement generatedTable = children
                .Descendants(ns + "Table")
                .FirstOrDefault();

            if (selectedTextNode != null && generatedTable != null)
            {
                selectedTextNode.ReplaceWith(generatedTable);
            }

            outline
                .Descendants(ns + "OE")
                .Where(oeNode =>
                    oeNode
                        .Elements(ns + "T")
                        .Any(textNode =>
                            textNode.Attribute("selected") != null &&
                            textNode.Attribute("selected").Value == "all"))
                .Remove();

            outline
                .Descendants(ns + "OEChildren")
                .Where(element =>
                    !element.HasElements &&
                    element.Attribute("selected") != null &&
                    element.Attribute("selected").Value == "partial")
                .Remove();
        }

        private XDocument CreatePageDocument(XElement outline)
        {
            XElement page = new XElement(ns + "Page");

            page.Add(outline);

            return new XDocument(page);
        }




    }
}