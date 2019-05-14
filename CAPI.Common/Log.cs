using log4net;
using log4net.Config;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace CAPI.Common
{
    public static class Log
    {
        public static ILog GetLogger([CallerFilePath] string filename = "")
        {
            var fileSplit = filename.Split('\\');

            if (fileSplit.Length > 1)
                filename = $@"{fileSplit[fileSplit.Length - 2]}\{fileSplit[fileSplit.Length - 1]}";

            if (Assembly.GetEntryAssembly() != null)
            {
                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
                return LogManager.GetLogger(logRepository.Name, filename);
            }
            else
            {
                return LogManager.GetLogger("test-logger");
            }
            
        }
    }
}
