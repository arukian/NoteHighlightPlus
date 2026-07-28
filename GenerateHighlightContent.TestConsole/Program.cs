using System;
using System.Diagnostics;
using System.IO;
using GenerateHighlightContent;
using Infrastructure.Core;

namespace GenerateHighlightContent.TestConsole
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
                Console.WriteLine("=== Assembly locations ===");

                Console.WriteLine(
                    "GenerateHighlightContent:");
                Console.WriteLine(
                    typeof(GenerateHighLight)
                        .Assembly.Location);

                Console.WriteLine();

                Console.WriteLine(
                    "Infrastructure.Core:");
                Console.WriteLine(
                    typeof(PathManager)
                        .Assembly.Location);

                Console.WriteLine();

                Console.WriteLine(
                    "Highlight folder:");
                Console.WriteLine(
                    PathManager.HighlightFolder);

                Console.WriteLine();

                var parameter =
                    new HighLightParameter
                    {
                        FileName = "PreviewTest.py",
                        Content =
                            "def greet(name):\r\n" +
                            "    print(\"Hello \" + name)\r\n\r\n" +
                            "greet(\"Shinx\")",

                        CodeType = "python",
                        HighLightStyle = "shinx",
                        ShowLineNumber = true,
                        Font = "Consolas",
                        FontSize = 10
                    };

                var generator =
                    new GenerateHighLight();

                string outputFile =
                    generator.GenerateHighLightCode(
                        parameter);

                Console.WriteLine();
                Console.WriteLine(
                    "Generated output:");

                Console.WriteLine(outputFile);

                if (!File.Exists(outputFile))
                {
                    throw new FileNotFoundException(
                        "The output file was not created.",
                        outputFile);
                }

                Process.Start(outputFile);
            }
            catch (Exception exception)
            {
                Console.WriteLine();
                Console.WriteLine("=== ERROR ===");
                Console.WriteLine(exception);
            }

            Console.WriteLine();
            Console.WriteLine(
                "Press any key to finish.");

            Console.ReadKey();
        }
    }
}