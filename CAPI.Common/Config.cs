using System.Configuration;

namespace CAPI.Common
{
    public static class Config
    {
        public static string GetProcessesRootDir()
        {
            return ConfigurationManager.AppSettings.Get("ProcessesRootDir");
        }
        public static string GetExecutablesPath()
        {
            return GetProcessesRootDir() + "\\" + ConfigurationManager.AppSettings.Get("ExecutablesDirName");
        }
        public static string GetOutputDir()
        {
            return ConfigurationManager.AppSettings.Get("OutputDir");
        }
        public static string GetFixedDicomDir()
        {
            return ConfigurationManager.AppSettings.Get("FixedDicomDir");
        }
        public static string GetFloatingDicomDir()
        {
            return ConfigurationManager.AppSettings.Get("FloatingDicomDir");
        }
        public static string GetJavaExePath()
        {
            return ConfigurationManager.AppSettings.Get("JavaBin");
        }
        public static string GetJavaUtilsPath()
        {
            return ConfigurationManager.AppSettings.Get("JavaUtilsPath");
        }
        public static string GetProcessesLogPath()
        {
            return ConfigurationManager.AppSettings.Get("ProcessesLogPath");
        }
        public static string GetImageRepositoryPath()
        {
            return ConfigurationManager.AppSettings.Get("ImageRepositoryPath");
        }
    }
}