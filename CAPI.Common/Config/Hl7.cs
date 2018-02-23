using Microsoft.Win32;

namespace CAPI.Common.Config
{
    public static class Hl7
    {
        private const string RegistryKeyPath = "SOFTWARE\\CAPI\\HL7";
        private static readonly RegistryKey RegKey = Registry.LocalMachine.OpenSubKey(RegistryKeyPath);

        public static string GetCompletedMrisPath()
        {
            return RegKey?.GetValue("CompletedMRIsPath").ToString() ?? "";
        }
    }
}