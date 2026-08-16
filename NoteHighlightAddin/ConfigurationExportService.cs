using Infrastructure.Core;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace NoteHighlightAddin
{
    /// <summary>
    /// Creates a portable backup of the user-editable NoteHighlight+ configuration.
    /// Executables, DLLs, WebView2 data and theme reset baselines are intentionally excluded.
    /// </summary>
    internal sealed class ConfigurationExportService
    {
        private const string ApplicationFolderName = "NoteHighlightPlus";
        private const string ThemePreferenceFileName = "last-theme.txt";

        public void Export(string destinationZipPath)
        {
            if (string.IsNullOrWhiteSpace(destinationZipPath))
            {
                throw new ArgumentException(
                    "The destination ZIP path cannot be empty.",
                    nameof(destinationZipPath));
            }

            string destinationDirectory =
                Path.GetDirectoryName(destinationZipPath);

            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            if (File.Exists(destinationZipPath))
            {
                File.Delete(destinationZipPath);
            }

            using (FileStream stream =
                new FileStream(
                    destinationZipPath,
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.None))
            using (ZipArchive archive =
                new ZipArchive(
                    stream,
                    ZipArchiveMode.Create,
                    leaveOpen: false))
            {
                AddDirectory(
                    archive,
                    PathManager.ThemesFolder,
                    "themes");

                AddDirectory(
                    archive,
                    PathManager.LanguagesFolder,
                    "langDefs");

                AddThemePreference(
                    archive);

                AddManifest(
                    archive);
            }
        }

        private static void AddDirectory(
            ZipArchive archive,
            string sourceDirectory,
            string archiveDirectory)
        {
            if (archive == null)
            {
                throw new ArgumentNullException(nameof(archive));
            }

            if (!Directory.Exists(sourceDirectory))
            {
                return;
            }

            foreach (string filePath in
                Directory.GetFiles(
                    sourceDirectory,
                    "*",
                    SearchOption.AllDirectories))
            {
                string relativePath =
                    filePath.Substring(sourceDirectory.Length)
                        .TrimStart(
                            Path.DirectorySeparatorChar,
                            Path.AltDirectorySeparatorChar);

                string entryName =
                    CombineArchivePath(
                        archiveDirectory,
                        relativePath);

                ZipArchiveEntry entry =
                    archive.CreateEntry(
                        entryName,
                        CompressionLevel.Optimal);

                using (Stream input =
                    File.OpenRead(filePath))
                using (Stream output =
                    entry.Open())
                {
                    input.CopyTo(output);
                }
            }
        }

        private static void AddThemePreference(
            ZipArchive archive)
        {
            string preferencePath =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData),
                    ApplicationFolderName,
                    ThemePreferenceFileName);

            if (!File.Exists(preferencePath))
            {
                return;
            }

            ZipArchiveEntry entry =
                archive.CreateEntry(
                    "preferences/" + ThemePreferenceFileName,
                    CompressionLevel.Optimal);

            using (Stream input =
                File.OpenRead(preferencePath))
            using (Stream output =
                entry.Open())
            {
                input.CopyTo(output);
            }
        }

        private static void AddManifest(
            ZipArchive archive)
        {
            ZipArchiveEntry entry =
                archive.CreateEntry(
                    "NoteHighlightPlus-backup.txt",
                    CompressionLevel.Optimal);

            using (StreamWriter writer =
                new StreamWriter(
                    entry.Open(),
                    new UTF8Encoding(false)))
            {
                writer.WriteLine("NoteHighlight+ configuration backup");
                writer.WriteLine("FormatVersion=1");
                writer.WriteLine(
                    "CreatedUtc=" +
                    DateTime.UtcNow.ToString("o"));
                writer.WriteLine();
                writer.WriteLine("Contents:");
                writer.WriteLine("- themes/: Highlight theme files");
                writer.WriteLine("- langDefs/: language definitions and group metadata");
                writer.WriteLine("- preferences/: portable UI preferences when available");
            }
        }

        private static string CombineArchivePath(
            string first,
            string second)
        {
            string normalizedSecond =
                (second ?? string.Empty)
                    .Replace('\\', '/');

            return
                (first ?? string.Empty).TrimEnd('/') +
                "/" +
                normalizedSecond.TrimStart('/');
        }
    }
}
