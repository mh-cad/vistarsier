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
        public static INifti BrainSuiteBSE(INifti input, DataReceivedEventHandler updates = null)
        {
            // Setup our temp file names.
            string niftiInPath = Tools.TEMPDIR + input.GetHashCode() + ".bse.in.nii";
            string niftiOutPath = Tools.TEMPDIR + input.GetHashCode() + ".bse.out.nii";
            // Write nifti to temp directory.
            input.WriteNifti(niftiInPath);

            var args = $"-i {niftiInPath} -o {niftiOutPath}";

            Tools.ExecProcess("../../../ThirdPartyTools/brain_suite/bse.exe", args, updates);

           // INifti output = input.DeepCopy(); // Sometimes this messes with the header and gives us a 4-up???
            input.ReadNifti(niftiOutPath);

            return input;
        }
    }
}
