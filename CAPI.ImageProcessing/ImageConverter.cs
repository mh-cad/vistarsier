using CAPI.Common.Services;
using CAPI.ImageProcessing.Abstraction;
using System.IO;
using System.Linq;

namespace CAPI.ImageProcessing
{
    public class ImageConverter : IImageConverter
    {
        public void DicomToNiix(string dicomDir, string outfile, string @params = "")
        {
            var dcm2NiiExe = ImgProcConfig.GetDcm2NiiExeFilePath();

            var tmpDir = $@"{Path.GetDirectoryName(outfile)}\tmp";
            if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            FileSystem.DirectoryExistsIfNotCreate(tmpDir);

            ProcessBuilder.CallExecutableFile(dcm2NiiExe, $"{@params} -o {tmpDir} {dicomDir}");

            if (!Directory.Exists(tmpDir))
                throw new DirectoryNotFoundException("dcm2niix output folder does not exist!");
            var outfiles = Directory.GetFiles(tmpDir);
            var nim = outfiles.Single(f => Path.GetExtension(f) == ".nii");
            File.Move(nim, outfile);

            Directory.Delete(tmpDir, true);
        }
    }
}