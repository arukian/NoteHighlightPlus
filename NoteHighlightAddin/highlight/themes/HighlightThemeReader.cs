using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NoteHighlightAddin.Highlighting.Themes
{
    /// <summary>
    /// Lee las propiedades visuales principales de un archivo .theme
    /// utilizado por highlight.exe.
    /// </summary>
    public sealed class HighlightThemeReader
        : IHighlightThemeReader
    {
        private static readonly Regex StringVariablePattern =
            new Regex(
                @"(?m)^\s*(?<name>[A-Za-z_]\w*)\s*=\s*""(?<value>[^""]*)""\s*$",
                RegexOptions.Compiled);

        private static readonly Regex DescriptionPattern =
            new Regex(
                @"(?m)^\s*Description\s*=\s*""(?<value>[^""]*)""",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase);

        private static readonly Regex NamedStylePattern =
            new Regex(
                @"(?m)^\s*(?<name>[A-Za-z_]\w*)\s*=\s*\{(?<body>[^{}]*)\}",
                RegexOptions.Compiled);

        private static readonly Regex AliasPattern =
            new Regex(
                @"(?m)^\s*(?<name>[A-Za-z_]\w*)\s*=\s*(?<target>[A-Za-z_]\w*)\s*$",
                RegexOptions.Compiled);

        private static readonly Regex ColourPattern =
            new Regex(
                @"\bColour\s*=\s*(?:""(?<literal>[^""]*)""|(?<variable>[A-Za-z_]\w*))",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase);

        private static readonly Regex BoldPattern =
            new Regex(
                @"\bBold\s*=\s*(?<value>true|false)",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase);

        private static readonly Regex ItalicPattern =
            new Regex(
                @"\bItalic\s*=\s*(?<value>true|false)",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase);

        public HighlightTheme Read(
            string filePath)
        {
            ValidateFilePath(
                filePath);

            string content =
                File.ReadAllText(
                    filePath);

            string normalizedContent =
                RemoveComments(
                    content);

            IDictionary<string, string> variables =
                ReadStringVariables(
                    normalizedContent);

            var theme =
                new HighlightTheme
                {
                    Name =
                        Path.GetFileNameWithoutExtension(
                            filePath),

                    Description =
                        ReadDescription(
                            normalizedContent)
                };

            ReadNamedStyles(
                normalizedContent,
                variables,
                theme);

            ResolveStyleAliases(
                normalizedContent,
                theme);

            ReadKeywordStyles(
                normalizedContent,
                variables,
                theme);

            return theme;
        }

        private static IDictionary<string, string> ReadStringVariables(
            string content)
        {
            var variables =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (Match match
                in StringVariablePattern.Matches(content))
            {
                string name =
                    match.Groups["name"].Value;

                string value =
                    match.Groups["value"].Value;

                variables[name] =
                    value;
            }

            return variables;
        }

        private static string ReadDescription(
            string content)
        {
            Match match =
                DescriptionPattern.Match(
                    content);

            if (!match.Success)
            {
                return null;
            }

            return match.Groups["value"].Value;
        }

        private static void ReadNamedStyles(
            string content,
            IDictionary<string, string> variables,
            HighlightTheme theme)
        {
            foreach (Match match
                in NamedStylePattern.Matches(content))
            {
                string styleName =
                    match.Groups["name"].Value;

                if (string.Equals(
                    styleName,
                    "Keywords",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (IsCollectionName(styleName))
                {
                    continue;
                }

                string body =
                    match.Groups["body"].Value;

                ThemeStyle style =
                    ParseStyle(
                        styleName,
                        body,
                        variables);

                theme.Styles[styleName] =
                    style;
            }
        }

        private static void ResolveStyleAliases(
            string content,
            HighlightTheme theme)
        {
            var pendingAliases =
                AliasPattern
                    .Matches(content)
                    .Cast<Match>()
                    .Select(
                        match =>
                            new
                            {
                                Name =
                                    match.Groups["name"].Value,

                                Target =
                                    match.Groups["target"].Value
                            })
                    .Where(
                        alias =>
                            !string.Equals(
                                alias.Name,
                                "Description",
                                StringComparison.OrdinalIgnoreCase))
                    .ToList();

            bool madeProgress;

            do
            {
                madeProgress =
                    false;

                foreach (var alias
                    in pendingAliases.ToList())
                {
                    ThemeStyle targetStyle;

                    if (!theme.Styles.TryGetValue(
                        alias.Target,
                        out targetStyle))
                    {
                        continue;
                    }

                    theme.Styles[alias.Name] =
                        CloneStyle(
                            alias.Name,
                            targetStyle);

                    pendingAliases.Remove(
                        alias);

                    madeProgress =
                        true;
                }
            }
            while (madeProgress);
        }

        private static void ReadKeywordStyles(
            string content,
            IDictionary<string, string> variables,
            HighlightTheme theme)
        {
            string keywordBlock =
                ExtractCollectionBody(
                    content,
                    "Keywords");

            if (string.IsNullOrWhiteSpace(keywordBlock))
            {
                return;
            }

            IList<string> entries =
                ExtractImmediateStyleBodies(
                    keywordBlock);

            for (int index = 0;
                index < entries.Count;
                index++)
            {
                string styleName =
                    "Keywords[" +
                    (index + 1) +
                    "]";

                ThemeStyle style =
                    ParseStyle(
                        styleName,
                        entries[index],
                        variables);

                theme.KeywordStyles.Add(
                    style);
            }
        }

        private static ThemeStyle ParseStyle(
            string name,
            string body,
            IDictionary<string, string> variables)
        {
            var style =
                new ThemeStyle
                {
                    Name =
                        name
                };

            Match colourMatch =
                ColourPattern.Match(
                    body);

            if (colourMatch.Success)
            {
                if (colourMatch.Groups["literal"].Success)
                {
                    style.Colour =
                        NormalizeColour(
                            colourMatch.Groups["literal"].Value);
                }
                else
                {
                    string variableName =
                        colourMatch.Groups["variable"].Value;

                    string variableValue;

                    if (variables.TryGetValue(
                        variableName,
                        out variableValue))
                    {
                        style.Colour =
                            NormalizeColour(
                                variableValue);
                    }
                }
            }

            Match boldMatch =
                BoldPattern.Match(
                    body);

            if (boldMatch.Success)
            {
                style.Bold =
                    ParseBoolean(
                        boldMatch.Groups["value"].Value);
            }

            Match italicMatch =
                ItalicPattern.Match(
                    body);

            if (italicMatch.Success)
            {
                style.Italic =
                    ParseBoolean(
                        italicMatch.Groups["value"].Value);
            }

            return style;
        }

        private static string ExtractCollectionBody(
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
                return null;
            }

            int openingBraceIndex =
                content.IndexOf(
                    '{',
                    startMatch.Index);

            if (openingBraceIndex < 0)
            {
                return null;
            }

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
                    (index == 0 ||
                     content[index - 1] != '\\'))
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
                    return content.Substring(
                        openingBraceIndex + 1,
                        index - openingBraceIndex - 1);
                }
            }

            return null;
        }

        private static IList<string> ExtractImmediateStyleBodies(
            string collectionBody)
        {
            var entries =
                new List<string>();

            int depth =
                0;

            int entryStart =
                -1;

            bool insideString =
                false;

            for (int index = 0;
                index < collectionBody.Length;
                index++)
            {
                char character =
                    collectionBody[index];

                if (character == '"' &&
                    (index == 0 ||
                     collectionBody[index - 1] != '\\'))
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
                        collectionBody.Substring(
                            entryStart,
                            index - entryStart));

                    entryStart =
                        -1;
                }
            }

            return entries;
        }

        private static ThemeStyle CloneStyle(
            string name,
            ThemeStyle source)
        {
            return new ThemeStyle
            {
                Name =
                    name,

                Colour =
                    source.Colour,

                Bold =
                    source.Bold,

                Italic =
                    source.Italic
            };
        }

        private static bool IsCollectionName(
            string name)
        {
            return string.Equals(
                       name,
                       "Categories",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       name,
                       "SemanticTokenTypes",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool ParseBoolean(
            string value)
        {
            return string.Equals(
                value,
                "true",
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeColour(
            string colour)
        {
            if (string.IsNullOrWhiteSpace(colour))
            {
                return "#000000";
            }

            return colour
                .Trim()
                .TrimEnd(';');
        }

        private static string RemoveComments(
            string content)
        {
            string withoutBlockComments =
                Regex.Replace(
                    content,
                    @"--\[\[.*?\]\]",
                    string.Empty,
                    RegexOptions.Singleline);

            return Regex.Replace(
                withoutBlockComments,
                @"(?m)--.*$",
                string.Empty);
        }

        private static void ValidateFilePath(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "The theme file path cannot be empty.",
                    nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The theme file was not found.",
                    filePath);
            }
        }
    }
}