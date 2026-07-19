using System;
using System.Configuration;
using System.Text;
using Helper;

namespace GenerateHighlightContent
{
    public class HighlightArgumentsBuilder
    {
        public string Build(
            HighLightSection section,
            HighLightParameter parameter,
            string inputFileName,
            string outputFileName)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(inputFileName))
            {
                throw new ArgumentException(
                    "El archivo de entrada no puede estar vacío.",
                    nameof(inputFileName));
            }

            if (string.IsNullOrWhiteSpace(outputFileName))
            {
                throw new ArgumentException(
                    "El archivo de salida no puede estar vacío.",
                    nameof(outputFileName));
            }

            StringBuilder builder = new StringBuilder();

            AppendConfigurationArguments(
                builder,
                section.GeneralArguments);

            AppendConfigurationArguments(
                builder,
                section.OutputArguments);

            AppendLineNumbersArgument(
                builder,
                section,
                parameter.ShowLineNumber);

            return builder
                .ToString()
                .TemplateSubstitute(new
                {
                    inputFileName = Quote(inputFileName),
                    outputFileName = Quote(outputFileName),
                    codeType = parameter.CodeType,
                    highLightStyle = parameter.HighLightStyle,
                    font = Quote(parameter.Font),
                    fontSize = parameter.FontSize
                });
        }

        private static void AppendConfigurationArguments(
            StringBuilder builder,
            ConfigurationElementCollection collection)
        {
            if (collection == null)
            {
                return;
            }

            foreach (Argument argument in collection)
            {
                if (argument.Option)
                {
                    continue;
                }

                builder.Append(argument.Key);

                if (!string.IsNullOrEmpty(argument.Value))
                {
                    builder.Append(" ");
                    builder.Append(
                        argument.Value.Contains(" ")
                            ? Quote(argument.Value)
                            : argument.Value);
                }

                builder.Append(" ");
            }
        }

        private static void AppendLineNumbersArgument(
            StringBuilder builder,
            HighLightSection section,
            bool showLineNumber)
        {
            if (!showLineNumber)
            {
                return;
            }

            Argument lineNumbersArgument =
                section.OutputArguments["LineNumbers"];

            if (lineNumbersArgument == null ||
                string.IsNullOrWhiteSpace(lineNumbersArgument.Key))
            {
                return;
            }

            builder.Append(" ");
            builder.Append(lineNumbersArgument.Key);
        }

        private static string Quote(string value)
        {
            return string.Format(
                "\"{0}\"",
                value ?? string.Empty);
        }
    }
}