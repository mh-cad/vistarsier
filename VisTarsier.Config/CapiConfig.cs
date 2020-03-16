using Newtonsoft.Json;
using System;
using System.IO;
using VisTarsier.Common;

namespace VisTarsier.Config
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
            var configFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../cfg/config.json"));
            var log = Log.GetLogger();
            log.Debug("Config path: " + configFilePath);
            if (File.Exists(configFilePath))
            {
                //log.Info("Config file found");
            }
            else
            {
                log.Error("Couldn't find config file at " + configFilePath);
            }

            return GetConfig(configFilePath);
        }

        public static void WriteConfig(CapiConfig config)
        {
            var configFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..{Path.DirectorySeparatorChar}cfg{Path.DirectorySeparatorChar}config.json"));
            FileSystem.DirectoryExistsIfNotCreate(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..{Path.DirectorySeparatorChar}cfg{Path.DirectorySeparatorChar}")));
            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, new CapiConfigJsonConverter())); 
          
        }

        public static void WriteRecipe(Recipe recipe)
        {
            var recipePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..{Path.DirectorySeparatorChar}cfg{Path.DirectorySeparatorChar}defaultrecipe.json"));
            FileSystem.DirectoryExistsIfNotCreate(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..{Path.DirectorySeparatorChar}cfg{Path.DirectorySeparatorChar}")));
            File.WriteAllText(recipePath, JsonConvert.SerializeObject(recipe, Formatting.Indented));
        }

        public static CapiConfig GetConfig(string configFilePath)
        {
            CapiConfig config;
            if (File.Exists(configFilePath))
            {
                var configFileContent = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<CapiConfig>(configFileContent, new CapiConfigJsonConverter());
            }
            else
            {
                config = GenerateDefault();
                try { File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, new CapiConfigJsonConverter())); }
                catch { }
            }

            return config;
        }

        public static Recipe GetDefaultRecipe()
        {
            Recipe recipe;
            var recipePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..{Path.DirectorySeparatorChar}cfg{Path.DirectorySeparatorChar}defaultrecipe.json"));
            if (File.Exists(recipePath))
            {
                var fileContent = File.ReadAllText(recipePath);
                recipe = JsonConvert.DeserializeObject<Recipe>(fileContent);
            }
            else
            {
                recipe = Recipe.Default();
            }

            return recipe;
        }

        public static CapiConfig GenerateDefault()
        {
            return new CapiConfig
            {
                AgentDbConnectionString = "Server=;Database=Capi;User Id=;Password=;Connection Timeout=120",
                Binaries = new Binaries(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..{Path.DirectorySeparatorChar}service{Path.DirectorySeparatorChar}3rdparty_bin{Path.DirectorySeparatorChar}"))),
                DicomConfig = new DicomConfig(),
                ImagePaths = new ImagePaths(),
                RunInterval = "30",
                ManualProcessPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..{Path.DirectorySeparatorChar}cases{Path.DirectorySeparatorChar}manual{Path.DirectorySeparatorChar}")),
                Hl7ProcessPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..{Path.DirectorySeparatorChar}cases{Path.DirectorySeparatorChar}hl7{Path.DirectorySeparatorChar}")),
                DefaultRecipePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..{Path.DirectorySeparatorChar}cfg{Path.DirectorySeparatorChar}default.recipe.json")),
                ProcessCasesAddedManually = true,
                ProcessCasesAddedByHL7 = true
            };
        }
    }
}
