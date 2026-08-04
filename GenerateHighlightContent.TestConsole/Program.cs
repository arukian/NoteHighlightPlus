using GenerateHighlightContent;
using Infrastructure.Core;
using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.KeywordGroups.Services;
using NoteHighlightAddin.Highlighting.Preview.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using NoteHighlightAddin;
using System.Windows.Forms;
using NoteHighlightAddin.Highlighting.Themes;

namespace GenerateHighlightContent.TestConsole
{
    internal static class Program
    {

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(
                new SettingsForm());
        }




        private static void PrintAssemblyLocations()
        {
            Console.WriteLine(
                "=== Assembly locations ===");

            Console.WriteLine(
                "GenerateHighlightContent:");

            Console.WriteLine(
                typeof(GenerateHighLight)
                    .Assembly.Location);

            Console.WriteLine();

            Console.WriteLine(
                "Infrastructure.Core:");

            Console.WriteLine(
                typeof(PathManager)
                    .Assembly.Location);

            Console.WriteLine();

            Console.WriteLine(
                "NoteHighlightAddin:");

            Console.WriteLine(
                typeof(PreviewHtmlService)
                    .Assembly.Location);
        }

        private static void PrintRequiredPaths()
        {
            Console.WriteLine();

            Console.WriteLine(
                "Highlight folder:");

            Console.WriteLine(
                PathManager.HighlightFolder);

            Console.WriteLine();

            Console.WriteLine(
                "Languages folder:");

            Console.WriteLine(
                PathManager.LanguagesFolder);

            Console.WriteLine();

            Console.WriteLine(
                "python.lang exists: "
                + File.Exists(
                    Path.Combine(
                        PathManager.LanguagesFolder,
                        "python.lang")));

            Console.WriteLine(
                "shinx.theme exists: "
                + File.Exists(
                    Path.Combine(
                        PathManager.ThemesFolder,
                        "shinx.theme")));
        }

        private static EditableLanguageConfiguration
            LoadPythonConfiguration()
        {
            var languageEditorService =
                new LanguageEditorService();

            return languageEditorService.Load(
                "python");
        }

        private static void PrintConfigurationSummary(
            EditableLanguageConfiguration configuration)
        {
            Console.WriteLine();

            Console.WriteLine(
                "=== Editable configuration ===");

            Console.WriteLine(
                "Language: "
                + configuration.Language);

            Console.WriteLine(
                "Description: "
                + configuration.Description);

            Console.WriteLine(
                "Case sensitive: "
                + configuration.CaseSensitive);

            Console.WriteLine(
                "Groups: "
                + configuration.Groups.Count);
        }

        private static HighLightParameter
            CreatePreviewParameter()
        {
            return new HighLightParameter
            {
                FileName =
                    "notehighlight_preview.py",

                Content =
                    "class PreviewExample:\r\n" +
                    "    def __init__(self, value):\r\n" +
                    "        self.value = value\r\n" +
                    "\r\n" +
                    "    def print_value(self):\r\n" +
                    "        if self.value is not None:\r\n" +
                    "            print(self.value)\r\n" +
                    "\r\n" +
                    "example = PreviewExample(True)\r\n" +
                    "example.print_value()\r\n",

                CodeType =
                    "python",

                HighLightStyle =
                    "shinx",

                ShowLineNumber =
                    true,

                HighlightColor =
                    Color.Transparent,

                Font =
                    "Consolas",

                FontSize =
                    10
            };
        }



        private static string GeneratePreview(
            EditableLanguageConfiguration configuration,
            HighLightParameter parameter)
        {
            IPreviewHtmlService previewService =
                new PreviewHtmlService();

            return previewService.GeneratePreviewHtml(
                configuration,
                parameter);
        }

        private static void ValidateHtml(
            string htmlPath)
        {
            if (string.IsNullOrWhiteSpace(
                htmlPath))
            {
                throw new InvalidOperationException(
                    "The preview service returned an empty path.");
            }

            if (!File.Exists(htmlPath))
            {
                throw new FileNotFoundException(
                    "The generated preview HTML was not found.",
                    htmlPath);
            }

            FileInfo htmlFile =
                new FileInfo(
                    htmlPath);

            if (htmlFile.Length == 0)
            {
                throw new InvalidOperationException(
                    "The generated preview HTML is empty.");
            }
        }
    }
}