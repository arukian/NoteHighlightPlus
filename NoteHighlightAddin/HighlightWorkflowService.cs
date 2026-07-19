using GenerateHighlightContent;

namespace NoteHighlightAddin
{
    public class HighlightWorkflowService
    {
        private readonly MainFormSettingsProvider _settingsProvider;
        private readonly HighLightParameterFactory _parameterFactory;
        private readonly HighlightGenerationService _generationService;
        private readonly HighlightClipboardService _clipboardService;

        public HighlightWorkflowService(
            MainFormSettingsProvider settingsProvider,
            HighLightParameterFactory parameterFactory,
            HighlightGenerationService generationService,
            HighlightClipboardService clipboardService)
        {
            _settingsProvider = settingsProvider;
            _parameterFactory = parameterFactory;
            _generationService = generationService;
            _clipboardService = clipboardService;
        }

        public HighlightWorkflowResult Execute(HighlightWorkflowRequest request)
        {
            MainFormSettings settings = _settingsProvider.Load();

            HighLightParameter parameters =
                _parameterFactory.Create(
                    request.FileName,
                    request.Content,
                    request.CodeType,
                    request.HighLightStyle,
                    request.ShowLineNumber,
                    request.HighlightColor,
                    settings.Font,
                    settings.FontSize);

            string outputFileName = _generationService.Generate(parameters);

            bool copiedToClipboard = false;

            if (request.CopyToClipboard &&
                !string.IsNullOrEmpty(outputFileName))
            {
                _clipboardService.Copy(
                    outputFileName,
                    request.DarkMode,
                    request.ShowLineNumber);

                copiedToClipboard = true;
            }

            return new HighlightWorkflowResult
            {
                Parameters = parameters,
                OutputFileName = outputFileName,
                CopiedToClipboard = copiedToClipboard
            };
        }
    }
}