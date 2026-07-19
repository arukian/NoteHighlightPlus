using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using Helper;
using Infrastructure.Core;

namespace GenerateHighlightContent
{
    public class GenerateHighLight : IGenerateHighLight
    {
        #region -- Field and Property --

        public string Content { get; set; }

        public string CodeType { get; set; }

        public string HighLightStyle { get; set; }

        public bool ShowLineNumber { get; set; }

        public string FileName { get; set; }

        public string Font { get; set; }

        public int FontSize { get; set; }

        private readonly HighLightSection _section;
        private readonly HighlightProcessRunner _processRunner;

        public HighLightSection Config
        {
            get { return _section; }
        }

        #endregion

        #region -- IGenerateHighLight Member --

        public GenerateHighLight()
            : this(
                HighlightConfigurationProvider.Load(
                    Assembly.GetCallingAssembly().Location),
                new HighlightProcessRunner())
        {
        }

        public GenerateHighLight(
            HighLightSection section,
            HighlightProcessRunner processRunner)
        {
            _section = section
                ?? throw new ArgumentNullException(nameof(section));

            _processRunner = processRunner
                ?? throw new ArgumentNullException(nameof(processRunner));
        }

        public string GenerateHighLightCode(
            HighLightParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            InitParameter(parameter);

            string tempPath = Path.GetTempPath();
            string inputFileName =
                Path.Combine(tempPath, FileName);

            string outputFileName =
                Path.Combine(tempPath, FileName) + ".html";

            File.WriteAllText(
                inputFileName,
                Content,
                Encoding.UTF8);

            try
            {
                string arguments = GenerateArguments(
                    inputFileName,
                    outputFileName);

                _processRunner.Run(
                    PathManager.HighlightFolder,
                    _section.ProcessName,
                    arguments);

                if (!File.Exists(outputFileName))
                {
                    throw new FileNotFoundException(
                        "Can not find outputFile.",
                        outputFileName);
                }

                return outputFileName;
            }
            finally
            {
                if (File.Exists(inputFileName))
                {
                    File.Delete(inputFileName);
                }
            }
        }

        private void InitParameter(
            HighLightParameter parameter)
        {
            Content = parameter.Content;
            CodeType = parameter.CodeType;
            HighLightStyle = parameter.HighLightStyle;
            ShowLineNumber = parameter.ShowLineNumber;
            FileName = parameter.FileName;
            Font = parameter.Font;
            FontSize = parameter.FontSize;
        }

        private string GenerateArguments(
            string inputFileName,
            string outputFileName)
        {
            StringBuilder sb = new StringBuilder();

            ReadConfigCollection(
                sb,
                _section.GeneralArguments);

            ReadConfigCollection(
                sb,
                _section.OutputArguments);

            if (ShowLineNumber)
            {
                Argument lineNumbersArgument =
                    _section.OutputArguments["LineNumbers"];

                if (lineNumbersArgument != null)
                {
                    sb.Append(" ");
                    sb.Append(lineNumbersArgument.Key);
                }
            }

            return sb
                .ToString()
                .TemplateSubstitute(new
                {
                    inputFileName =
                        string.Format(
                            "\"{0}\"",
                            inputFileName),

                    outputFileName =
                        string.Format(
                            "\"{0}\"",
                            outputFileName),

                    codeType = CodeType,
                    highLightStyle = HighLightStyle,

                    font =
                        string.Format(
                            "\"{0}\"",
                            Font),

                    fontSize = FontSize
                });
        }

        private void ReadConfigCollection(
            StringBuilder sb,
            ConfigurationElementCollection collection)
        {
            foreach (Argument item in collection)
            {
                if (item.Option)
                {
                    continue;
                }

                sb.Append(item.Key);

                if (!string.IsNullOrEmpty(item.Value))
                {
                    sb.Append(" ");

                    sb.Append(
                        item.Value.Contains(" ")
                            ? string.Format(
                                "\"{0}\"",
                                item.Value)
                            : item.Value);
                }

                sb.Append(" ");
            }
        }

        #endregion
    }
}
