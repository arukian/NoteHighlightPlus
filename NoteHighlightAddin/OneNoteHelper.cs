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
    }


}