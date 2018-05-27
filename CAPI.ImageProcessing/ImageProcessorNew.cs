using CAPI.Common.Services;
using CAPI.ImageProcessing.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CAPI.ImageProcessing
{
    public class ImageProcessorNew //: IImageProcessorNew
    {
        private readonly IImageConverter _imageConverter;

        private readonly string _executablesPath;
        private readonly string _javaClassPath;
        private const string Fixed = "fixed";
        private const string Floating = "floating";
        private readonly string _fixedDicomPath;
        private readonly string _processesRootDir;

        private const string FlippedSuffix = "_flipped";
        //private const string Dcm2NiiExe = "dcm2nii.exe";
        //private const string Dcm2NiiHdrParams = "-n N -f Y -r Y";
        //private const string Dcm2NiiNiiParams = "-n Y -g N -f Y -r Y";
        //private const string BseExe = "bse09e.exe";
        //private const string BseExe = "bse.exe";
        //private const string BrainSurfaceSuffix = "_brain_surface";
        //private const string BrainSurfaceExtSuffix = "_brain_surface_extracted";
        //private const string RegistrationExeFileName = "registration.exe";
        //private const string ReformatXFileName = "reformatx.exe";
        //private const string CmtkParams = "--initxlate --dofs 6 --auto-multi-levels 4 --out-matrix";
        //private const string CmtkRawXformFileName = "cmtk_xform_mat.txt";
        //private const string CmtkResultXformFileName = "fixed_to_floating_rap_xform.txt";
        //private const string CmtkOutputDirName = "cmtk_xform";
        //private const string MiconvFileName = "miconv.exe";
        private const string DstPrefixPositive = "flair_new_with_changes_overlay_positive";
        private const string DstPrefixNegative = "flair_new_with_changes_overlay_negative";
        private const string StructChangesDarkFloat2BrightFixed = "structural_changes_dark_in_floating_to_bright_in_fixed";
        private const string StructChangesBrightFloat2DarkFixed = "structural_changes_bright_in_floating_to_dark_in_fixed";
        private const string StructChangesBrainMask = "structural_changes_brain_surface_mask";
        private const string DcmtkFolderName = "dcmtk-3.6.0-win32-i386";
        private const string Img2DcmFileName = "img2dcm.exe";
        private const string FlairOldReslicesFolderName = "flair_old_resliced";
        private const string DicomFilesWithNewHeadersFolder = "flair_old_resliced_new_header";
        private const string DcmdumpFileName = "dcmdump.exe";
        private const string DcmodifyFileName = "dcmodify.exe";

        private static Dictionary<string, string> GetFileNamesAndContentsToCreate()
        {
            return new Dictionary<string, string>
            {
                {"fixed.hdr.properties", "series_description=fixed stack hdr"},
                {"fixed.img.properties", "series_description=fixed stack img"},
                {"floating.hdr.properties", "series_description=floating stack hdr"},
                {"floating.img.properties", "series_description=floating stack img"},
                {"fixed_to_floating_rap_xform.txt.properties", "series_description=fixed to floating xform in RAP format"},
                {"structural_changes_dark_in_floating_to_bright_in_fixed.nii.properties", "series_description=growing lesions"},
                {"structural_changes_bright_in_floating_to_dark_in_fixed.nii.properties", "series_description=shrinking lesions"},
                {"structural_changes_brain_surface_mask.nii.properties", "series_description=brain surface"}
            };
        } // TODO3: Hard-coded name
        private static Dictionary<string, string> GetFilesToBeRenamed()
        {
            return new Dictionary<string, string>
            {
                {"diff_dark_in_floating_to_bright_in_fixed.nii", "structural_changes_dark_in_floating_to_bright_in_fixed.nii"},
                {"diff_bright_in_floating_to_dark_in_fixed.nii", "structural_changes_bright_in_floating_to_dark_in_fixed.nii"},
                {"diff_brain_surface_mask.nii", "structural_changes_brain_surface_mask.nii"}
            };
        } // TODO3: Hard-coded name

        public ImageProcessorNew()
        {
            _javaClassPath = ImgProcConfig.GetJavaClassPath();
        }

        public static void ExtractBrainMask(string infile, string @params, string brain, string mask)
        {
            var bseExe = ImgProcConfig.GetBseExeFilePath();
            var arguments = $"-i {infile} --mask {mask} -o {brain} {@params}";

            if (!Directory.Exists(Path.GetDirectoryName(brain))) throw new DirectoryNotFoundException();
            if (!Directory.Exists(Path.GetDirectoryName(mask))) throw new DirectoryNotFoundException();

            ProcessBuilder.CallExecutableFile(bseExe, arguments);

            if (!File.Exists(brain) || !File.Exists(mask))
                throw new FileNotFoundException("Brain mask removal failed to create brain/mask.");
        }

        public static void Registration(string fixedNii, string floatingNii, string floatingResliced)
        {
            var outputPath = Directory.GetParent(Path.GetDirectoryName(fixedNii)).FullName;

            CreateRawXform(outputPath, fixedNii, floatingNii);

            CreateResultXform(outputPath, fixedNii, floatingNii);

            ResliceFloatingImages(outputPath, fixedNii, floatingNii, floatingResliced);
        }

        //public static void Registration(string outputPath, string fixedFullPath, string floatingFullPath,
        //    string floatingReslicedFullPath)
        //{
        //    CreateRawXform(outputPath, fixedFullPath, floatingFullPath);

        //    CreateResultXform(outputPath, fixedFullPath, floatingFullPath);

        //    ResliceFloatingImages(outputPath, fixedFullPath, floatingFullPath, floatingReslicedFullPath);
        //}

        private static void CreateRawXform(string outputPath, string fixedNii, string floatingNii)
        {
            var registrationFile = ImgProcConfig.GetRegistrationFilePath();
            var registrationParams = ImgProcConfig.GetRegistrationParams();
            var cmtkOutputDir = $@"{outputPath}\{ImgProcConfig.GetCmtkFolderName()}";
            var rawForm = $@"{outputPath}\{ImgProcConfig.GetCmtkRawxformFile()}";

            if (Directory.Exists(cmtkOutputDir)) Directory.Delete(cmtkOutputDir);
            FileSystem.DirectoryExists(cmtkOutputDir);

            var arguments = $@"{registrationParams} --out-matrix {rawForm} -o . {fixedNii} {floatingNii}";

            ProcessBuilder.CallExecutableFile(registrationFile, arguments, cmtkOutputDir);
        }
        private static void CreateResultXform(string workingDir, string fixedNii, string floatingNii) // Outputs to the same folder as fixed series
        {
            var rawForm = $@"{workingDir}\{ImgProcConfig.GetCmtkRawxformFile()}";
            var resultForm = $@"{workingDir}\{ImgProcConfig.GetCmtkResultxformFile()}";

            var javaClasspath = ImgProcConfig.GetJavaClassPath();

            var methodname = Properties.Settings.Default.javaClassConvertCmtkXform;

            var javaArgument = $"-classpath {javaClasspath} {methodname} {fixedNii} {floatingNii} {rawForm} {resultForm}";

            ProcessBuilder.CallJava(javaArgument, methodname);

            File.Delete(rawForm);
        }
        private static void ResliceFloatingImages(string outputPath, string fixedNii, string floatingNii, string floatingResliced)
        {
            var cmtkOutputDir = $@"{outputPath}\{ImgProcConfig.GetCmtkFolderName()}";

            Environment.SetEnvironmentVariable("CMTK_WRITE_UNCOMPRESSED", "1"); // So that output is in nii format instead of nii.gz

            var arguments = $@"-o {floatingResliced} --floating {floatingNii} {fixedNii} {cmtkOutputDir}";

            var reformatxFilePath = ImgProcConfig.GetReformatXFilePath();

            ProcessBuilder.CallExecutableFile(reformatxFilePath, arguments);
        }

        //private IFrameOfReference GetFrameOfReference(string workingDir, string fixedToFloatingRapXformTxt)
        //// TODO3: out IFrameOfReference not implemented
        //{
        //    return new FrameOfReference();
        //}

        public static void BiasFieldCorrection(string inNii, string @params, string outNii)
        {
            var bfcExe = ImgProcConfig.GetBfcExeFilePath();
            var arguments = $"-i {inNii} -o {outNii} {@params}";

            ProcessBuilder.CallExecutableFile(bfcExe, arguments);
        }

        public void TakeDifference(string fixedHdrFullPath, string floatingReslicedNiiFullPath,
            string brainSurfaceNiiFullPath, string outputDir,
            out string darkInFloating2BrightInFixed, out string brightInFloating2DarkInFixed, out string brainMask,
            string sliceInset = "0")
        // Outputs to the same folder as fixed series
        {
            darkInFloating2BrightInFixed = $@"{outputDir}\{StructChangesDarkFloat2BrightFixed}.nii";
            brightInFloating2DarkInFixed = $@"{outputDir}\{StructChangesBrightFloat2DarkFixed}.nii";
            brainMask = $@"{outputDir}\{StructChangesBrainMask}.nii";

            try
            {
                const string methodName = "au.com.nicta.preprocess.main.MsProgression"; // TODO3: Hard-coded method name
                var arguments = $"-classpath \"{_javaClassPath}\" {methodName} " +
                                $"\"{outputDir}\" " +
                                $"\"{fixedHdrFullPath}\" \"{floatingReslicedNiiFullPath}\" " +
                                $"\"{brainSurfaceNiiFullPath}\" {sliceInset}";

                ProcessBuilder.CallJava(arguments, methodName);

                CreatePropertiesFiles(outputDir);
                RenameDiffNiiFiles(outputDir);
            }
            catch (Exception ex)
            {
                throw ex; // TODO3: Exception Handling
            }
        }
        private static void CreatePropertiesFiles(string outputDir)
        {
            foreach (var filenameAndContent in GetFileNamesAndContentsToCreate())
                File.WriteAllText($"{outputDir}\\{filenameAndContent.Key}", filenameAndContent.Value);
        }
        private static void RenameDiffNiiFiles(string outputDir)
        {
            foreach (var fileToBeRenamed in GetFilesToBeRenamed())
            {
                var sourceFileName = $@"{outputDir}\{fileToBeRenamed.Key}";
                var targetFileName = $@"{outputDir}\{fileToBeRenamed.Value}";

                if (!File.Exists(sourceFileName))
                    throw new FileNotFoundException($"File was not found to be removed: {sourceFileName}");

                File.Move(sourceFileName, targetFileName);
            }
        }

        public void FlipAndConvertFloatingToDicom(string seriesNii)
        {
            //var reslicedFloatingName = seriesNii.Description;
            //var outputDir = seriesNii.FolderPath;

            //FlipFloatingReslicedImages(reslicedFloatingName, outputDir);
            //ConvertNii2Dicom(reslicedFloatingName, outputDir);
            //MatchDicom2Nii(reslicedFloatingName, outputDir);
        }
        private void FlipFloatingReslicedImages(string reslicedFloatingName, string outputDir)
        {
            const string methodName = "au.com.nicta.preprocess.main.FlipNii"; // TODO3: Hard-coded method name
            var arguments =
                $"-classpath {_javaClassPath} {methodName} {outputDir}\\{reslicedFloatingName}.nii " +
                $@"{outputDir}\{reslicedFloatingName}{FlippedSuffix}.nii";
            ProcessBuilder.CallJava(arguments, methodName);
        }
        private void ConvertNii2Dicom(string reslicedFloatingName, string outputDir)
        {
            var arguments = $@"{outputDir}\{reslicedFloatingName}{FlippedSuffix}.nii {outputDir}\{reslicedFloatingName}{FlippedSuffix}.dcm";
            //ProcessBuilder.CallExecutableFile($"{_executablesPath}\\odin\\{MiconvFileName}", arguments); // TODO3: Hard-coded method name
        }
        private void MatchDicom2Nii(string reslicedFloatingName, string outputDir)
        {
            const string methodName = "au.com.nicta.preprocess.main.MatchDicom2Nii2Dicom"; // TODO3: Hard-coded method name
            var arguments =
                $"-classpath {_javaClassPath} {methodName} {outputDir}/{Fixed}.img " +
                $"{_fixedDicomPath} {outputDir}/{reslicedFloatingName}{FlippedSuffix}_dcm {outputDir}/{FlairOldReslicesFolderName}";
            ProcessBuilder.CallJava(arguments, methodName);
        }

        public void ColorMap(
            string fixedHdrFullPath, string fixedDicomFolderPath,
            string brainSurfaceNiiFullPath,
            string darkFloatToBrightFixedNiiFullPath,
            string brightFloatToDarkFixedNiiFullPath,
            string outputDir, out string positive, out string negative)
        {
            var colorMapConfigFullPath = $"{_processesRootDir}/colormap.config";

            const string methodName = "au.com.nicta.preprocess.main.ColorMap";
            var arguments =
                $"-classpath {_javaClassPath} {methodName} {colorMapConfigFullPath} " +
                $@"{outputDir}\{DstPrefixPositive} " +
                $"{fixedHdrFullPath} {fixedDicomFolderPath} {brainSurfaceNiiFullPath} " +
                $"{darkFloatToBrightFixedNiiFullPath} {brightFloatToDarkFixedNiiFullPath} positive";

            ProcessBuilder.CallJava(arguments, methodName);

            positive = $@"{outputDir}\{DstPrefixPositive}";

            arguments =
                $"-classpath {_javaClassPath} {methodName} {colorMapConfigFullPath} " +
                $@"{outputDir}\{DstPrefixNegative} " +
                $"{fixedHdrFullPath} {fixedDicomFolderPath} {brainSurfaceNiiFullPath} " +
                $"{darkFloatToBrightFixedNiiFullPath} {brightFloatToDarkFixedNiiFullPath} negative";

            ProcessBuilder.CallJava(arguments, methodName);

            negative = $@"{outputDir}\{DstPrefixNegative}";
        }

        public void ConvertBmpsToDicom(string outputDir)
        {
            var folders = new[] { $"{outputDir}\\{DstPrefixNegative}", $"{outputDir}\\{DstPrefixPositive}" };
            foreach (var folder in folders)
            {
                if (!Directory.Exists($"{folder}_dcm")) Directory.CreateDirectory($"{folder}_dcm");
                var files = Directory.GetFiles(folder);
                foreach (var file in files)
                {
                    var filenameNoExt = Path.GetFileNameWithoutExtension(file);
                    var arguments = $"-df {outputDir}\\{Fixed}\\Dicom\\{filenameNoExt} " + // Copy dicom headers from dicom file: -df =  dataset file
                                    $"-i BMP {filenameNoExt}.bmp {folder}_dcm\\{filenameNoExt}"; // TODO3: Hard-coded method name
                    ProcessBuilder.CallExecutableFile($@"{_executablesPath}\{DcmtkFolderName}\{Img2DcmFileName}",
                        arguments, folder);
                }
            }
        }

        public void CopyDicomHeaders(string fixedDicomFolderPath, string outputDir,
            out string dicomFolderNewHeaders)
        {
            var fixedFiles = Directory.GetFiles(fixedDicomFolderPath);
            if (!fixedFiles.Any())
                throw new FileNotFoundException($"No files found in folder: {fixedDicomFolderPath}");

            dicomFolderNewHeaders = $"{outputDir}\\{DicomFilesWithNewHeadersFolder}";
            if (!Directory.Exists(dicomFolderNewHeaders)) Directory.CreateDirectory(dicomFolderNewHeaders);
            var keys = new[] { "(0020,0032)", "(0020,0037)" }; // TODO3: Hard-coded data

            foreach (var fixedFileFullPath in fixedFiles)
            {
                var filenameWithExt = Path.GetFileName(fixedFileFullPath);
                var copiedFileFullPath = $"{dicomFolderNewHeaders}\\{filenameWithExt}";
                File.Copy(fixedFileFullPath, copiedFileFullPath, true);

                foreach (var key in keys)
                {
                    var arguments = $"+L -M {fixedFileFullPath}";
                    var stdout = ProcessBuilder.CallExecutableFile(
                        $"{_executablesPath}\\{DcmtkFolderName}\\{DcmdumpFileName}", arguments);

                    var match = Regex.Match(stdout, $"{key}.*").Value;
                    var value = Regex.Match(match, @"\[(.*)\]").Value.Replace("[", "").Replace("]", "");

                    arguments = $"--no-backup -m {key}={value} {copiedFileFullPath}";
                    ProcessBuilder.CallExecutableFile(
                        $"{_executablesPath}\\{DcmtkFolderName}\\{DcmodifyFileName}", arguments);
                }
            }
        }

        public void Resize(string inHdr, string outNii, int destinationWidth)
        {
            try
            {
                const string methodname = "au.com.nicta.preprocess.main.ResizeNii";
                var javaArgument = $"-classpath {_javaClassPath} {methodname} " +
                                   $@"{inHdr} {outNii} {destinationWidth}";

                ProcessBuilder.CallJava(javaArgument, methodname);
            }
            catch
            {
                // TODO3: Exception Handling
            }
        }

        public void ResizeBacktToOriginalSize(string resizedHdr, string outNii, string seriesHdr)
        {
            try
            {
                const string methodname = "au.com.nicta.preprocess.main.ResizeNiiToSameSize";
                var javaArgument = $"-classpath {_javaClassPath} {methodname} " +
                                   $@"{resizedHdr} {outNii} {seriesHdr}";

                ProcessBuilder.CallJava(javaArgument, methodname);
            }
            catch
            {
                // TODO3: Exception Handling
            }
        }
    }
}