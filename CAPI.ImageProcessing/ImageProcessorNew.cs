using CAPI.Common.Services;
using CAPI.ImageProcessing.Abstraction;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CAPI.ImageProcessing
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImageProcessorNew : IImageProcessorNew
    {
        public void ExtractBrainMask(string inNii, string bseParams, string outBrainNii, string outMaskNii)
        {
            var bseExe = ImgProcConfig.GetBseExeFilePath();
            var arguments = $"-i {inNii} --mask {outMaskNii} -o {outBrainNii} {bseParams}";

            if (!Directory.Exists(Path.GetDirectoryName(outBrainNii))) throw new DirectoryNotFoundException();
            if (!Directory.Exists(Path.GetDirectoryName(outMaskNii))) throw new DirectoryNotFoundException();

            ProcessBuilder.CallExecutableFile(bseExe, arguments);

            if (!File.Exists(outBrainNii) || !File.Exists(outMaskNii))
                throw new FileNotFoundException("Brain mask removal failed to create brain/mask.");
        }

        public void Registration(string currentNii, string priorNii, string outPriorReslicedNii)
        {
            var outputPath = Directory.GetParent(Path.GetDirectoryName(currentNii)).FullName;

            CreateRawXform(outputPath, currentNii, priorNii);

            CreateResultXform(outputPath, currentNii, priorNii);

            ResliceFloatingImages(outputPath, currentNii, priorNii, outPriorReslicedNii);
        }

        private static void CreateRawXform(string outputPath, string fixedNii, string floatingNii)
        {
            var registrationFile = ImgProcConfig.GetRegistrationFilePath();
            var registrationParams = ImgProcConfig.GetRegistrationParams();
            var cmtkOutputDir = $@"{outputPath}\{ImgProcConfig.GetCmtkFolderName()}";
            var rawForm = $@"{outputPath}\{ImgProcConfig.GetCmtkRawxformFile()}";

            if (Directory.Exists(cmtkOutputDir)) Directory.Delete(cmtkOutputDir);
            FileSystem.DirectoryExistsIfNotCreate(cmtkOutputDir);

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

        public void BiasFieldCorrection(string inNii, string bfcParams, string outNii)
        {
            var bfcExe = ImgProcConfig.GetBfcExeFilePath();
            var arguments = $"-i {inNii} -o {outNii} {bfcParams}";

            ProcessBuilder.CallExecutableFile(bfcExe, arguments);
        }

        public void Compare(
            string currentNiiFile, string priorNiiFile, string lookupTableFile,
            SliceType sliceType, string resultNiiFile)
        {
            var currentNii = new Nifti().ReadNifti(currentNiiFile);
            var priorNii = new Nifti().ReadNifti(priorNiiFile);

            var lookupTable = new SubtractionLookUpTable();
            lookupTable.LoadImage(lookupTableFile);

            var result = new Nifti().Compare(currentNii, priorNii, sliceType, lookupTable);

            FileSystem.DirectoryExistsIfNotCreate(Path.GetDirectoryName(resultNiiFile));

            result.WriteNifti(resultNiiFile);
        }

        public void CompareBrainNiftiWithReslicedBrainNifti_OutNifti(
            string currentNii, string priorNii, string lookupTable, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string resultNii, string outPriorReslicedNii)
        {
            FileSystem.FilesExist(new[] { currentNii, priorNii, lookupTable });

            var fixedFile = currentNii;
            var floatingFile = priorNii;

            if (extractBrain)
            {
                var bseParams = ImgProcConfig.GetBseParams();
                var fixedBrain = currentNii.Replace(".nii", ".brain.nii");
                var fixedMask = currentNii.Replace(".nii", ".mask.nii");
                ExtractBrainMask(fixedFile, bseParams, fixedBrain, fixedMask);
                fixedFile = fixedBrain;

                var floatingBrain = priorNii.Replace(".nii", ".brain.nii");
                var floatingMask = priorNii.Replace(".nii", ".mask.nii");
                ExtractBrainMask(floatingFile, bseParams, floatingBrain, floatingMask);
                floatingFile = floatingBrain;
            }

            if (register)
            {
                var resliced = priorNii.Replace(".nii", ".resliced.nii");
                Registration(fixedFile, floatingFile, resliced);
                if (!File.Exists(resliced))
                    throw new FileNotFoundException($"Registration process failed to created resliced file {outPriorReslicedNii}");
                FileSystem.DirectoryExistsIfNotCreate(Path.GetDirectoryName(outPriorReslicedNii));
                File.Move(resliced, outPriorReslicedNii);
                floatingFile = outPriorReslicedNii;
            }

            if (biasFieldCorrect)
            {
                var bfcParams = ImgProcConfig.GetBfcParams();

                var fixedBfc = currentNii.Replace(".nii", ".bfc.nii");
                BiasFieldCorrection(fixedFile, bfcParams, fixedBfc);
                fixedFile = fixedBfc;

                var floatingBfc = priorNii.Replace(".nii", ".bfc.nii");
                BiasFieldCorrection(floatingFile, bfcParams, floatingBfc);
                floatingFile = floatingBfc;
            }

            Compare(fixedFile, floatingFile, lookupTable, sliceType, resultNii);
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void CompareDicomInNiftiOut(
            string currentDicomFolder, string priorDicomFolder, string lookupTable, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string resultNii, string outPriorReslicedNii)
        {
            if (!File.Exists(lookupTable))
                throw new FileNotFoundException($"Unable to locate Lookup Table in the following path: {lookupTable}");

            // Generate Nifti file from Dicom and pass to ProcessNifti Method for current seires
            if (!FileSystem.DirectoryIsValidAndNotEmpty(currentDicomFolder))
                throw new DirectoryNotFoundException($"Dicom folder either does not exist or contains no files: {currentDicomFolder}");

            var currentNifti = Path.Combine(Path.GetDirectoryName(currentDicomFolder), "fixed.nii");

            ImageConverter.DicomToNiix(currentDicomFolder, currentNifti);

            // Generate Nifti file from Dicom and pass to ProcessNifti Method for prior seires
            if (!FileSystem.DirectoryIsValidAndNotEmpty(priorDicomFolder))
                throw new DirectoryNotFoundException($"Dicom folder either does not exist or contains no files: {priorDicomFolder}");

            var priorNifti = Path.Combine(Path.GetDirectoryName(priorDicomFolder), "floating.nii");

            ImageConverter.DicomToNiix(priorDicomFolder, priorNifti);

            CompareBrainNiftiWithReslicedBrainNifti_OutNifti(currentNifti, priorNifti, lookupTable, sliceType,
                extractBrain, register, biasFieldCorrect,
                resultNii, outPriorReslicedNii);
        }

        #region "Unused methods"
        //private const string DicomFilesWithNewHeadersFolder = "flair_old_resliced_new_header";
        //private const string DcmdumpFileName = "dcmdump.exe";
        //private const string DcmodifyFileName = "dcmodify.exe";
        private readonly string _executablesPath;
        private readonly string _fixedDicomPath;
        private const string FlippedSuffix = "_flipped";
        private const string DcmtkFolderName = "dcmtk-3.6.0-win32-i386";
        private const string Img2DcmFileName = "img2dcm.exe";
        private const string FlairOldReslicesFolderName = "flair_old_resliced";
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
        public static void TakeDifference(string fixedBrainNii, string floatingBrainNii, string fixedMaskNii,
        string changesPositive, string changesNegative, string changesMask, string sliceInset = "0")
        {
            var outputDir = Path.GetDirectoryName(changesPositive);
            var javaClassPath = ImgProcConfig.GetJavaClassPath();

            var methodName = ImgProcConfig.GetMsProgressionJavaClassName();
            var arguments = $"-classpath \"{javaClassPath}\" {methodName} \"{outputDir}\" " +
                            $"\"{fixedBrainNii}\" \"{floatingBrainNii}\" \"{fixedMaskNii}\" {sliceInset}";

            ProcessBuilder.CallJava(arguments, methodName);

            //CreatePropertiesFiles(outputDir);
            RenameDiffNiiFiles(changesPositive, changesNegative, changesMask, outputDir);
        }
        private static void CreatePropertiesFiles(string outputDir)
        {
            foreach (var filenameAndContent in GetFileNamesAndContentsToCreate())
                File.WriteAllText($"{outputDir}\\{filenameAndContent.Key}", filenameAndContent.Value);
        }

        private static void RenameDiffNiiFiles(string subPos, string subNeg, string subMask, string outputDir)
        {
            var outPositive = $@"{outputDir}\{ImgProcConfig.GetSubtractPositiveNii()}";
            var outNegative = $@"{outputDir}\{ImgProcConfig.GetSubtractNegativeNii()}";
            var outMask = $@"{outputDir}\{ImgProcConfig.GetSubtractMaskNii()}";

            if (!File.Exists(outPositive)) throw new FileNotFoundException($"File was not found to be renamed: {outPositive}");
            if (!File.Exists(outNegative)) throw new FileNotFoundException($"File was not found to be renamed: {outNegative}");
            if (!File.Exists(outMask)) throw new FileNotFoundException($"File was not found to be renamed: {outMask}");

            File.Move(outPositive, subPos);
            File.Move(outNegative, subNeg);
            File.Move(outMask, subMask);
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
            var javaClassPath = ImgProcConfig.GetJavaClassPath();
            const string methodName = "au.com.nicta.preprocess.main.FlipNii"; // TODO3: Hard-coded method name
            var arguments =
                $"-classpath {javaClassPath} {methodName} {outputDir}\\{reslicedFloatingName}.nii " +
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
            var javaClassPath = ImgProcConfig.GetJavaClassPath();
            const string methodName = "au.com.nicta.preprocess.main.MatchDicom2Nii2Dicom"; // TODO3: Hard-coded method name
            var arguments =
                $"-classpath {javaClassPath} {methodName} {outputDir}/Fixed.img " +
                $"{_fixedDicomPath} {outputDir}/{reslicedFloatingName}{FlippedSuffix}_dcm {outputDir}/{FlairOldReslicesFolderName}";
            ProcessBuilder.CallJava(arguments, methodName);
        }

        public static void ColorMap(
            string fixedNii, string fixedDicomFolder, string fixedMaskNii,
            string subtractPositive, string subtractNegative,
            string positiveFolder, string negativeFolder)
        {
            var settings = Properties.Settings.Default;
            var colorMapConfigFile = ImgProcConfig.GetColorMapConfigFile();
            var javaClassPath = ImgProcConfig.GetJavaClassPath();
            var outputDir = Path.GetDirectoryName(positiveFolder);

            var methodName = ImgProcConfig.GetColorMapJavaClassName();
            var arguments =
                $"-classpath \"{javaClassPath}\" {methodName} \"{colorMapConfigFile}\" " +
                $"\"{outputDir}\\{settings.colormapPositiveImages}\" " +
                $"\"{fixedNii}\" \"{fixedDicomFolder}\" \"{fixedMaskNii}\" " +
                $"\"{subtractPositive}\" \"{subtractNegative}\" positive";

            ProcessBuilder.CallJava(arguments, methodName);

            if (!string.Equals($@"{outputDir}\{settings.colormapPositiveImages}",
                positiveFolder, StringComparison.CurrentCultureIgnoreCase))
                Directory.Move($@"{outputDir}\{settings.colormapPositiveImages}", positiveFolder);
            //positiveFolder = $@"{outputDir}\{DstPrefixPositive}";

            arguments =
                $"-classpath {javaClassPath} {methodName} {colorMapConfigFile} " +
                $@"{outputDir}\{settings.colormapNegativeImages} " +
                $"{fixedNii} {fixedDicomFolder} {fixedMaskNii} " +
                $"{subtractPositive} {subtractNegative} negative";

            ProcessBuilder.CallJava(arguments, methodName);

            if (!string.Equals($@"{outputDir}\{settings.colormapNegativeImages}",
                negativeFolder, StringComparison.CurrentCultureIgnoreCase))
                Directory.Move($@"{outputDir}\{settings.colormapNegativeImages}", negativeFolder);
            //negativeFolder = $@"{outputDir}\{DstPrefixNegative}";
        }

        public void ConvertBmpsToDicom(string outputDir)
        {
            var settings = Properties.Settings.Default;
            var folders = new[] { $"{outputDir}\\{settings.colormapNegativeImages}", $"{outputDir}\\{settings.colormapPositiveImages}" };
            foreach (var folder in folders)
            {
                if (!Directory.Exists($"{folder}_dcm")) Directory.CreateDirectory($"{folder}_dcm");
                var files = Directory.GetFiles(folder);
                foreach (var file in files)
                {
                    var filenameNoExt = Path.GetFileNameWithoutExtension(file);
                    var arguments = $"-df {outputDir}\\Fixed\\Dicom\\{filenameNoExt} " + // Copy dicom headers from dicom file: -df =  dataset file
                                    $"-i BMP {filenameNoExt}.bmp {folder}_dcm\\{filenameNoExt}"; // TODO3: Hard-coded method name
                    ProcessBuilder.CallExecutableFile($@"{_executablesPath}\{DcmtkFolderName}\{Img2DcmFileName}",
                        arguments, folder);
                }
            }
        }
        #endregion
    }
}