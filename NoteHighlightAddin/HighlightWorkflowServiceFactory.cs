namespace NoteHighlightAddin
{
    public class HighlightWorkflowServiceFactory
    {
        private readonly MainFormSettingsProvider _settingsProvider;

        public HighlightWorkflowServiceFactory(
            MainFormSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public HighlightWorkflowService Create()
        {
            return new HighlightWorkflowService(
                _settingsProvider,
                new HighLightParameterFactory(),
                new HighlightGenerationService(),
                new HighlightClipboardService());
        }
    }
}