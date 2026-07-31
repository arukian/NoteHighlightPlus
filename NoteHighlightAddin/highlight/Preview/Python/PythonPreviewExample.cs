using System.Collections.Generic;

namespace NoteHighlightAddin.Highlighting.Preview.Services.Builders
{
    internal sealed class PythonPreviewExample
    {
        public PythonPreviewExample(
            IEnumerable<string> triggerWords,
            IEnumerable<string> dependencies,
            string code)
        {
            TriggerWords =
                new HashSet<string>(
                    triggerWords,
                    System.StringComparer.Ordinal);

            Dependencies =
                new HashSet<string>(
                    dependencies,
                    System.StringComparer.Ordinal);

            Code =
                code ?? string.Empty;
        }

        public ISet<string> TriggerWords
        {
            get;
        }

        public ISet<string> Dependencies
        {
            get;
        }

        public string Code
        {
            get;
        }

        public bool Matches(
            ISet<string> selectedWords)
        {
            foreach (string triggerWord in TriggerWords)
            {
                if (selectedWords.Contains(
                    triggerWord))
                {
                    return true;
                }
            }

            return false;
        }
    }
}