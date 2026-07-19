using System;
using System.IO;
using System.Text;

namespace GenerateHighlightContent
{
    public class HighlightTempFileManager
    {
        public HighlightTempFiles Create(
            string fileName,
            string content)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(
                    "El nombre del archivo temporal no puede estar vacío.",
                    nameof(fileName));
            }

            string tempPath = Path.GetTempPath();

            string inputFileName =
                Path.Combine(tempPath, fileName);

            string outputFileName =
                Path.Combine(tempPath, fileName) + ".html";

            File.WriteAllText(
                inputFileName,
                content ?? string.Empty,
                Encoding.UTF8);

            return new HighlightTempFiles(
                inputFileName,
                outputFileName);
        }

        public void EnsureOutputExists(
            HighlightTempFiles files)
        {
            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            if (!File.Exists(files.OutputFileName))
            {
                throw new FileNotFoundException(
                    "Can not find outputFile.",
                    files.OutputFileName);
            }
        }

        public void DeleteInput(
            HighlightTempFiles files)
        {
            if (files == null)
            {
                return;
            }

            if (File.Exists(files.InputFileName))
            {
                File.Delete(files.InputFileName);
            }
        }
    }

    public class HighlightTempFiles
    {
        public HighlightTempFiles(
            string inputFileName,
            string outputFileName)
        {
            InputFileName = inputFileName;
            OutputFileName = outputFileName;
        }

        public string InputFileName { get; }

        public string OutputFileName { get; }
    }
}