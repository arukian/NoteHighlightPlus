using GenerateHighlightContent.LanguageDefinitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace NoteHighlightAddin.Highlighting.KeywordGroups.Readers
{
    /// <summary>
    /// Reads highlight.exe .lang files into the structural
    /// HighlightLanguageDefinition model.
    ///
    /// The parser is Lua-aware enough to ignore braces that appear inside
    /// quoted strings, Lua long-bracket strings and Lua comments.
    /// </summary>
    public sealed class HighlightLanguageDefinitionReader
        : ILanguageDefinitionReader
    {
        public HighlightLanguageDefinition Read(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(
                filePath))
            {
                throw new ArgumentException(
                    "The language definition path cannot be empty.",
                    nameof(filePath));
            }

            if (!File.Exists(
                filePath))
            {
                throw new FileNotFoundException(
                    "The language definition file was not found.",
                    filePath);
            }

            string content =
                File.ReadAllText(
                    filePath);

            var definition =
                new HighlightLanguageDefinition
                {
                    Language =
                        Path.GetFileNameWithoutExtension(
                            filePath),

                    Description =
                        ReadDescription(
                            content)
                        ?? Path.GetFileNameWithoutExtension(
                            filePath),

                    CaseSensitive =
                        ReadCaseSensitive(
                            content)
                };

            foreach (string extension
                in ReadExtensions(
                    content))
            {
                definition.Extensions.Add(
                    extension);
            }

            string keywordsContent =
                ExtractNamedSection(
                    content,
                    "Keywords");

            if (keywordsContent == null)
            {
                throw new InvalidDataException(
                    "The language definition does not contain " +
                    "a valid Keywords section.");
            }

            List<string> groupBlocks =
                ExtractTopLevelGroupBlocks(
                    keywordsContent);

            foreach (string groupBlock
                in groupBlocks)
            {
                ReadGroup(
                    definition,
                    groupBlock);
            }

            return definition;
        }


        private static void ReadGroup(
            HighlightLanguageDefinition definition,
            string groupBlock)
        {
            int id =
                ReadGroupId(
                    groupBlock);

            HighlightKeywordGroup group =
                definition.Groups.FirstOrDefault(
                    item => item.Id == id);

            if (group == null)
            {
                group =
                    new HighlightKeywordGroup
                    {
                        Id = id
                    };

                definition.Groups.Add(
                    group);
            }

            foreach (string word
                in ReadWords(
                    groupBlock))
            {
                if (!group.Words.Contains(
                    word,
                    StringComparer.Ordinal))
                {
                    group.Words.Add(
                        word);
                }
            }

            string regex =
                ReadRegex(
                    groupBlock);

            if (!string.IsNullOrWhiteSpace(
                regex) &&
                !group.Regex.Contains(
                    regex,
                    StringComparer.Ordinal))
            {
                group.Regex.Add(
                    regex);
            }
        }


        private static string ReadDescription(
            string content)
        {
            Match match =
                Regex.Match(
                    content,
                    @"\bDescription\s*=\s*[""'](?<value>[^""']*)[""']",
                    RegexOptions.IgnoreCase);

            return match.Success
                ? match.Groups["value"].Value
                : null;
        }


        /// <summary>
        /// highlight language files exist in more than one generation.
        ///
        /// Newer files may use CaseSensitive=true/false.
        /// Many stock highlight files use IgnoreCase=true/false.
        ///
        /// IgnoreCase=false means the language IS case-sensitive.
        /// </summary>
        private static bool ReadCaseSensitive(
            string content)
        {
            bool? caseSensitive =
                ReadBooleanProperty(
                    content,
                    "CaseSensitive");

            if (caseSensitive.HasValue)
            {
                return caseSensitive.Value;
            }

            bool? ignoreCase =
                ReadBooleanProperty(
                    content,
                    "IgnoreCase");

            if (ignoreCase.HasValue)
            {
                return !ignoreCase.Value;
            }

            return true;
        }


        private static bool? ReadBooleanProperty(
            string content,
            string propertyName)
        {
            Match match =
                Regex.Match(
                    content,
                    @"\b" +
                    Regex.Escape(
                        propertyName) +
                    @"\s*=\s*(?<value>true|false)",
                    RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return null;
            }

            return string.Equals(
                match.Groups["value"].Value,
                "true",
                StringComparison.OrdinalIgnoreCase);
        }


        private static IEnumerable<string> ReadExtensions(
            string content)
        {
            string extensionsContent =
                ExtractNamedSection(
                    content,
                    "Extensions");

            if (string.IsNullOrWhiteSpace(
                extensionsContent))
            {
                return Enumerable.Empty<string>();
            }

            MatchCollection matches =
                Regex.Matches(
                    extensionsContent,
                    @"""(?<value>(?:\\.|[^""\\])*)""",
                    RegexOptions.Singleline);

            return matches
                .Cast<Match>()
                .Select(
                    match =>
                        UnescapeQuotedValue(
                            match.Groups["value"].Value))
                .Where(
                    value =>
                        !string.IsNullOrWhiteSpace(
                            value))
                .Distinct(
                    StringComparer.Ordinal)
                .ToList();
        }


        private static string ExtractNamedSection(
            string content,
            string sectionName)
        {
            Match sectionStart =
                Regex.Match(
                    content,
                    @"\b" +
                    Regex.Escape(
                        sectionName) +
                    @"\s*=\s*\{",
                    RegexOptions.IgnoreCase);

            if (!sectionStart.Success)
            {
                return null;
            }

            int openingBraceIndex =
                content.IndexOf(
                    '{',
                    sectionStart.Index);

            if (openingBraceIndex < 0)
            {
                return null;
            }

            int closingBraceIndex =
                FindMatchingBrace(
                    content,
                    openingBraceIndex);

            return content.Substring(
                openingBraceIndex + 1,
                closingBraceIndex -
                openingBraceIndex - 1);
        }


        private static List<string> ExtractTopLevelGroupBlocks(
            string sectionContent)
        {
            var blocks =
                new List<string>();

            int depth = 0;
            int blockStart = -1;

            bool insideString = false;
            char stringDelimiter = '\0';
            bool escaped = false;

            for (int index = 0;
                index < sectionContent.Length;
                index++)
            {
                char current =
                    sectionContent[index];

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

                if (TrySkipLuaComment(
                    sectionContent,
                    ref index))
                {
                    continue;
                }

                if (TrySkipLuaLongBracket(
                    sectionContent,
                    ref index))
                {
                    continue;
                }

                if (current == '"' ||
                    current == '\'')
                {
                    insideString = true;
                    stringDelimiter = current;
                    continue;
                }

                if (current == '{')
                {
                    if (depth == 0)
                    {
                        blockStart = index;
                    }

                    depth++;
                    continue;
                }

                if (current != '}')
                {
                    continue;
                }

                depth--;

                if (depth == 0 &&
                    blockStart >= 0)
                {
                    blocks.Add(
                        sectionContent.Substring(
                            blockStart,
                            index - blockStart + 1));

                    blockStart = -1;
                }

                if (depth < 0)
                {
                    throw new InvalidDataException(
                        "The section contains unbalanced braces.");
                }
            }

            if (depth != 0)
            {
                throw new InvalidDataException(
                    "The section contains unbalanced braces.");
            }

            return blocks;
        }


        private static int ReadGroupId(
            string groupBlock)
        {
            Match match =
                Regex.Match(
                    groupBlock,
                    @"\bId\s*=\s*(?<id>\d+)",
                    RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                throw new InvalidDataException(
                    "A keyword group does not contain a valid Id.");
            }

            return int.Parse(
                match.Groups["id"].Value);
        }


        private static IEnumerable<string> ReadWords(
            string groupBlock)
        {
            Match listStart =
                Regex.Match(
                    groupBlock,
                    @"\bList\s*=\s*\{",
                    RegexOptions.IgnoreCase);

            if (!listStart.Success)
            {
                return Enumerable.Empty<string>();
            }

            int openingBraceIndex =
                groupBlock.IndexOf(
                    '{',
                    listStart.Index);

            if (openingBraceIndex < 0)
            {
                return Enumerable.Empty<string>();
            }

            int closingBraceIndex =
                FindMatchingBrace(
                    groupBlock,
                    openingBraceIndex);

            string listContent =
                groupBlock.Substring(
                    openingBraceIndex + 1,
                    closingBraceIndex -
                    openingBraceIndex - 1);

            MatchCollection wordMatches =
                Regex.Matches(
                    listContent,
                    @"""(?<word>(?:\\.|[^""\\])*)""",
                    RegexOptions.Singleline);

            return wordMatches
                .Cast<Match>()
                .Select(
                    match =>
                        UnescapeQuotedValue(
                            match.Groups["word"].Value))
                .ToList();
        }


        private static string ReadRegex(
            string groupBlock)
        {
            Match assignmentMatch =
                Regex.Match(
                    groupBlock,
                    @"\bRegex\s*=",
                    RegexOptions.IgnoreCase);

            if (!assignmentMatch.Success)
            {
                return null;
            }

            int valueStart =
                assignmentMatch.Index +
                assignmentMatch.Length;

            while (valueStart < groupBlock.Length &&
                   char.IsWhiteSpace(
                       groupBlock[valueStart]))
            {
                valueStart++;
            }

            return ReadLongBracketValue(
                groupBlock,
                valueStart);
        }


        private static string ReadLongBracketValue(
            string content,
            int startIndex)
        {
            int openingContentIndex;
            int equalsCount;

            if (!TryReadLuaLongBracketOpening(
                content,
                startIndex,
                out openingContentIndex,
                out equalsCount))
            {
                throw new InvalidDataException(
                    "A Regex value does not use a supported " +
                    "Lua long-bracket expression.");
            }

            string closingDelimiter =
                "]" +
                new string(
                    '=',
                    equalsCount) +
                "]";

            int valueEnd =
                content.IndexOf(
                    closingDelimiter,
                    openingContentIndex,
                    StringComparison.Ordinal);

            if (valueEnd < 0)
            {
                throw new InvalidDataException(
                    "A Regex value has no closing delimiter.");
            }

            return content.Substring(
                openingContentIndex,
                valueEnd -
                openingContentIndex);
        }


        private static int FindMatchingBrace(
            string content,
            int openingBrace)
        {
            int depth = 0;

            bool insideString = false;
            char stringDelimiter = '\0';
            bool escaped = false;

            for (int index = openingBrace;
                index < content.Length;
                index++)
            {
                char current =
                    content[index];

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

                if (TrySkipLuaComment(
                    content,
                    ref index))
                {
                    continue;
                }

                if (TrySkipLuaLongBracket(
                    content,
                    ref index))
                {
                    continue;
                }

                if (current == '"' ||
                    current == '\'')
                {
                    insideString = true;
                    stringDelimiter = current;
                    continue;
                }

                if (current == '{')
                {
                    depth++;
                    continue;
                }

                if (current != '}')
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
                "The section has no matching closing brace.");
        }


        private static bool TrySkipLuaLongBracket(
            string content,
            ref int index)
        {
            int valueStart;
            int equalsCount;

            if (!TryReadLuaLongBracketOpening(
                content,
                index,
                out valueStart,
                out equalsCount))
            {
                return false;
            }

            string closingDelimiter =
                "]" +
                new string(
                    '=',
                    equalsCount) +
                "]";

            int closingIndex =
                content.IndexOf(
                    closingDelimiter,
                    valueStart,
                    StringComparison.Ordinal);

            if (closingIndex < 0)
            {
                throw new InvalidDataException(
                    "A Lua long-bracket value has no closing delimiter.");
            }

            index =
                closingIndex +
                closingDelimiter.Length -
                1;

            return true;
        }


        private static bool TryReadLuaLongBracketOpening(
            string content,
            int startIndex,
            out int valueStart,
            out int equalsCount)
        {
            valueStart = -1;
            equalsCount = 0;

            if (startIndex < 0 ||
                startIndex >= content.Length ||
                content[startIndex] != '[')
            {
                return false;
            }

            int cursor =
                startIndex + 1;

            while (cursor < content.Length &&
                   content[cursor] == '=')
            {
                equalsCount++;
                cursor++;
            }

            if (cursor >= content.Length ||
                content[cursor] != '[')
            {
                equalsCount = 0;
                return false;
            }

            valueStart =
                cursor + 1;

            return true;
        }


        private static bool TrySkipLuaComment(
            string content,
            ref int index)
        {
            if (index < 0 ||
                index + 1 >= content.Length ||
                content[index] != '-' ||
                content[index + 1] != '-')
            {
                return false;
            }

            int commentStart =
                index + 2;

            int longCommentIndex =
                commentStart;

            if (TrySkipLuaLongBracket(
                content,
                ref longCommentIndex))
            {
                index =
                    longCommentIndex;

                return true;
            }

            int lineEnd =
                content.IndexOf(
                    '\n',
                    commentStart);

            index =
                lineEnd < 0
                    ? content.Length - 1
                    : lineEnd;

            return true;
        }


        private static string UnescapeQuotedValue(
            string value)
        {
            var result =
                new StringBuilder();

            bool escaped = false;

            foreach (char current
                in value)
            {
                if (!escaped)
                {
                    if (current == '\\')
                    {
                        escaped = true;
                    }
                    else
                    {
                        result.Append(
                            current);
                    }

                    continue;
                }

                switch (current)
                {
                    case '"':
                        result.Append(
                            '"');
                        break;

                    case '\\':
                        result.Append(
                            '\\');
                        break;

                    default:
                        result.Append(
                            '\\');

                        result.Append(
                            current);
                        break;
                }

                escaped = false;
            }

            if (escaped)
            {
                result.Append(
                    '\\');
            }

            return result.ToString();
        }
    }
}