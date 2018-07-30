using System;
using System.Configuration;
using System.IO;

namespace CAPI.ImageProcessing
{
    public static class ImgProcConfig
    {
        private static readonly KeyValueConfigurationCollection ExeAppConfig = null;//Common.Config.Helper.GetExeAppConfig();
        
        public static string GetJavaClassPath()
        {
            var javaClasspath = ExeAppConfig["JavaClasspath"].Value;
            if (!string.IsNullOrEmpty(javaClasspath)) return javaClasspath;
            throw new ArgumentNullException(nameof(javaClasspath), "Java Classpath has no value in executing app config file.");
        }

        public static string GetDcm2NiiExeFilePath()
        {
            var folderPath = ExeAppConfig["ImgProcBinFolderPath"].Value;
            var filepath = Path.Combine(folderPath, Properties.Settings.Default.dcm2niiFilename);
            if (File.Exists(filepath)) return filepath;
            throw new FileNotFoundException("Dcm2Nii file does not exist!", filepath);
        }

        public static string GetBseExeFilePath()
        {
            var folderPath = ExeAppConfig["ImgProcBinFolderPath"].Value;
            var filepath = Path.Combine(folderPath, Properties.Settings.Default.bseFilename);
            if (File.Exists(filepath)) return filepath;
            throw new FileNotFoundException("BSE file does not exist!", filepath);
        }
        public static string GetBseParams()
        {
            return Properties.Settings.Default.bseParams;
        }

        public static string GetRegistrationFilePath()
        {
            var folderPath = ExeAppConfig["ImgProcBinFolderPath"].Value;
            var filepath = Path.Combine(folderPath, Properties.Settings.Default.registrationFilename);
            if (File.Exists(filepath)) return filepath;
            throw new FileNotFoundException("Registration file does not exist!", filepath);
        }
        public static string GetRegistrationParams()
        {
            return Properties.Settings.Default.registrationParams;
        }

        public static string GetReformatXFilePath()
        {
            var folderPath = ExeAppConfig["ImgProcBinFolderPath"].Value;
            var filepath = Path.Combine(folderPath, Properties.Settings.Default.reformatxFilename);
            if (File.Exists(filepath)) return filepath;
            throw new FileNotFoundException("ReformatX file does not exist!", filepath);
        }

        public static string GetCmtkRawxformFile()
        {
            return Properties.Settings.Default.cmtkRawXformFilename;
        }
        public static string GetCmtkResultxformFile()
        {
            return Properties.Settings.Default.reformatResultsFilename;
        }

        public static string GetCmtkFolderName()
        {
            return Properties.Settings.Default.reformatResultsFolderName;
        }

        public static string GetBfcExeFilePath()
        {
            var folderPath = ExeAppConfig["ImgProcBinFolderPath"].Value;
            var filepath = Path.Combine(folderPath, Properties.Settings.Default.bfcFilename);
            if (File.Exists(filepath)) return filepath;
            throw new FileNotFoundException("BFC file does not exist!", filepath);
        }
        public static string GetBfcParams()
        {
            return Properties.Settings.Default.bfcParams;
        }
    }
}