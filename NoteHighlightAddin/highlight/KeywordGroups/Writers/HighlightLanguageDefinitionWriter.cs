using GenerateHighlightContent.LanguageDefinitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Writers
{
    /// <summary>
    /// Convierte un HighlightLanguageDefinition al formato .lang
    /// utilizado por highlight.exe.
    /// </summary>
    public sealed class HighlightLanguageDefinitionWriter
        : ILanguageDefinitionWriter
    {
        private const string Indent = "    ";

        public void Write(
            HighlightLanguageDefinition definition,
            string filePath)
        {
            ValidateDefinition(
                definition);

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "The language definition path cannot be empty.",
                    nameof(filePath));
            }

            string directory =
                Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directory) &&
                !Directory.Exists(directory))
            {
                Directory.CreateDirectory(
                    directory);
            }

            string content =
                Serialize(
                    definition);

            File.WriteAllText(
                filePath,
                content,
                new UTF8Encoding(false));
        }

        public string Serialize(
            HighlightLanguageDefinition definition)
        {
            ValidateDefinition(
                definition);

            var builder =
                new StringBuilder();

            WriteHeader(
                builder,
                definition);

            WriteKeywords(
                builder,
                definition.Groups);

            return builder.ToString();
        }

        private static void WriteHeader(
            StringBuilder builder,
            HighlightLanguageDefinition definition)
        {
            builder.AppendLine(
                $"Description = \"{EscapeQuotedValue(definition.Description)}\"");

            builder.AppendLine(
                $"CaseSensitive = {ToLuaBoolean(definition.CaseSensitive)}");

            WriteExtensions(
                builder,
                definition.Extensions);

            builder.AppendLine();
        }

        private static void WriteExtensions(
            StringBuilder builder,
            IEnumerable<string> extensions)
        {
            builder.AppendLine(
                "Extensions = {");

            IEnumerable<string> validExtensions =
                extensions == null
                    ? Enumerable.Empty<string>()
                    : extensions
                        .Where(extension =>
                            !string.IsNullOrWhiteSpace(extension))
                        .Select(extension =>
                            extension.Trim())
                        .Distinct(StringComparer.Ordinal);

            foreach (string extension in validExtensions)
            {
                builder.Append(Indent);
                builder.Append('"');
                builder.Append(
                    EscapeQuotedValue(extension));
                builder.AppendLine("\",");
            }

            builder.AppendLine(
                "}");
        }

        private static void WriteKeywords(
            StringBuilder builder,
            IEnumerable<HighlightKeywordGroup> groups)
        {
            builder.AppendLine(
                "Keywords = {");

            IEnumerable<HighlightKeywordGroup> validGroups =
                groups == null
                    ? Enumerable.Empty<HighlightKeywordGroup>()
                    : groups
                        .Where(group => group != null)
                        .OrderBy(group => group.Id);

            foreach (HighlightKeywordGroup group in validGroups)
            {
                WriteWordGroup(
                    builder,
                    group);

                WriteRegexGroups(
                    builder,
                    group);
            }

            builder.AppendLine(
                "}");
        }

        private static void WriteWordGroup(
            StringBuilder builder,
            HighlightKeywordGroup group)
        {
            List<string> words =
                group.Words == null
                    ? new List<string>()
                    : group.Words
                        .Where(word =>
                            !string.IsNullOrWhiteSpace(word))
                        .Select(word =>
                            word.Trim())
                        .Distinct(StringComparer.Ordinal)
                        .ToList();

            if (words.Count == 0)
            {
                return;
            }

            builder.Append(Indent);
            builder.AppendLine("{");

            builder.Append(Indent);
            builder.Append(Indent);
            builder.Append("Id = ");
            builder.Append(group.Id);
            builder.AppendLine(",");

            builder.Append(Indent);
            builder.Append(Indent);
            builder.AppendLine("List = {");

            foreach (string word in words)
            {
                builder.Append(Indent);
                builder.Append(Indent);
                builder.Append(Indent);
                builder.Append('"');
                builder.Append(
                    EscapeQuotedValue(word));
                builder.AppendLine("\",");
            }

            builder.Append(Indent);
            builder.Append(Indent);
            builder.AppendLine("}");

            builder.Append(Indent);
            builder.AppendLine("},");
        }

        private static void WriteRegexGroups(
            StringBuilder builder,
            HighlightKeywordGroup group)
        {
            IEnumerable<string> regexValues =
                group.Regex == null
                    ? Enumerable.Empty<string>()
                    : group.Regex
                        .Where(regex =>
                            !string.IsNullOrWhiteSpace(regex))
                        .Distinct(StringComparer.Ordinal);

            foreach (string regex in regexValues)
            {
                builder.Append(Indent);
                builder.AppendLine("{");

                builder.Append(Indent);
                builder.Append(Indent);
                builder.Append("Id = ");
                builder.Append(group.Id);
                builder.AppendLine(",");

                builder.Append(Indent);
                builder.Append(Indent);
                builder.Append("Regex = ");

                builder.AppendLine(
                    CreateLongBracketValue(regex));

                builder.Append(Indent);
                builder.AppendLine("},");
            }
        }

        private static string CreateLongBracketValue(
            string value)
        {
            string safeValue =
                value ?? string.Empty;

            int equalsCount = 0;

            while (ContainsClosingDelimiter(
                safeValue,
                equalsCount))
            {
                equalsCount++;
            }

            string equals =
                new string(
                    '=',
                    equalsCount);

            return
                "[" +
                equals +
                "[" +
                safeValue +
                "]" +
                equals +
                "]";
        }

        private static bool ContainsClosingDelimiter(
            string value,
            int equalsCount)
        {
            string delimiter =
                "]" +
                new string(
                    '=',
                    equalsCount) +
                "]";

            return value.IndexOf(
                delimiter,
                StringComparison.Ordinal) >= 0;
        }

        private static string EscapeQuotedValue(
            string value)
        {
            if (string.IsNullOrEmpty(value))
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

        private static string ToLuaBoolean(
            bool value)
        {
            return value
                ? "true"
                : "false";
        }

        private static void ValidateDefinition(
            HighlightLanguageDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            if (definition.Groups == null)
            {
                throw new InvalidOperationException(
                    "The language definition has no group collection.");
            }

            foreach (HighlightKeywordGroup group
                in definition.Groups)
            {
                if (group == null)
                {
                    throw new InvalidOperationException(
                        "The language definition contains a null group.");
                }

                if (group.Id < 1)
                {
                    throw new InvalidOperationException(
                        $"Keyword group Id {group.Id} is invalid. " +
                        "Group identifiers must be greater than zero.");
                }
            }
        }
    }
}