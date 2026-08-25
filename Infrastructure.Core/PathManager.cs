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
            Root = ResolveApplicationRoot();

            UserRoot = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Arukian",
                "NoteHighlight+");

            EnsureUserHighlightWorkspace();
        }

        // new method added to determine the root directory of the application, whether it's running inside OneNote or from a console application.
        private static string ResolveApplicationRoot()
        {
            // When running inside OneNote, prefer the actual add-in assembly.
            Assembly addInAssembly =
                AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(
                        assembly =>
                            string.Equals(
                                assembly.GetName().Name,
                                "NoteHighlightAddin",
                                StringComparison.OrdinalIgnoreCase));

            if (addInAssembly != null &&
                !string.IsNullOrWhiteSpace(addInAssembly.Location))
            {
                return Path.GetDirectoryName(
                    addInAssembly.Location);
            }

            // When running from TestConsole, use the executable folder.
            Assembly entryAssembly =
                Assembly.GetEntryAssembly();

            if (entryAssembly != null &&
                !string.IsNullOrWhiteSpace(entryAssembly.Location))
            {
                return Path.GetDirectoryName(
                    entryAssembly.Location);
            }

            // Last fallback for unusual hosts.
            return Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
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
        /// Highlight executable used from the user's workspace.
        /// Keeping the executable beside themes/langDefs ensures Highlight
        /// resolves the editable user configuration.
        /// </summary>
        public static string HighlightExe =>
            CombineUser("highlight", "highlight.exe");

        private static void EnsureUserHighlightWorkspace()
        {

            string userHighlightFolder =
                CombineUser("highlight");

            Directory.CreateDirectory(userHighlightFolder);

            CopyRuntimeFile(
                CombineInstalled("highlight", "highlight.exe"),
                CombineUser("highlight", "highlight.exe"));

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

        // adding method to copy exe and other files from the installed directory to the user's workspace, ensuring that the user has a working copy of the necessary files.

        private static void CopyRuntimeFile(
    string sourceFile,
    string destinationFile)
        {
            if (!File.Exists(sourceFile))
            {
                return;
            }

            string destinationDirectory =
                Path.GetDirectoryName(
                    destinationFile);

            if (!string.IsNullOrWhiteSpace(
                destinationDirectory))
            {
                Directory.CreateDirectory(
                    destinationDirectory);
            }

            File.Copy(
                sourceFile,
                destinationFile,
                overwrite: true);
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
