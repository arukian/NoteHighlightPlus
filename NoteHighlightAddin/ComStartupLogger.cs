using System;
using System.IO;
using System.Text;

namespace NoteHighlightAddin
{
    internal static class ComStartupLogger
    {
        private static readonly object SyncRoot = new object();

        private static string GetLogDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Arukian",
                "NoteHighlight+",
                "Logs");
        }

        private static string GetLogFile()
        {
            return Path.Combine(
                GetLogDirectory(),
                "com-startup.log");
        }

        public static void Write(string message)
        {
            try
            {
                lock (SyncRoot)
                {
                    string directory = GetLogDirectory();

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    var text = new StringBuilder();

                    text.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    text.Append(" | PID=");
                    text.Append(System.Diagnostics.Process.GetCurrentProcess().Id);
                    text.Append(" | ");
                    text.AppendLine(message);

                    File.AppendAllText(
                        GetLogFile(),
                        text.ToString(),
                        Encoding.UTF8);
                }
            }
            catch
            {
                // El logger nunca debe impedir que cargue el add-in.
            }
        }

        public static void WriteException(
            string stage,
            Exception exception)
        {
            Write(
                "EXCEPTION @ " +
                stage +
                Environment.NewLine +
                exception);
        }
    }
}