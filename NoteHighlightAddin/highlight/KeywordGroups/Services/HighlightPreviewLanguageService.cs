using GenerateHighlightContent.LanguageDefinitions;
using NoteHighlightAddin.Highlighting.KeywordGroups.Writers;
using System;
using System.IO;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Services
{
    /// <summary>
    /// Creates isolated temporary .lang files for the future live preview.
    /// It does not modify the original language definition.
    /// </summary>
    public sealed class HighlightPreviewLanguageService
        : IHighlightPreviewLanguageService
    {
        private const string PreviewFolderName =
            "NoteHighlight2016";

        private const string PreviewLanguageFolderName =
            "PreviewLanguages";

        private readonly HighlightLanguageMapper mapper;
        private readonly ILanguageDefinitionWriter writer;
        private readonly string previewFolder;

        private string currentPreviewFilePath;
        private bool disposed;

        public HighlightPreviewLanguageService()
            : this(
                new HighlightLanguageMapper(),
                new HighlightLanguageDefinitionWriter(),
                CreateDefaultPreviewFolder())
        {
        }

        public HighlightPreviewLanguageService(
            HighlightLanguageMapper mapper,
            ILanguageDefinitionWriter writer,
            string previewFolder)
        {
            this.mapper =
                mapper ?? throw new ArgumentNullException(
                    nameof(mapper));

            this.writer =
                writer ?? throw new ArgumentNullException(
                    nameof(writer));

            if (string.IsNullOrWhiteSpace(previewFolder))
            {
                throw new ArgumentException(
                    "The preview folder cannot be empty.",
                    nameof(previewFolder));
            }

            this.previewFolder =
                previewFolder;
        }

        public string GeneratePreviewLanguage(
            EditableLanguageConfiguration configuration)
        {
            ThrowIfDisposed();

            ValidateConfiguration(
                configuration);

            EnsurePreviewFolderExists();

            Cleanup();

            HighlightLanguageDefinition definition =
                mapper.ToLanguageDefinition(
                    configuration);

            string normalizedLanguageName =
                NormalizeLanguageName(
                    configuration.Language);

            string previewFileName =
                normalizedLanguageName
                + ".preview."
                + Guid.NewGuid().ToString("N")
                + ".lang";

            string previewFilePath =
                Path.Combine(
                    previewFolder,
                    previewFileName);

            writer.Write(
                definition,
                previewFilePath);

            currentPreviewFilePath =
                previewFilePath;

            return previewFilePath;
        }

        public void Cleanup()
        {
            if (disposed)
            {
                return;
            }

            DeleteCurrentPreviewFile();

            if (!Directory.Exists(previewFolder))
            {
                return;
            }

            string[] previewFiles;

            try
            {
                previewFiles =
                    Directory.GetFiles(
                        previewFolder,
                        "*.preview.*.lang",
                        SearchOption.TopDirectoryOnly);
            }
            catch (IOException)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            foreach (string previewFile in previewFiles)
            {
                TryDeleteFile(
                    previewFile);
            }

            TryDeletePreviewFolderWhenEmpty();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Cleanup();

            disposed =
                true;

            GC.SuppressFinalize(
                this);
        }

        private static string CreateDefaultPreviewFolder()
        {
            return Path.Combine(
                Path.GetTempPath(),
                PreviewFolderName,
                PreviewLanguageFolderName);
        }

        private void EnsurePreviewFolderExists()
        {
            if (!Directory.Exists(previewFolder))
            {
                Directory.CreateDirectory(
                    previewFolder);
            }
        }

        private void DeleteCurrentPreviewFile()
        {
            if (string.IsNullOrWhiteSpace(
                currentPreviewFilePath))
            {
                return;
            }

            TryDeleteFile(
                currentPreviewFilePath);

            currentPreviewFilePath =
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
                // A preview process may still be using the file.
                // The next cleanup attempt can remove it.
            }
            catch (UnauthorizedAccessException)
            {
                // Cleanup must not interrupt the settings form.
            }
        }

        private void TryDeletePreviewFolderWhenEmpty()
        {
            try
            {
                if (Directory.Exists(previewFolder) &&
                    Directory.GetFileSystemEntries(
                        previewFolder).Length == 0)
                {
                    Directory.Delete(
                        previewFolder);
                }
            }
            catch (IOException)
            {
                // The folder can remain until the next cleanup attempt.
            }
            catch (UnauthorizedAccessException)
            {
                // Cleanup must not interrupt the settings form.
            }
        }

        private static string NormalizeLanguageName(
            string language)
        {
            string normalizedLanguageName =
                Path.GetFileNameWithoutExtension(
                    language.Trim());

            if (string.IsNullOrWhiteSpace(
                normalizedLanguageName))
            {
                return "language";
            }

            foreach (char invalidCharacter
                in Path.GetInvalidFileNameChars())
            {
                normalizedLanguageName =
                    normalizedLanguageName.Replace(
                        invalidCharacter,
                        '_');
            }

            return normalizedLanguageName;
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

            if (configuration.Groups == null)
            {
                throw new InvalidOperationException(
                    "The editable language configuration has no group collection.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    nameof(HighlightPreviewLanguageService));
            }
        }
    }
}