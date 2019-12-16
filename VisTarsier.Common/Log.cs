using log4net;
using log4net.Config;
using log4net.Core;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace VisTarsier.Common
{
    public static class Log
    {
        public static ILog GetLogger([CallerFilePath] string filename = "")
        {
            try
            {
                return GetLog4Net(filename);
            }
            catch
            {
                return new FailedLogger();
            }
        }

        private static ILog GetLog4Net(string filename)
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
                const string V = "test-logger";
                return LogManager.GetLogger(V, typeof(Log));
            }
        }

        public class FailedLogger : ILog
        {
            public bool IsDebugEnabled => throw new NotImplementedException();

            public bool IsInfoEnabled => throw new NotImplementedException();

            public bool IsWarnEnabled => throw new NotImplementedException();

            public bool IsErrorEnabled => throw new NotImplementedException();

            public bool IsFatalEnabled => throw new NotImplementedException();

            public ILogger Logger => throw new NotImplementedException();

            public void Debug(object message)
            {
                System.Console.WriteLine(message?.ToString());
            }

            public void Debug(object message, Exception exception)
            {
                System.Console.WriteLine(message.ToString());
                System.Console.WriteLine(exception.StackTrace.ToString());
            }

            public void DebugFormat(string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void DebugFormat(string format, object arg0)
            {
                throw new NotImplementedException();
            }

            public void DebugFormat(string format, object arg0, object arg1)
            {
                throw new NotImplementedException();
            }

            public void DebugFormat(string format, object arg0, object arg1, object arg2)
            {
                throw new NotImplementedException();
            }

            public void DebugFormat(IFormatProvider provider, string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void Error(object message)
            {
                throw new NotImplementedException();
            }

            public void Error(object message, Exception exception)
            {
                throw new NotImplementedException();
            }

            public void ErrorFormat(string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void ErrorFormat(string format, object arg0)
            {
                throw new NotImplementedException();
            }

            public void ErrorFormat(string format, object arg0, object arg1)
            {
                throw new NotImplementedException();
            }

            public void ErrorFormat(string format, object arg0, object arg1, object arg2)
            {
                throw new NotImplementedException();
            }

            public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void Fatal(object message)
            {
                throw new NotImplementedException();
            }

            public void Fatal(object message, Exception exception)
            {
                throw new NotImplementedException();
            }

            public void FatalFormat(string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void FatalFormat(string format, object arg0)
            {
                throw new NotImplementedException();
            }

            public void FatalFormat(string format, object arg0, object arg1)
            {
                throw new NotImplementedException();
            }

            public void FatalFormat(string format, object arg0, object arg1, object arg2)
            {
                throw new NotImplementedException();
            }

            public void FatalFormat(IFormatProvider provider, string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void Info(object message)
            {
                System.Console.WriteLine(message.ToString());
            }

            public void Info(object message, Exception exception)
            {
                System.Console.WriteLine(message.ToString());
            }

            public void InfoFormat(string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void InfoFormat(string format, object arg0)
            {
                throw new NotImplementedException();
            }

            public void InfoFormat(string format, object arg0, object arg1)
            {
                throw new NotImplementedException();
            }

            public void InfoFormat(string format, object arg0, object arg1, object arg2)
            {
                throw new NotImplementedException();
            }

            public void InfoFormat(IFormatProvider provider, string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void Warn(object message)
            {
                System.Console.WriteLine(message.ToString());
            }

            public void Warn(object message, Exception exception)
            {
                System.Console.WriteLine(message.ToString());
            }

            public void WarnFormat(string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void WarnFormat(string format, object arg0)
            {
                throw new NotImplementedException();
            }

            public void WarnFormat(string format, object arg0, object arg1)
            {
                throw new NotImplementedException();
            }

            public void WarnFormat(string format, object arg0, object arg1, object arg2)
            {
                throw new NotImplementedException();
            }

            public void WarnFormat(IFormatProvider provider, string format, params object[] args)
            {
                throw new NotImplementedException();
            }
        }
    }
}
