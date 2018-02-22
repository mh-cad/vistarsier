using log4net;
using System.Runtime.CompilerServices;

namespace CAPI.JobManager
{
    internal static class LogHelper
    {
        public static ILog GetLogger([CallerFilePath] string filename = "")
        {
            var fileSplit = filename.Split('\\');

            if (fileSplit.Length > 1)
                filename = $@"{fileSplit[fileSplit.Length - 2]}\{fileSplit[fileSplit.Length - 1]}";

            return LogManager.GetLogger(filename);
        }
    }
}