using CAPI.Common.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace CAPI.ImageProcessing
{
    public class ImageProcessorNew //: IImageProcessorNew
    {
        private readonly string _executablesPath;
        //private const string Fixed = "fixed";
        //private const string Floating = "floating";
        private readonly string _fixedDicomPath;
        private readonly string _processesRootDir;

        private const string FlippedSuffix = "_flipped";
        //private const string MiconvFileName = "miconv.exe";
        //private const string DstPrefixPositive = "flair_new_with_changes_overlay_positive";
        //private const string DstPrefixNegative = "flair_new_with_changes_overlay_negative";
        //private const string StructChangesDarkFloat2BrightFixed = "structural_changes_dark_in_floating_to_bright_in_fixed";
        //private const string StructChangesBrightFloat2DarkFixed = "structural_changes_bright_in_floating_to_dark_in_fixed";
        //private const string StructChangesBrainMask = "structural_changes_brain_surface_mask";

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

        public static void BiasFieldCorrection(string inNii, string @params, string outNii)
        {
            var bfcExe = ImgProcConfig.GetBfcExeFilePath();
            var arguments = $"-i {inNii} -o {outNii} {@params}";

            ProcessBuilder.CallExecutableFile(bfcExe, arguments);
        }

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
    }
}