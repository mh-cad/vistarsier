using VisTarsier.Common;
using VisTarsier.Config;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace VisTarsier.NiftiLib.Processing
{
    /// <summary>
    /// Helper tools for the Image processing library.
    /// </summary>
    public static class Tools
    {
        public const string TEMPDIR = "temp";

        /// <summary>
        /// Converts the given Dicom path to a nifti file which is loaded and returned as a nifti object.
        /// </summary>
        /// <param name="dicomPath">Path to the DICOM</param>
        /// <param name="updates">Event handler to handle updates.</param>
        /// <returns></returns>
        public static string Dcm2Nii(string dicomPath, string name, DataReceivedEventHandler updates = null)
        {
            if (!FileSystem.DirectoryIsValidAndNotEmpty(dicomPath))
            {
                Log.GetLogger().Error("Directory " + dicomPath + " does not seem to exist, or is empty.");
                return null;
            }

            var niftiPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(dicomPath), name));

            var tmpDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(niftiPath), "tmp"));
            if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            FileSystem.DirectoryExistsIfNotCreate(tmpDir);


            var args = $" -o \"{tmpDir}\" \"{dicomPath}\"";

            ProcessBuilder.CallExecutableFile(CapiConfig.GetConfig().Binaries.dcm2niix, args, outputDataReceived: updates);

            if (!Directory.Exists(tmpDir))
                throw new DirectoryNotFoundException("dcm2niix output folder does not exist!");
            var outFiles = Directory.GetFiles(tmpDir);
            // Rather than cracking a tanty when we have more than one nii in the stack, we're just going to use the biggest one.
            // This is in case we have a reference slide at the front or end of the dicom stack, which can happen.
            var nims = outFiles.Where(f => Path.GetExtension(f) == ".nii").OrderByDescending(f => new FileInfo(f)?.Length);
            var nim = nims.FirstOrDefault();

            if(nim == null)
            {
                if (!File.Exists(CapiConfig.GetConfig().Binaries.dcm2niix))
                {
                    Log.GetLogger().Error("Could not find dcm2niix at: " + CapiConfig.GetConfig().Binaries.dcm2niix);
                }

                var log = Log.GetLogger();
                log.Error("Could not find valid output for dcm2niix. Files output were: ");

                foreach (var of in outFiles)
                {
                    log.Error($"[{of}]");        
                }

                throw new FileNotFoundException("Could not find output of dcm2niix");
            }

            if (File.Exists(niftiPath)) File.Delete(niftiPath);
            File.Move(nim, niftiPath);

            Directory.Delete(tmpDir, true);

            return niftiPath;
        }


    }
}
