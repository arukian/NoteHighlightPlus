using GenerateHighlightContent;

namespace NoteHighlightAddin
{
    public class HighlightGenerationService
    {
        public string Generate(HighLightParameter parameters)
        {
            IGenerateHighLight generator = new GenerateHighLight();

            return generator.GenerateHighLightCode(parameters);
        }
    }
}