using System;
using System.Reflection;
using Infrastructure.Core;

namespace GenerateHighlightContent
{
    public class GenerateHighLight : IGenerateHighLight
    {
        #region -- Field and Property --

        private readonly HighLightSection _section;
        private readonly HighlightProcessRunner _processRunner;
        private readonly HighlightArgumentsBuilder _argumentsBuilder;
        private readonly HighlightTempFileManager _tempFileManager;

        public HighLightSection Config
        {
            get { return _section; }
        }

        #endregion

        #region -- IGenerateHighLight Member --

        public GenerateHighLight()
            : this(
                HighlightConfigurationProvider.LoadFirstAvailable(
                    Assembly.GetEntryAssembly()?.Location,
                    Assembly.GetCallingAssembly().Location,
                    typeof(GenerateHighLight).Assembly.Location),
                new HighlightProcessRunner(),
                new HighlightArgumentsBuilder(),
                new HighlightTempFileManager())
        {
        }

        public GenerateHighLight(
            HighLightSection section,
            HighlightProcessRunner processRunner,
            HighlightArgumentsBuilder argumentsBuilder,
            HighlightTempFileManager tempFileManager)
        {
            _section = section
                ?? throw new ArgumentNullException(nameof(section));

            _processRunner = processRunner
                ?? throw new ArgumentNullException(nameof(processRunner));

            _argumentsBuilder = argumentsBuilder
                ?? throw new ArgumentNullException(nameof(argumentsBuilder));

            _tempFileManager = tempFileManager
                ?? throw new ArgumentNullException(nameof(tempFileManager));
        }

        public string GenerateHighLightCode(
    HighLightParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            HighlightTempFiles files =
                _tempFileManager.Create(
                    parameter.FileName,
                    parameter.Content);

            string arguments = null;

            try
            {
                arguments =
                    _argumentsBuilder.Build(
                        _section,
                        parameter,
                        files.InputFileName,
                        files.OutputFileName);

                _processRunner.Run(
                    PathManager.HighlightFolder,
                    _section.ProcessName,
                    arguments);

                if (!System.IO.File.Exists(
                    files.OutputFileName))
                {
                    throw new System.IO.FileNotFoundException(
                        "Highlight finished without creating the output HTML."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Language:"
                        + Environment.NewLine
                        + parameter.CodeType
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Theme:"
                        + Environment.NewLine
                        + parameter.HighLightStyle
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Working directory:"
                        + Environment.NewLine
                        + PathManager.HighlightFolder
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Arguments:"
                        + Environment.NewLine
                        + arguments
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Expected output:"
                        + Environment.NewLine
                        + files.OutputFileName,
                        files.OutputFileName);
                }

                return files.OutputFileName;
            }
            finally
            {
                _tempFileManager.DeleteInput(files);
            }
        }

        #endregion
    }
}