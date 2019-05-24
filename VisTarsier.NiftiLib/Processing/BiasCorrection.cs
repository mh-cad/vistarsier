using VisTarsier.Common;
using VisTarsier.Config;
using System.Diagnostics;

namespace VisTarsier.NiftiLib.Processing
{
    public class BiasCorrection
    {
        /// <summary>
        /// Uses the ANTS implementation of the N4 bias correction algorithm.
        /// </summary>
        /// <param name="input">The input nifti to be corrected</param>
        /// <param name="updates">Event handler for updates from the process</param>
        /// <returns>New, corrected nifti</returns>
        public static INifti<float> AntsN4(INifti<float> input, DataReceivedEventHandler updates = null)
        {
            // Setup our temp file names.
            string niftiInPath = Tools.TEMPDIR + input.GetHashCode() + ".antsN4.in.nii";
            string niftiOutPath = Tools.TEMPDIR + input.GetHashCode() + ".antsN4.out.nii";
            // Write nifti to temp directory.
            input.WriteNifti(niftiInPath);

            var args = $"-i {niftiInPath} -o {niftiOutPath}";

            ProcessBuilder.CallExecutableFile(CapiConfig.GetConfig().Binaries.N4BiasFieldCorrection, args, outputDataReceived: updates);

            var output = input.DeepCopy();
            output.ReadNifti(niftiOutPath);
            output.RecalcHeaderMinMax();

            return output;
        }

        /// <summary>
        /// Uses the ANTS implementation of the N4 bias correction algorithm to correct the given file.
        /// </summary>
        /// <param name="inputFile">Path to input nifti file.</param>
        /// <param name="updates">Event handler for updates from the process</param>
        /// <returns>Path for output nifti file.</returns>
        public static string AntsN4(string inputFile, DataReceivedEventHandler updates = null)
        {
            string niftiInPath = inputFile;
            string niftiOutPath = inputFile + ".antsN4.out.nii";
            var args = $"-i {niftiInPath} -o {niftiOutPath}";

            ProcessBuilder.CallExecutableFile(CapiConfig.GetConfig().Binaries.N4BiasFieldCorrection, args, outputDataReceived: updates);
            return niftiOutPath;
        }
    }
}
