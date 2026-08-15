using ICSharpCode.TextEditor;
using System;
using System.Windows.Forms;

namespace NoteHighlightAddin
{
    public class MainFormInitializer
    {
        private readonly ThemeComboBoxBinder _themeBinder;
        private readonly CodeEditorConfigurator _editorConfigurator;
        private readonly MainFormSettingsProvider _settingsProvider;
        private readonly MainFormSettingsBinder _settingsBinder;
        private readonly ThemePreferenceProvider _themePreferenceProvider;

        public MainFormInitializer(
            ThemeComboBoxBinder themeBinder,
            CodeEditorConfigurator editorConfigurator,
            MainFormSettingsProvider settingsProvider,
            MainFormSettingsBinder settingsBinder,
            ThemePreferenceProvider themePreferenceProvider)
        {
            _themeBinder = themeBinder
                ?? throw new ArgumentNullException(nameof(themeBinder));

            _editorConfigurator = editorConfigurator
                ?? throw new ArgumentNullException(nameof(editorConfigurator));

            _settingsProvider = settingsProvider
                ?? throw new ArgumentNullException(nameof(settingsProvider));

            _settingsBinder = settingsBinder
                ?? throw new ArgumentNullException(nameof(settingsBinder));

            _themePreferenceProvider = themePreferenceProvider
                ?? throw new ArgumentNullException(nameof(themePreferenceProvider));
        }

        public void Initialize(
            TextEditorControl codeEditor,
            string codeType,
            ComboBox codeStyle,
            Button backgroundButton,
            CheckBox saveToClipboard,
            CheckBox showLineNumber)
        {
            _themeBinder.LoadThemes(codeStyle);

            _editorConfigurator.Configure(
                codeEditor,
                codeType);

            MainFormSettings settings =
                _settingsProvider.Load();

            _settingsBinder.Apply(
                settings,
                codeStyle,
                backgroundButton,
                saveToClipboard,
                showLineNumber);

            string preferredTheme =
                _themePreferenceProvider.ReadThemeName();

            _themeBinder.TrySelectTheme(
                codeStyle,
                preferredTheme);

            if (codeStyle.SelectedIndex < 0 &&
                codeStyle.Items.Count > 0)
            {
                codeStyle.SelectedIndex = 0;
            }
        }
    }
}