using NoteHighlightAddin.Highlighting.KeywordGroups.Services;
using System;
using System.IO;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Testing
{
    /// <summary>
    /// Temporary helper used to verify preview .lang generation before
    /// integrating highlight.exe and the preview UI.
    /// </summary>
    public static class HighlightPreviewLanguageServiceTester
    {
        public static string Generate(
            EditableLanguageConfiguration configuration)
        {
            using (var service =
                new HighlightPreviewLanguageService())
            {
                string previewFilePath =
                    service.GeneratePreviewLanguage(
                        configuration);

                if (!File.Exists(previewFilePath))
                {
                    throw new InvalidOperationException(
                        "The preview language file was not generated.");
                }

                // Copy it outside the service-owned folder so it remains
                // available for manual inspection after Dispose().
                string inspectionFilePath =
                    Path.Combine(
                        Path.GetTempPath(),
                        "NoteHighlight-preview-language-test.lang");

                File.Copy(
                    previewFilePath,
                    inspectionFilePath,
                    true);

                return inspectionFilePath;
            }
        }
    }
}