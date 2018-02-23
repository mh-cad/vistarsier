using Microsoft.Win32;

namespace CAPI.ImageProcessing
{
    public static class ImgProcConfig
    {
        private const string RegistryKeyPath = "SOFTWARE\\CAPI\\ImageProcessing\\FilesAndParameters";
        private static readonly RegistryKey RegKey = Registry.LocalMachine.OpenSubKey(RegistryKeyPath);

        public static string GetDcm2NiiExe()
        {
            return RegKey?.GetValue("Dcm2NiiExe").ToString() ?? "";
        }
        public static string GetDcm2NiiHdrParams()
        {
            return RegKey?.GetValue("Dcm2NiiHdrParams").ToString() ?? "";
        }
        public static string GetDcm2NiiNiiParams()
        {
            return RegKey?.GetValue("Dcm2NiiNiiParams").ToString() ?? "";
        }
        public static string GetMiconvFileName()
        {
            return RegKey?.GetValue("MiconvFileName").ToString() ?? "";
        }
    }
}
