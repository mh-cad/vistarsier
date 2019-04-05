using CAPI.ImageProcessing.Abstraction;
using System.Diagnostics;

namespace CAPI.ImageProcessing
{
    /// <summary>
    /// Helper tools for the Image processing library.
    /// </summary>
    public class Tools
    {
        public const string TEMPDIR = "temp";

        /// <summary>
        /// Execyte a processs
        /// </summary>
        /// <param name="filename">The file to be executed</param>
        /// <param name="args">Arguments to be passed on execution</param>
        /// <param name="updates">Event handler to handle updates.</param>
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

        /// <summary>
        /// Converts the given Dicom path to a nifti file which is loaded and returned as a nifti object.
        /// </summary>
        /// <param name="dicomPath">Path to the DICOM</param>
        /// <param name="updates">Event handler to handle updates.</param>
        /// <returns></returns>
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
