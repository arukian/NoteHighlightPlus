using NoteHighlightAddin.Highlighting.KeywordGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoteHighlightAddin.Highlighting.Preview.Services.Builders
{
    internal sealed class PythonPreviewSampleBuilder
        : IPreviewSampleBuilder
    {
        private const int MaximumDisplayedWords =
            20;

        private static readonly HashSet<string>
            PythonReservedWords =
                new HashSet<string>(
                    StringComparer.Ordinal)
                {
                    "and",
                    "as",
                    "assert",
                    "async",
                    "await",
                    "break",
                    "class",
                    "continue",
                    "def",
                    "del",
                    "elif",
                    "else",
                    "except",
                    "False",
                    "finally",
                    "for",
                    "from",
                    "global",
                    "if",
                    "import",
                    "in",
                    "is",
                    "lambda",
                    "None",
                    "nonlocal",
                    "not",
                    "or",
                    "pass",
                    "raise",
                    "return",
                    "True",
                    "try",
                    "while",
                    "with",
                    "yield"
                };

        private static readonly HashSet<string>
    PythonBuiltInNames =
        new HashSet<string>(
            StringComparer.Ordinal)
        {
            "abs",
            "all",
            "any",
            "ascii",
            "bin",
            "bool",
            "breakpoint",
            "bytearray",
            "bytes",
            "callable",
            "chr",
            "classmethod",
            "compile",
            "complex",
            "delattr",
            "dict",
            "dir",
            "divmod",
            "enumerate",
            "eval",
            "exec",
            "filter",
            "float",
            "format",
            "frozenset",
            "getattr",
            "globals",
            "hasattr",
            "hash",
            "help",
            "hex",
            "id",
            "input",
            "int",
            "isinstance",
            "issubclass",
            "iter",
            "len",
            "list",
            "locals",
            "map",
            "max",
            "memoryview",
            "min",
            "next",
            "object",
            "oct",
            "open",
            "ord",
            "pow",
            "print",
            "property",
            "range",
            "repr",
            "reversed",
            "round",
            "set",
            "setattr",
            "slice",
            "sorted",
            "staticmethod",
            "str",
            "sum",
            "super",
            "tuple",
            "type",
            "vars",
            "zip",
            "__import__"
        };

        private static bool RequiresGeneralValues(
    ISet<string> words)
        {
            return ContainsAny(
                words,
                "assert",
                "and",
                "or",
                "not",
                "is",
                "in",
                "print",
                "len",
                "range",
                "list",
                "dict",
                "set",
                "tuple",
                "str",
                "int",
                "float",
                "bool",
                "enumerate",
                "zip",
                "+",
                "-",
                "*",
                "/",
                "//",
                "%",
                "**",
                "==",
                "!=",
                ">",
                "<",
                ">=",
                "<=");
        }

        private static bool RequiresAsyncSupport(
    ISet<string> words)
        {
            return ContainsAny(
                words,
                "async",
                "await");
        }

        public bool CanHandle(
            string language)
        {
            return string.Equals(
                language,
                "python",
                StringComparison.OrdinalIgnoreCase);
        }

        private static void AppendRequiredContext(
            StringBuilder builder,
            ISet<string> words)
        {
            bool requiresContext =
                RequiresGeneralValues(
                    words)
                ||
                RequiresAsyncSupport(
                    words);

            if (!requiresContext)
            {
                return;
            }

            builder.AppendLine(
                "# Supporting values:");

            if (RequiresGeneralValues(
                words))
            {
                AppendGeneralValues(
                    builder);
            }

            if (RequiresAsyncSupport(
                words))
            {
                AppendAsyncSupport(
                    builder);
            }

            builder.AppendLine();
        }

        private static void AppendGeneralValues(
    StringBuilder builder)
        {
            builder.AppendLine(
                "items = [1, 2, 3]");

            builder.AppendLine(
                "values = [4, 5, 6]");

            builder.AppendLine(
                "value = 42");

            builder.AppendLine(
                "result = None");

            builder.AppendLine(
                "left = 10");

            builder.AppendLine(
                "right = 5");

            builder.AppendLine(
                "enabled = True");

            builder.AppendLine(
                "visible = False");
        }

        private static void AppendAsyncSupport(
    StringBuilder builder)
        {
            builder.AppendLine();

            builder.AppendLine(
                "async def fetch_value():");

            builder.AppendLine(
                "    return 42");
        }

        public string Generate(
            EditableLanguageConfiguration configuration,
            KeywordGroupConfiguration selectedGroup,
            IReadOnlyList<string> words)
        {
            var builder =
                new StringBuilder();

            IReadOnlyList<string> normalizedWords =
                NormalizeWords(
                    words);

            var wordSet =
                new HashSet<string>(
                    normalizedWords,
                    StringComparer.Ordinal);

            IReadOnlyList<PythonPreviewExample> matchingExamples =
                GetMatchingExamples(
                    wordSet);

            IReadOnlyList<string> dependencies =
                GetRequiredDependencies(
                    matchingExamples);

            AppendDependencies(
                builder,
                dependencies);

            AppendExamples(
                builder,
                matchingExamples);

            AppendHeader(
                builder,
                selectedGroup);

            AppendBaseSample(
                builder);

            AppendSelectedGroupHeader(
                builder);

            AppendRequiredContext(builder, wordSet);

            bool generatedContextualCode =
                false;

            generatedContextualCode |=
                AppendLiteralExamples(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendControlFlowExample(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendDeclarationExample(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendExceptionExample(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendImportExample(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendAsyncExample(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendLogicalOperatorExample(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendSymbolOperatorExamples(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendBuiltInExamples(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendIdentifierExamples(
                    builder,
                    normalizedWords);

            if (!generatedContextualCode)
            {
                AppendFallbackWords(
                    builder,
                    normalizedWords);
            }

            return builder.ToString();
        }

        private static void AppendHeader(
            StringBuilder builder,
            KeywordGroupConfiguration selectedGroup)
        {
            builder.AppendLine(
                "# NoteHighlight+ contextual preview");

            builder.AppendLine(
                "# Selected group: "
                + GetGroupDisplayName(
                    selectedGroup));

            builder.AppendLine();
        }

        private static void AppendBaseSample(
            StringBuilder builder)
        {
            builder.AppendLine(
                "class PreviewExample:");

            builder.AppendLine(
                "    def __init__(self, value):");

            builder.AppendLine(
                "        self.value = value");

            builder.AppendLine();

            builder.AppendLine(
                "    def process(self, items):");

            builder.AppendLine(
                "        for item in items:");

            builder.AppendLine(
                "            if item is not None:");

            builder.AppendLine(
                "                print(item)");

            builder.AppendLine();

            builder.AppendLine(
                "        return self.value");

            builder.AppendLine();

            builder.AppendLine(
                "example = PreviewExample(True)");

            builder.AppendLine(
                "result = example.process([1, 2, 3])");

            builder.AppendLine();
        }

        private static void AppendSelectedGroupHeader(
            StringBuilder builder)
        {
            builder.AppendLine(
                "# Context generated from the selected group:");

            builder.AppendLine();
        }

        private static bool AppendLiteralExamples(
            StringBuilder builder,
            ISet<string> words)
        {
            bool generated =
                false;

            int index =
                1;

            foreach (string literal in new[]
            {
                "True",
                "False",
                "None"
            })
            {
                if (!words.Contains(
                    literal))
                {
                    continue;
                }

                builder.AppendLine(
                    "preview_value_"
                    + index
                    + " = "
                    + literal);

                index++;
                generated = true;
            }

            if (generated)
            {
                builder.AppendLine();
            }

            return generated;
        }

        private static bool AppendControlFlowExample(
            StringBuilder builder,
            ISet<string> words)
        {
            if (!ContainsAny(
                words,
                "if",
                "elif",
                "else",
                "for",
                "while",
                "break",
                "continue",
                "return",
                "pass"))
            {
                return false;
            }

            builder.AppendLine(
                "def process_items(items):");

            if (words.Contains(
                "for"))
            {
                builder.AppendLine(
                    "    for item in items:");

                AppendConditionalBody(
                    builder,
                    words,
                    "        ");
            }
            else if (words.Contains(
                "while"))
            {
                builder.AppendLine(
                    "    while items:");

                builder.AppendLine(
                    "        item = items.pop(0)");

                AppendConditionalBody(
                    builder,
                    words,
                    "        ");
            }
            else
            {
                AppendConditionalBody(
                    builder,
                    words,
                    "    ");
            }

            if (words.Contains(
                "return"))
            {
                builder.AppendLine(
                    "    return items");
            }
            else
            {
                builder.AppendLine(
                    "    return None");
            }

            builder.AppendLine();

            return true;
        }

        private static void AppendConditionalBody(
            StringBuilder builder,
            ISet<string> words,
            string indentation)
        {
            if (words.Contains(
                "if"))
            {
                builder.AppendLine(
                    indentation
                    + "if item is None:");

                AppendFlowStatement(
                    builder,
                    words,
                    indentation + "    ");

                if (words.Contains(
                    "elif"))
                {
                    builder.AppendLine(
                        indentation
                        + "elif item == 0:");

                    builder.AppendLine(
                        indentation
                        + "    pass");
                }

                if (words.Contains(
                    "else"))
                {
                    builder.AppendLine(
                        indentation
                        + "else:");

                    builder.AppendLine(
                        indentation
                        + "    print(item)");
                }

                return;
            }

            AppendFlowStatement(
                builder,
                words,
                indentation);
        }

        private static void AppendFlowStatement(
            StringBuilder builder,
            ISet<string> words,
            string indentation)
        {
            if (words.Contains(
                "continue"))
            {
                builder.AppendLine(
                    indentation
                    + "continue");

                return;
            }

            if (words.Contains(
                "break"))
            {
                builder.AppendLine(
                    indentation
                    + "break");

                return;
            }

            if (words.Contains(
                "return"))
            {
                builder.AppendLine(
                    indentation
                    + "return item");

                return;
            }

            builder.AppendLine(
                indentation
                + "pass");
        }

        private static bool AppendDeclarationExample(
            StringBuilder builder,
            ISet<string> words)
        {
            if (!ContainsAny(
                words,
                "class",
                "def",
                "lambda",
                "yield"))
            {
                return false;
            }

            if (words.Contains(
                "class"))
            {
                builder.AppendLine(
                    "class GeneratedExample:");

                if (words.Contains(
                    "def"))
                {
                    builder.AppendLine(
                        "    def execute(self, value):");

                    if (words.Contains(
                        "yield"))
                    {
                        builder.AppendLine(
                            "        yield value");
                    }
                    else
                    {
                        builder.AppendLine(
                            "        return value");
                    }
                }
                else
                {
                    builder.AppendLine(
                        "    pass");
                }

                builder.AppendLine();
            }
            else if (words.Contains(
                "def"))
            {
                builder.AppendLine(
                    "def generated_function(value):");

                if (words.Contains(
                    "yield"))
                {
                    builder.AppendLine(
                        "    yield value");
                }
                else
                {
                    builder.AppendLine(
                        "    return value");
                }

                builder.AppendLine();
            }

            if (words.Contains(
                "lambda"))
            {
                builder.AppendLine(
                    "double_value = lambda value: value * 2");

                builder.AppendLine();
            }

            return true;
        }

        private static bool AppendExceptionExample(
    StringBuilder builder,
    ISet<string> words)
        {
            if (!ContainsAny(
                words,
                "try",
                "except",
                "finally",
                "raise",
                "assert"))
            {
                return false;
            }

            bool generated =
                false;

            if (words.Contains(
                "assert"))
            {
                builder.AppendLine(
                    "assert value is not None");

                builder.AppendLine();

                generated = true;
            }

            bool requiresTryBlock =
                ContainsAny(
                    words,
                    "try",
                    "except",
                    "finally");

            if (requiresTryBlock)
            {
                builder.AppendLine(
                    "try:");

                if (words.Contains(
                    "raise"))
                {
                    builder.AppendLine(
                        "    raise ValueError(\"Preview error\")");
                }
                else
                {
                    builder.AppendLine(
                        "    parsed_value = int(\"42\")");
                }

                builder.AppendLine(
                    "except ValueError as error:");

                builder.AppendLine(
                    "    print(error)");

                if (words.Contains(
                    "finally"))
                {
                    builder.AppendLine(
                        "finally:");

                    builder.AppendLine(
                        "    print(\"Finished\")");
                }

                builder.AppendLine();

                generated = true;
            }
            else if (words.Contains(
                "raise"))
            {
                builder.AppendLine(
                    "raise ValueError(\"Preview error\")");

                builder.AppendLine();

                generated = true;
            }

            return generated;
        }

        private static bool AppendImportExample(
            StringBuilder builder,
            ISet<string> words)
        {
            if (!ContainsAny(
                words,
                "import",
                "from",
                "as"))
            {
                return false;
            }

            if (words.Contains(
                "from"))
            {
                if (words.Contains(
                    "as"))
                {
                    builder.AppendLine(
                        "from pathlib import Path as FilePath");
                }
                else
                {
                    builder.AppendLine(
                        "from pathlib import Path");
                }
            }
            else if (words.Contains(
                "as"))
            {
                builder.AppendLine(
                    "import datetime as dt");
            }
            else
            {
                builder.AppendLine(
                    "import datetime");
            }

            builder.AppendLine();

            return true;
        }

        private static bool AppendAsyncExample(
            StringBuilder builder,
            ISet<string> words)
        {
            if (!ContainsAny(
                words,
                "async",
                "await"))
            {
                return false;
            }

            builder.AppendLine(
                "async def load_preview():");

            if (words.Contains(
                "await"))
            {
                builder.AppendLine(
                    "    loaded_value = await fetch_value()");
            }
            else
            {
                builder.AppendLine(
                    "    loaded_value = 42");
            }

            builder.AppendLine(
                "    return loaded_value");

            builder.AppendLine();

            return true;
        }

        private static bool AppendLogicalOperatorExample(
            StringBuilder builder,
            ISet<string> words)
        {
            if (!ContainsAny(
                words,
                "and",
                "or",
                "not",
                "is",
                "in"))
            {
                return false;
            }

            if (words.Contains(
                "and"))
            {
                builder.AppendLine(
                    "both_enabled = enabled and visible");
            }

            if (words.Contains(
                "or"))
            {
                builder.AppendLine(
                    "any_enabled = enabled or visible");
            }

            if (words.Contains(
                "not"))
            {
                builder.AppendLine(
                    "is_disabled = not enabled");
            }

            if (words.Contains(
                "is"))
            {
                builder.AppendLine(
                    "has_no_value = value is None");
            }

            if (words.Contains(
                "in"))
            {
                builder.AppendLine(
                    "contains_value = value in items");
            }

            builder.AppendLine();

            return true;
        }

        private static bool AppendSymbolOperatorExamples(
            StringBuilder builder,
            ISet<string> words)
        {
            bool generated =
                false;

            generated |= AppendOperator(
                builder,
                words,
                "+",
                "sum_result = left + right");

            generated |= AppendOperator(
                builder,
                words,
                "-",
                "difference = left - right");

            generated |= AppendOperator(
                builder,
                words,
                "*",
                "product = left * right");

            generated |= AppendOperator(
                builder,
                words,
                "/",
                "quotient = left / right");

            generated |= AppendOperator(
                builder,
                words,
                "//",
                "integer_quotient = left // right");

            generated |= AppendOperator(
                builder,
                words,
                "%",
                "remainder = left % right");

            generated |= AppendOperator(
                builder,
                words,
                "**",
                "power = left ** right");

            generated |= AppendOperator(
                builder,
                words,
                "==",
                "is_equal = left == right");

            generated |= AppendOperator(
                builder,
                words,
                "!=",
                "is_different = left != right");

            generated |= AppendOperator(
                builder,
                words,
                ">",
                "is_greater = left > right");

            generated |= AppendOperator(
                builder,
                words,
                "<",
                "is_smaller = left < right");

            generated |= AppendOperator(
                builder,
                words,
                ">=",
                "is_greater_or_equal = left >= right");

            generated |= AppendOperator(
                builder,
                words,
                "<=",
                "is_smaller_or_equal = left <= right");

            if (generated)
            {
                builder.AppendLine();
            }

            return generated;
        }

        private static bool AppendOperator(
            StringBuilder builder,
            ISet<string> words,
            string operatorText,
            string example)
        {
            if (!words.Contains(
                operatorText))
            {
                return false;
            }

            builder.AppendLine(
                example);

            return true;
        }

        private static bool AppendBuiltInExamples(
            StringBuilder builder,
            ISet<string> words)
        {
            bool generated =
                false;

            generated |= AppendBuiltIn(
                builder,
                words,
                "print",
                "print(\"NoteHighlight+ preview\")");

            generated |= AppendBuiltIn(
                builder,
                words,
                "exec",
                "exec(\"generated_value = 42\")");

            generated |= AppendBuiltIn(
                builder,
                words,
                "len",
                "item_count = len(items)");

            generated |= AppendBuiltIn(
                builder,
                words,
                "range",
                "numbers = range(10)");

            generated |= AppendBuiltIn(
                builder,
                words,
                "list",
                "preview_list = list(items)");

            generated |= AppendBuiltIn(
                builder,
                words,
                "dict",
                "preview_dictionary = dict()");

            generated |= AppendBuiltIn(
                builder,
                words,
                "set",
                "preview_set = set(items)");

            generated |= AppendBuiltIn(
                builder,
                words,
                "tuple",
                "preview_tuple = tuple(items)");

            generated |= AppendBuiltIn(
                builder,
                words,
                "str",
                "text_value = str(value)");

            generated |= AppendBuiltIn(
                builder,
                words,
                "int",
                "integer_value = int(value)");

            generated |= AppendBuiltIn(
                builder,
                words,
                "float",
                "decimal_value = float(value)");

            generated |= AppendBuiltIn(
                builder,
                words,
                "bool",
                "boolean_value = bool(value)");

            generated |= AppendBuiltIn(
                builder,
                words,
                "enumerate",
                "indexed_items = enumerate(items)");

            generated |= AppendBuiltIn(
                builder,
                words,
                "zip",
                "combined_items = zip(items, values)");

            if (generated)
            {
                builder.AppendLine();
            }

            return generated;
        }

        private static bool AppendBuiltIn(
            StringBuilder builder,
            ISet<string> words,
            string builtInName,
            string example)
        {
            if (!words.Contains(
                builtInName))
            {
                return false;
            }

            builder.AppendLine(
                example);

            return true;
        }

        private static bool AppendIdentifierExamples(
            StringBuilder builder,
            IEnumerable<string> words)
        {
            bool generated =
                false;

            int index =
                1;

            foreach (string word in words)
            {
                if (PythonReservedWords.Contains(
                    word))
                {
                    continue;
                }

                if (PythonBuiltInNames.Contains(word))
                {
                    continue;
                }

                if (IsKnownOperator(
                    word))
                {
                    continue;
                }

                if (!IsValidIdentifier(
                    word))
                {
                    continue;
                }

                builder.AppendLine(
                    word + " = None");

                index++;
                generated = true;
            }

            if (generated)
            {
                builder.AppendLine();
            }

            return generated;
        }

        private static void AppendFallbackWords(
            StringBuilder builder,
            IReadOnlyList<string> words)
        {
            if (words == null ||
                words.Count == 0)
            {
                builder.AppendLine(
                    "# No literal words are defined in this group.");

                return;
            }

            builder.AppendLine(
                "# Words without a specialized Python example:");

            foreach (string word in words)
            {
                builder.AppendLine(
                    "# "
                    + word);
            }
        }

        private static IReadOnlyList<string> NormalizeWords(
            IReadOnlyList<string> words)
        {
            if (words == null)
            {
                return new List<string>();
            }

            return words
                .Where(word =>
                    !string.IsNullOrWhiteSpace(word))
                .Select(word =>
                    word.Trim())
                .Distinct(
                    StringComparer.Ordinal)
                .Take(
                    MaximumDisplayedWords)
                .ToList();
        }

        private static readonly IReadOnlyList<PythonPreviewExample>
    Examples =
        PythonPreviewExampleCatalog.CreateExamples();

        private static IReadOnlyList<PythonPreviewExample>
    GetMatchingExamples(
        ISet<string> selectedWords)
        {
            return Examples
                .Where(example =>
                    example.Matches(
                        selectedWords))
                .ToList();
        }

        private static IReadOnlyList<string>
    GetRequiredDependencies(
        IEnumerable<PythonPreviewExample> examples)
        {
            return examples
                .SelectMany(example =>
                    example.Dependencies)
                .Distinct(
                    StringComparer.Ordinal)
                .ToList();
        }

        private static void AppendDependencies(
    StringBuilder builder,
    IEnumerable<string> dependencies)
        {
            bool wroteHeader =
                false;

            foreach (string dependency in dependencies)
            {
                string dependencyCode;

                if (!PythonPreviewDependencyCatalog.TryGetCode(
                    dependency,
                    out dependencyCode))
                {
                    continue;
                }

                if (!wroteHeader)
                {
                    builder.AppendLine(
                        "# Supporting values:");

                    wroteHeader = true;
                }

                builder.AppendLine(
                    dependencyCode);

                builder.AppendLine();
            }
        }

        private static void AppendExamples(
    StringBuilder builder,
    IEnumerable<PythonPreviewExample> examples)
        {
            foreach (PythonPreviewExample example in examples)
            {
                builder.AppendLine(
                    example.Code);

                builder.AppendLine();
            }
        }

        private static bool ContainsAny(
            ISet<string> words,
            params string[] candidates)
        {
            foreach (string candidate in candidates)
            {
                if (words.Contains(
                    candidate))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsKnownOperator(
            string value)
        {
            switch (value)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "//":
                case "%":
                case "**":
                case "==":
                case "!=":
                case ">":
                case "<":
                case ">=":
                case "<=":
                    return true;

                default:
                    return false;
            }
        }

        private static bool IsValidIdentifier(
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return false;
            }

            if (!IsIdentifierStartCharacter(
                value[0]))
            {
                return false;
            }

            for (int index = 1;
                 index < value.Length;
                 index++)
            {
                if (!IsIdentifierCharacter(
                    value[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsIdentifierStartCharacter(
            char character)
        {
            return char.IsLetter(
                       character)
                   ||
                   character == '_';
        }

        private static bool IsIdentifierCharacter(
            char character)
        {
            return char.IsLetterOrDigit(
                       character)
                   ||
                   character == '_';
        }

        private static string GetGroupDisplayName(
            KeywordGroupConfiguration selectedGroup)
        {
            if (selectedGroup == null ||
                string.IsNullOrWhiteSpace(
                    selectedGroup.DisplayName))
            {
                return "No group selected";
            }

            return selectedGroup.DisplayName.Trim();
        }
    }
}