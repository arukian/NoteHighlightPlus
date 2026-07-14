using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateHighlightContent
{
    public static class HighlightConfigurationProvider
    {
        private const string SectionName = "HighLightSection";

        public static HighLightSection Load(string assemblyLocation)
        {
            if (string.IsNullOrWhiteSpace(assemblyLocation))
            {
                throw new ArgumentException(
                    "La ubicación del ensamblado no puede estar vacía.",
                    nameof(assemblyLocation));
            }

            Configuration configuration =
                ConfigurationManager.OpenExeConfiguration(
                    assemblyLocation);

            HighLightSection section =
                configuration.GetSection(SectionName)
                as HighLightSection;

            if (section == null)
            {
                throw new ConfigurationErrorsException(
                    $"No se encontró la sección '{SectionName}' " +
                    $"en la configuración asociada a: {assemblyLocation}");
            }

            return section;
        }
    }
}