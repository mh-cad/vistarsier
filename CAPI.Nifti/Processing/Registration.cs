using System;
using System.Diagnostics;

namespace CAPI.NiftiLib.Processing
{
    public static class Registration
    {
        /// <summary>
        /// Uses the CMTK registration and reformatx tools to register and reslice the floating nifti to match the reference nifti.
        /// </summary>
        /// <param name="floating">Nifti to be registered</param>
        /// <param name="reference">Reference nifti</param>
        /// <param name="updates">Event handler for updates from the toolchain...</param>
        /// <returns></returns>
        public static INifti CMTKRegistration(INifti floating, INifti reference, DataReceivedEventHandler updates = null)
        {
            // Setup our temp file names.
            string niftiInPath = Tools.TEMPDIR + floating.GetHashCode() + ".cmtkrego.in.nii";
            string niftiRefPath = Tools.TEMPDIR + floating.GetHashCode() + ".cmtkrego.ref.nii";
            string niftiOutPath = Tools.TEMPDIR + floating.GetHashCode() + ".cmtkrego.out.nii";
            string regOutPath = Tools.TEMPDIR + floating.GetHashCode() + ".cmtkrego.reg";

           // Directory.CreateDirectory(regOutPath);

            // Write nifti to temp directory.
            floating.WriteNifti(niftiInPath);
            reference.WriteNifti(niftiRefPath);

            Environment.SetEnvironmentVariable("CMTK_WRITE_UNCOMPRESSED", "1");

            var args = $"-o {regOutPath} {niftiRefPath} {niftiInPath}";
            Tools.ExecProcess("ThirdPartyTools/CMTK/registration.exe", args, updates);

            args = $"-o {niftiOutPath} --floating {niftiInPath} {niftiRefPath} {regOutPath}";
            Tools.ExecProcess("ThirdPartyTools/CMTK/reformatx.exe", args, updates);

            //INifti output = floating.DeepCopy();
            floating.ReadNifti(niftiOutPath);

            return floating;
        }

       
        /// <summary>
        /// Uses the CMTK registration and reformatx tools to register and reslice the floating nifti file to match the reference nifti file.
        /// </summary>
        /// <param name="floatingFile">The file path for the floating nifti file.</param>
        /// <param name="referenceFile">The file path for the reference nifti file.</param>
        /// <param name="updates">Event handler for updates from the tools.</param>
        /// <returns></returns>
        public static string CMTKRegistration(string floatingFile, string referenceFile, DataReceivedEventHandler updates = null)
        {
            string niftiInPath = floatingFile;
            string niftiRefPath = referenceFile;
            string niftiOutPath = floatingFile + ".cmtkrego.out.nii";
            string regOutPath = "reg";

            Environment.SetEnvironmentVariable("CMTK_WRITE_UNCOMPRESSED", "1");

            var args = $"-o {regOutPath} {niftiRefPath} {niftiInPath}";
            Tools.ExecProcess("ThirdPartyTools/CMTK/registration.exe", args, updates);

            args = $"-o {niftiOutPath} --floating {niftiInPath} {niftiRefPath} {regOutPath}";
            Tools.ExecProcess("ThirdPartyTools/CMTK/reformatx.exe", args, updates);

            return niftiOutPath;
        }

        public static string CMTKResliceUsingPrevious(string floatingFile, string niftiRefPath, DataReceivedEventHandler updates = null)
        {
            Environment.SetEnvironmentVariable("CMTK_WRITE_UNCOMPRESSED", "1");
            string niftiOutPath = floatingFile + ".cmtkrego.out.nii";

            string regOutPath = "reg";

            var args = $"-o {niftiOutPath} --floating {floatingFile} {niftiRefPath} {regOutPath}";
            Tools.ExecProcess("ThirdPartyTools/CMTK/reformatx.exe", args, updates);

            return niftiOutPath;
        }

    }
}
