using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NoteHighlightAddin.Highlighting.Themes
{
    /// <summary>
    /// Modifica entradas concretas de Keywords dentro de un archivo
    /// .theme preservando el resto de su contenido.
    /// </summary>
    public sealed class HighlightThemeWriter
        : IHighlightThemeWriter
    {
        private static readonly Regex ColourPattern =
            new Regex(
                @"\bColour\s*=\s*(?:""[^""]*""|[A-Za-z_]\w*)",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase);

        public void UpdateKeywordColour(
            string filePath,
            int groupId,
            string colour)
        {
            ValidateFilePath(
                filePath);

            if (groupId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(groupId),
                    "The group ID must be greater than zero.");
            }

            string normalizedColour =
                NormalizeColour(
                    colour);

            string content =
                File.ReadAllText(
                    filePath);

            TextRange keywordsRange =
                FindCollectionBody(
                    content,
                    "Keywords");

            IList<TextRange> keywordEntries =
                FindImmediateEntries(
                    content,
                    keywordsRange);

            int entryIndex =
                groupId - 1;

            if (entryIndex >= keywordEntries.Count)
            {
                throw new InvalidOperationException(
                    "The theme does not define Keywords[" +
                    groupId +
                    "].");
            }

            TextRange entryRange =
                keywordEntries[entryIndex];

            string entryBody =
                content.Substring(
                    entryRange.Start,
                    entryRange.Length);

            string updatedEntryBody =
                UpdateColourProperty(
                    entryBody,
                    normalizedColour);

            string updatedContent =
                content.Substring(
                    0,
                    entryRange.Start) +
                updatedEntryBody +
                content.Substring(
                    entryRange.End);

            WriteSafely(
                filePath,
                updatedContent);
        }

        private static string UpdateColourProperty(
            string entryBody,
            string colour)
        {
            string colourProperty =
                "Colour=\"" +
                colour +
                "\"";

            if (ColourPattern.IsMatch(
                entryBody))
            {
                return ColourPattern.Replace(
                    entryBody,
                    colourProperty,
                    1);
            }

            string trimmedBody =
                entryBody.TrimEnd();

            if (string.IsNullOrWhiteSpace(
                trimmedBody))
            {
                return colourProperty;
            }

            bool needsSeparator =
                !trimmedBody.EndsWith(
                    ",",
                    StringComparison.Ordinal);

            return trimmedBody +
                (needsSeparator ? ", " : " ") +
                colourProperty;
        }

        private static TextRange FindCollectionBody(
            string content,
            string collectionName)
        {
            Match startMatch =
                Regex.Match(
                    content,
                    @"(?m)^\s*" +
                    Regex.Escape(collectionName) +
                    @"\s*=\s*\{",
                    RegexOptions.IgnoreCase);

            if (!startMatch.Success)
            {
                throw new InvalidOperationException(
                    "The theme does not contain a " +
                    collectionName +
                    " collection.");
            }

            int openingBraceIndex =
                content.IndexOf(
                    '{',
                    startMatch.Index);

            int closingBraceIndex =
                FindMatchingBrace(
                    content,
                    openingBraceIndex);

            return new TextRange(
                openingBraceIndex + 1,
                closingBraceIndex);
        }

        private static IList<TextRange> FindImmediateEntries(
            string content,
            TextRange collectionRange)
        {
            var entries =
                new List<TextRange>();

            int depth =
                0;

            int entryStart =
                -1;

            bool insideString =
                false;

            for (int index = collectionRange.Start;
                index < collectionRange.End;
                index++)
            {
                char character =
                    content[index];

                if (character == '"' &&
                    !IsEscaped(
                        content,
                        index))
                {
                    insideString =
                        !insideString;

                    continue;
                }

                if (insideString)
                {
                    continue;
                }

                if (character == '{')
                {
                    if (depth == 0)
                    {
                        entryStart =
                            index + 1;
                    }

                    depth++;
                    continue;
                }

                if (character != '}')
                {
                    continue;
                }

                depth--;

                if (depth == 0 &&
                    entryStart >= 0)
                {
                    entries.Add(
                        new TextRange(
                            entryStart,
                            index));

                    entryStart =
                        -1;
                }
            }

            return entries;
        }

        private static int FindMatchingBrace(
            string content,
            int openingBraceIndex)
        {
            int depth =
                0;

            bool insideString =
                false;

            for (int index = openingBraceIndex;
                index < content.Length;
                index++)
            {
                char character =
                    content[index];

                if (character == '"' &&
                    !IsEscaped(
                        content,
                        index))
                {
                    insideString =
                        !insideString;

                    continue;
                }

                if (insideString)
                {
                    continue;
                }

                if (character == '{')
                {
                    depth++;
                    continue;
                }

                if (character != '}')
                {
                    continue;
                }

                depth--;

                if (depth == 0)
                {
                    return index;
                }
            }

            throw new InvalidDataException(
                "The theme contains an unclosed collection.");
        }

        private static bool IsEscaped(
            string content,
            int index)
        {
            int slashCount =
                0;

            for (int current = index - 1;
                current >= 0 &&
                content[current] == '\\';
                current--)
            {
                slashCount++;
            }

            return slashCount % 2 != 0;
        }

        private static string NormalizeColour(
            string colour)
        {
            if (string.IsNullOrWhiteSpace(
                colour))
            {
                throw new ArgumentException(
                    "The colour cannot be empty.",
                    nameof(colour));
            }

            string normalized =
                colour.Trim();

            if (!normalized.StartsWith(
                "#",
                StringComparison.Ordinal))
            {
                normalized =
                    "#" + normalized;
            }

            if (!Regex.IsMatch(
                normalized,
                @"^#[0-9A-Fa-f]{6}$"))
            {
                throw new ArgumentException(
                    "The colour must use the #RRGGBB format.",
                    nameof(colour));
            }

            return normalized.ToUpperInvariant();
        }

        private static void WriteSafely(
            string filePath,
            string content)
        {
            string temporaryPath =
                filePath + ".tmp";

            string backupPath =
                filePath + ".bak";

            try
            {
                File.WriteAllText(
                    temporaryPath,
                    content,
                    new UTF8Encoding(false));

                File.Copy(
                    filePath,
                    backupPath,
                    true);

                File.Copy(
                    temporaryPath,
                    filePath,
                    true);
            }
            finally
            {
                if (File.Exists(
                    temporaryPath))
                {
                    File.Delete(
                        temporaryPath);
                }
            }
        }

        private static void ValidateFilePath(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(
                filePath))
            {
                throw new ArgumentException(
                    "The theme file path cannot be empty.",
                    nameof(filePath));
            }

            if (!File.Exists(
                filePath))
            {
                throw new FileNotFoundException(
                    "The theme file was not found.",
                    filePath);
            }
        }

        private sealed class TextRange
        {
            public TextRange(
                int start,
                int end)
            {
                Start =
                    start;

                End =
                    end;
            }

            public int Start
            {
                get;
            }

            public int End
            {
                get;
            }

            public int Length
            {
                get
                {
                    return End - Start;
                }
            }
        }
    }
}