using System;
using System.IO;

namespace NoteHighlightAddin
{
    /// <summary>
    /// Stores the language selected by the user so SettingsForm
    /// can restore the last language being edited.
    /// </summary>
    public sealed class LanguagePreferenceProvider
    {
        private const string PreferenceFolderName =
            "NoteHighlightPlus";

        private const string PreferenceFileName =
            "last-language.txt";

        public string ReadLanguageTag()
        {
            try
            {
                string preferenceFile =
                    GetPreferenceFilePath();

                if (!File.Exists(preferenceFile))
                {
                    return null;
                }

                string languageTag =
                    File.ReadAllText(
                        preferenceFile)
                        .Trim();

                return string.IsNullOrWhiteSpace(
                    languageTag)
                    ? null
                    : languageTag;
            }
            catch
            {
                // Language preference persistence is optional.
                return null;
            }
        }

        public void SaveLanguageTag(
            string languageTag)
        {
            if (string.IsNullOrWhiteSpace(
                languageTag))
            {
                return;
            }

            try
            {
                string preferenceFile =
                    GetPreferenceFilePath();

                string preferenceFolder =
                    Path.GetDirectoryName(
                        preferenceFile);

                if (!Directory.Exists(
                    preferenceFolder))
                {
                    Directory.CreateDirectory(
                        preferenceFolder);
                }

                File.WriteAllText(
                    preferenceFile,
                    languageTag.Trim());
            }
            catch
            {
                // Language preference persistence is optional.
            }
        }

        public void Clear()
        {
            try
            {
                string preferenceFile =
                    GetPreferenceFilePath();

                if (File.Exists(
                    preferenceFile))
                {
                    File.Delete(
                        preferenceFile);
                }
            }
            catch
            {
                // Language preference cleanup is optional.
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