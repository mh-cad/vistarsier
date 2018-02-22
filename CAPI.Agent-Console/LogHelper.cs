using log4net;
using System.Runtime.CompilerServices;

namespace CAPI.Agent_Console
{
    internal static class LogHelper
    {
        public static ILog GetLogger([CallerFilePath] string filename = "")
        {
            return LogManager.GetLogger(filename);
        }
    }
}