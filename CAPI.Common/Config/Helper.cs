using CAPI.General;
using System;
using System.Configuration;
using System.IO;

namespace CAPI.Common.Config
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public static class Helper
    {
        private static KeyValueConfigurationCollection GetExeAppConfig()
        {
            var appconfigFilename = Environment.MachineName + ".config";
            var appconfigFolderPath = Environment.CurrentDirectory;
            var appconfigFullPath = Path.Combine(appconfigFolderPath, appconfigFilename);

            return ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap { ExeConfigFilename = appconfigFullPath }, ConfigurationUserLevel.None)
                .AppSettings.Settings;
        }

        public static string GetTestResourcesPath()
        {
            return "../../../resources";
            //var exeAppConfig = GetExeAppConfig();
            //var folderPath = exeAppConfig["TestResources"].Value;
            //if (Directory.Exists(folderPath)) return folderPath;
            //throw new DirectoryNotFoundException($"Test Resources folder does not exist: [{folderPath}]");
        }

        public static string GetProcessLogPath()
        {
            var exeAppConfig = GetExeAppConfig();
            var folderPath = exeAppConfig["ProcessesLogPath"].Value;
            if (!Directory.Exists(folderPath)) FileSystem.DirectoryExistsIfNotCreate(folderPath);
            if (Directory.Exists(folderPath)) return folderPath;
            throw new DirectoryNotFoundException($"Processes Log folder does not exist: [{folderPath}]");
        }
    }
}