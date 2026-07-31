using System.Collections.Generic;

namespace NoteHighlightAddin.Highlighting.Preview.Services.Builders
{
    internal static class PythonPreviewExampleCatalog
    {
        public static IReadOnlyList<PythonPreviewExample>
            CreateExamples()
        {
            return new List<PythonPreviewExample>
            {
                new PythonPreviewExample(
                    new[]
                    {
                        "True",
                        "False",
                        "None"
                    },
                    new string[0],
                    "enabled_value = True\n"
                    + "disabled_value = False\n"
                    + "empty_value = None"),

                new PythonPreviewExample(
                    new[]
                    {
                        "and",
                        "or",
                        "not"
                    },
                    new[]
                    {
                        "flags"
                    },
                    "both_enabled = enabled and visible\n"
                    + "any_enabled = enabled or visible\n"
                    + "is_disabled = not enabled"),

                new PythonPreviewExample(
                    new[]
                    {
                        "+",
                        "-",
                        "*",
                        "/"
                    },
                    new[]
                    {
                        "left-right"
                    },
                    "sum_result = left + right\n"
                    + "difference = left - right\n"
                    + "product = left * right\n"
                    + "quotient = left / right"),

                new PythonPreviewExample(
                    new[]
                    {
                        "in",
                        "is"
                    },
                    new[]
                    {
                        "items",
                        "value"
                    },
                    "contains_value = value in items\n"
                    + "has_value = value is not None"),

                new PythonPreviewExample(
                    new[]
                    {
                        "async",
                        "await"
                    },
                    new[]
                    {
                        "fetch-value"
                    },
                    "async def load_preview():\n"
                    + "    loaded_value = await fetch_value()\n"
                    + "    return loaded_value"),

                new PythonPreviewExample(
                    new[]
                    {
                        "Path"
                    },
                    new[]
                    {
                        "pathlib"
                    },
                    "preview_path = Path(\"preview.py\")")
            };
        }
    }
}