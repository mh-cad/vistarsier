using VisTarsier.Common;
using VisTarsier.Config;
using System.Diagnostics;
using System.IO;

namespace VisTarsier.NiftiLib.Processing
{
    public class BrainExtraction
    {
        /// <summary>
        /// Uses the BrainSuite BSE tool to extract the brain from a given INifti.
        /// </summary>
        /// <param name="input">Nifti which contains the brain to be extracted</param>
        /// <param name="updates">Data handler for updates from the BSE tool.</param>
        /// <returns>The INifti containing the extracted brain.</returns>
        public static INifti<float> BrainSuiteBSE(INifti<float> input, DataReceivedEventHandler updates = null)
        {
            // Setup our temp file names.
            string niftiInPath = Path.GetFullPath(Tools.TEMPDIR + input.GetHashCode() + ".bse.in.nii");
            string niftiOutPath = Path.GetFullPath(Tools.TEMPDIR + input.GetHashCode() + ".bse.out.nii");
            // Write nifti to temp directory.
            input.WriteNifti(niftiInPath);

            var args = $"--auto --trim -i \"{niftiInPath}\" -o \"{niftiOutPath}\"";

            ProcessBuilder.CallExecutableFile(CapiConfig.GetConfig().Binaries.bse, args, outputDataReceived: updates);

            var output = input.DeepCopy(); // Sometimes this messes with the header and gives us a 4-up???
            output.ReadNifti(niftiOutPath);

            return output;
        }

        /// <summary>
        /// Uses the BrainSuite BSE tool to extract the brain from the given Nifti file path.
        /// </summary>
        /// <param name="inputFile">Path to .nii file which needs a brain extractin'</param>
        /// <param name="updates">Event handler to accept progress updates from the tool.</param>
        /// <returns>The path of the output file.</returns>
        public static string BrainSuiteBSE(string inputFile, DataReceivedEventHandler updates = null)
        {
            string niftiInPath = Path.GetFullPath(inputFile);
            string niftiOutPath = Path.GetFullPath(inputFile + ".bse.out.nii");

            var args = $"--auto --trim -i \"{niftiInPath}\" -o \"{niftiOutPath}\"";

            ProcessBuilder.CallExecutableFile(CapiConfig.GetConfig().Binaries.bse, args, outputDataReceived: updates);


            return niftiOutPath;
        }
    }
}
