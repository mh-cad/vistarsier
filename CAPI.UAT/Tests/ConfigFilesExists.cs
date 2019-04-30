using CAPI.Agent;
using CAPI.Config;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CAPI.UAT.Tests
{
    public class ConfigFilesExists : IUatTest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SuccessMessage { get; set; }
        public string FailureMessage { get; set; }
        public string TestGroup { get; set; }
        public CapiConfig CapiConfig { get; set; }
        public AgentRepository Context { get; set; }

        public ConfigFilesExists()
        {
            Name = "Config File Exists?";
            Description = "Checks if config.json file exists in the same folder as the CAPI";
            SuccessMessage = "config.json file was found in application root folder";
            FailureMessage = "config.json file was not found in application root folder";
            TestGroup = "Config";
        }

        public bool Run()
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;
            var folder = Path.GetDirectoryName(codeBase);
            if (!Directory.Exists(folder))
                throw new Exception($"Root folder of application was not found in following path [{folder}]");

            var files = Directory.GetFiles(folder);
            if (files == null || files.Length == 0)
                throw new Exception($"No files were found in application root folder [{folder}]");

            var configFile = files.FirstOrDefault(f => f.ToLower().EndsWith("config.json"));

            var configFileExists = !string.IsNullOrEmpty(configFile) && File.Exists(configFile);

            if (configFileExists) Logger.Write($"Found config.json file at {configFile}", true, Logger.TextType.Success);

            return configFileExists;
        }

        public void FailureResolution()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"application root folder: [{AppDomain.CurrentDomain.BaseDirectory}]");
            Console.WriteLine($"{Environment.NewLine}Write a path you'd want the config.json template file to be copied to:");
            Console.WriteLine("(Press Enter to copy Config template file to application root folder above)");
            var enteredFolder = Console.ReadLine();
            enteredFolder = string.IsNullOrEmpty(enteredFolder) ? AppDomain.CurrentDomain.BaseDirectory : enteredFolder;
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources\\config.template.json");
            if (!string.IsNullOrEmpty(enteredFolder) && Directory.Exists(enteredFolder))
            {
                var copiedFilePath = Path.Combine(enteredFolder, "config.json");
                File.Copy(path, copiedFilePath);
                if (File.Exists(copiedFilePath))
                    Console.WriteLine($"Config file was successfully copied to: [{AppDomain.CurrentDomain.BaseDirectory}]");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid folder path was entered!");
            }
        }
    }
}
