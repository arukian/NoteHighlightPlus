using Infrastructure.Core;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace NoteHighlightAddin
{
    /// <summary>
    /// Resolves Ribbon/file-extension aliases (for example "cs") to the
    /// canonical highlight language definition name (for example "csharp").
    /// </summary>
    internal static class LanguageDefinitionResolver
    {
        private static readonly Regex FileTypeEntryRegex =
            new Regex(
                "Lang\\s*=\\s*\\\"(?<lang>[^\\\"]+)\\\"\\s*,\\s*Extensions\\s*=\\s*\\{(?<extensions>[^}]*)\\}",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex QuotedValueRegex =
            new Regex(
                "\\\"(?<value>[^\\\"]+)\\\"",
                RegexOptions.Compiled);

        public static string Resolve(string candidate)
        {
            string normalized = Normalize(candidate);

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            if (LanguageDefinitionExists(normalized))
            {
                return normalized;
            }

            string mappedLanguage =
                ResolveFromFileTypes(normalized);

            if (!string.IsNullOrWhiteSpace(mappedLanguage)
                && LanguageDefinitionExists(mappedLanguage))
            {
                return mappedLanguage;
            }

            return null;
        }

        private static string ResolveFromFileTypes(string candidate)
        {
            string fileTypesPath =
                Path.Combine(
                    PathManager.HighlightFolder,
                    "filetypes.conf");

            if (!File.Exists(fileTypesPath))
            {
                return null;
            }

            try
            {
                string content = File.ReadAllText(fileTypesPath);

                foreach (Match entryMatch in FileTypeEntryRegex.Matches(content))
                {
                    string language =
                        Normalize(entryMatch.Groups["lang"].Value);

                    if (string.Equals(
                        language,
                        candidate,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return language;
                    }

                    string extensions =
                        entryMatch.Groups["extensions"].Value;

                    foreach (Match extensionMatch in QuotedValueRegex.Matches(extensions))
                    {
                        string extension =
                            Normalize(extensionMatch.Groups["value"].Value);

                        if (string.Equals(
                            extension,
                            candidate,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            return language;
                        }
                    }
                }
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }

            return null;
        }

        private static bool LanguageDefinitionExists(string languageName)
        {
            string path =
                Path.Combine(
                    PathManager.LanguagesFolder,
                    languageName + ".lang");

            return File.Exists(path);
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Path.GetFileNameWithoutExtension(value.Trim())
                .ToLowerInvariant();
        }
    }
}
