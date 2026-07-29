using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace GenerateHighlightContent
{
    public static class HighlightConfigurationProvider
    {
        private const string SectionName = "HighLightSection";

        public static HighLightSection Load(
            string assemblyLocation)
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

        public static HighLightSection LoadFirstAvailable(
            params string[] assemblyLocations)
        {
            IEnumerable<string> validLocations =
                assemblyLocations
                    .Where(location =>
                        !string.IsNullOrWhiteSpace(location))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase);

            var checkedLocations =
                new List<string>();

            foreach (string assemblyLocation in validLocations)
            {
                checkedLocations.Add(
                    assemblyLocation);

                Configuration configuration =
                    ConfigurationManager.OpenExeConfiguration(
                        assemblyLocation);

                HighLightSection section =
                    configuration.GetSection(SectionName)
                    as HighLightSection;

                if (section != null)
                {
                    return section;
                }
            }

            throw new ConfigurationErrorsException(
                $"No se encontró la sección '{SectionName}' " +
                "en ninguna de las configuraciones revisadas."
                + Environment.NewLine
                + Environment.NewLine
                + string.Join(
                    Environment.NewLine,
                    checkedLocations));
        }
    }
}