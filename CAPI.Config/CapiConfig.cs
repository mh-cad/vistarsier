using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CAPI.Config
{
    [JsonConverter(typeof(CapiConfigJsonConverter))]
    // ReSharper disable once ClassNeverInstantiated.Global
    /// <summary>
    /// Finds the proper config file based on the arguments passed
    /// </summary>
    public class CapiConfig
    {
        public Binaries Binaries { get; set; }
        public DicomConfig DicomConfig { get; set; }
        public ImagePaths ImagePaths { get; set; }

        public string RunInterval { get; set; }
        public string AgentDbConnectionString { get; set; }
        public string ManualProcessPath { get; set; }
        public string Hl7ProcessPath { get; set; }
        public string DefaultRecipePath { get; set; }
        public bool ProcessCasesAddedManually { get; set; }
        public bool ProcessCasesAddedByHL7 { get; set; }

        public static CapiConfig GetConfig()
        {
            var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

            CapiConfig config;

            if (File.Exists(configFilePath))
            {
                var configFileContent = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<CapiConfig>(configFileContent, new CapiConfigJsonConverter());
            }
            else
            {
                config = GenerateDefault();
                File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, new CapiConfigJsonConverter()));
            }

            return config;
        }

        public static CapiConfig GenerateDefault()
        {
            return new CapiConfig
            {
                Binaries = new Binaries(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "3rdparty_bin/")),
                DicomConfig = new DicomConfig(),
                ImagePaths = new ImagePaths(),
                RunInterval = "30",
                ManualProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cases/manual/"),
                Hl7ProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cases/hl7/"),
                DefaultRecipePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default.recipe.json"),
                ProcessCasesAddedManually = true,
                ProcessCasesAddedByHL7 = true
            };
        }
    }
}
