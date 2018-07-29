using CAPI.Common.Config;
using CAPI.Common.Services;
using CAPI.ImageProcessing.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CAPI.ImageProcessing
{
    public class ImageConverter : IImageConverter
    {
        private readonly string _dicom2NiiFullPath;
        private readonly string _miconvFullPath;
        private readonly string _viewableDir;
        private readonly string _dcm2NiiHdrParams;
        private readonly string _dcm2NiiNiiParams;
        private readonly string _javaClassPath;

        public ImageConverter()
        {
            var dcm2NiiExe = ImgProcConfig.GetDcm2NiiExeFilePath();
            _dcm2NiiHdrParams = "";//ImgProcConfig.GetDcm2NiiHdrParams();
            _dcm2NiiNiiParams = "";//ImgProcConfig.GetDcm2NiiNiiParams();
            var miconvFileName = "";//ImgProcConfig.GetMiconvFileName();

            var javaUtilsPath = ImgProc.GetJavaUtilsPath();
            _javaClassPath = $".;{javaUtilsPath}/PreprocessJavaUtils.jar;{javaUtilsPath}/lib/NICTA.jar;" +
                             $"{javaUtilsPath}/lib/vecmath.jar;{javaUtilsPath}/lib/ij.jar";

            var executablesPath = ImgProc.GetExecutablesPath();
            _dicom2NiiFullPath = Path.Combine(executablesPath, dcm2NiiExe);
            _miconvFullPath = Path.Combine(executablesPath, "odin", miconvFileName); // TODO3: Hard-coded path
            var imageRepoDir = ImgProc.GetImageRepositoryPath();
            _viewableDir = $"{imageRepoDir}\\Viewable"; // TODO3: Hard-coded path
        }

        public static void DicomToNiix(string dicomDir, string outfile, string @params = "")
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

        #region "Unused Methods"

        public void Dicom2Hdr(string dicomDir, string outputDir, string outputFileNameNoExt)
        {
            CallDicomToNii(dicomDir, outputDir, outputFileNameNoExt, _dcm2NiiHdrParams);
        }

        public void DicomToNii(string dicomDir, string outputDir, string outputFileNameNoExt)
        {
            CallDicomToNii(dicomDir, outputDir, outputFileNameNoExt, _dcm2NiiNiiParams);
        }

        private void CallDicomToNii(
            string dicomDir, string outputDir,
            string outputFileNameNoExt, string parameters)
        {
            // Make sure dicom files exist in the folder
            if (Directory.GetFiles(dicomDir).Length == 0)
                throw new Exception($"There is no files in following path: {dicomDir}");

            // Make sure temp folder exists
            var tmpDir = $"{outputDir}\\tmpDir";
            if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            Directory.CreateDirectory(tmpDir);

            ProcessBuilder.CallExecutableFile(
                $@"{_dicom2NiiFullPath}", $"{parameters} -o {tmpDir} {dicomDir}");

            // if dcm2nii reorients files, it first builds output file(s) then reorients into file(s) starting with 'o'
            // This will check if dcm2nii has tried reorienting the file and then skips file(s) not starting with 'o'
            var reorient = parameters.ToLower().Contains("-r y");

            // Copy files into output folder with desirable filenames
            var outputFiles = Directory.GetFiles(tmpDir).ToList();
            outputFiles.ForEach(f =>
            {
                if (f == null) return;
                // Refer to comment above reorient declaration
                if (reorient && !Path.GetFileName(f).ToLower().StartsWith("o")) return;
                var extension = Path.GetExtension(f);

                File.Copy(f, $@"{outputDir}\\{outputFileNameNoExt}{extension}");
            });

            // Remove temp folder
            if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
        }

        public IEnumerable<string> ConvertDicom2Viewable(
            string dicomDir, string outputDir = "", string outFileFormat = "png")
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
            return Directory.GetFiles(seriesDirPath).Select(f => f.Replace($@"{_viewableDir}\", "")).ToArray();
        }

        //private void ConvertNii2Dicom(string reslicedFloatingName, string outputDir)
        //{
        //var arguments = $@"{outputDir}\{reslicedFloatingName}{FlippedSuffix}.nii {outputDir}\{reslicedFloatingName}{FlippedSuffix}.dcm";
        //ProcessBuilder.CallExecutableFile($"{_executablesPath}\\odin\\{MiconvFileName}", arguments);
        //}

        public void Hdr2Nii(string fromHdrFileFullPath,
            string intoHdrFileFullPath, out string niiFileFullPath)
        {
            niiFileFullPath = intoHdrFileFullPath.Replace(".hdr", ".nii");

            //var arguments = $"-rf analyze -wf analyze {intoHdrFileFullPath} {niiFileFullPath}";
            //ProcessBuilder.CallExecutableFile(_miconvFullPath, arguments);

            const string methodName = "au.com.nicta.preprocess.main.CopyNiftiImage2PatientTransform";
            var arguments = $"-classpath \"{_javaClassPath}\" {methodName} " +
                $"\"{fromHdrFileFullPath}\" \"{intoHdrFileFullPath}\" \"{niiFileFullPath}\"";

            ProcessBuilder.CallJava(arguments, methodName);
        }

        #endregion
    }
}