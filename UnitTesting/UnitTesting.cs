using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using GenerateHighlightContent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoteHighlightAddin;

namespace UnitTesting
{
    [TestClass]
    public class UnitTesting
    {
        private static readonly XNamespace OneNoteNamespace =
            "http://schemas.microsoft.com/office/onenote/2013/onenote";

        [TestMethod]
        public void FormatNewCode()
        {
            string htmlCode = Resource1.HTMLContent1;
            string[] position = { "198.0", "950.3999633789062" };

            HighLightParameter parameters = CreateParameters();
            HighLightSection config = LoadConfiguration();
            HtmlInserter htmlInserter = CreateHtmlInserter();

            XDocument output = htmlInserter.InsertHighLightCode(
                htmlCode,
                position,
                parameters,
                null,
                config,
                false,
                false,
                false,
                false);

            Assert.AreEqual(Resource1.Output1, output.ToString(), false);
        }

        [TestMethod]
        public void FormatSelectedCode_AllSelected()
        {
            XDocument output = FormatSelectedCode(
                Resource1.HTMLContent2,
                Resource1.Page2);

            Assert.AreEqual(Resource1.Output2, output.ToString(), false);
        }

        [TestMethod]
        public void FormatSelectedCode_WordSelected()
        {
            XDocument output = FormatSelectedCode(
                Resource1.HTMLContent5,
                Resource1.Page5);

            Assert.AreEqual(Resource1.Output5, output.ToString(), false);
        }

        [TestMethod]
        public void FormatSelectedCode_LineSelected()
        {
            XDocument output = FormatSelectedCode(
                Resource1.HTMLContent3,
                Resource1.Page3);

            Assert.AreEqual(Resource1.Output3, output.ToString(), false);
        }

        [TestMethod]
        public void FormatSelectedCode_PartialLineSelected()
        {
            XDocument output = FormatSelectedCode(
                Resource1.HTMLContent6,
                Resource1.Page6);

            Assert.AreEqual(Resource1.Output6, output.ToString(), false);
        }

        [TestMethod]
        public void EditFormattedCode_OutlineSelected()
        {
            XDocument output = FormatSelectedCode(
                Resource1.HTMLContent4,
                Resource1.Page4);

            Assert.AreEqual(Resource1.Output4, output.ToString(), false);
        }

        private static XDocument FormatSelectedCode(
            string htmlCode,
            string pageXml)
        {
            HighLightParameter parameters = CreateParameters();
            HighLightSection config = LoadConfiguration();
            HtmlInserter htmlInserter = CreateHtmlInserter();

            XElement outline = XDocument
                .Parse(pageXml)
                .Descendants(OneNoteNamespace + "Outline")
                .FirstOrDefault(element =>
                    element.Attribute("selected") != null &&
                    (
                        element.Attribute("selected").Value == "all" ||
                        element.Attribute("selected").Value == "partial"
                    ));

            bool selectedTextFormated;

            OneNoteHelper.GetSelectedText(
                pageXml,
                OneNoteNamespace,
                out selectedTextFormated);

            bool isInline = OneNoteHelper.IsSelectedTextInline(
                pageXml,
                OneNoteNamespace);

            return htmlInserter.InsertHighLightCode(
                htmlCode,
                null,
                parameters,
                outline,
                config,
                selectedTextFormated,
                isInline,
                false,
                false);
        }

        private static HtmlInserter CreateHtmlInserter()
        {
            return new HtmlInserter(OneNoteNamespace);
        }

        private static HighLightParameter CreateParameters()
        {
            return new HighLightParameter
            {
                ShowLineNumber = true,
                HighlightColor =
                    System.Drawing.Color.FromArgb(240, 240, 240)
            };
        }

        private static HighLightSection LoadConfiguration()
        {
            ExeConfigurationFileMap fileMap =
                new ExeConfigurationFileMap
                {
                    ExeConfigFilename = "Test.config"
                };

            Configuration configuration =
                ConfigurationManager.OpenMappedExeConfiguration(
                    fileMap,
                    ConfigurationUserLevel.None);

            return configuration.GetSection("HighLightSection")
                as HighLightSection;
        }
    }
}
