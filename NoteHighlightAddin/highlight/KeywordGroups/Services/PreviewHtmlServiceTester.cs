using GenerateHighlightContent;
using NoteHighlightAddin.Highlighting.KeywordGroups;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace NoteHighlightAddin.Highlighting.Preview.Services
{
    /// <summary>
    /// Temporary tester used to validate the complete preview flow.
    /// Remove it after WebView2 integration.
    /// </summary>
    [ComVisible(false)]
    internal static class PreviewHtmlServiceTester
    {
        private static IPreviewHtmlService _previewService;

        public static string GeneratePreview(
            EditableLanguageConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(
                    nameof(configuration));
            }

            EnsurePreviewService();

            HighLightParameter parameter =
                CreatePreviewParameter(
                    configuration);

            string htmlPath =
                _previewService.GeneratePreviewHtml(
                    configuration,
                    parameter);

            ValidateHtmlFile(
                htmlPath);

            return htmlPath;
        }

        public static void GenerateAndOpenPreview(
            EditableLanguageConfiguration configuration)
        {
            string htmlPath =
                GeneratePreview(
                    configuration);

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = htmlPath,
                    UseShellExecute = true
                });
        }

        public static void Cleanup()
        {
            if (_previewService == null)
            {
                return;
            }

            _previewService.Dispose();
            _previewService = null;
        }

        private static void EnsurePreviewService()
        {
            if (_previewService != null)
            {
                return;
            }

            _previewService =
                new PreviewHtmlService();
        }

        private static HighLightParameter CreatePreviewParameter(
            EditableLanguageConfiguration configuration)
        {
            return new HighLightParameter
            {
                FileName =
                    "notehighlight_preview.py",

                Content =
                    CreateSampleCode(),

                CodeType =
                    configuration.Language,

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

        private static string CreateSampleCode()
        {
            return
                "class PreviewExample:\r\n" +
                "    def __init__(self, value):\r\n" +
                "        self.value = value\r\n" +
                "\r\n" +
                "    def print_value(self):\r\n" +
                "        if self.value is not None:\r\n" +
                "            print(self.value)\r\n" +
                "\r\n" +
                "example = PreviewExample(True)\r\n" +
                "example.print_value()\r\n";
        }

        private static void ValidateHtmlFile(
            string htmlPath)
        {
            if (string.IsNullOrWhiteSpace(
                htmlPath))
            {
                throw new InvalidOperationException(
                    "The preview service returned an empty HTML path.");
            }

            if (!File.Exists(htmlPath))
            {
                throw new FileNotFoundException(
                    "The preview HTML file was not generated.",
                    htmlPath);
            }

            FileInfo htmlFile =
                new FileInfo(
                    htmlPath);

            if (htmlFile.Length == 0)
            {
                throw new InvalidOperationException(
                    "The generated preview HTML file is empty.");
            }
        }
    }
}