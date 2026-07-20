using GenerateHighlightContent.LanguageDefinitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Readers
{
    public class HighlightLanguageDefinitionReader
        : ILanguageDefinitionReader
    {
        public HighlightLanguageDefinition Read(
            string filePath)
        {
            ValidateFilePath(
                filePath);

            string content =
                File.ReadAllText(filePath);

            string keywordsContent =
                ExtractKeywordsContent(content);

            List<string> groupBlocks =
                ExtractTopLevelGroupBlocks(
                    keywordsContent);

            var definition =
                new HighlightLanguageDefinition
                {
                    Language =
                        Path.GetFileNameWithoutExtension(
                            filePath),

                    Description =
                        ReadStringProperty(
                            content,
                            "Description"),

                    CaseSensitive =
                        ReadBooleanProperty(
                            content,
                            "CaseSensitive") ?? false
                };

            foreach (string extension in ReadStringListProperty(
                content,
                "Extensions"))
            {
                AddUniqueValue(
                    definition.Extensions,
                    extension);
            }

            foreach (string groupBlock in groupBlocks)
            {
                ReadGroup(
                    definition,
                    groupBlock);
            }

            return definition;
        }

        private static void ValidateFilePath(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "The language definition path cannot be empty.",
                    nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The language definition file was not found.",
                    filePath);
            }
        }

        private static string ExtractKeywordsContent(
            string content)
        {
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

            return keywordsContent;
        }

        private static string ExtractNamedSection(
            string content,
            string sectionName)
        {
            Match sectionStart =
                Regex.Match(
                    content,
                    @"\b" +
                    Regex.Escape(sectionName) +
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

                if (depth < 0)
                {
                    throw new InvalidDataException(
                        "The section contains unbalanced braces.");
                }

                if (depth == 0 &&
                    blockStart >= 0)
                {
                    blocks.Add(
                        sectionContent.Substring(
                            blockStart,
                            index - blockStart + 1));

                    blockStart = -1;
                }
            }

            if (depth != 0)
            {
                throw new InvalidDataException(
                    "The section contains unbalanced braces.");
            }

            return blocks;
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

            foreach (string word in ReadWords(groupBlock))
            {
                AddUniqueValue(
                    group.Words,
                    word);
            }

            string regex =
                ReadRegex(
                    groupBlock);

            if (!string.IsNullOrWhiteSpace(regex))
            {
                AddUniqueValue(
                    group.Regex,
                    regex);
            }
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

            int id;

            if (!int.TryParse(
                match.Groups["id"].Value,
                out id))
            {
                throw new InvalidDataException(
                    "A keyword group contains an invalid Id.");
            }

            return id;
        }

        private static bool? ReadBooleanProperty(
            string content,
            string propertyName)
        {
            Match match =
                Regex.Match(
                    content,
                    @"\b" +
                    Regex.Escape(propertyName) +
                    @"\s*=\s*(?<value>true|false)",
                    RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return null;
            }

            bool value;

            if (!bool.TryParse(
                match.Groups["value"].Value,
                out value))
            {
                return null;
            }

            return value;
        }

        private static string ReadStringProperty(
            string content,
            string propertyName)
        {
            Match match =
                Regex.Match(
                    content,
                    @"\b" +
                    Regex.Escape(propertyName) +
                    @"\s*=\s*[""'](?<value>(?:\\.|[^""'])*)[""']",
                    RegexOptions.IgnoreCase |
                    RegexOptions.Singleline);

            if (!match.Success)
            {
                return null;
            }

            return UnescapeQuotedValue(
                match.Groups["value"].Value);
        }

        private static IEnumerable<string> ReadStringListProperty(
            string content,
            string propertyName)
        {
            Match listMatch =
                Regex.Match(
                    content,
                    @"\b" +
                    Regex.Escape(propertyName) +
                    @"\s*=\s*\{(?<content>.*?)\}",
                    RegexOptions.IgnoreCase |
                    RegexOptions.Singleline);

            if (!listMatch.Success)
            {
                return Enumerable.Empty<string>();
            }

            return ReadQuotedValues(
                listMatch.Groups["content"].Value);
        }

        private static IEnumerable<string> ReadWords(
            string groupBlock)
        {
            Match listMatch =
                Regex.Match(
                    groupBlock,
                    @"\bList\s*=\s*\{(?<content>.*?)\}",
                    RegexOptions.IgnoreCase |
                    RegexOptions.Singleline);

            if (!listMatch.Success)
            {
                return Enumerable.Empty<string>();
            }

            return ReadQuotedValues(
                listMatch.Groups["content"].Value);
        }

        private static IEnumerable<string> ReadQuotedValues(
            string content)
        {
            MatchCollection matches =
                Regex.Matches(
                    content,
                    @"[""'](?<value>(?:\\.|[^""'\\])*)[""']",
                    RegexOptions.Singleline);

            return matches
                .Cast<Match>()
                .Select(
                    match => UnescapeQuotedValue(
                        match.Groups["value"].Value))
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
                   char.IsWhiteSpace(groupBlock[valueStart]))
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
            if (startIndex >= content.Length ||
                content[startIndex] != '[')
            {
                throw new InvalidDataException(
                    "A Regex value does not use a supported " +
                    "Lua long-bracket expression.");
            }

            int index =
                startIndex + 1;

            int equalsCount = 0;

            while (index < content.Length &&
                   content[index] == '=')
            {
                equalsCount++;
                index++;
            }

            if (index >= content.Length ||
                content[index] != '[')
            {
                throw new InvalidDataException(
                    "A Regex value has an invalid opening delimiter.");
            }

            string closingDelimiter =
                "]" +
                new string(
                    '=',
                    equalsCount) +
                "]";

            int valueStart =
                index + 1;

            int valueEnd =
                content.IndexOf(
                    closingDelimiter,
                    valueStart,
                    StringComparison.Ordinal);

            if (valueEnd < 0)
            {
                throw new InvalidDataException(
                    "A Regex value has no closing delimiter.");
            }

            return content.Substring(
                valueStart,
                valueEnd - valueStart);
        }

        private static void AddUniqueValue(
            IList<string> values,
            string value)
        {
            if (values == null ||
                string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (values.Contains(
                value,
                StringComparer.Ordinal))
            {
                return;
            }

            values.Add(
                value);
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

                if (depth < 0)
                {
                    break;
                }
            }

            throw new InvalidDataException(
                "The section has no matching closing brace.");
        }

        private static string UnescapeQuotedValue(
            string value)
        {
            var result =
                new StringBuilder();

            bool escaped = false;

            foreach (char current in value)
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
                        result.Append('"');
                        break;

                    case '\'':
                        result.Append('\'');
                        break;

                    case '\\':
                        result.Append('\\');
                        break;

                    default:
                        result.Append('\\');
                        result.Append(
                            current);
                        break;
                }

                escaped = false;
            }

            if (escaped)
            {
                result.Append('\\');
            }

            return result.ToString();
        }
    }
}