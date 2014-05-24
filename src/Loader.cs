using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CoffeeScript
{
    /// <summary>
    /// Get the coffee-script.js content form diff sources.
    /// </summary>
    public class Loader
    {
        public static string FromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("coffee-script.js not found.", filePath);

            return File.ReadAllText(filePath);
        }

        public static string FromConfig(string configKey)
        {
            var coffeScriptPath = ConfigurationManager.AppSettings[configKey];
            if (coffeScriptPath == null)
                throw new ArgumentException("Invalid Config.coffescript key provided.", configKey);

            if (!File.Exists(coffeScriptPath))
                throw new FileNotFoundException(string.Format("Path to CoffeeScript.js, provided via config '{0}', not found.", configKey),
                    coffeScriptPath);

            return File.ReadAllText(coffeScriptPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        public static string FromResource(Assembly assembly, string resourceName)
        {
            return GetTextResource(assembly, resourceName);
        }
        private static string GetTextResource(Assembly assembly, string resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string Default()
        {
            return GetTextResource(Assembly.GetExecutingAssembly(), "CoffeeScript.coffee-script.js");
        }
    }
}
