using GenerateHighlightContent.LanguageDefinitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

            if (!string.IsNullOrWhiteSpace(
                definition.OriginalContent))
            {
                return SerializePreservingOriginalContent(
                    definition);
            }

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


        private static string SerializePreservingOriginalContent(
            HighlightLanguageDefinition definition)
        {
            string content =
                definition.OriginalContent;

            content = ReplaceQuotedProperty(
                content,
                "Description",
                definition.Description);

            content = ReplaceCaseSensitivity(
                content,
                definition.CaseSensitive);

            var extensionsBuilder = new StringBuilder();
            WriteExtensions(
                extensionsBuilder,
                definition.Extensions);

            content = ReplaceNamedSection(
                content,
                "Extensions",
                extensionsBuilder.ToString().TrimEnd());

            var keywordsBuilder = new StringBuilder();
            WriteKeywords(
                keywordsBuilder,
                definition.Groups);

            content = ReplaceNamedSection(
                content,
                "Keywords",
                keywordsBuilder.ToString().TrimEnd());

            return content;
        }

        private static string ReplaceQuotedProperty(
            string content,
            string propertyName,
            string value)
        {
            string replacement =
                propertyName +
                " = \"" +
                EscapeQuotedValue(value) +
                "\"";

            string pattern =
                @"\b" +
                Regex.Escape(propertyName) +
                @"\s*=\s*[""'][^""']*[""']";

            if (Regex.IsMatch(
                content,
                pattern,
                RegexOptions.IgnoreCase))
            {
                return Regex.Replace(
                    content,
                    pattern,
                    replacement,
                    RegexOptions.IgnoreCase);
            }

            return replacement +
                Environment.NewLine +
                content;
        }

        private static string ReplaceCaseSensitivity(
            string content,
            bool caseSensitive)
        {
            string casePattern =
                @"\bCaseSensitive\s*=\s*(?:true|false)";

            if (Regex.IsMatch(
                content,
                casePattern,
                RegexOptions.IgnoreCase))
            {
                return Regex.Replace(
                    content,
                    casePattern,
                    "CaseSensitive = " +
                    ToLuaBoolean(caseSensitive),
                    RegexOptions.IgnoreCase);
            }

            string ignoreCasePattern =
                @"\bIgnoreCase\s*=\s*(?:true|false)";

            if (Regex.IsMatch(
                content,
                ignoreCasePattern,
                RegexOptions.IgnoreCase))
            {
                return Regex.Replace(
                    content,
                    ignoreCasePattern,
                    "IgnoreCase = " +
                    ToLuaBoolean(!caseSensitive),
                    RegexOptions.IgnoreCase);
            }

            return "CaseSensitive = " +
                ToLuaBoolean(caseSensitive) +
                Environment.NewLine +
                content;
        }

        private static string ReplaceNamedSection(
            string content,
            string sectionName,
            string replacement)
        {
            Match match = Regex.Match(
                content,
                @"\b" + Regex.Escape(sectionName) + @"\s*=\s*\{",
                RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return content.TrimEnd() +
                    Environment.NewLine +
                    Environment.NewLine +
                    replacement +
                    Environment.NewLine;
            }

            int openingBrace =
                content.IndexOf('{', match.Index);

            int closingBrace =
                FindMatchingBrace(
                    content,
                    openingBrace);

            int start = match.Index;
            int length = closingBrace - start + 1;

            return content.Remove(start, length)
                .Insert(start, replacement);
        }

        private static int FindMatchingBrace(
            string content,
            int openingBraceIndex)
        {
            int depth = 0;
            bool insideString = false;
            char stringDelimiter = '\0';
            bool escaped = false;

            for (int index = openingBraceIndex;
                index < content.Length;
                index++)
            {
                char current = content[index];

                if (insideString)
                {
                    if (escaped)
                    {
                        escaped = false;
                        continue;
                    }

                    if (current == '\\')
                    {
                        escaped = true;
                        continue;
                    }

                    if (current == stringDelimiter)
                    {
                        insideString = false;
                    }

                    continue;
                }

                if (TrySkipLuaComment(content, ref index) ||
                    TrySkipLuaLongBracket(content, ref index))
                {
                    continue;
                }

                if (current == '"' || current == '\'')
                {
                    insideString = true;
                    stringDelimiter = current;
                    continue;
                }

                if (current == '{')
                {
                    depth++;
                }
                else if (current == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return index;
                    }
                }
            }

            throw new InvalidDataException(
                "The language definition contains unbalanced braces.");
        }

        private static bool TrySkipLuaLongBracket(
            string content,
            ref int index)
        {
            if (content[index] != '[')
            {
                return false;
            }

            int cursor = index + 1;

            while (cursor < content.Length &&
                   content[cursor] == '=')
            {
                cursor++;
            }

            if (cursor >= content.Length ||
                content[cursor] != '[')
            {
                return false;
            }

            int equalsCount = cursor - index - 1;
            string closing =
                "]" + new string('=', equalsCount) + "]";

            int closeIndex = content.IndexOf(
                closing,
                cursor + 1,
                StringComparison.Ordinal);

            if (closeIndex < 0)
            {
                throw new InvalidDataException(
                    "The language definition contains an unterminated Lua long bracket.");
            }

            index = closeIndex + closing.Length - 1;
            return true;
        }

        private static bool TrySkipLuaComment(
            string content,
            ref int index)
        {
            if (index + 1 >= content.Length ||
                content[index] != '-' ||
                content[index + 1] != '-')
            {
                return false;
            }

            int longBracketStart = index + 2;

            if (longBracketStart < content.Length &&
                content[longBracketStart] == '[')
            {
                int temp = longBracketStart;

                if (TrySkipLuaLongBracket(
                    content,
                    ref temp))
                {
                    index = temp;
                    return true;
                }
            }

            int lineEnd = content.IndexOf(
                '\n',
                index + 2);

            index = lineEnd < 0
                ? content.Length - 1
                : lineEnd;

            return true;
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