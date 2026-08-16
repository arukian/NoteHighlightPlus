using Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NoteHighlightAddin
{
    internal sealed class ConfigurationImportPlan
    {
        public ConfigurationImportPlan()
        {
            Entries =
                new List<ConfigurationImportEntry>();
        }

        public IList<ConfigurationImportEntry> Entries
        {
            get;
            private set;
        }

        public int TotalFiles =>
            Entries.Count;

        public int NewFiles =>
            Entries.Count(
                item => !item.WillOverwrite);

        public int ExistingFiles =>
            Entries.Count(
                item => item.WillOverwrite);

        public int ThemeCount =>
            Entries.Count(
                item => item.Category == "Theme");

        public int LanguageFileCount =>
            Entries.Count(
                item => item.Category == "Language");

        public int MetadataFileCount =>
            Entries.Count(
                item => item.Category == "Group metadata");

        public int PreferenceCount =>
            Entries.Count(
                item => item.Category == "Preference");

        public int ConflictCount =>
            ExistingFiles;
    }


    internal sealed class ConfigurationImportEntry
    {
        public string ArchivePath
        {
            get;
            set;
        }

        public string DestinationPath
        {
            get;
            set;
        }

        public string Category
        {
            get;
            set;
        }

        public bool WillOverwrite
        {
            get;
            set;
        }
    }


    internal sealed class ConfigurationImportResult
    {
        public int ImportedFiles
        {
            get;
            set;
        }

        public int SkippedFiles
        {
            get;
            set;
        }

        public bool RibbonConfigurationImported
        {
            get;
            set;
        }
    }


    /// <summary>
    /// Validates and imports NoteHighlight+ configuration backups.
    ///
    /// The caller can choose between:
    /// - overwriteExisting = true: merge and overwrite matching files.
    /// - overwriteExisting = false: import only files that do not exist.
    ///
    /// Files that are not present in the backup are always preserved.
    /// </summary>
    internal sealed class ConfigurationImportService
    {
        private const string ApplicationFolderName =
            "NoteHighlightPlus";

        private const string ThemePreferenceFileName =
            "last-theme.txt";

        private const string ManifestFileName =
            "NoteHighlightPlus-backup.txt";

        private const string SupportedFormatVersion =
            "1";


        public ConfigurationImportPlan Analyze(
            string backupZipPath)
        {
            ValidateBackupPath(
                backupZipPath);

            using (FileStream stream =
                new FileStream(
                    backupZipPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
            using (ZipArchive archive =
                new ZipArchive(
                    stream,
                    ZipArchiveMode.Read,
                    leaveOpen: false))
            {
                ValidateManifest(
                    archive);

                var plan =
                    new ConfigurationImportPlan();

                var destinationPaths =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase);

                foreach (ZipArchiveEntry entry
                    in archive.Entries)
                {
                    if (string.IsNullOrEmpty(
                        entry.Name))
                    {
                        continue;
                    }

                    string normalizedPath =
                        NormalizeArchivePath(
                            entry.FullName);

                    if (string.Equals(
                        normalizedPath,
                        ManifestFileName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ConfigurationImportEntry importEntry =
                        CreateImportEntry(
                            normalizedPath);

                    if (!destinationPaths.Add(
                        importEntry.DestinationPath))
                    {
                        throw new InvalidDataException(
                            "The backup contains duplicate configuration entries.");
                    }

                    importEntry.WillOverwrite =
                        File.Exists(
                            importEntry.DestinationPath);

                    plan.Entries.Add(
                        importEntry);
                }

                if (plan.Entries.Count == 0)
                {
                    throw new InvalidDataException(
                        "The backup does not contain supported configuration files.");
                }

                return plan;
            }
        }


        public ConfigurationImportResult Import(
            string backupZipPath,
            bool overwriteExisting)
        {
            ConfigurationImportPlan plan =
                Analyze(
                    backupZipPath);

            return Import(
                backupZipPath,
                plan,
                overwriteExisting);
        }


        private ConfigurationImportResult Import(
            string backupZipPath,
            ConfigurationImportPlan plan,
            bool overwriteExisting)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(
                    nameof(plan));
            }

            if (plan.Entries.Count == 0)
            {
                throw new InvalidOperationException(
                    "There is nothing to import.");
            }

            ValidateBackupPath(
                backupZipPath);

            string rollbackDirectory =
                Path.Combine(
                    Path.GetTempPath(),
                    "NoteHighlightPlus-Import-" +
                    Guid.NewGuid().ToString("N"));

            var changedFiles =
                new List<ChangedFile>();

            var result =
                new ConfigurationImportResult();

            Directory.CreateDirectory(
                rollbackDirectory);

            try
            {
                using (FileStream stream =
                    new FileStream(
                        backupZipPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read))
                using (ZipArchive archive =
                    new ZipArchive(
                        stream,
                        ZipArchiveMode.Read,
                        leaveOpen: false))
                {
                    ValidateManifest(
                        archive);

                    foreach (ConfigurationImportEntry planEntry
                        in plan.Entries)
                    {
                        bool exists =
                            File.Exists(
                                planEntry.DestinationPath);

                        if (exists &&
                            !overwriteExisting)
                        {
                            result.SkippedFiles++;
                            continue;
                        }

                        ZipArchiveEntry archiveEntry =
                            FindEntry(
                                archive,
                                planEntry.ArchivePath);

                        if (archiveEntry == null)
                        {
                            throw new InvalidDataException(
                                "The backup changed after validation. Missing entry: " +
                                planEntry.ArchivePath);
                        }

                        string destinationDirectory =
                            Path.GetDirectoryName(
                                planEntry.DestinationPath);

                        if (!string.IsNullOrWhiteSpace(
                            destinationDirectory))
                        {
                            Directory.CreateDirectory(
                                destinationDirectory);
                        }

                        var changedFile =
                            new ChangedFile
                            {
                                DestinationPath =
                                    planEntry.DestinationPath,

                                ExistedBefore =
                                    exists
                            };

                        if (changedFile.ExistedBefore)
                        {
                            changedFile.RollbackPath =
                                Path.Combine(
                                    rollbackDirectory,
                                    Guid.NewGuid().ToString("N") +
                                    ".bak");

                            File.Copy(
                                changedFile.DestinationPath,
                                changedFile.RollbackPath,
                                true);
                        }

                        changedFiles.Add(
                            changedFile);

                        using (Stream input =
                            archiveEntry.Open())
                        using (FileStream output =
                            new FileStream(
                                planEntry.DestinationPath,
                                FileMode.Create,
                                FileAccess.Write,
                                FileShare.None))
                        {
                            input.CopyTo(
                                output);
                        }

                        result.ImportedFiles++;
                    }
                }
            }
            catch
            {
                RollBack(
                    changedFiles);

                throw;
            }
            finally
            {
                TryDeleteDirectory(
                    rollbackDirectory);
            }

            // Current backup format does not export ribbon.xml.
            result.RibbonConfigurationImported =
                false;

            return result;
        }


        private static ConfigurationImportEntry CreateImportEntry(
            string normalizedPath)
        {
            if (normalizedPath.StartsWith(
                "themes/",
                StringComparison.OrdinalIgnoreCase))
            {
                string relativePath =
                    normalizedPath.Substring(
                        "themes/".Length);

                if (string.IsNullOrWhiteSpace(
                    relativePath) ||
                    !relativePath.EndsWith(
                        ".theme",
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException(
                        "Unsupported file in themes/: " +
                        normalizedPath);
                }

                return new ConfigurationImportEntry
                {
                    ArchivePath =
                        normalizedPath,

                    DestinationPath =
                        GetSafeDestinationPath(
                            PathManager.ThemesFolder,
                            relativePath),

                    Category =
                        "Theme"
                };
            }

            if (normalizedPath.StartsWith(
                "langDefs/",
                StringComparison.OrdinalIgnoreCase))
            {
                string relativePath =
                    normalizedPath.Substring(
                        "langDefs/".Length);

                bool isLanguage =
                    relativePath.EndsWith(
                        ".lang",
                        StringComparison.OrdinalIgnoreCase);

                bool isMetadata =
                    relativePath.EndsWith(
                        ".groups.json",
                        StringComparison.OrdinalIgnoreCase);

                if (string.IsNullOrWhiteSpace(
                    relativePath) ||
                    (!isLanguage &&
                     !isMetadata))
                {
                    throw new InvalidDataException(
                        "Unsupported file in langDefs/: " +
                        normalizedPath);
                }

                return new ConfigurationImportEntry
                {
                    ArchivePath =
                        normalizedPath,

                    DestinationPath =
                        GetSafeDestinationPath(
                            PathManager.LanguagesFolder,
                            relativePath),

                    Category =
                        isLanguage
                            ? "Language"
                            : "Group metadata"
                };
            }

            if (string.Equals(
                normalizedPath,
                "preferences/" +
                ThemePreferenceFileName,
                StringComparison.OrdinalIgnoreCase))
            {
                string preferenceDirectory =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.ApplicationData),
                        ApplicationFolderName);

                return new ConfigurationImportEntry
                {
                    ArchivePath =
                        normalizedPath,

                    DestinationPath =
                        Path.Combine(
                            preferenceDirectory,
                            ThemePreferenceFileName),

                    Category =
                        "Preference"
                };
            }

            throw new InvalidDataException(
                "The backup contains an unsupported entry: " +
                normalizedPath);
        }


        private static string GetSafeDestinationPath(
            string rootDirectory,
            string relativePath)
        {
            string normalizedRelativePath =
                relativePath.Replace(
                    '/',
                    Path.DirectorySeparatorChar);

            string fullRoot =
                Path.GetFullPath(
                    rootDirectory)
                    .TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            string destinationPath =
                Path.GetFullPath(
                    Path.Combine(
                        rootDirectory,
                        normalizedRelativePath));

            if (!destinationPath.StartsWith(
                fullRoot,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The backup contains an unsafe path: " +
                    relativePath);
            }

            return destinationPath;
        }


        private static string NormalizeArchivePath(
            string archivePath)
        {
            if (string.IsNullOrWhiteSpace(
                archivePath))
            {
                throw new InvalidDataException(
                    "The backup contains an invalid empty path.");
            }

            string normalized =
                archivePath
                    .Replace('\\', '/')
                    .TrimStart('/');

            string[] segments =
                normalized.Split(
                    new[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0 ||
                segments.Any(
                    segment =>
                        segment == "." ||
                        segment == ".." ||
                        segment.Contains(":")))
            {
                throw new InvalidDataException(
                    "The backup contains an unsafe path: " +
                    archivePath);
            }

            return string.Join(
                "/",
                segments);
        }


        private static void ValidateManifest(
            ZipArchive archive)
        {
            ZipArchiveEntry manifestEntry =
                archive.Entries.FirstOrDefault(
                    entry =>
                        !string.IsNullOrEmpty(
                            entry.Name) &&
                        string.Equals(
                            NormalizeArchivePath(
                                entry.FullName),
                            ManifestFileName,
                            StringComparison.OrdinalIgnoreCase));

            if (manifestEntry == null)
            {
                throw new InvalidDataException(
                    "This ZIP is not a recognized NoteHighlight+ configuration backup.");
            }

            string manifest;

            using (StreamReader reader =
                new StreamReader(
                    manifestEntry.Open()))
            {
                manifest =
                    reader.ReadToEnd();
            }

            string versionLine =
                manifest
                    .Split(
                        new[] { "\r\n", "\n" },
                        StringSplitOptions.None)
                    .FirstOrDefault(
                        line =>
                            line.StartsWith(
                                "FormatVersion=",
                                StringComparison.OrdinalIgnoreCase));

            string version =
                versionLine == null
                    ? null
                    : versionLine
                        .Substring(
                            "FormatVersion=".Length)
                        .Trim();

            if (!string.Equals(
                version,
                SupportedFormatVersion,
                StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Unsupported NoteHighlight+ backup format version.");
            }
        }


        private static ZipArchiveEntry FindEntry(
            ZipArchive archive,
            string normalizedPath)
        {
            return archive.Entries.FirstOrDefault(
                entry =>
                    !string.IsNullOrEmpty(
                        entry.Name) &&
                    string.Equals(
                        NormalizeArchivePath(
                            entry.FullName),
                        normalizedPath,
                        StringComparison.OrdinalIgnoreCase));
        }


        private static void ValidateBackupPath(
            string backupZipPath)
        {
            if (string.IsNullOrWhiteSpace(
                backupZipPath))
            {
                throw new ArgumentException(
                    "The backup ZIP path cannot be empty.",
                    nameof(backupZipPath));
            }

            if (!File.Exists(
                backupZipPath))
            {
                throw new FileNotFoundException(
                    "The backup ZIP file was not found.",
                    backupZipPath);
            }
        }


        private static void RollBack(
            IEnumerable<ChangedFile> changedFiles)
        {
            foreach (ChangedFile changedFile
                in changedFiles.Reverse())
            {
                try
                {
                    if (changedFile.ExistedBefore)
                    {
                        if (File.Exists(
                            changedFile.RollbackPath))
                        {
                            File.Copy(
                                changedFile.RollbackPath,
                                changedFile.DestinationPath,
                                true);
                        }
                    }
                    else if (File.Exists(
                        changedFile.DestinationPath))
                    {
                        File.Delete(
                            changedFile.DestinationPath);
                    }
                }
                catch
                {
                    // Preserve the original import exception.
                }
            }
        }


        private static void TryDeleteDirectory(
            string directoryPath)
        {
            try
            {
                if (Directory.Exists(
                    directoryPath))
                {
                    Directory.Delete(
                        directoryPath,
                        true);
                }
            }
            catch
            {
                // Temporary rollback files may be left for OS cleanup.
            }
        }


        private sealed class ChangedFile
        {
            public string DestinationPath
            {
                get;
                set;
            }

            public string RollbackPath
            {
                get;
                set;
            }

            public bool ExistedBefore
            {
                get;
                set;
            }
        }
    }
}
