using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GenerateHighlightContent
{
    public static class HighlightConfigurationProvider
    {
        private const string SectionName =
            "HighLightSection";

        public static HighLightSection Load(
            string assemblyLocation)
        {
            ValidateAssemblyLocation(
                assemblyLocation);

            string configFilePath =
                GetConfigFilePath(
                    assemblyLocation);

            return LoadFromConfigFile(
                configFilePath);
        }

        

        public static HighLightSection LoadFirstAvailable(
            params string[] assemblyLocations)
        {
            IEnumerable<string> validLocations =
                (assemblyLocations ??
                    Array.Empty<string>())
                .Where(location =>
                    !string.IsNullOrWhiteSpace(location))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase);

            var checkedConfigFiles =
                new List<string>();

            foreach (string assemblyLocation
                in validLocations)
            {
                string configFilePath =
                    GetConfigFilePath(
                        assemblyLocation);

                checkedConfigFiles.Add(
                    configFilePath);

                if (!File.Exists(configFilePath))
                {
                    continue;
                }

                HighLightSection section =
                    TryLoadFromConfigFile(
                        configFilePath);

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
                    checkedConfigFiles));
        }

        private static HighLightSection LoadFromConfigFile(
            string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException(
                    "No se encontró el archivo de configuración.",
                    configFilePath);
            }

            HighLightSection section =
                TryLoadFromConfigFile(
                    configFilePath);

            if (section == null)
            {
                throw new ConfigurationErrorsException(
                    $"No se encontró la sección '{SectionName}' " +
                    $"en el archivo de configuración:"
                    + Environment.NewLine
                    + configFilePath);
            }

            return section;
        }

        private static HighLightSection TryLoadFromConfigFile(
    string configFilePath)
        {
            ResolveEventHandler assemblyResolver =
                delegate (object sender, ResolveEventArgs args)
                {
                    return ResolveConfigurationAssembly(
                        args,
                        configFilePath);
                };

            AppDomain.CurrentDomain.AssemblyResolve +=
                assemblyResolver;

            try
            {
                var configurationFileMap =
                    new ExeConfigurationFileMap
                    {
                        ExeConfigFilename =
                            configFilePath
                    };

                Configuration configuration =
                    ConfigurationManager
                        .OpenMappedExeConfiguration(
                            configurationFileMap,
                            ConfigurationUserLevel.None);

                return configuration
                    .GetSection(
                        SectionName)
                    as HighLightSection;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -=
                    assemblyResolver;
            }
        }

        private static Assembly ResolveConfigurationAssembly(
    ResolveEventArgs args,
    string configFilePath)
        {
            if (args == null ||
                string.IsNullOrWhiteSpace(args.Name))
            {
                return null;
            }

            AssemblyName requestedAssemblyName;

            try
            {
                requestedAssemblyName =
                    new AssemblyName(
                        args.Name);
            }
            catch
            {
                return null;
            }

            Assembly currentAssembly =
                typeof(HighLightSection).Assembly;

            AssemblyName currentAssemblyName =
                currentAssembly.GetName();

            if (string.Equals(
                requestedAssemblyName.Name,
                currentAssemblyName.Name,
                StringComparison.OrdinalIgnoreCase))
            {
                return currentAssembly;
            }

            string configDirectory =
                Path.GetDirectoryName(
                    configFilePath);

            if (string.IsNullOrWhiteSpace(configDirectory))
            {
                return null;
            }

            string candidatePath =
                Path.Combine(
                    configDirectory,
                    requestedAssemblyName.Name + ".dll");

            if (!File.Exists(candidatePath))
            {
                return null;
            }

            Assembly alreadyLoadedAssembly =
                AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(
                        assembly =>
                            string.Equals(
                                assembly.GetName().Name,
                                requestedAssemblyName.Name,
                                StringComparison.OrdinalIgnoreCase));

            if (alreadyLoadedAssembly != null)
            {
                return alreadyLoadedAssembly;
            }

            return Assembly.LoadFrom(
                candidatePath);
        }

        private static string GetConfigFilePath(
            string assemblyLocation)
        {
            ValidateAssemblyLocation(
                assemblyLocation);

            string normalizedLocation =
                Path.GetFullPath(
                    assemblyLocation.Trim());

            if (normalizedLocation.EndsWith(
                ".config",
                StringComparison.OrdinalIgnoreCase))
            {
                return normalizedLocation;
            }

            return normalizedLocation + ".config";
        }

        private static void ValidateAssemblyLocation(
            string assemblyLocation)
        {
            if (string.IsNullOrWhiteSpace(
                assemblyLocation))
            {
                throw new ArgumentException(
                    "La ubicación del ensamblado no puede estar vacía.",
                    nameof(assemblyLocation));
            }
        }
    }
}