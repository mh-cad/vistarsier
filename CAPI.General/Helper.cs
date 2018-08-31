using System;
using System.Configuration;
using System.IO;

namespace CAPI.General
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Helper
    {
        //private readonly IFileSystem _fileSystem;

        //public Helper(IFileSystem fileSystem)
        //{
        //    _fileSystem = fileSystem;
        //}

        private static KeyValueConfigurationCollection GetExeAppConfig()
        {
            var appconfigFilename = Environment.MachineName + ".config";
            var appconfigFolderPath = Environment.CurrentDirectory;
            var appconfigFullPath = Path.Combine(appconfigFolderPath, appconfigFilename);

            return ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap { ExeConfigFilename = appconfigFullPath }, ConfigurationUserLevel.None)
                .AppSettings.Settings;
        }

        public static string GetJavaExePath()
        {
            var exeAppConfig = GetExeAppConfig();
            var filePath = exeAppConfig["JavaExeFilePath"].Value;
            if (File.Exists(filePath)) return filePath;
            throw new DirectoryNotFoundException($"Java.exe does not exist in following path: [{filePath}]");
        }
    }
}