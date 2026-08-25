using GenerateHighlightContent;
using Infrastructure.Core;
using NoteHighlightAddin;
using NoteHighlightAddin.Highlighting.KeywordGroups;
using NoteHighlightAddin.Highlighting.KeywordGroups.Services;
using NoteHighlightAddin.Highlighting.Preview.Services;
using NoteHighlightAddin.Highlighting.Themes;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;
using GenerateHighlightContent;

namespace GenerateHighlightContent.TestConsole
{
    internal static class Program
    {

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            TestHighlightLanguageSource();

            Console.WriteLine(
                "Press any key to finish.");

            Console.ReadKey();
        }
        




        private static void TestRealThemeRoundTrip()
        {
            Console.WriteLine(
                "=== Real theme roundtrip test ===");

            string sourcePath =
                Path.Combine(
                    PathManager.ThemesFolder,
                    "shinx.theme");

            string outputPath =
                Path.Combine(
                    PathManager.ThemesFolder,
                    "shinx-roundtrip.theme");

            IHighlightThemeReader reader =
                new HighlightThemeReader();

            IHighlightThemeSerializer serializer =
                new HighlightThemeSerializer();

            HighlightTheme original =
                reader.Read(
                    sourcePath);

            serializer.Serialize(
                original,
                outputPath);

            HighlightTheme roundTrip =
                reader.Read(
                    outputPath);

            CompareThemes(
                original,
                roundTrip);

            CompareCategories(
                original,
                roundTrip);

            CompareSemanticTokenTypes(
                original,
                roundTrip);

            Console.WriteLine();
            Console.WriteLine(
                "Roundtrip file:");

            Console.WriteLine(
                outputPath);

            Console.WriteLine();
            Console.WriteLine(
                "RESULT: REAL THEME ROUNDTRIP PASSED");

            Console.WriteLine(
                "=== Real theme roundtrip test finished ===");

            Console.WriteLine();
        }

        private static void CompareCategories(
            HighlightTheme expected,
            HighlightTheme actual)
        {
            AssertEqual(
                "Category count",
                expected.Categories.Count,
                actual.Categories.Count);

            for (int index = 0;
                index < expected.Categories.Count;
                index++)
            {
                AssertEqual(
                    "Category[" + index + "]",
                    expected.Categories[index],
                    actual.Categories[index]);
            }
        }

        private static void CompareSemanticTokenTypes(
            HighlightTheme expected,
            HighlightTheme actual)
        {
            AssertEqual(
                "Semantic token count",
                expected.SemanticTokenTypes.Count,
                actual.SemanticTokenTypes.Count);

            for (int index = 0;
                index < expected.SemanticTokenTypes.Count;
                index++)
            {
                SemanticTokenStyle expectedToken =
                    expected.SemanticTokenTypes[index];

                SemanticTokenStyle actualToken =
                    actual.SemanticTokenTypes[index];

                AssertEqual(
                    "SemanticToken[" + index + "].Type",
                    expectedToken.Type,
                    actualToken.Type);

                AssertEqual(
                    "SemanticToken[" + index + "].StyleReference",
                    expectedToken.StyleReference,
                    actualToken.StyleReference);
            }
        }


        private static void TestThemeCategoriesAndSemanticTokens()
        {
            Console.WriteLine(
                "=== Theme categories and semantic tokens test ===");

            string themePath =
                Path.Combine(
                    PathManager.ThemesFolder,
                    "shinx.theme");

            Console.WriteLine(
                "Theme file: "
                + themePath);

            IHighlightThemeReader reader =
                new HighlightThemeReader();

            HighlightTheme theme =
                reader.Read(
                    themePath);

            Console.WriteLine();
            Console.WriteLine(
                "Theme: "
                + theme.Name);

            Console.WriteLine(
                "Description: "
                + (theme.Description ?? "<none>"));

            Console.WriteLine();
            Console.WriteLine(
                "Categories: "
                + theme.Categories.Count);

            foreach (string category
                in theme.Categories)
            {
                Console.WriteLine(
                    "  "
                    + category);
            }

            Console.WriteLine();
            Console.WriteLine(
                "Semantic token types: "
                + theme.SemanticTokenTypes.Count);

            foreach (SemanticTokenStyle token
                in theme.SemanticTokenTypes)
            {
                Console.WriteLine(
                    "  "
                    + token.Type
                    + " -> "
                    + token.StyleReference);
            }

            Console.WriteLine();
            Console.WriteLine(
                "=== Theme categories and semantic tokens test finished ===");

            Console.WriteLine();
        }


        private static void TestThemeRoundTrip()
        {
            Console.WriteLine(
                "=== Theme roundtrip test ===");

            string sourcePath =
                Path.Combine(
                    PathManager.ThemesFolder,
                    "theme-model-test.theme");

            string outputPath =
                Path.Combine(
                    PathManager.ThemesFolder,
                    "theme-roundtrip.theme");

            IHighlightThemeReader reader =
                new HighlightThemeReader();

            IHighlightThemeSerializer serializer =
                new HighlightThemeSerializer();

            HighlightTheme original =
                reader.Read(
                    sourcePath);

            serializer.Serialize(
                original,
                outputPath);

            HighlightTheme roundTrip =
                reader.Read(
                    outputPath);

            CompareThemes(
                original,
                roundTrip);

            Console.WriteLine();
            Console.WriteLine(
                "Roundtrip file:");

            Console.WriteLine(
                outputPath);

            Console.WriteLine();
            Console.WriteLine(
                "RESULT: ROUNDTRIP PASSED");

            Console.WriteLine(
                "=== Theme roundtrip test finished ===");

            Console.WriteLine();
        }

        private static void CompareThemes(
            HighlightTheme expected,
            HighlightTheme actual)
        {
            AssertEqual(
                "Description",
                expected.Description,
                actual.Description);

            AssertEqual(
                "Variable count",
                expected.Variables.Count,
                actual.Variables.Count);

            foreach (var variable
                in expected.Variables)
            {
                string actualValue;

                if (!actual.Variables.TryGetValue(
                    variable.Key,
                    out actualValue))
                {
                    throw new InvalidOperationException(
                        "Missing variable: "
                        + variable.Key);
                }

                AssertEqual(
                    "Variable " + variable.Key,
                    variable.Value,
                    actualValue);
            }

            AssertEqual(
                "Alias count",
                expected.StyleAliases.Count,
                actual.StyleAliases.Count);

            foreach (var alias
                in expected.StyleAliases)
            {
                string actualTarget;

                if (!actual.StyleAliases.TryGetValue(
                    alias.Key,
                    out actualTarget))
                {
                    throw new InvalidOperationException(
                        "Missing alias: "
                        + alias.Key);
                }

                AssertEqual(
                    "Alias " + alias.Key,
                    alias.Value,
                    actualTarget);
            }

            AssertEqual(
                "General style count",
                expected.Styles.Count,
                actual.Styles.Count);

            foreach (var styleEntry
                in expected.Styles)
            {
                ThemeStyle actualStyle;

                if (!actual.Styles.TryGetValue(
                    styleEntry.Key,
                    out actualStyle))
                {
                    throw new InvalidOperationException(
                        "Missing style: "
                        + styleEntry.Key);
                }

                CompareStyle(
                    "Style " + styleEntry.Key,
                    styleEntry.Value,
                    actualStyle);
            }

            AssertEqual(
                "Keyword style count",
                expected.KeywordStyles.Count,
                actual.KeywordStyles.Count);

            for (int index = 0;
                index < expected.KeywordStyles.Count;
                index++)
            {
                CompareStyle(
                    "Keywords[" + (index + 1) + "]",
                    expected.KeywordStyles[index],
                    actual.KeywordStyles[index]);
            }
        }

        private static void CompareStyle(
            string label,
            ThemeStyle expected,
            ThemeStyle actual)
        {
            AssertEqual(
                label + ".Colour",
                expected.Colour,
                actual.Colour);

            AssertEqual(
                label + ".ColourReference",
                expected.ColourReference,
                actual.ColourReference);

            AssertEqual(
                label + ".Bold",
                expected.Bold,
                actual.Bold);

            AssertEqual(
                label + ".Italic",
                expected.Italic,
                actual.Italic);
        }

        private static void AssertEqual<T>(
            string label,
            T expected,
            T actual)
        {
            if (!object.Equals(
                expected,
                actual))
            {
                throw new InvalidOperationException(
                    label
                    + " mismatch. Expected: "
                    + FormatValue(expected)
                    + ", Actual: "
                    + FormatValue(actual));
            }

            Console.WriteLine(
                "PASS: "
                + label);
        }

        private static string FormatValue<T>(
            T value)
        {
            if (object.Equals(
                value,
                null))
            {
                return "<null>";
            }

            return value.ToString();
        }


        private static void TestThemeReaderModel()
        {
            Console.WriteLine(
                "=== Theme reader model test ===");

            string themePath =
                Path.Combine(
                    PathManager.ThemesFolder,
                    "shinx.theme");

            Console.WriteLine(
                "Theme file: "
                + themePath);

            IHighlightThemeReader reader =
                new HighlightThemeReader();

            HighlightTheme theme =
                reader.Read(
                    themePath);

            Console.WriteLine();
            Console.WriteLine(
                "Theme: "
                + theme.Name);

            Console.WriteLine(
                "Description: "
                + (theme.Description ?? "<none>"));

            Console.WriteLine();
            Console.WriteLine(
                "Variables: "
                + theme.Variables.Count);

            foreach (var variable
                in theme.Variables)
            {
                Console.WriteLine(
                    "  "
                    + variable.Key
                    + " = "
                    + variable.Value);
            }

            Console.WriteLine();
            Console.WriteLine(
                "Aliases: "
                + theme.StyleAliases.Count);

            foreach (var alias
                in theme.StyleAliases)
            {
                Console.WriteLine(
                    "  "
                    + alias.Key
                    + " -> "
                    + alias.Value);
            }

            Console.WriteLine();
            Console.WriteLine(
                "General styles: "
                + theme.Styles.Count);

            foreach (var styleEntry
                in theme.Styles)
            {
                ThemeStyle style =
                    styleEntry.Value;

                Console.WriteLine(
                    "  "
                    + style.Name
                    + " | Colour="
                    + style.Colour
                    + " | Reference="
                    + (style.ColourReference ?? "<literal>")
                    + " | Bold="
                    + style.Bold
                    + " | Italic="
                    + style.Italic);
            }

            Console.WriteLine();
            Console.WriteLine(
                "Keyword styles: "
                + theme.KeywordStyles.Count);

            foreach (ThemeStyle style
                in theme.KeywordStyles)
            {
                Console.WriteLine(
                    "  "
                    + style.Name
                    + " | Colour="
                    + style.Colour
                    + " | Reference="
                    + (style.ColourReference ?? "<literal>")
                    + " | Bold="
                    + style.Bold
                    + " | Italic="
                    + style.Italic);
            }

            Console.WriteLine();
            Console.WriteLine(
                "=== Theme reader model test finished ===");

            Console.WriteLine();
        }


        private static void PrintAssemblyLocations()
        {
            Console.WriteLine(
                "=== Assembly locations ===");

            Console.WriteLine(
                "GenerateHighlightContent:");

            Console.WriteLine(
                typeof(GenerateHighLight)
                    .Assembly.Location);

            Console.WriteLine();

            Console.WriteLine(
                "Infrastructure.Core:");

            Console.WriteLine(
                typeof(PathManager)
                    .Assembly.Location);

            Console.WriteLine();

            Console.WriteLine(
                "NoteHighlightAddin:");

            Console.WriteLine(
                typeof(PreviewHtmlService)
                    .Assembly.Location);
        }

        private static void PrintRequiredPaths()
        {
            Console.WriteLine();

            Console.WriteLine(
                "Highlight folder:");

            Console.WriteLine(
                PathManager.HighlightFolder);

            Console.WriteLine();

            Console.WriteLine(
                "Languages folder:");

            Console.WriteLine(
                PathManager.LanguagesFolder);

            Console.WriteLine();

            Console.WriteLine(
                "python.lang exists: "
                + File.Exists(
                    Path.Combine(
                        PathManager.LanguagesFolder,
                        "python.lang")));

            Console.WriteLine(
                "shinx.theme exists: "
                + File.Exists(
                    Path.Combine(
                        PathManager.ThemesFolder,
                        "shinx.theme")));
        }

        private static EditableLanguageConfiguration
            LoadPythonConfiguration()
        {
            var languageEditorService =
                new LanguageEditorService();

            return languageEditorService.Load(
                "python");
        }

        private static void PrintConfigurationSummary(
            EditableLanguageConfiguration configuration)
        {
            Console.WriteLine();

            Console.WriteLine(
                "=== Editable configuration ===");

            Console.WriteLine(
                "Language: "
                + configuration.Language);

            Console.WriteLine(
                "Description: "
                + configuration.Description);

            Console.WriteLine(
                "Case sensitive: "
                + configuration.CaseSensitive);

            Console.WriteLine(
                "Groups: "
                + configuration.Groups.Count);
        }



        private static void TestRealLanguageRoundTrip()
        {
            Console.WriteLine(
                "=== Real language roundtrip test ===");

            string sourcePath =
                Path.Combine(
                    PathManager.LanguagesFolder,
                    "python.lang");

            string roundTripFolder = Path.Combine(
        Path.GetTempPath(),
        "NoteHighlight+",
        "LanguageRoundTrip",
        Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(
                roundTripFolder);

            string outputPath =
                Path.Combine(
                    roundTripFolder,
                    "python.lang");

            Console.WriteLine(
                "Source:");

            Console.WriteLine(
                sourcePath);

            Console.WriteLine();

            var service =
                new LanguageEditorService();

            EditableLanguageConfiguration original =
                service.LoadFromFile(
                    sourcePath);

            Console.WriteLine(
                "Language: "
                + original.Language);

            Console.WriteLine(
                "Groups: "
                + original.Groups.Count);

            service.SaveAs(
                original,
                outputPath);

            EditableLanguageConfiguration roundTrip =
                service.LoadFromFile(
                    outputPath);

            CompareLanguages(
                original,
                roundTrip);

            Console.WriteLine();
            Console.WriteLine(
                "Roundtrip file:");

            Console.WriteLine(
                outputPath);

            Console.WriteLine();
            Console.WriteLine(
                "RESULT: REAL LANGUAGE ROUNDTRIP PASSED");

            Console.WriteLine(
                "=== Real language roundtrip test finished ===");

            Console.WriteLine();
        }

        private static void TestHighlightLanguageSource()
        {
            Console.WriteLine(
                "=== Highlight language source test ===");

            var parameter =
                new HighLightParameter
                {
                    FileName = "language-source-test.py",

                    Content =
                        "NHP_APPDATA_TEST\r\n" +
                        "if True:\r\n" +
                        "    print(\"test\")",

                    CodeType =
                        "python",

                    HighLightStyle =
                        "shinx",

                    ShowLineNumber =
                        false,

                    Font =
                        "Consolas",

                    FontSize =
                        10
                };

            var generator =
                new GenerateHighLight();

            string outputFile =
                generator.GenerateHighLightCode(
                    parameter);

            Console.WriteLine();
            Console.WriteLine(
                "Generated output:");

            Console.WriteLine(
                outputFile);

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = outputFile,
                    UseShellExecute = true
                });

            Console.WriteLine();
            Console.WriteLine(
                "=== Highlight language source test finished ===");
        }

        private static void CompareLanguages(
    EditableLanguageConfiguration expected,
    EditableLanguageConfiguration actual)
        {
            AssertEqual(
                "Language",
                expected.Language,
                actual.Language);

            AssertEqual(
                "Description",
                expected.Description,
                actual.Description);

            AssertEqual(
                "CaseSensitive",
                expected.CaseSensitive,
                actual.CaseSensitive);

            AssertEqual(
                "Extension count",
                expected.Extensions.Count,
                actual.Extensions.Count);

            for (int index = 0;
                index < expected.Extensions.Count;
                index++)
            {
                AssertEqual(
                    "Extension[" + index + "]",
                    expected.Extensions[index],
                    actual.Extensions[index]);
            }

            AssertEqual(
                "Group count",
                expected.Groups.Count,
                actual.Groups.Count);

            foreach (KeywordGroupConfiguration expectedGroup
                in expected.Groups)
            {
                KeywordGroupConfiguration actualGroup =
                    actual.Groups.FirstOrDefault(
                        group =>
                            group.Id == expectedGroup.Id);

                if (actualGroup == null)
                {
                    throw new InvalidOperationException(
                        "Missing group: "
                        + expectedGroup.Id);
                }

                Console.WriteLine();
                Console.WriteLine(
                    "--- Group "
                    + expectedGroup.Id
                    + " ---");

                AssertEqual(
                    "Group " + expectedGroup.Id + " word count",
                    expectedGroup.Words.Count,
                    actualGroup.Words.Count);

                foreach (string word
                    in expectedGroup.Words)
                {
                    bool exists =
                        actualGroup.Words.Contains(
                            word);

                    AssertEqual(
                        "Group "
                        + expectedGroup.Id
                        + " word: "
                        + word,
                        true,
                        exists);
                }

                AssertEqual(
                    "Group " + expectedGroup.Id + " regex count",
                    expectedGroup.Regex.Count,
                    actualGroup.Regex.Count);

                foreach (string regex
                    in expectedGroup.Regex)
                {
                    bool exists =
                        actualGroup.Regex.Contains(
                            regex);

                    AssertEqual(
                        "Group "
                        + expectedGroup.Id
                        + " regex",
                        true,
                        exists);
                }
            }
        }

        private static HighLightParameter
            CreatePreviewParameter()
        {
            return new HighLightParameter
            {
                FileName =
                    "notehighlight_preview.py",

                Content = "NHP_APPDATA_TEST",

                CodeType =
                    "python",

                HighLightStyle =
                    "shinx",

                ShowLineNumber =
                    true,

                HighlightColor =
                    Color.Transparent,

                Font =
                    "Consolas",

                FontSize =
                    10
            };
        }



        private static string GeneratePreview(
            EditableLanguageConfiguration configuration,
            HighLightParameter parameter)
        {
            IPreviewHtmlService previewService =
                new PreviewHtmlService();

            return previewService.GeneratePreviewHtml(
                configuration,
                parameter);
        }

        private static void ValidateHtml(
            string htmlPath)
        {
            if (string.IsNullOrWhiteSpace(
                htmlPath))
            {
                throw new InvalidOperationException(
                    "The preview service returned an empty path.");
            }

            if (!File.Exists(htmlPath))
            {
                throw new FileNotFoundException(
                    "The generated preview HTML was not found.",
                    htmlPath);
            }

            FileInfo htmlFile =
                new FileInfo(
                    htmlPath);

            if (htmlFile.Length == 0)
            {
                throw new InvalidOperationException(
                    "The generated preview HTML is empty.");
            }
        }
    }
}