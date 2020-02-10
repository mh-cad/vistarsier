using VisTarsier.Common;
using VisTarsier.Config;
using System;
using System.Diagnostics;
using System.IO;

namespace VisTarsier.NiftiLib.Processing
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
        public static INifti<float> CMTKRegistration(INifti<float> floating, INifti<float> reference, DataReceivedEventHandler updates = null)
        {
            // Setup our temp file names.
            string niftiInPath = Tools.TEMPDIR + floating.GetHashCode() + ".cmtkrego.in.nii";
            string niftiRefPath = Tools.TEMPDIR + floating.GetHashCode() + ".cmtkrego.ref.nii";
            string niftiOutPath = Tools.TEMPDIR + floating.GetHashCode() + ".cmtkrego.out.nii";

            // Write nifti to temp directory.
            floating.WriteNifti(niftiInPath);
            reference.WriteNifti(niftiRefPath);

            niftiOutPath = CMTKRegistration(niftiInPath, niftiRefPath, updates);

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

            var args = $"-o \"{regOutPath}\" \"{niftiRefPath}\" \"{niftiInPath}\"";
            ProcessBuilder.CallExecutableFile(CapiConfig.GetConfig().Binaries.registration, args, outputDataReceived: updates);

            args = $"-o \"{niftiOutPath}\" --floating \"{niftiInPath}\" \"{niftiRefPath}\" \"{regOutPath}\"";
            ProcessBuilder.CallExecutableFile(CapiConfig.GetConfig().Binaries.reformatx, args, outputDataReceived: updates);

            return niftiOutPath;
        }

        public static string CMTKResliceUsingPrevious(string floatingFile, string niftiRefPath, DataReceivedEventHandler updates = null)
        {
            Environment.SetEnvironmentVariable("CMTK_WRITE_UNCOMPRESSED", "1");
            string niftiOutPath = floatingFile + ".cmtkrego.out.nii";

            string regOutPath = "reg";

            var args = $"-o \"{niftiOutPath}\" --floating \"{floatingFile}\" \"{niftiRefPath}\" \"{regOutPath}\"";
            ProcessBuilder.CallExecutableFile(CapiConfig.GetConfig().Binaries.reformatx, args, outputDataReceived: updates);

            return niftiOutPath;
        }

        public static string ANTSRegistration(string floatingFile, string fixedFile, DataReceivedEventHandler updates = null)
        {
            string niftiOutPath = floatingFile + "warped.nii";

            // ANTS didn't like me splitting args into a nicely tabbed string so it's all one big line.
            // --dimensionality 3 :: we have a 3-d image
            // --float 1 :: we want to keep things floating point
            // --interpolation Linear :: using linear interpolation for the warped image
            // --use-histogram-matching 0 :: histogram matching is pre-normalisation (we're avoiding this in case we have hyper-intense fat on one and not the other)
            // --initial-moving-transform [{fixedFile},{floatingFile}, 1] :: inputs
            // --transform Rigid[0.1] :: using an rigid transform (6 degrees of freedom, linear)
            // --metric MI[{fixedFile},{floatingFile},1,32,Regular,0.25] :: metric is mutual information, although we could use cross correlation so long as we're registering the same sequence type
            // --convergence [1000x500x250x100,1e-6,10] :: convergence is the iterations at each level and the threashold for imporovement in the metric
            // --shrink-factors 8x4x2x1 :: shrink factors control the resolution at each level
            // --smoothing-sigmas 3x2x1x0vox :: not really sure what smoothing sigmas do but the values should be fine
            // --output [_, {niftiOutPath}] :: output the transform value + our sweet nifti file.
            var args = $" --dimensionality 3 --float 1 --interpolation Linear --use-histogram-matching 0 --initial-moving-transform [\"{fixedFile}\",\"{floatingFile}\", 1] --transform Affine[0.1] --metric MI[\"{fixedFile}\",\"{floatingFile}\",1,32,Regular,0.25] --convergence [1000x500x250x100,1e-6,10] --shrink-factors 8x4x2x1 --smoothing-sigmas 3x2x1x0vox --output [\"{niftiOutPath}\", \"{niftiOutPath}\"]";

            ProcessBuilder.CallExecutableFile(CapiConfig.GetConfig().Binaries.antsRegistration, args, outputDataReceived: updates);

            return niftiOutPath;
        }

        public static INifti<float> ANTSRegistration(INifti<float> floating, INifti<float> reference, DataReceivedEventHandler updates = null)
        {
            // Setup our temp file names.
            string niftiInPath = Path.GetFullPath(Tools.TEMPDIR + floating.GetHashCode() + ".antsrego.in.nii");
            string niftiRefPath = Path.GetFullPath(Tools.TEMPDIR + floating.GetHashCode() + ".antsrego.ref.nii");

            floating.WriteNifti(niftiInPath);
            reference.WriteNifti(niftiRefPath);

            string niftiOutPath = Path.GetFullPath(ANTSRegistration(niftiInPath, niftiRefPath, updates));

            var output = floating.DeepCopy();
            output = output.ReadNifti(niftiOutPath);

            return output;
        }

        public static string ANTSApplyTransforms(string floatingFile, string referenceFile, DataReceivedEventHandler updates = null)
        {
            floatingFile = Path.GetFullPath(floatingFile);
            referenceFile = Path.GetFullPath(referenceFile);

            string niftiOutPath = Path.Combine("warped.nii");

            string args = $"-d 3 --float 1 -i \"{floatingFile}\" -r \"{referenceFile}\" -o \"{niftiOutPath}\" -n Linear -t \"{niftiOutPath}0GenericAffine.mat\"";

            ProcessBuilder.CallExecutableFile(CapiConfig.GetConfig().Binaries.antsApplyTransforms, args, outputDataReceived: updates);

            return niftiOutPath;
        }

    }
}
