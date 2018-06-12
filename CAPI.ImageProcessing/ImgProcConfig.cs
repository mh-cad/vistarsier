using System;
using System.Configuration;
using System.IO;

namespace CAPI.ImageProcessing
{
    public static class ImgProcConfig
    {
        private static readonly KeyValueConfigurationCollection ExeAppConfig = Common.Config.Helper.GetExeAppConfig();

        public static string GetImgProcBinPath()
        {
            var imgProcBinFolderPath = ExeAppConfig["ImgProcBinFolderPath"].Value;
            if (Directory.Exists(imgProcBinFolderPath)) return imgProcBinFolderPath;
            throw new DirectoryNotFoundException($"Image Processing bin folder does not exist in following path [{imgProcBinFolderPath}]");
        }

        public static string GetJavaExeBin()
        {
            var javaExePath = ExeAppConfig["JavaExeFilePath"].Value;
            if (File.Exists(javaExePath)) return javaExePath;
            throw new FileNotFoundException("java.exe file does not exist in specified path", javaExePath);
        }

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
        public static string GetDcm2NiiParams()
        {
            return Properties.Settings.Default.dcm2niiParams;
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

        public static string GetMsProgressionJavaClassName()
        {
            return Properties.Settings.Default.javaClassMsProgression;
        }

        public static string GetSubtractPositiveNii()
        {
            return Properties.Settings.Default.subPosOutFile;
        }
        public static string GetSubtractNegativeNii()
        {
            return Properties.Settings.Default.subNegOutFile;
        }
        public static string GetSubtractMaskNii()
        {
            return Properties.Settings.Default.subMaskOutFile;
        }

        public static string GetSubtractPositiveImgFolder()
        {
            return Properties.Settings.Default.colormapPositiveImages;
        }
        public static string GetSubtractNegativeImgFolder()
        {
            return Properties.Settings.Default.colormapNegativeImages;
        }

        public static string GetSubtractPositiveDcmFolder()
        {
            return Properties.Settings.Default.colormapPositiveDcm;
        }
        public static string GetSubtractNegativeDcmFolder()
        {
            return Properties.Settings.Default.colormapNegativeDcm;
        }

        public static string GetColorMapJavaClassName()
        {
            return Properties.Settings.Default.javaClassColorMap;
        }
        public static string GetColorMapConfigFile()
        {
            var folderPath = ExeAppConfig["ImgProcConfigFolderPath"].Value;
            var filepath = Path.Combine(folderPath, Properties.Settings.Default.colomapConfigFilename);
            if (File.Exists(filepath)) return filepath;
            throw new FileNotFoundException("BFC file does not exist!", filepath);
        }
    }
}