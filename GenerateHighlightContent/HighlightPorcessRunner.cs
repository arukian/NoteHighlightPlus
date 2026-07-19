using System;
using System.Diagnostics;
using System.IO;
using Helper;

namespace GenerateHighlightContent
{
    public class HighlightProcessRunner
    {
        public void Run(
            string workingDirectory,
            string processName,
            string arguments)
        {
            if (string.IsNullOrWhiteSpace(workingDirectory))
            {
                throw new ArgumentException(
                    "El directorio de trabajo no puede estar vacío.",
                    nameof(workingDirectory));
            }

            if (string.IsNullOrWhiteSpace(processName))
            {
                throw new ArgumentException(
                    "El nombre del proceso no puede estar vacío.",
                    nameof(processName));
            }

            if (!Directory.Exists(workingDirectory))
            {
                throw new DirectoryNotFoundException(
                    "No se encontró el directorio de highlight: " +
                    workingDirectory);
            }

            string executablePath =
                Path.Combine(workingDirectory, processName);

            if (!File.Exists(executablePath))
            {
                throw new FileNotFoundException(
                    "No se encontró el ejecutable de highlight.",
                    executablePath);
            }

            ProcessHelper helper = new ProcessHelper(
                workingDirectory,
                processName);

            helper.Arguments = arguments ?? string.Empty;
            helper.IsWaitForInputIdle = false;
            helper.WindowStyle = ProcessWindowStyle.Hidden;

            helper.ProcessStart();
        }
    }
}
