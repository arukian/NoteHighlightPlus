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
    public static string Root
        {
            get;
        }

    static PathManager()
        {
            Root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string Ribbon => Combine("ribbon.xml");

    public static string HighlightFolder => Combine("highlight");

    public static string ThemesFolder => Combine("highlight", "themes");
    
    public static string LanguagesFolder => Combine("highlight", "langDefs");

     public static string HighlightExe => Combine("highlight", "highlight.exe");

    private static string Combine(params string[] parts)
        {
            return Path.Combine(new[] { Root }.Concat(parts). ToArray());
        }
    }
}
