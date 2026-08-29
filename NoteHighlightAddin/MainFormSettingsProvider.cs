namespace NoteHighlightAddin
{
    public class MainFormSettingsProvider
    {
        public MainFormSettings Load()
        {
            var settings =
                NoteHighlightForm.Properties.Settings.Default;

            return new MainFormSettings
            {
                HighLightStyle =
                    settings.HighLightStyle,

                BackgroundColor =
                    settings.BackgroundColor,

                HasBackgroundOverride =
                    settings.HasBackgroundOverride,

                SaveOnClipboard =
                    settings.SaveOnClipboard,

                ShowLineNumber =
                    settings.ShowLineNumber,

                Font =
                    settings.Font,

                FontSize =
                    settings.FontSize
            };
        }

        public void Save(MainFormSettings formSettings)
        {
            var settings = NoteHighlightForm.Properties.Settings.Default;

            settings.HighLightStyle = formSettings.HighLightStyle;
            settings.BackgroundColor = formSettings.BackgroundColor;
            settings.HasBackgroundOverride = formSettings.HasBackgroundOverride;
            settings.SaveOnClipboard = formSettings.SaveOnClipboard;
            settings.ShowLineNumber = formSettings.ShowLineNumber;

            settings.Save();
        }
    }
}