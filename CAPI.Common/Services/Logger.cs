using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CAPI.Common.Services
{
    public static class Logger
    {
        private static void Write(string logContent, LogType logType = LogType.Info, string processName = "")
        {
            var processesLogPath = Config.GetProcessesLogPath();
            if (!Directory.Exists(processesLogPath)) Directory.CreateDirectory(processesLogPath);
            var logPath = $@"{processesLogPath}\\{DateTime.Now:yyyyMMdd}";
            if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
            
            var sb = new StringBuilder();
            var newLine = logType == LogType.StdOut ? Environment.NewLine : " ";
            if (logType == LogType.StdOut) sb.AppendLine("----------------------------");
            sb.AppendLine($"{DateTime.Now:yyyy-MM-dd_HH:mm:ss} [{logType}]{newLine}{logContent}");
            File.AppendAllText($"{logPath}\\{DateTime.Now:yyyyMMddHHmmss}[{logType}]_{processName}.txt", sb.ToString());
        }

        public static void ProcessErrorLogWrite(Process proc, string processName)
        {
            while (!proc.StandardError.EndOfStream)
            {
                var stderr = proc.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(stderr)) Write (processName + " ERROR: " + stderr, LogType.Error);
            }
        }

        public static void ProcessStdOutLogWrite(Process proc, string processName)
        {
            while (!proc.StandardOutput.EndOfStream)
            {
                var stdout = proc.StandardOutput.ReadToEnd();
                if (!string.IsNullOrEmpty(stdout))
                    Write($"{processName} Standard Output:{Environment.NewLine}{stdout}", LogType.StdOut, processName);
            }
        }

        public enum LogType
        {
            Info,
            Error,
            StdOut
        }
    }
}