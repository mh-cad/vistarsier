using CAPI.Common.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using System.IO;
using System.Linq;

namespace CAPI.ImageProcessing
{
    public class ImageConverter : IImageConverter
    {
        private readonly IFileSystem _filesystem;
        private readonly IProcessBuilder _processBuilder;

        public ImageConverter(IFileSystem filesystem, IProcessBuilder processBuilder)
        {
            _filesystem = filesystem;
            _processBuilder = processBuilder;
        }

        public void DicomToNiix(string dicomDir, string outfile, string @params = "")
        {
            var dcm2NiiExe = ImgProcConfig.GetDcm2NiiExeFilePath();

            var tmpDir = $@"{Path.GetDirectoryName(outfile)}\tmp";
            if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            _filesystem.DirectoryExistsIfNotCreate(tmpDir);

            _processBuilder.CallExecutableFile(dcm2NiiExe, $"{@params} -o {tmpDir} {dicomDir}");

            if (!Directory.Exists(tmpDir))
                throw new DirectoryNotFoundException("dcm2niix output folder does not exist!");
            var outfiles = Directory.GetFiles(tmpDir);
            var nim = outfiles.Single(f => Path.GetExtension(f) == ".nii");
            File.Move(nim, outfile);

            Directory.Delete(tmpDir, true);
        }
    }
}