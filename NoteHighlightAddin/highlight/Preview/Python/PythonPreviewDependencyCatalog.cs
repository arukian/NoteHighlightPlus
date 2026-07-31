using System.Collections.Generic;

namespace NoteHighlightAddin.Highlighting.Preview.Services.Builders
{
    internal static class PythonPreviewDependencyCatalog
    {
        private static readonly IReadOnlyDictionary<string, string>
            Dependencies =
                new Dictionary<string, string>
                {
                    {
                        "items",
                        "items = [1, 2, 3]"
                    },
                    {
                        "values",
                        "values = [4, 5, 6]"
                    },
                    {
                        "value",
                        "value = 42"
                    },
                    {
                        "result",
                        "result = None"
                    },
                    {
                        "left-right",
                        "left = 10\nright = 5"
                    },
                    {
                        "flags",
                        "enabled = True\nvisible = False"
                    },
                    {
                        "fetch-value",
                        "async def fetch_value():\n"
                        + "    return 42"
                    },
                    {
                        "pathlib",
                        "from pathlib import Path"
                    }
                };

        public static bool TryGetCode(
            string dependency,
            out string code)
        {
            return Dependencies.TryGetValue(
                dependency,
                out code);
        }
    }
}