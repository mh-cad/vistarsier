using CAPI.ImageProcessing.Abstraction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPI.ImageProcessing
{
    public class Tools
    {
        public const string TEMPDIR = "./temp";

        public static void ExecProcess(string filename, string args, DataReceivedEventHandler updates = null)
        { 
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.OutputDataReceived += updates;

            process.WaitForExit();
        }
    }
}
