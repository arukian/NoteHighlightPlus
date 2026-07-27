using GenerateHighlightContent;
using Infrastructure.Core;
using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.KeywordGroups.Services;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NoteHighlightAddin.Highlighting.Preview.Services
{
    /// <summary>
    /// Generates HTML previews using temporary Highlight
    /// language definitions.
    /// </summary>
    [ComVisible(false)]
    public sealed class PreviewHtmlService
        : IPreviewHtmlService
    {
        private const string LanguageDefinitionsFolderName =
            "langDefs";

        private readonly IHighlightPreviewLanguageService
            previewLanguageService;

        private readonly IGenerateHighLight
            generateHighLight;

        private string currentInstalledLanguagePath;
        private string currentHtmlPath;
        private bool disposed;

        public PreviewHtmlService()
            : this(
                new HighlightPreviewLanguageService(),
                new GenerateHighLight())
        {
        }

        public PreviewHtmlService(
            IHighlightPreviewLanguageService previewLanguageService,
            IGenerateHighLight generateHighLight)
        {
            this.previewLanguageService =
                previewLanguageService
                ?? throw new ArgumentNullException(
                    nameof(previewLanguageService));

            this.generateHighLight =
                generateHighLight
                ?? throw new ArgumentNullException(
                    nameof(generateHighLight));
        }

        public string GeneratePreviewHtml(
            EditableLanguageConfiguration configuration,
            HighLightParameter parameter)
        {
            ThrowIfDisposed();

            ValidateConfiguration(
                configuration);

            ValidateParameter(
                parameter);

            CleanupGeneratedFiles();

            string previewLanguagePath =
                previewLanguageService.GeneratePreviewLanguage(
                    configuration);

            string previewLanguageName =
                CreatePreviewLanguageName(
                    configuration.Language);

            currentInstalledLanguagePath =
                InstallPreviewLanguage(
                    previewLanguagePath,
                    previewLanguageName);

            HighLightParameter previewParameter =
                CreatePreviewParameter(
                    parameter,
                    previewLanguageName);

            try
            {
                currentHtmlPath =
                    generateHighLight.GenerateHighLightCode(
                        previewParameter);

                return currentHtmlPath;
            }
            finally
            {
                DeleteInstalledLanguage();
            }
        }

        public void Cleanup()
        {
            if (disposed)
            {
                return;
            }

            CleanupGeneratedFiles();

            previewLanguageService.Cleanup();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Cleanup();

            previewLanguageService.Dispose();

            disposed =
                true;

            GC.SuppressFinalize(
                this);
        }

        private static string InstallPreviewLanguage(
            string sourceLanguagePath,
            string previewLanguageName)
        {
            if (string.IsNullOrWhiteSpace(
                sourceLanguagePath))
            {
                throw new ArgumentException(
                    "The preview language path cannot be empty.",
                    nameof(sourceLanguagePath));
            }

            if (!File.Exists(sourceLanguagePath))
            {
                throw new FileNotFoundException(
                    "The generated preview language file was not found.",
                    sourceLanguagePath);
            }

            string languageDefinitionsFolder =
                Path.Combine(
                    PathManager.HighlightFolder,
                    LanguageDefinitionsFolderName);

            Directory.CreateDirectory(
                languageDefinitionsFolder);

            string destinationLanguagePath =
                Path.Combine(
                    languageDefinitionsFolder,
                    previewLanguageName + ".lang");

            File.Copy(
                sourceLanguagePath,
                destinationLanguagePath,
                true);

            return destinationLanguagePath;
        }

        private static HighLightParameter CreatePreviewParameter(
            HighLightParameter source,
            string previewLanguageName)
        {
            return new HighLightParameter
            {
                Content =
                    source.Content,

                CodeType =
                    previewLanguageName,

                HighLightStyle =
                    source.HighLightStyle,

                ShowLineNumber =
                    source.ShowLineNumber,

                FileName =
                    CreatePreviewSourceFileName(
                        source.FileName),

                HighlightColor =
                    source.HighlightColor,

                Font =
                    source.Font,

                FontSize =
                    source.FontSize
            };
        }

        private static string CreatePreviewSourceFileName(
            string originalFileName)
        {
            string extension =
                Path.GetExtension(
                    originalFileName);

            if (string.IsNullOrWhiteSpace(extension))
            {
                extension =
                    ".txt";
            }

            return
                "notehighlight_preview_"
                + Guid.NewGuid().ToString("N")
                + extension;
        }

        private static string CreatePreviewLanguageName(
            string language)
        {
            string normalizedName =
                Path.GetFileNameWithoutExtension(
                    language.Trim());

            if (string.IsNullOrWhiteSpace(
                normalizedName))
            {
                normalizedName =
                    "language";
            }

            foreach (char invalidCharacter
                in Path.GetInvalidFileNameChars())
            {
                normalizedName =
                    normalizedName.Replace(
                        invalidCharacter,
                        '_');
            }

            normalizedName =
                normalizedName.Replace(
                    '.',
                    '_');

            return
                "preview_"
                + normalizedName
                + "_"
                + Guid.NewGuid().ToString("N");
        }

        private void CleanupGeneratedFiles()
        {
            DeleteInstalledLanguage();
            DeleteCurrentHtml();
        }

        private void DeleteInstalledLanguage()
        {
            if (string.IsNullOrWhiteSpace(
                currentInstalledLanguagePath))
            {
                return;
            }

            TryDeleteFile(
                currentInstalledLanguagePath);

            currentInstalledLanguagePath =
                null;
        }

        private void DeleteCurrentHtml()
        {
            if (string.IsNullOrWhiteSpace(
                currentHtmlPath))
            {
                return;
            }

            TryDeleteFile(
                currentHtmlPath);

            currentHtmlPath =
                null;
        }

        private static void TryDeleteFile(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(
                        filePath);
                }
            }
            catch (IOException)
            {
                // WebView2 or Highlight may still be using the file.
                // A later cleanup attempt can remove it.
            }
            catch (UnauthorizedAccessException)
            {
                // Preview cleanup must not interrupt SettingsForm.
            }
        }

        private static void ValidateConfiguration(
            EditableLanguageConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(
                    nameof(configuration));
            }

            if (string.IsNullOrWhiteSpace(
                configuration.Language))
            {
                throw new InvalidOperationException(
                    "The editable language configuration has no language name.");
            }
        }

        private static void ValidateParameter(
            HighLightParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(
                    nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(
                parameter.FileName))
            {
                throw new ArgumentException(
                    "The preview source file name cannot be empty.",
                    nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(
                parameter.HighLightStyle))
            {
                throw new ArgumentException(
                    "The preview theme cannot be empty.",
                    nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(
                parameter.Font))
            {
                throw new ArgumentException(
                    "The preview font cannot be empty.",
                    nameof(parameter));
            }

            if (parameter.FontSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(parameter),
                    "The preview font size must be greater than zero.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    nameof(PreviewHtmlService));
            }
        }
    }
}