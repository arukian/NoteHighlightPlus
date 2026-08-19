using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Infrastructure.Core
{
    public static class PathManager
    {
        public static string Root { get; }

        public static string UserRoot { get; }

        static PathManager()
        {
            Root = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            UserRoot = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Arukian",
                "NoteHighlight+");

            EnsureUserHighlightWorkspace();
        }

        public static string Ribbon =>
            CombineInstalled("ribbon.xml");

        /// <summary>
        /// Editable Highlight working directory for the current user.
        /// Themes, language definitions and filetypes.conf live here.
        /// </summary>
        public static string HighlightFolder =>
            CombineUser("highlight");

        /// <summary>
        /// Read-only Highlight directory installed with the add-in.
        /// </summary>
        public static string InstalledHighlightFolder =>
            CombineInstalled("highlight");

        public static string ThemesFolder =>
            CombineUser("highlight", "themes");

        public static string LanguagesFolder =>
            CombineUser("highlight", "langDefs");

        /// <summary>
        /// highlight.exe remains in Program Files. The process is executed
        /// with HighlightFolder as its working directory so it reads the
        /// user's editable themes/langDefs without writing to Program Files.
        /// </summary>
        public static string HighlightExe =>
            CombineInstalled("highlight", "highlight.exe");

        private static void EnsureUserHighlightWorkspace()
        {
            string userHighlightFolder =
                CombineUser("highlight");

            Directory.CreateDirectory(userHighlightFolder);

            CopyDirectoryMissing(
                CombineInstalled("highlight", "themes"),
                CombineUser("highlight", "themes"));

            CopyDirectoryMissing(
                CombineInstalled("highlight", "langDefs"),
                CombineUser("highlight", "langDefs"));

            CopyFileIfMissing(
                CombineInstalled("highlight", "filetypes.conf"),
                CombineUser("highlight", "filetypes.conf"));
        }

        private static void CopyDirectoryMissing(
            string sourceDirectory,
            string destinationDirectory)
        {
            if (!Directory.Exists(sourceDirectory))
            {
                return;
            }

            Directory.CreateDirectory(destinationDirectory);

            foreach (string sourceFile in
                Directory.GetFiles(sourceDirectory))
            {
                string destinationFile = Path.Combine(
                    destinationDirectory,
                    Path.GetFileName(sourceFile));

                if (!File.Exists(destinationFile))
                {
                    File.Copy(
                        sourceFile,
                        destinationFile,
                        overwrite: false);
                }
            }

            foreach (string sourceSubdirectory in
                Directory.GetDirectories(sourceDirectory))
            {
                string destinationSubdirectory = Path.Combine(
                    destinationDirectory,
                    Path.GetFileName(sourceSubdirectory));

                CopyDirectoryMissing(
                    sourceSubdirectory,
                    destinationSubdirectory);
            }
        }

        private static void CopyFileIfMissing(
            string sourceFile,
            string destinationFile)
        {
            if (!File.Exists(sourceFile) ||
                File.Exists(destinationFile))
            {
                return;
            }

            string destinationDirectory =
                Path.GetDirectoryName(destinationFile);

            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Copy(
                sourceFile,
                destinationFile,
                overwrite: false);
        }

        private static string CombineInstalled(
            params string[] parts)
        {
            return Path.Combine(
                new[] { Root }.Concat(parts).ToArray());
        }

        private static string CombineUser(
            params string[] parts)
        {
            return Path.Combine(
                new[] { UserRoot }.Concat(parts).ToArray());
        }
    }
}
