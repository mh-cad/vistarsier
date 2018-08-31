using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CAPI.Common.Config
{
    [JsonConverter(typeof(CapiConfigJsonConverter))]
    // ReSharper disable once ClassNeverInstantiated.Global
    /// <summary>
    /// Finds the proper config file based on the arguments passed
    /// </summary>
    public class CapiConfig //: ICapiConfig
    {
        public CapiConfig() { }

        public CapiConfig(DicomConfig dicomConfig, ImgProcConfig imgProcConfig, TestsConfig testsConfig)
        {
            DicomConfig = dicomConfig;
            ImgProcConfig = imgProcConfig;
            TestsConfig = testsConfig;
        }

        public DicomConfig DicomConfig { get; set; }
        public ImgProcConfig ImgProcConfig { get; set; }
        public TestsConfig TestsConfig { get; set; }

        public string RunInterval { get; set; }
        public string AgentDbConnectionString { get; set; }
        public string ManualProcessPath { get; set; }
        public string Hl7ProcessPath { get; set; }
        public string DefaultRecipePath { get; set; }
        public bool ProcessCasesAddedManually { get; set; }
        public bool ProcessCasesAddedByHL7 { get; set; }

        /// <summary>
        /// Arguments passed from running app
        /// [option] arg1 = -f / arg2 = %config_file_path%
        /// [option] -dev => for development environment
        /// [option] -staging => for staging environment
        /// </summary>
        /// <param name="args"></param>
        /// <returns>CapiConfig which contains DicomConfig, ImgProcConfig and TestsConfig</returns>
        public CapiConfig GetConfig(string[] args = null)
        {
            var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

            // Has arguaments
            if (args != null && args.Length > 0)
            {
                if (args.Contains("-f", StringComparer.InvariantCultureIgnoreCase))
                { // config file provided
                    var configFileFlag = args.Single(a => a.Equals("-f", StringComparison.InvariantCultureIgnoreCase));
                    var configFileIndex = Array.IndexOf(args, configFileFlag) + 1;
                    if (args.Length <= configFileIndex)
                        throw new ArgumentException("-f Flag should be followed by config file full path");
                    var configFilePathArg = args[configFileIndex];
                    if (!File.Exists(configFilePathArg))
                        throw new FileNotFoundException($"Unable to locate the following file: [{configFilePathArg}]");
                    configFilePath = configFilePathArg;
                }
                else
                { // config file not provided
                    var configFileName = GetNonProductionConfigFile(args);
                    configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);
                }
            }

            if (string.IsNullOrEmpty(configFilePath) || !File.Exists(configFilePath))
                throw new FileNotFoundException($"Unable to locate the following file: [{configFilePath}]");
            var configFileContent = File.ReadAllText(configFilePath);
            var config = JsonConvert.DeserializeObject<CapiConfig>(configFileContent, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            return config;
        }

        private static string GetNonProductionConfigFile(IReadOnlyList<string> args)
        {
            var env = "."; // default config file = config{env}json = config.json  
            if (string.Equals(args[0], "-dev", StringComparison.InvariantCultureIgnoreCase))
                env = $".dev.{Environment.MachineName}.";
            else if (string.Equals(args[0], "-staging", StringComparison.InvariantCultureIgnoreCase))
                env = $".staging.{Environment.MachineName}.";

            var nonProdConfigFileName = $@"config{env}json";
            if (!File.Exists(nonProdConfigFileName))
                throw new FileNotFoundException(nonProdConfigFileName);
            return nonProdConfigFileName;
        }
    }
}
