using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GenerateHighlightContent
{
    /// <summary>
    /// Executes highlight.exe and reports the process output when execution fails.
    /// </summary>
    public sealed class HighlightProcessRunner
    {
        public void Run(
            string workingDirectory,
            string processName,
            string arguments)
        {
            if (string.IsNullOrWhiteSpace(workingDirectory))
            {
                throw new ArgumentException(
                    "The Highlight working directory cannot be empty.",
                    nameof(workingDirectory));
            }

            if (!Directory.Exists(workingDirectory))
            {
                throw new DirectoryNotFoundException(
                    "The Highlight working directory was not found: "
                    + workingDirectory);
            }

            if (string.IsNullOrWhiteSpace(processName))
            {
                throw new ArgumentException(
                    "The Highlight process name cannot be empty.",
                    nameof(processName));
            }

            string processPath =
                Path.IsPathRooted(processName)
                    ? processName
                    : Path.Combine(
                        workingDirectory,
                        processName);

            if (!File.Exists(processPath))
            {
                throw new FileNotFoundException(
                    "The Highlight executable was not found.",
                    processPath);
            }

            ProcessStartInfo startInfo =
                new ProcessStartInfo
                {
                    WorkingDirectory =
                        workingDirectory,

                    FileName =
                        processPath,

                    Arguments =
                        arguments ?? string.Empty,

                    UseShellExecute =
                        false,

                    RedirectStandardOutput =
                        true,

                    RedirectStandardError =
                        true,

                    CreateNoWindow =
                        true,

                    WindowStyle =
                        ProcessWindowStyle.Hidden
                };

            using (Process process = new Process())
            {
                StringBuilder standardOutput =
                    new StringBuilder();

                StringBuilder standardError =
                    new StringBuilder();

                process.StartInfo =
                    startInfo;

                process.OutputDataReceived +=
                    (sender, eventArgs) =>
                    {
                        if (eventArgs.Data != null)
                        {
                            standardOutput.AppendLine(
                                eventArgs.Data);
                        }
                    };

                process.ErrorDataReceived +=
                    (sender, eventArgs) =>
                    {
                        if (eventArgs.Data != null)
                        {
                            standardError.AppendLine(
                                eventArgs.Data);
                        }
                    };

                try
                {
                    process.Start();
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        "Highlight could not be started."
                        + Environment.NewLine
                        + "Executable: "
                        + processPath
                        + Environment.NewLine
                        + "Arguments: "
                        + startInfo.Arguments,
                        exception);
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                // Garantiza que los eventos asíncronos terminen de procesarse.
                process.WaitForExit();

                string output =
                    standardOutput.ToString().Trim();

                string error =
                    standardError.ToString().Trim();

                if (process.ExitCode == 0 && string.IsNullOrWhiteSpace(error))
                {
                    return;
                }

                throw new InvalidOperationException(
                    "Highlight execution failed."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Exit code: "
                    + process.ExitCode
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Executable:"
                    + Environment.NewLine
                    + processPath
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Arguments:"
                    + Environment.NewLine
                    + startInfo.Arguments
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Standard output:"
                    + Environment.NewLine
                    + (string.IsNullOrWhiteSpace(output)
                        ? "(empty)"
                        : output)
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Standard error:"
                    + Environment.NewLine
                    + (string.IsNullOrWhiteSpace(error)
                        ? "(empty)"
                        : error));
            }
        }
    }
}