﻿using System;
using System.Configuration;
using System.IO;

namespace CAPI.Common.Config
{
    public static class Helper
    {
        public static KeyValueConfigurationCollection GetExeAppConfig()
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
            var exeAppConfig = GetExeAppConfig();
            var folderPath = exeAppConfig["TestResources"].Value;
            if (Directory.Exists(folderPath)) return folderPath;
            throw new DirectoryNotFoundException($"Test Resources folder does not exist: [{folderPath}]");
        }

        public static string GetProcessLogPath()
        {
            var exeAppConfig = GetExeAppConfig();
            var folderPath = exeAppConfig["ProcessesLogPath"].Value;
            if (!Directory.Exists(folderPath)) Services.FileSystem.DirectoryExistsIfNotCreate(folderPath);
            if (Directory.Exists(folderPath)) return folderPath;
            throw new DirectoryNotFoundException($"Processes Log folder does not exist: [{folderPath}]");
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