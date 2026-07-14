using System;
using System.Linq;
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
    }
}