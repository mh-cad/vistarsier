using CAPI.General.Abstractions.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CAPI.General.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ProcessBuilder : IProcessBuilder
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
                        FileName = Path.Combine(processPath, processFileNameExt),
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

        public Process CallExecutableFile(string fileFullPath, string arguments, string workingDir = "", DataReceivedEventHandler outputDataReceived = null,
            DataReceivedEventHandler errorOccuredInProcess = null)
        {
            if (!File.Exists(fileFullPath))
                throw new FileNotFoundException($"Executable file not found at location [{fileFullPath}]");

            var fileNameExt = fileFullPath.Split('\\').LastOrDefault();
            var folderPath = fileFullPath.Replace($"\\{fileNameExt}", "");

            var process = Build(folderPath, fileNameExt, arguments, workingDir);

            RunProcess(process, outputDataReceived, errorOccuredInProcess);

            return process;
        }

        public Process CallJava(string javaFullPath, string arguments, string methodCalled, string workingDir = "", DataReceivedEventHandler outputDataReceived = null,
            DataReceivedEventHandler errorOccuredInProcess = null)
        {
            if (!File.Exists(javaFullPath))
                throw new FileNotFoundException($"Java.exe file not found at location [{javaFullPath}]");
            if (string.IsNullOrEmpty(arguments))
                throw new ArgumentNullException(nameof(arguments), "No arguments are passed for java process");

            var javaFileNamExt = javaFullPath.Split('\\').LastOrDefault();
            var javaFolderPath = javaFullPath.Replace($"\\{javaFileNamExt}", "");

            var process = Build(javaFolderPath, javaFileNamExt, arguments, workingDir);

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
    }
}