using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Office.Interop.OneNote;
using OneNoteApplication =
    Microsoft.Office.Interop.OneNote.Application;

namespace NoteHighlightAddin
{
    public class OneNoteService
    {
        private readonly OneNoteApplication _application;

        public XNamespace Namespace { get; private set; }

        public OneNoteService(OneNoteApplication application)
        {
            _application = application
                ?? throw new ArgumentNullException(nameof(application));
        }

        public XElement GetCurrentPageNode()
        {
            string hierarchyXml;

            _application.GetHierarchy(
                null,
                HierarchyScope.hsPages,
                out hierarchyXml,
                XMLSchema.xs2013);

            XDocument document = XDocument.Parse(hierarchyXml);

            Namespace = document.Root.Name.Namespace;

            return document
                .Descendants(Namespace + "Page")
                .FirstOrDefault(
                    page =>
                        page.Attribute("isCurrentlyViewed") != null &&
                        page.Attribute("isCurrentlyViewed").Value == "true");
        }

        public string GetPageXml(string pageId)
        {
            if (string.IsNullOrWhiteSpace(pageId))
            {
                throw new ArgumentException(
                    "El ID de la página no puede estar vacío.",
                    nameof(pageId));
            }

            string pageXml;

            _application.GetPageContent(
                pageId,
                out pageXml,
                PageInfo.piSelection);

            return pageXml;
        }

        public void UpdatePageContent(XDocument page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            _application.UpdatePageContent(
                page.ToString(),
                DateTime.MinValue);
        }
    }
}