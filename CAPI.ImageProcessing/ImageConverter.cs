using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CAPI.ImageProcessing
{
    public class ImageConverter : IImageConverter
    {
        private readonly IFileSystem _filesystem;
        private readonly IProcessBuilder _processBuilder;
        private readonly Common.Abstractions.Config.IImgProcConfig _config;
        private readonly ILog _log;

        public ImageConverter(
            IFileSystem filesystem, IProcessBuilder processBuilder,
            Common.Abstractions.Config.IImgProcConfig config, ILog log)
        {
            _filesystem = filesystem;
            _processBuilder = processBuilder;
            _config = config;
            _log = log;
        }

        public void DicomToNiix(string dicomDir, string outfile, string @params = "")
        {
            var dcm2NiiExe = Path.Combine(_config.ImgProcBinFolderPath, _config.Dcm2NiiExeRelFilePath);

            if (!File.Exists(dcm2NiiExe))
                throw new FileNotFoundException($"Unable to find {nameof(dcm2NiiExe)} file: [{dcm2NiiExe}]");

            var tmpDir = $@"{Path.GetDirectoryName(outfile)}\tmp";
            if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            _filesystem.DirectoryExistsIfNotCreate(tmpDir);

            var process = _processBuilder.CallExecutableFile(dcm2NiiExe, $"{@params} -o {tmpDir} {dicomDir}");
            process.OutputDataReceived += OutputDataReceivedInProcess;
            process.ErrorDataReceived += ErrorOccuredInProcess;
            process.WaitForExit();

            if (!Directory.Exists(tmpDir))
                throw new DirectoryNotFoundException("dcm2niix output folder does not exist!");
            var outFiles = Directory.GetFiles(tmpDir);
            var nim = outFiles.Single(f => Path.GetExtension(f) == ".nii");
            File.Move(nim, outfile);

            Directory.Delete(tmpDir, true);
        }

        private void OutputDataReceivedInProcess(object sender, DataReceivedEventArgs e)
        {
            var consoleColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            if (!string.IsNullOrEmpty(e.Data) && !string.IsNullOrWhiteSpace(e.Data))
                _log.Info($"Process stdout:{Environment.NewLine}{e.Data}");

            Console.ForegroundColor = consoleColor;
        }
        private void ErrorOccuredInProcess(object sender, DataReceivedEventArgs e)
        {
            _log.Error(e.Data);

            //throw new Exception("Error occured while running a third-party process!");
        }
    }
}