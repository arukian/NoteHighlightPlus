using System;
using System.IO;

namespace NoteHighlightAddin
{
    /// <summary>
    /// Keeps a persistent baseline copy of each theme so the Theme Editor
    /// can restore a theme without depending on the mutable highlight/themes
    /// directory.
    /// </summary>
    internal sealed class ThemeResetService
    {
        private readonly string _baselineFolder;

        public ThemeResetService()
        {
            _baselineFolder = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "NoteHighlightPlus",
                "theme-baselines");
        }

        public void EnsureBaseline(
            string themeName,
            string themeFilePath)
        {
            ValidateThemeArguments(
                themeName,
                themeFilePath);

            if (!File.Exists(themeFilePath))
            {
                throw new FileNotFoundException(
                    "The theme file could not be found.",
                    themeFilePath);
            }

            Directory.CreateDirectory(
                _baselineFolder);

            string baselinePath =
                GetBaselinePath(themeName);

            if (File.Exists(baselinePath))
            {
                return;
            }

            File.Copy(
                themeFilePath,
                baselinePath,
                false);
        }

        public bool CanReset(
            string themeName)
        {
            if (string.IsNullOrWhiteSpace(
                themeName))
            {
                return false;
            }

            return File.Exists(
                GetBaselinePath(themeName));
        }

        public void RestoreBaseline(
            string themeName,
            string destinationThemePath)
        {
            ValidateThemeArguments(
                themeName,
                destinationThemePath);

            string baselinePath =
                GetBaselinePath(themeName);

            if (!File.Exists(baselinePath))
            {
                throw new FileNotFoundException(
                    "No reset baseline exists for this theme.",
                    baselinePath);
            }

            string destinationDirectory =
                Path.GetDirectoryName(
                    destinationThemePath);

            if (!string.IsNullOrWhiteSpace(
                destinationDirectory))
            {
                Directory.CreateDirectory(
                    destinationDirectory);
            }

            File.Copy(
                baselinePath,
                destinationThemePath,
                true);
        }

        private string GetBaselinePath(
            string themeName)
        {
            string safeThemeName =
                Path.GetFileNameWithoutExtension(
                    themeName.Trim());

            return Path.Combine(
                _baselineFolder,
                safeThemeName + ".theme");
        }

        private static void ValidateThemeArguments(
            string themeName,
            string themeFilePath)
        {
            if (string.IsNullOrWhiteSpace(
                themeName))
            {
                throw new ArgumentException(
                    "A theme name is required.",
                    nameof(themeName));
            }

            if (string.IsNullOrWhiteSpace(
                themeFilePath))
            {
                throw new ArgumentException(
                    "A theme file path is required.",
                    nameof(themeFilePath));
            }
        }
    }
}
