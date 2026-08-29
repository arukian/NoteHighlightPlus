using GenerateHighlightContent.LanguageDefinitions;
using NoteHighlightAddin.Highlighting.KeywordGroups.Readers;
using NoteHighlightAddin.Highlighting.KeywordGroups.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Testing
{
    public sealed class HighlightLanguageRoundTripTester
    {
        private readonly ILanguageDefinitionReader reader;
        private readonly ILanguageDefinitionWriter writer;
        private readonly HighlightLanguageMapper mapper;

        public HighlightLanguageRoundTripTester()
            : this(
                new HighlightLanguageDefinitionReader(),
                new HighlightLanguageDefinitionWriter(),
                new HighlightLanguageMapper())
        {
        }

        public HighlightLanguageRoundTripTester(
            ILanguageDefinitionReader reader,
            ILanguageDefinitionWriter writer,
            HighlightLanguageMapper mapper)
        {
            this.reader =
                reader ?? throw new ArgumentNullException(
                    nameof(reader));

            this.writer =
                writer ?? throw new ArgumentNullException(
                    nameof(writer));

            this.mapper =
                mapper ?? throw new ArgumentNullException(
                    nameof(mapper));
        }

        public RoundTripTestResult Test(
            string sourceFilePath)
        {
            ValidateSourceFile(
                sourceFilePath);

            string temporaryFilePath =
                CreateTemporaryFilePath(
                    sourceFilePath);

            try
            {
                HighlightLanguageDefinition originalDefinition =
                    reader.Read(
                        sourceFilePath);

                EditableLanguageConfiguration editableConfiguration =
                    mapper.ToEditableConfiguration(
                        originalDefinition);

                HighlightLanguageDefinition mappedDefinition =
                    mapper.ToLanguageDefinition(
                        editableConfiguration);

                writer.Write(
                    mappedDefinition,
                    temporaryFilePath);

                HighlightLanguageDefinition generatedDefinition =
                    reader.Read(
                        temporaryFilePath);

                List<string> differences =
                    CompareDefinitions(
                        originalDefinition,
                        generatedDefinition);

                ComparePreservedSyntaxSections(
                    differences,
                    File.ReadAllText(sourceFilePath),
                    File.ReadAllText(temporaryFilePath));

                return new RoundTripTestResult
                {
                    SourceFilePath =
                        sourceFilePath,

                    GeneratedFilePath =
                        temporaryFilePath,

                    IsEquivalent =
                        differences.Count == 0,

                    Differences =
                        differences
                };
            }
            catch
            {
                DeleteFileIfExists(
                    temporaryFilePath);

                throw;
            }
        }

        private static List<string> CompareDefinitions(
            HighlightLanguageDefinition expected,
            HighlightLanguageDefinition actual)
        {
            var differences =
                new List<string>();

            CompareValue(
                differences,
                "Description",
                expected.Description,
                actual.Description);

            if (expected.CaseSensitive != actual.CaseSensitive)
            {
                differences.Add(
                    $"CaseSensitive differs. " +
                    $"Expected: {expected.CaseSensitive}. " +
                    $"Actual: {actual.CaseSensitive}.");
            }

            CompareStringCollections(
                differences,
                "Extensions",
                expected.Extensions,
                actual.Extensions);

            CompareGroups(
                differences,
                expected.Groups,
                actual.Groups);

            return differences;
        }

        private static void ComparePreservedSyntaxSections(
            List<string> differences,
            string expectedContent,
            string actualContent)
        {
            string[] sectionNames =
            {
                "Strings",
                "Comments",
                "Operators",
                "PreProcessor"
            };

            foreach (string sectionName in sectionNames)
            {
                bool expectedContains =
                    ContainsSection(
                        expectedContent,
                        sectionName);

                bool actualContains =
                    ContainsSection(
                        actualContent,
                        sectionName);

                if (expectedContains && !actualContains)
                {
                    differences.Add(
                        $"Generated definition lost preserved section '{sectionName}'.");
                }
            }
        }

        private static bool ContainsSection(
            string content,
            string sectionName)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }

            return System.Text.RegularExpressions.Regex.IsMatch(
                content,
                @"\b" +
                System.Text.RegularExpressions.Regex.Escape(sectionName) +
                @"\s*=\s*\{",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private static void CompareGroups(
            List<string> differences,
            IEnumerable<HighlightKeywordGroup> expectedGroups,
            IEnumerable<HighlightKeywordGroup> actualGroups)
        {
            Dictionary<int, HighlightKeywordGroup> expectedById =
                CreateGroupDictionary(
                    expectedGroups,
                    "expected",
                    differences);

            Dictionary<int, HighlightKeywordGroup> actualById =
                CreateGroupDictionary(
                    actualGroups,
                    "actual",
                    differences);

            int[] allIds =
                expectedById.Keys
                    .Union(actualById.Keys)
                    .OrderBy(id => id)
                    .ToArray();

            foreach (int id in allIds)
            {
                HighlightKeywordGroup expectedGroup;
                HighlightKeywordGroup actualGroup;

                bool expectedExists =
                    expectedById.TryGetValue(
                        id,
                        out expectedGroup);

                bool actualExists =
                    actualById.TryGetValue(
                        id,
                        out actualGroup);

                if (!expectedExists)
                {
                    differences.Add(
                        $"Generated definition contains unexpected " +
                        $"keyword group {id}.");

                    continue;
                }

                if (!actualExists)
                {
                    differences.Add(
                        $"Generated definition is missing " +
                        $"keyword group {id}.");

                    continue;
                }

                CompareStringCollections(
                    differences,
                    $"Group {id} words",
                    expectedGroup.Words,
                    actualGroup.Words);

                CompareStringCollections(
                    differences,
                    $"Group {id} regex",
                    expectedGroup.Regex,
                    actualGroup.Regex);
            }
        }

        private static Dictionary<int, HighlightKeywordGroup>
            CreateGroupDictionary(
                IEnumerable<HighlightKeywordGroup> groups,
                string collectionName,
                List<string> differences)
        {
            var result =
                new Dictionary<int, HighlightKeywordGroup>();

            if (groups == null)
            {
                differences.Add(
                    $"The {collectionName} group collection is null.");

                return result;
            }

            foreach (HighlightKeywordGroup group in groups)
            {
                if (group == null)
                {
                    differences.Add(
                        $"The {collectionName} group collection " +
                        "contains a null group.");

                    continue;
                }

                if (result.ContainsKey(group.Id))
                {
                    differences.Add(
                        $"The {collectionName} definition contains " +
                        $"duplicate group Id {group.Id}.");

                    continue;
                }

                result.Add(
                    group.Id,
                    group);
            }

            return result;
        }

        private static void CompareStringCollections(
            List<string> differences,
            string propertyName,
            IEnumerable<string> expected,
            IEnumerable<string> actual)
        {
            List<string> expectedValues =
                NormalizeCollection(
                    expected);

            List<string> actualValues =
                NormalizeCollection(
                    actual);

            string[] missingValues =
                expectedValues
                    .Except(
                        actualValues,
                        StringComparer.Ordinal)
                    .ToArray();

            string[] unexpectedValues =
                actualValues
                    .Except(
                        expectedValues,
                        StringComparer.Ordinal)
                    .ToArray();

            foreach (string missingValue in missingValues)
            {
                differences.Add(
                    $"{propertyName} is missing value: " +
                    $"'{missingValue}'.");
            }

            foreach (string unexpectedValue in unexpectedValues)
            {
                differences.Add(
                    $"{propertyName} contains unexpected value: " +
                    $"'{unexpectedValue}'.");
            }

            if (expectedValues.Count != actualValues.Count &&
                missingValues.Length == 0 &&
                unexpectedValues.Length == 0)
            {
                differences.Add(
                    $"{propertyName} contains duplicated values. " +
                    $"Expected count: {expectedValues.Count}. " +
                    $"Actual count: {actualValues.Count}.");
            }
        }

        private static List<string> NormalizeCollection(
            IEnumerable<string> values)
        {
            if (values == null)
            {
                return new List<string>();
            }

            return values
                .Where(value =>
                    value != null)
                .ToList();
        }

        private static void CompareValue(
            List<string> differences,
            string propertyName,
            string expected,
            string actual)
        {
            if (string.Equals(
                expected,
                actual,
                StringComparison.Ordinal))
            {
                return;
            }

            differences.Add(
                $"{propertyName} differs. " +
                $"Expected: '{expected ?? "<null>"}'. " +
                $"Actual: '{actual ?? "<null>"}'.");
        }

        private static string CreateTemporaryFilePath(
            string sourceFilePath)
        {
            string languageName =
                Path.GetFileNameWithoutExtension(
                    sourceFilePath);

            return Path.Combine(
                Path.GetTempPath(),
                $"{languageName}.roundtrip.{Guid.NewGuid():N}.lang");
        }

        private static void ValidateSourceFile(
            string sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                throw new ArgumentException(
                    "The source language definition path cannot be empty.",
                    nameof(sourceFilePath));
            }

            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException(
                    "The source language definition file was not found.",
                    sourceFilePath);
            }
        }

        private static void DeleteFileIfExists(
            string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath) &&
                File.Exists(filePath))
            {
                File.Delete(
                    filePath);
            }
        }
    }
}