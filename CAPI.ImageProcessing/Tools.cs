using CAPI.ImageProcessing.Abstraction;
using System.Diagnostics;

namespace CAPI.ImageProcessing
{
    public class Tools
    {
        public const string TEMPDIR = "temp";

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

            process.OutputDataReceived += updates;
            process.ErrorDataReceived += updates;
            process.Start();
            process.BeginOutputReadLine();
            

            process.WaitForExit();
        }

        public static INifti Dcm2Nii(string dicomPath, DataReceivedEventHandler updates = null)
        {
            var outFile = TEMPDIR + dicomPath.GetHashCode() + ".dcm2nii";
            var args = $@" -o {outFile} {dicomPath}";
            ExecProcess("../../../ThirdPartyTools/dcm2niix.exe", args, updates);
            INifti nifti = new Nifti();
            nifti.ReadNifti(outFile);

            return nifti;
        }
    }
}
