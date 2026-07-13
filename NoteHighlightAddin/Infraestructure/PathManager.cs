using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace NoteHighlightAddin.Infrastructure
{
    public static class PathManager
    {
    public static string Root => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public static string Ribbon => Path.Combine(Root, "ribbon.xml");

    public static string HighlightFolder => Path.Combine(Root, "highlight");

    public static string ThemesFolder => Path.Combine(HighlightFolder, "themes");


    public static string LanguagesFolder => Path.Combine(HighlightFolder, "langDefs");

     public static string HighlightExe => Path.Combine(HighlightFolder, "highlight.exe");
    }
}
