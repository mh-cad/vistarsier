using CAPI.ImageProcessing.Abstraction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPI.ImageProcessing
{
    public class BiasCorrection
    { 
        //public static INifti BrainSuiteBFC(INifti input, string args, DataReceivedEventHandler updates = null)
        //{

        //    // Setup our temp file names.
        //    string niftiInPath = TEMPDIR + input.GetHashCode() + ".bsbfc.in.nii";
        //    string niftiOutPath = TEMPDIR + input.GetHashCode() + ".bsbfc.out.nii";
        //    // Write nifti to temp directory.
        //    input.WriteNifti(niftiInPath);

        //    args += $"-i {niftiInPath} -o {niftiOutPath}";

        //    Tools.ExecProcess("../../../ThirdPartyTools/bfc.exe", args, updates);

        //    INifti output = new Nifti();
        //    output.ReadNifti(niftiOutPath);

        //    return output;
        //}

        //public static INifti BrainSuiteBFC(INifti input, INifti mask, string args, DataReceivedEventHandler updates = null)
        //{
        //    return null;
        //}

        public static INifti AntsN4(INifti input, DataReceivedEventHandler updates = null)
        {
            // Setup our temp file names.
            string niftiInPath = Tools.TEMPDIR + input.GetHashCode() + ".antsN4.in.nii";
            string niftiOutPath = Tools.TEMPDIR + input.GetHashCode() + ".antsN4.out.nii";
            // Write nifti to temp directory.
            input.WriteNifti(niftiInPath);

            var args = $"-i {niftiInPath} -o {niftiOutPath}";

            Tools.ExecProcess("../../../ThirdPartyTools/ants/N4BiasFieldCorrection.exe", args, updates);

            //INifti output = input.DeepCopy();
            input.ReadNifti(niftiOutPath);
            input.voxels = input.voxels;

            return input;
        }

        public static string AntsN4(string inputFile, DataReceivedEventHandler updates = null)
        {
            string niftiInPath = inputFile;
            string niftiOutPath = Tools.TEMPDIR + inputFile + ".antsN4.out.nii";
            var args = $"-i {niftiInPath} -o {niftiOutPath}";

            Tools.ExecProcess("../../../ThirdPartyTools/ants/N4BiasFieldCorrection.exe", args, updates);
            return niftiOutPath;
        }
    }
}
