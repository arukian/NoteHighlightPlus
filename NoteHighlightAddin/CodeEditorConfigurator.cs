using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System.Text;

namespace NoteHighlightAddin
{
    public class CodeEditorConfigurator
    {
        private readonly CodeEditorLanguageMapper _languageMapper;

        public CodeEditorConfigurator(
            CodeEditorLanguageMapper languageMapper)
        {
            _languageMapper = languageMapper;
        }

        public void Configure(
            TextEditorControl editor,
            string codeType)
        {
            string highlightingName =
                _languageMapper.GetHighlightingName(codeType);

            editor.Document.HighlightingStrategy =
                HighlightingStrategyFactory
                    .CreateHighlightingStrategy(
                        highlightingName);

            editor.Encoding = Encoding.UTF8;
        }
    }
}