using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Core
{
    public static class AppConfiguration
    {
        public static string GetAppSetting(
            string key,
            string defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    "La clave de configuración no puede estar vacía.",
                    nameof(key));
            }

            string value =
                System.Configuration.ConfigurationManager.AppSettings[key];

            return string.IsNullOrWhiteSpace(value)
                ? defaultValue
                : value;
        }
    }
}