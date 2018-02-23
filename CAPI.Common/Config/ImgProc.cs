using Microsoft.Win32;

namespace CAPI.Common.Config
{
    public static class ImgProc
    {
        private const string RegistryKeyPath = "SOFTWARE\\CAPI\\ImageProcessing";
        private static readonly RegistryKey RegKey = Registry.LocalMachine.OpenSubKey(RegistryKeyPath);

        public static string GetProcessesRootDir()
        {
            return RegKey?.GetValue("ProcessesRootDir").ToString() ?? "";
        }
        public static string GetExecutablesPath()
        {
            var executablesFolderName = RegKey?.GetValue("ExecutablesPath").ToString() ?? "";

            return string.IsNullOrEmpty(executablesFolderName) ? "" :
                GetProcessesRootDir() + "\\" + executablesFolderName;
        }
        public static string GetJavaExePath()
        {
            return RegKey?.GetValue("JavaBinPath").ToString() ?? "";
        }
        public static string GetJavaUtilsPath()
        {
            return RegKey?.GetValue("JavaUtilsPath").ToString() ?? "";
        }
        public static string GetProcessesLogPath()
        {
            return RegKey?.GetValue("ProcessesLogPath").ToString() ?? "";
        }
        public static string GetImageRepositoryPath()
        {
            return RegKey?.GetValue("ImageRepositoryPath").ToString() ?? "";
        }
        public static string GetManualProcessPath()
        {
            return RegKey?.GetValue("ManualProcessPath").ToString() ?? "";
        }
        public static string GetProcessCasesAddedManually()
        {
            return RegKey?.GetValue("ProcessCasesAddedManually").ToString() ?? "";
        }
        public static string GetProcessCasesAddedByHl7()
        {
            return RegKey?.GetValue("ProcessCasesAddedByHL7").ToString() ?? "";
        }
    }
}