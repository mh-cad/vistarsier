using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace VisTarsier.Common
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public static class ProcessBuilder
    {
        private static Process Build(string processPath, string processFileNameExt, string arguments, string workingDir)
        {
            if (string.IsNullOrEmpty(workingDir)) workingDir = processPath;
            var proc = new Process
            {
                StartInfo =
                    new ProcessStartInfo
                    {
                        WorkingDirectory = workingDir,
                        FileName = Path.GetFullPath(Path.Combine(processPath, processFileNameExt)),
                        CreateNoWindow = false,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        Arguments = arguments
                    }
            };
            return proc;
        }

        public static Process CallExecutableFile(string fileFullPath, string arguments, string workingDir = "",
                                          DataReceivedEventHandler outputDataReceived = null,
                                          DataReceivedEventHandler errorOccuredInProcess = null)
        {
            fileFullPath = Path.GetFullPath(fileFullPath);

            if (!File.Exists(fileFullPath))
                throw new FileNotFoundException($"Executable file not found at location [{fileFullPath}]");

            
            var fileNameExt = Path.GetFileName(fileFullPath);//fileFullPath.Split('\\','/').LastOrDefault();
            var folderPath = Path.GetDirectoryName(fileFullPath); // fileFullPath.Replace($"\\{fileNameExt}", "");

            Log.GetLogger().Debug("Running: " + fileFullPath);
            Log.GetLogger().Debug("Args: " + arguments);

            var process = Build(folderPath, fileNameExt, arguments, workingDir);

            RunProcess(process, outputDataReceived, errorOccuredInProcess);

            return process;
        }

        private static void RunProcess(Process process,
                               DataReceivedEventHandler outputDataReceived = null,
                               DataReceivedEventHandler errorOccuredInProcess = null)
        {
            process.Start();

            if (outputDataReceived != null)
                process.OutputDataReceived += outputDataReceived;
            process.BeginOutputReadLine();

            if (errorOccuredInProcess != null)
                process.ErrorDataReceived += errorOccuredInProcess;
            process.BeginErrorReadLine();

            process.WaitForExit();
        }

        public static void OutputDataReceivedInProcess(object sender, DataReceivedEventArgs e)
        {
            var consoleColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (!string.IsNullOrEmpty(e.Data) && !string.IsNullOrWhiteSpace(e.Data))
                    Log.GetLogger().Info($"Process stdout:{Environment.NewLine}{e.Data}");
            }
            finally
            {
                Console.ForegroundColor = consoleColor;
            }
        }
        public static void ErrorOccuredInProcess(object sender, DataReceivedEventArgs e)
        {
            // Swallow log4j initialisation warnings
            if (e?.Data == null || string.IsNullOrEmpty(e.Data) || string.IsNullOrWhiteSpace(e.Data) || e.Data.ToLower().Contains("log4j")) return;

            var consoleColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                if (!string.IsNullOrEmpty(e.Data) && !string.IsNullOrWhiteSpace(e.Data))
                    Log.GetLogger().Error($"Process error:{Environment.NewLine}{e.Data}");
            }
            finally
            {
                Console.ForegroundColor = consoleColor;
            }
        }
    }
}