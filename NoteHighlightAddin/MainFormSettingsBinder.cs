using System.Windows.Forms;

namespace NoteHighlightAddin
{
    public class MainFormSettingsBinder
    {
        public void Apply(
            MainFormSettings settings,
            ComboBox codeStyle,
            Button backgroundButton,
            CheckBox saveToClipboard,
            CheckBox showLineNumber)
        {
            if (settings.HighLightStyle >= 0 &&
                settings.HighLightStyle < codeStyle.Items.Count)
            {
                codeStyle.SelectedIndex =
                    settings.HighLightStyle;
            }

            backgroundButton.BackColor =
                settings.BackgroundColor;

            saveToClipboard.Checked =
                settings.SaveOnClipboard;

            showLineNumber.Checked =
                settings.ShowLineNumber;
        }

        public MainFormSettings Capture(
            ComboBox codeStyle,
            Button backgroundButton,
            CheckBox saveToClipboard,
            CheckBox showLineNumber)
        {
            return new MainFormSettings
            {
                HighLightStyle =
                    codeStyle.SelectedIndex,

                BackgroundColor =
                    backgroundButton.BackColor,

                SaveOnClipboard =
                    saveToClipboard.Checked,

                ShowLineNumber =
                    showLineNumber.Checked
            };
        }
    }
}