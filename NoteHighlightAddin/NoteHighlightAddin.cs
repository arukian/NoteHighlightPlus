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

        public bool TrySelectTheme(
            ComboBox comboBox,
            string themeName)
        {
            if (comboBox == null)
            {
                throw new ArgumentNullException(
                    nameof(comboBox));
            }

            if (string.IsNullOrWhiteSpace(themeName))
            {
                return false;
            }

            for (int index = 0;
                index < comboBox.Items.Count;
                index++)
            {
                string item =
                    comboBox.Items[index] as string;

                if (!string.Equals(
                    item,
                    themeName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                comboBox.SelectedIndex = index;
                return true;
            }

            return false;
        }
    }
}