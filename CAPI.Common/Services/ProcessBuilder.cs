using System.Diagnostics;
using System.Linq;

namespace CAPI.Common.Services
{
    public static class ProcessBuilder
    {
        public static Process Build(string processPath, string processFileNameExt, string arguments, string workingDir)
        {
            if (string.IsNullOrEmpty(workingDir)) workingDir = processPath;
            var proc = new Process
            {
                StartInfo =
                    new ProcessStartInfo
                    {
                        WorkingDirectory = workingDir,
                        FileName = $"{processPath}\\{processFileNameExt}",
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

        public static string CallExecutableFile(string fileFullPath, string arguments, string workingDir = "")
        {
            var fileNameExt = fileFullPath.Split('\\').LastOrDefault();
            var folderPath = fileFullPath.Replace($"\\{fileNameExt}", "");

            var process = Build(folderPath, fileNameExt, arguments, workingDir);

            process.Start();
            var stdout = process.StandardOutput.ReadToEnd();
            //Logger.Write(stdout, Logger.LogType.StdOut, fileNameExt);
            Logger.ProcessErrorLogWrite(process, $"{fileNameExt}");
            process.WaitForExit();
            return stdout;
        }

        public static 
            void CallJava(string arguments, string methodCalled, string workingDir = "")
        {
            var javaFullPath = Config.ImgProc.GetJavaExePath();
            var javaFileNamExt = javaFullPath.Split('\\').LastOrDefault();
            var javaFolderPath = javaFullPath.Replace($"\\{javaFileNamExt}","");

            var process = Build(javaFolderPath, javaFileNamExt, arguments, workingDir);

            process.Start();
            var stdout = process.StandardOutput.ReadToEnd();
            //Logger.Write(stdout, Logger.LogType.StdOut, methodCalled);
            Logger.ProcessErrorLogWrite(process, $"{javaFileNamExt}");
            process.WaitForExit();
        }
    }
}