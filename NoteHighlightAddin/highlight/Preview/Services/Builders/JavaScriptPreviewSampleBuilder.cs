using NoteHighlightAddin.Highlighting.KeywordGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoteHighlightAddin.Highlighting.Preview.Services.Builders
{
    internal sealed class JavaScriptPreviewSampleBuilder
        : IPreviewSampleBuilder
    {
        private const int MaximumDisplayedWords =
            20;

        private static readonly HashSet<string>
            JavaScriptReservedWords =
                new HashSet<string>(
                    StringComparer.Ordinal)
                {
                    "await",
                    "break",
                    "case",
                    "catch",
                    "class",
                    "const",
                    "continue",
                    "debugger",
                    "default",
                    "delete",
                    "do",
                    "else",
                    "export",
                    "extends",
                    "false",
                    "finally",
                    "for",
                    "function",
                    "if",
                    "import",
                    "in",
                    "instanceof",
                    "let",
                    "new",
                    "null",
                    "return",
                    "static",
                    "super",
                    "switch",
                    "this",
                    "throw",
                    "true",
                    "try",
                    "typeof",
                    "undefined",
                    "var",
                    "void",
                    "while",
                    "with",
                    "yield",
                    "async",
                    "of"
                };

        private static readonly HashSet<string>
            JavaScriptBuiltInNames =
                new HashSet<string>(
                    StringComparer.Ordinal)
                {
                    "Array",
                    "Object",
                    "String",
                    "Number",
                    "Boolean",
                    "Date",
                    "RegExp",
                    "Map",
                    "Set",
                    "WeakMap",
                    "WeakSet",
                    "Promise",
                    "Symbol",
                    "BigInt",
                    "Math",
                    "JSON",
                    "console",
                    "parseInt",
                    "parseFloat",
                    "isNaN",
                    "isFinite",
                    "eval",
                    "Error",
                    "TypeError",
                    "RangeError",
                    "ReferenceError"
                };

        public bool CanHandle(
            string language)
        {
            return string.Equals(
                       language,
                       "javascript",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   string.Equals(
                       language,
                       "js",
                       StringComparison.OrdinalIgnoreCase);
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

            AppendHeader(
                builder,
                selectedGroup);

            AppendBaseSample(
                builder);

            AppendSelectedGroupHeader(
                builder);

            AppendRequiredContext(
                builder,
                wordSet);

            bool generatedContextualCode =
                false;

            generatedContextualCode |=
                AppendLiteralExamples(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendVariableDeclarationExamples(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendControlFlowExample(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendFunctionAndClassExamples(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendExceptionExample(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendAsyncExample(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendModuleExample(
                    builder,
                    wordSet);

            generatedContextualCode |=
                AppendTypeOperatorExamples(
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
                "// NoteHighlight+ contextual preview");

            builder.AppendLine(
                "// Selected group: "
                + GetGroupDisplayName(
                    selectedGroup));

            builder.AppendLine();
        }

        private static void AppendBaseSample(
            StringBuilder builder)
        {
            builder.AppendLine(
                "class PreviewExample {");

            builder.AppendLine(
                "    constructor(value) {");

            builder.AppendLine(
                "        this.value = value;");

            builder.AppendLine(
                "    }");

            builder.AppendLine();

            builder.AppendLine(
                "    process(items) {");

            builder.AppendLine(
                "        for (const item of items) {");

            builder.AppendLine(
                "            if (item !== null) {");

            builder.AppendLine(
                "                console.log(item);");

            builder.AppendLine(
                "            }");

            builder.AppendLine(
                "        }");

            builder.AppendLine();

            builder.AppendLine(
                "        return this.value;");

            builder.AppendLine(
                "    }");

            builder.AppendLine(
                "}");

            builder.AppendLine();

            builder.AppendLine(
                "const example = new PreviewExample(true);");

            builder.AppendLine(
                "const result = example.process([1, 2, 3]);");

            builder.AppendLine();
        }

        private static void AppendSelectedGroupHeader(
            StringBuilder builder)
        {
            builder.AppendLine(
                "// Context generated from the selected group:");

            builder.AppendLine();
        }

        private static void AppendRequiredContext(
            StringBuilder builder,
            ISet<string> words)
        {
            bool requiresGeneralValues =
                RequiresGeneralValues(
                    words);

            bool requiresAsyncSupport =
                RequiresAsyncSupport(
                    words);

            if (!requiresGeneralValues &&
                !requiresAsyncSupport)
            {
                return;
            }

            builder.AppendLine(
                "// Supporting values:");

            if (requiresGeneralValues)
            {
                builder.AppendLine(
                    "const items = [1, 2, 3];");

                builder.AppendLine(
                    "const values = [4, 5, 6];");

                builder.AppendLine(
                    "const value = 42;");

                builder.AppendLine(
                    "const left = 10;");

                builder.AppendLine(
                    "const right = 5;");

                builder.AppendLine(
                    "const enabled = true;");

                builder.AppendLine(
                    "const visible = false;");

                builder.AppendLine(
                    "const objectValue = {};");
            }

            if (requiresAsyncSupport)
            {
                builder.AppendLine();

                builder.AppendLine(
                    "async function fetchValue() {");

                builder.AppendLine(
                    "    return 42;");

                builder.AppendLine(
                    "}");
            }

            builder.AppendLine();
        }

        private static bool AppendLiteralExamples(
            StringBuilder builder,
            ISet<string> words)
        {
            bool generated =
                false;

            if (words.Contains(
                "true"))
            {
                builder.AppendLine(
                    "const enabledValue = true;");

                generated = true;
            }

            if (words.Contains(
                "false"))
            {
                builder.AppendLine(
                    "const disabledValue = false;");

                generated = true;
            }

            if (words.Contains(
                "null"))
            {
                builder.AppendLine(
                    "const emptyValue = null;");

                generated = true;
            }

            if (words.Contains(
                "undefined"))
            {
                builder.AppendLine(
                    "let missingValue = undefined;");

                generated = true;
            }

            if (generated)
            {
                builder.AppendLine();
            }

            return generated;
        }

        private static bool AppendVariableDeclarationExamples(
            StringBuilder builder,
            ISet<string> words)
        {
            bool generated =
                false;

            if (words.Contains(
                "const"))
            {
                builder.AppendLine(
                    "const constantValue = 42;");

                generated = true;
            }

            if (words.Contains(
                "let"))
            {
                builder.AppendLine(
                    "let mutableValue = 10;");

                generated = true;
            }

            if (words.Contains(
                "var"))
            {
                builder.AppendLine(
                    "var legacyValue = \"preview\";");

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
                "else",
                "for",
                "while",
                "do",
                "switch",
                "case",
                "default",
                "break",
                "continue",
                "return"))
            {
                return false;
            }

            builder.AppendLine(
                "function processItems(items) {");

            if (words.Contains(
                "for"))
            {
                builder.AppendLine(
                    "    for (const item of items) {");

                AppendConditionalBody(
                    builder,
                    words,
                    "        ");

                builder.AppendLine(
                    "    }");
            }
            else if (words.Contains(
                "while"))
            {
                builder.AppendLine(
                    "    while (items.length > 0) {");

                builder.AppendLine(
                    "        const item = items.shift();");

                AppendConditionalBody(
                    builder,
                    words,
                    "        ");

                builder.AppendLine(
                    "    }");
            }
            else if (words.Contains(
                "do"))
            {
                builder.AppendLine(
                    "    do {");

                builder.AppendLine(
                    "        console.log(items.length);");

                builder.AppendLine(
                    "    } while (items.length > 0);");
            }
            else if (words.Contains(
                "switch"))
            {
                builder.AppendLine(
                    "    switch (items.length) {");

                builder.AppendLine(
                    "        case 0:");

                builder.AppendLine(
                    "            break;");

                builder.AppendLine(
                    "        default:");

                builder.AppendLine(
                    "            console.log(items);");

                builder.AppendLine(
                    "    }");
            }
            else
            {
                AppendConditionalBody(
                    builder,
                    words,
                    "    ");
            }

            builder.AppendLine(
                "    return items;");

            builder.AppendLine(
                "}");

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
                    + "if (item === null) {");

                AppendFlowStatement(
                    builder,
                    words,
                    indentation + "    ");

                builder.AppendLine(
                    indentation
                    + "}");

                if (words.Contains(
                    "else"))
                {
                    builder.AppendLine(
                        indentation
                        + "else {");

                    builder.AppendLine(
                        indentation
                        + "    console.log(item);");

                    builder.AppendLine(
                        indentation
                        + "}");
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
                    + "continue;");

                return;
            }

            if (words.Contains(
                "break"))
            {
                builder.AppendLine(
                    indentation
                    + "break;");

                return;
            }

            if (words.Contains(
                "return"))
            {
                builder.AppendLine(
                    indentation
                    + "return item;");

                return;
            }

            builder.AppendLine(
                indentation
                + "console.log(item);");
        }

        private static bool AppendFunctionAndClassExamples(
            StringBuilder builder,
            ISet<string> words)
        {
            if (!ContainsAny(
                words,
                "function",
                "class",
                "extends",
                "new",
                "this",
                "static",
                "yield"))
            {
                return false;
            }

            if (words.Contains(
                "class"))
            {
                if (words.Contains(
                    "extends"))
                {
                    builder.AppendLine(
                        "class GeneratedExample extends PreviewExample {");
                }
                else
                {
                    builder.AppendLine(
                        "class GeneratedExample {");
                }

                if (words.Contains(
                    "static"))
                {
                    builder.AppendLine(
                        "    static create() {");

                    builder.AppendLine(
                        "        return new GeneratedExample();");

                    builder.AppendLine(
                        "    }");
                }

                builder.AppendLine(
                    "    execute(value) {");

                if (words.Contains(
                    "this"))
                {
                    builder.AppendLine(
                        "        this.value = value;");
                }

                builder.AppendLine(
                    "        return value;");

                builder.AppendLine(
                    "    }");

                builder.AppendLine(
                    "}");

                builder.AppendLine();
            }

            if (words.Contains(
                "function"))
            {
                if (words.Contains(
                    "yield"))
                {
                    builder.AppendLine(
                        "function* generateValues() {");

                    builder.AppendLine(
                        "    yield 42;");

                    builder.AppendLine(
                        "}");
                }
                else
                {
                    builder.AppendLine(
                        "function generatedFunction(value) {");

                    builder.AppendLine(
                        "    return value;");

                    builder.AppendLine(
                        "}");
                }

                builder.AppendLine();
            }

            if (words.Contains(
                "new"))
            {
                builder.AppendLine(
                    "const generatedInstance = new GeneratedExample();");

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
                "catch",
                "finally",
                "throw"))
            {
                return false;
            }

            bool requiresTryBlock =
                ContainsAny(
                    words,
                    "try",
                    "catch",
                    "finally");

            if (requiresTryBlock)
            {
                builder.AppendLine(
                    "try {");

                if (words.Contains(
                    "throw"))
                {
                    builder.AppendLine(
                        "    throw new Error(\"Preview error\");");
                }
                else
                {
                    builder.AppendLine(
                        "    const parsedValue = Number(\"42\");");
                }

                builder.AppendLine(
                    "}");

                builder.AppendLine(
                    "catch (error) {");

                builder.AppendLine(
                    "    console.log(error);");

                builder.AppendLine(
                    "}");

                if (words.Contains(
                    "finally"))
                {
                    builder.AppendLine(
                        "finally {");

                    builder.AppendLine(
                        "    console.log(\"Finished\");");

                    builder.AppendLine(
                        "}");
                }

                builder.AppendLine();

                return true;
            }

            builder.AppendLine(
                "throw new Error(\"Preview error\");");

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
                "await",
                "Promise"))
            {
                return false;
            }

            builder.AppendLine(
                "async function loadPreview() {");

            if (words.Contains(
                "await"))
            {
                builder.AppendLine(
                    "    const loadedValue = await fetchValue();");
            }
            else
            {
                builder.AppendLine(
                    "    const loadedValue = Promise.resolve(42);");
            }

            builder.AppendLine(
                "    return loadedValue;");

            builder.AppendLine(
                "}");

            builder.AppendLine();

            return true;
        }

        private static bool AppendModuleExample(
            StringBuilder builder,
            ISet<string> words)
        {
            if (!ContainsAny(
                words,
                "import",
                "export",
                "from",
                "default"))
            {
                return false;
            }

            if (words.Contains(
                "import"))
            {
                if (words.Contains(
                    "from"))
                {
                    builder.AppendLine(
                        "import { readFile } from \"fs\";");
                }
                else
                {
                    builder.AppendLine(
                        "import \"./preview-module.js\";");
                }
            }

            if (words.Contains(
                "export"))
            {
                if (words.Contains(
                    "default"))
                {
                    builder.AppendLine(
                        "export default PreviewExample;");
                }
                else
                {
                    builder.AppendLine(
                        "export { PreviewExample };");
                }
            }

            builder.AppendLine();

            return true;
        }

        private static bool AppendTypeOperatorExamples(
            StringBuilder builder,
            ISet<string> words)
        {
            bool generated =
                false;

            if (words.Contains(
                "typeof"))
            {
                builder.AppendLine(
                    "const valueType = typeof value;");

                generated = true;
            }

            if (words.Contains(
                "instanceof"))
            {
                builder.AppendLine(
                    "const isPreview = example instanceof PreviewExample;");

                generated = true;
            }

            if (words.Contains(
                "in"))
            {
                builder.AppendLine(
                    "const hasValue = \"value\" in objectValue;");

                generated = true;
            }

            if (words.Contains(
                "delete"))
            {
                builder.AppendLine(
                    "delete objectValue.value;");

                generated = true;
            }

            if (generated)
            {
                builder.AppendLine();
            }

            return generated;
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
                "const sumResult = left + right;");

            generated |= AppendOperator(
                builder,
                words,
                "-",
                "const difference = left - right;");

            generated |= AppendOperator(
                builder,
                words,
                "*",
                "const product = left * right;");

            generated |= AppendOperator(
                builder,
                words,
                "/",
                "const quotient = left / right;");

            generated |= AppendOperator(
                builder,
                words,
                "%",
                "const remainder = left % right;");

            generated |= AppendOperator(
                builder,
                words,
                "**",
                "const power = left ** right;");

            generated |= AppendOperator(
                builder,
                words,
                "==",
                "const looselyEqual = left == right;");

            generated |= AppendOperator(
                builder,
                words,
                "===",
                "const strictlyEqual = left === right;");

            generated |= AppendOperator(
                builder,
                words,
                "!=",
                "const looselyDifferent = left != right;");

            generated |= AppendOperator(
                builder,
                words,
                "!==",
                "const strictlyDifferent = left !== right;");

            generated |= AppendOperator(
                builder,
                words,
                ">",
                "const isGreater = left > right;");

            generated |= AppendOperator(
                builder,
                words,
                "<",
                "const isSmaller = left < right;");

            generated |= AppendOperator(
                builder,
                words,
                ">=",
                "const isGreaterOrEqual = left >= right;");

            generated |= AppendOperator(
                builder,
                words,
                "<=",
                "const isSmallerOrEqual = left <= right;");

            generated |= AppendOperator(
                builder,
                words,
                "&&",
                "const bothEnabled = enabled && visible;");

            generated |= AppendOperator(
                builder,
                words,
                "||",
                "const anyEnabled = enabled || visible;");

            generated |= AppendOperator(
                builder,
                words,
                "!",
                "const isDisabled = !enabled;");

            generated |= AppendOperator(
                builder,
                words,
                "??",
                "const safeValue = value ?? 0;");

            generated |= AppendOperator(
                builder,
                words,
                "?.",
                "const nestedValue = objectValue?.value;");

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
                "console",
                "console.log(\"NoteHighlight+ preview\");");

            generated |= AppendBuiltIn(
                builder,
                words,
                "Array",
                "const previewArray = Array.from(items);");

            generated |= AppendBuiltIn(
                builder,
                words,
                "Object",
                "const previewObject = Object.create(null);");

            generated |= AppendBuiltIn(
                builder,
                words,
                "String",
                "const textValue = String(value);");

            generated |= AppendBuiltIn(
                builder,
                words,
                "Number",
                "const numberValue = Number(value);");

            generated |= AppendBuiltIn(
                builder,
                words,
                "Boolean",
                "const booleanValue = Boolean(value);");

            generated |= AppendBuiltIn(
                builder,
                words,
                "Date",
                "const currentDate = new Date();");

            generated |= AppendBuiltIn(
                builder,
                words,
                "Map",
                "const previewMap = new Map();");

            generated |= AppendBuiltIn(
                builder,
                words,
                "Set",
                "const previewSet = new Set(items);");

            generated |= AppendBuiltIn(
                builder,
                words,
                "JSON",
                "const jsonText = JSON.stringify(objectValue);");

            generated |= AppendBuiltIn(
                builder,
                words,
                "Math",
                "const maximumValue = Math.max(left, right);");

            generated |= AppendBuiltIn(
                builder,
                words,
                "parseInt",
                "const integerValue = parseInt(\"42\", 10);");

            generated |= AppendBuiltIn(
                builder,
                words,
                "parseFloat",
                "const decimalValue = parseFloat(\"42.5\");");

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

            foreach (string word in words)
            {
                if (JavaScriptReservedWords.Contains(
                    word))
                {
                    continue;
                }

                if (JavaScriptBuiltInNames.Contains(
                    word))
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
                    "const "
                    + word
                    + " = null;");

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
                    "// No literal words are defined in this group.");

                return;
            }

            builder.AppendLine(
                "// Words without a specialized JavaScript example:");

            foreach (string word in words)
            {
                builder.AppendLine(
                    "// "
                    + word);
            }
        }

        private static bool RequiresGeneralValues(
            ISet<string> words)
        {
            return ContainsAny(
                words,
                "typeof",
                "instanceof",
                "in",
                "delete",
                "Array",
                "Object",
                "String",
                "Number",
                "Boolean",
                "Map",
                "Set",
                "JSON",
                "Math",
                "parseInt",
                "parseFloat",
                "+",
                "-",
                "*",
                "/",
                "%",
                "**",
                "==",
                "===",
                "!=",
                "!==",
                ">",
                "<",
                ">=",
                "<=",
                "&&",
                "||",
                "!",
                "??",
                "?.");
        }

        private static bool RequiresAsyncSupport(
            ISet<string> words)
        {
            return ContainsAny(
                words,
                "async",
                "await",
                "Promise");
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
                case "%":
                case "**":
                case "==":
                case "===":
                case "!=":
                case "!==":
                case ">":
                case "<":
                case ">=":
                case "<=":
                case "&&":
                case "||":
                case "!":
                case "??":
                case "?.":
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
                   character == '_'
                   ||
                   character == '$';
        }

        private static bool IsIdentifierCharacter(
            char character)
        {
            return char.IsLetterOrDigit(
                       character)
                   ||
                   character == '_'
                   ||
                   character == '$';
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