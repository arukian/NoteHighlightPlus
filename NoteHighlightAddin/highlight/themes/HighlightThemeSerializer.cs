using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoteHighlightAddin.Highlighting.Themes
{
    /// <summary>
    /// Serializa un HighlightTheme al formato .theme
    /// utilizado por highlight.exe.
    ///
    /// Esta primera versión serializa únicamente la información
    /// que actualmente conserva HighlightTheme:
    /// Description, Variables, Styles, StyleAliases y Keywords.
    /// </summary>
    public sealed class HighlightThemeSerializer
        : IHighlightThemeSerializer
    {
        public void Serialize(
            HighlightTheme theme,
            string filePath)
        {
            ValidateArguments(
                theme,
                filePath);

            string content =
                BuildThemeContent(
                    theme);

            string directory =
                Path.GetDirectoryName(
                    filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(
                    directory);
            }

            File.WriteAllText(
                filePath,
                content,
                new UTF8Encoding(
                    false));
        }

        private static string BuildThemeContent(
            HighlightTheme theme)
        {
            var builder =
                new StringBuilder();

            WriteDescription(
                builder,
                theme);

            WriteVariables(
                builder,
                theme);

            WriteCategories(
                builder,
                theme);

            WriteGeneralStyles(
                builder,
                theme);

            WriteAliases(
                builder,
                theme);

            WriteKeywordStyles(
                builder,
                theme);

            WriteSemanticTokenTypes(
                builder,
                theme);

            return builder.ToString();
        }

        private static void WriteDescription(
            StringBuilder builder,
            HighlightTheme theme)
        {
            if (string.IsNullOrWhiteSpace(
                theme.Description))
            {
                return;
            }

            builder.Append(
                "Description=\"");

            builder.Append(
                EscapeString(
                    theme.Description));

            builder.AppendLine(
                "\"");

            builder.AppendLine();
        }

        private static void WriteVariables(
            StringBuilder builder,
            HighlightTheme theme)
        {
            if (theme.Variables == null ||
                theme.Variables.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, string> variable
                in theme.Variables)
            {
                builder.Append(
                    variable.Key);

                builder.Append(
                    " = \"");

                builder.Append(
                    EscapeString(
                        variable.Value));

                builder.AppendLine(
                    "\"");
            }

            builder.AppendLine();
        }

        private static void WriteCategories(
            StringBuilder builder,
            HighlightTheme theme)
        {
            if (theme.Categories == null ||
                theme.Categories.Count == 0)
            {
                return;
            }

            builder.Append(
                "Categories = {");

            for (int index = 0;
                index < theme.Categories.Count;
                index++)
            {
                if (index > 0)
                {
                    builder.Append(
                        ", ");
                }

                builder.Append(
                    "\"");

                builder.Append(
                    EscapeString(
                        theme.Categories[index]));

                builder.Append(
                    "\"");
            }

            builder.AppendLine(
                "}");

            builder.AppendLine();
        }

        private static void WriteGeneralStyles(
            StringBuilder builder,
            HighlightTheme theme)
        {
            if (theme.Styles == null ||
                theme.Styles.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, ThemeStyle> entry
                in theme.Styles)
            {
                if (IsAlias(
                    theme,
                    entry.Key))
                {
                    continue;
                }

                WriteNamedStyle(
                    builder,
                    entry.Key,
                    entry.Value);
            }

            builder.AppendLine();
        }

        private static void WriteAliases(
            StringBuilder builder,
            HighlightTheme theme)
        {
            if (theme.StyleAliases == null ||
                theme.StyleAliases.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, string> alias
                in theme.StyleAliases)
            {
                builder.Append(
                    alias.Key);

                builder.Append(
                    " = ");

                builder.AppendLine(
                    alias.Value);
            }

            builder.AppendLine();
        }

        private static void WriteKeywordStyles(
            StringBuilder builder,
            HighlightTheme theme)
        {
            if (theme.KeywordStyles == null ||
                theme.KeywordStyles.Count == 0)
            {
                return;
            }

            builder.AppendLine(
                "Keywords = {");

            foreach (ThemeStyle style
                in theme.KeywordStyles)
            {
                builder.Append(
                    "  ");

                WriteInlineStyle(
                    builder,
                    style);

                builder.AppendLine(
                    ",");
            }

            builder.AppendLine(
                "}");

            builder.AppendLine();
        }

        private static void WriteNamedStyle(
            StringBuilder builder,
            string name,
            ThemeStyle style)
        {
            builder.Append(
                name);

            builder.Append(
                " = ");

            WriteInlineStyle(
                builder,
                style);

            builder.AppendLine();
        }

        private static void WriteInlineStyle(
            StringBuilder builder,
            ThemeStyle style)
        {
            builder.Append(
                "{ ");

            WriteColour(
                builder,
                style);

            if (style.Bold)
            {
                builder.Append(
                    ", Bold=true");
            }

            if (style.Italic)
            {
                builder.Append(
                    ", Italic=true");
            }

            builder.Append(
                " }");
        }

        private static void WriteColour(
            StringBuilder builder,
            ThemeStyle style)
        {
            builder.Append(
                "Colour=");

            if (!string.IsNullOrWhiteSpace(
                style.ColourReference))
            {
                builder.Append(
                    style.ColourReference);

                return;
            }

            builder.Append(
                "\"");

            builder.Append(
                EscapeString(
                    style.Colour));

            builder.Append(
                "\"");
        }

        private static void WriteSemanticTokenTypes(
            StringBuilder builder,
            HighlightTheme theme)
        {
            if (theme.SemanticTokenTypes == null ||
                theme.SemanticTokenTypes.Count == 0)
            {
                return;
            }

            builder.AppendLine(
                "SemanticTokenTypes = {");

            foreach (SemanticTokenStyle token
                in theme.SemanticTokenTypes)
            {
                builder.Append(
                    "  { Type = \"");

                builder.Append(
                    EscapeString(
                        token.Type));

                builder.Append(
                    "\", Style = ");

                builder.Append(
                    token.StyleReference);

                builder.AppendLine(
                    " },");
            }

            builder.AppendLine(
                "}");

            builder.AppendLine();
        }

        private static bool IsAlias(
            HighlightTheme theme,
            string styleName)
        {
            if (theme.StyleAliases == null)
            {
                return false;
            }

            return theme.StyleAliases.ContainsKey(
                styleName);
        }

        private static string EscapeString(
            string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value
                .Replace(
                    "\\",
                    "\\\\")
                .Replace(
                    "\"",
                    "\\\"");
        }

        private static void ValidateArguments(
            HighlightTheme theme,
            string filePath)
        {
            if (theme == null)
            {
                throw new ArgumentNullException(
                    nameof(theme));
            }

            if (string.IsNullOrWhiteSpace(
                filePath))
            {
                throw new ArgumentException(
                    "The theme file path cannot be empty.",
                    nameof(filePath));
            }
        }
    }
}