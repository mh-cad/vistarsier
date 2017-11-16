using System.Collections.Generic;
using System.IO;
using System.Linq;
using CAPI.Common;
using CAPI.Common.Services;

namespace CAPI.ImageProcessing
{
    public class ImageConverter
    {
        private readonly string _dicom2NiiFullPath;
        private readonly string _miconvFullPath;
        private readonly string _imageRepoDir;
        private readonly string _viewableDir;
        private const string Dcm2NiiExe = "dcm2nii.exe"; // TODO3: Hard-coded file name
        private const string Dcm2NiiHdrParams = "-n N -f Y -r N"; // TODO3: Hard-coded Parameters
        private const string Dcm2NiiNiiParams = "-n Y -f Y -r N -g N"; // TODO3: Hard-coded Parameters
        private const string MiconvFileName = "miconv.exe"; // TODO3: Hard-coded file name

        public ImageConverter()
        {
            var executablesPath = Config.GetExecutablesPath();
            _dicom2NiiFullPath = Path.Combine(executablesPath, Dcm2NiiExe);
            _miconvFullPath = Path.Combine(executablesPath, "odin", MiconvFileName); // TODO3: Hard-coded path
            _imageRepoDir = Config.GetImageRepositoryPath();
            _viewableDir = $"{_imageRepoDir}\\Viewable"; // TODO3: Hard-coded path
        }

        public void ConvertDicom2Hdr(string dicomDir, string outputDir, string outputFileNameNoExt)
        {
            CallDicomToNii(dicomDir, outputDir, outputFileNameNoExt, Dcm2NiiHdrParams);
        }

        public void ConvertDicomToNii(string dicomDir, string outputDir, string outputFileNameNoExt)
        {
            CallDicomToNii(dicomDir, outputDir, outputFileNameNoExt, Dcm2NiiNiiParams);
        }

        private void CallDicomToNii(string dicomDir, string outputDir, string outputFileNameNoExt, string parameters)
        {
            // Make sure temp folder exists
            var tmpDir = $"{outputDir}\\tmpDir";
            if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            Directory.CreateDirectory(tmpDir);

            ProcessBuilder.CallExecutableFile($@"{_dicom2NiiFullPath}", $"{parameters} -o {tmpDir} {dicomDir}");

            // Copy files into output folder with desirable filenames
            var outputFiles = Directory.GetFiles(tmpDir).ToList();
            outputFiles.ForEach(f =>
            {
                var extension = Path.GetExtension(f);
                File.Copy(f, $@"{outputDir}\\{outputFileNameNoExt}{extension}");
            });

            // Remove temp folder
            if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
        }

        public IEnumerable<string> ConvertDicom2Viewable(string dicomDir, string outputDir = "", string outFileFormat = "png")
        {
            if (string.IsNullOrEmpty(outputDir)) outputDir = _viewableDir;
            var dicomFilesFullPaths = Directory.GetFiles(dicomDir);
            var seriesDirPath = $@"{outputDir}\{Path.GetFileName(dicomDir)}";
            if (!Directory.Exists(seriesDirPath)) Directory.CreateDirectory(seriesDirPath);
            foreach (var dicomFileFullPath in dicomFilesFullPaths)
            {
                var fileNameNoExt = Path.GetFileNameWithoutExtension(dicomFileFullPath); // Sometimes dicom files have no extension
                var arguments = $@"{dicomFileFullPath} {seriesDirPath}\{fileNameNoExt}.{outFileFormat}";
                ProcessBuilder.CallExecutableFile(_miconvFullPath, arguments);
            }
            return Directory.GetFiles(seriesDirPath).Select(f => f.Replace($@"{_viewableDir}\","")).ToArray();
        }

        //private void ConvertNii2Dicom(string reslicedFloatingName, string outputDir)
        //{
            //var arguments = $@"{outputDir}\{reslicedFloatingName}{FlippedSuffix}.nii {outputDir}\{reslicedFloatingName}{FlippedSuffix}.dcm";
            //ProcessBuilder.CallExecutableFile($"{_executablesPath}\\odin\\{MiconvFileName}", arguments);
        //}
    }
}