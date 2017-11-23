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
        private readonly string _viewableDir;
        private readonly string _dcm2NiiHdrParams;
        private readonly string _dcm2NiiNiiParams;

        public ImageConverter()
        {
            var dcm2NiiExe = Properties.Settings.Default.Dcm2NiiExe;
            _dcm2NiiHdrParams = Properties.Settings.Default.Dcm2NiiHdrParams;
            _dcm2NiiNiiParams = Properties.Settings.Default.Dcm2NiiNiiParams;
            var miconvFileName = Properties.Settings.Default.MiconvFileName;

            var executablesPath = Config.GetExecutablesPath();
            _dicom2NiiFullPath = Path.Combine(executablesPath, dcm2NiiExe);
            _miconvFullPath = Path.Combine(executablesPath, "odin", miconvFileName); // TODO3: Hard-coded path
            var imageRepoDir = Config.GetImageRepositoryPath();
            _viewableDir = $"{imageRepoDir}\\Viewable"; // TODO3: Hard-coded path
        }

        public void Dicom2Hdr(string dicomDir, string outputDir, string outputFileNameNoExt)
        {
            CallDicomToNii(dicomDir, outputDir, outputFileNameNoExt, _dcm2NiiHdrParams);
        }

        public void DicomToNii(string dicomDir, string outputDir, string outputFileNameNoExt)
        {
            CallDicomToNii(dicomDir, outputDir, outputFileNameNoExt, _dcm2NiiNiiParams);
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

        
        public bool Hdr2Nii(string hdrFileFullPath, string outputDir, string niiFileNameNoExt)
        {

            return false;
        }
    }
}