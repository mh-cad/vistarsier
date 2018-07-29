using CAPI.Common.Config;
using Newtonsoft.Json;
using System;
using System.IO;

namespace CAPI.Tests
{
    public static class CapiConfigGetter
    {
        public static CapiConfig GetCapiConfig()
        {
            var machineName = Environment.MachineName;
            var configFileName = $"config.dev.{machineName}.json";
            var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);

            if (!File.Exists(configFilePath))
                throw new FileNotFoundException(configFilePath);

            return JsonConvert.DeserializeObject<CapiConfig>(configFilePath);
        }
    }
}
