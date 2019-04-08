using CAPI.ImageProcessing.Abstraction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPI.ImageProcessing
{
    public class BrainExtraction
    {
        /// <summary>
        /// Uses the BrainSuite BSE tool to extract the brain from a given INifti.
        /// </summary>
        /// <param name="input">Nifti which contains the brain to be extracted</param>
        /// <param name="updates">Data handler for updates from the BSE tool.</param>
        /// <returns>The INifti containing the extracted brain.</returns>
        public static INifti BrainSuiteBSE(INifti input, DataReceivedEventHandler updates = null)
        {
            // Setup our temp file names.
            string niftiInPath = Tools.TEMPDIR + input.GetHashCode() + ".bse.in.nii";
            string niftiOutPath = Tools.TEMPDIR + input.GetHashCode() + ".bse.out.nii";
            // Write nifti to temp directory.
            input.WriteNifti(niftiInPath);

            var args = $"-i {niftiInPath} -o {niftiOutPath}";

            Tools.ExecProcess("ThirdPartyTools/brain_suite/bse.exe", args, updates);

            INifti output = input.DeepCopy(); // Sometimes this messes with the header and gives us a 4-up???
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
            string niftiInPath = inputFile;
            string niftiOutPath = inputFile + ".bse.out.nii";

            var args = $"-i {niftiInPath} -o {niftiOutPath}";

            Tools.ExecProcess("ThirdPartyTools/brain_suite/bse.exe", args, updates);

            return niftiOutPath;
        }
    }
}
