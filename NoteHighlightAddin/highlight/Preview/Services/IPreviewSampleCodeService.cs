using NoteHighlightAddin.Highlighting.KeywordGroups;
using System.Runtime.InteropServices;

namespace NoteHighlightAddin.Highlighting.Preview.Services
{
    /// <summary>
    /// Generates contextual source code for the preview.
    /// </summary>
    [ComVisible(false)]
    public interface IPreviewSampleCodeService
    {
        string Generate(
            EditableLanguageConfiguration configuration,
            KeywordGroupConfiguration selectedGroup);
    }
}