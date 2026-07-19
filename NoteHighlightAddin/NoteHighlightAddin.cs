using System;
using System.Windows.Forms;

namespace NoteHighlightAddin
{
    public class ThemeComboBoxBinder
    {
        private readonly ThemeProvider _themeProvider;

        public ThemeComboBoxBinder(
            ThemeProvider themeProvider)
        {
            _themeProvider = themeProvider;
        }

        public void LoadThemes(
            ComboBox comboBox)
        {
            if (comboBox == null)
            {
                throw new ArgumentNullException(
                    nameof(comboBox));
            }

            comboBox.Items.Clear();

            foreach (string themeName
                in _themeProvider.GetThemeNames())
            {
                comboBox.Items.Add(themeName);
            }
        }
    }
}