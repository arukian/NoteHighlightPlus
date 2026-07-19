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

        public MainFormInitializer(
            ThemeComboBoxBinder themeBinder,
            CodeEditorConfigurator editorConfigurator,
            MainFormSettingsProvider settingsProvider,
            MainFormSettingsBinder settingsBinder)
        {
            _themeBinder = themeBinder
                ?? throw new ArgumentNullException(nameof(themeBinder));

            _editorConfigurator = editorConfigurator
                ?? throw new ArgumentNullException(nameof(editorConfigurator));

            _settingsProvider = settingsProvider
                ?? throw new ArgumentNullException(nameof(settingsProvider));

            _settingsBinder = settingsBinder
                ?? throw new ArgumentNullException(nameof(settingsBinder));
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
        }
    }
}