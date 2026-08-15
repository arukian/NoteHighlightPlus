using System;
using System.IO;

namespace NoteHighlightAddin
{
    /// <summary>
    /// Stores the theme selected by the user so SettingsForm and MainForm
    /// share the same preferred theme by name.
    /// </summary>
    public sealed class ThemePreferenceProvider
    {
        private const string PreferenceFolderName =
            "NoteHighlightPlus";

        private const string PreferenceFileName =
            "last-theme.txt";

        public string ReadThemeName()
        {
            try
            {
                string preferenceFile =
                    GetPreferenceFilePath();

                if (!File.Exists(preferenceFile))
                {
                    return null;
                }

                string themeName =
                    File.ReadAllText(preferenceFile)
                        .Trim();

                return string.IsNullOrWhiteSpace(themeName)
                    ? null
                    : themeName;
            }
            catch
            {
                // Theme preference persistence is optional.
                return null;
            }
        }

        public void SaveThemeName(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
            {
                return;
            }

            try
            {
                string preferenceFile =
                    GetPreferenceFilePath();

                string preferenceFolder =
                    Path.GetDirectoryName(preferenceFile);

                if (!Directory.Exists(preferenceFolder))
                {
                    Directory.CreateDirectory(preferenceFolder);
                }

                File.WriteAllText(
                    preferenceFile,
                    themeName.Trim());
            }
            catch
            {
                // Theme preference persistence is optional.
            }
        }

        public void Clear()
        {
            try
            {
                string preferenceFile =
                    GetPreferenceFilePath();

                if (File.Exists(preferenceFile))
                {
                    File.Delete(preferenceFile);
                }
            }
            catch
            {
                // Theme preference cleanup is optional.
            }
        }

        private static string GetPreferenceFilePath()
        {
            string preferenceFolder =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData),
                    PreferenceFolderName);

            return Path.Combine(
                preferenceFolder,
                PreferenceFileName);
        }
    }
}
