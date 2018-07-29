using Newtonsoft.Json;
using System;
using System.IO;

namespace CAPI.Common.Config
{
    public class CapiConfig
    {
        public DicomConfig DicomConfig { get; set; }
        public ImgProcConfig ImgProcConfig { get; set; }
        public TestsConfig TestsConfig { get; set; }

        public static CapiConfig GetConfig(string[] args = null)
        {
            var configFile = "config.json";
            var env = ".";
            if (args != null && args.Length > 0)
            {
                if (string.Equals(args[0], "-dev", StringComparison.InvariantCultureIgnoreCase))
                    env = $".dev.{Environment.MachineName}";
                else if (string.Equals(args[0], "-staging", StringComparison.InvariantCultureIgnoreCase))
                    env = $".staging.{Environment.MachineName}";

                var devConfigFile = $@"config{env}.json";
                if (!File.Exists(devConfigFile))
                    throw new FileNotFoundException(devConfigFile);
                configFile = devConfigFile;
            }

            var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile);
            var config = JsonConvert.DeserializeObject<CapiConfig>(File.ReadAllText(configFilePath));

            return config;
        }
    }
}
