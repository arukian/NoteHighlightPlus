using Infrastructure.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoteHighlightAddin
{
    public class ThemeProvider
    {
        public IReadOnlyList<string> GetThemeNames()
        {
            if (!Directory.Exists(PathManager.ThemesFolder))
            {
                throw new DirectoryNotFoundException(
                    $"Theme folder was not found: {PathManager.ThemesFolder}");
            }

            return Directory
                .GetFiles(PathManager.ThemesFolder, "*.theme")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(themeName => !string.IsNullOrWhiteSpace(themeName))
                .OrderBy(themeName => themeName)
                .ToList();
        }
    }
}