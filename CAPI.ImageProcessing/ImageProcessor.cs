using CAPI.Common.Config;
using CAPI.Common.Services;
using CAPI.ImageProcessing.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CAPI.ImageProcessing
{
    public class ImageProcessor : IImageProcessor
    {
        private readonly IImageConverter _imageConverter;

        private readonly string _executablesPath;
        private readonly string _javaClassPath;
        private const string Fixed = "fixed";
        private const string Floating = "floating";
        private readonly string _fixedDicomPath;
        private readonly string _processesRootDir;

        private const string FlippedSuffix = "_flipped";
        private const string Dcm2NiiExe = "dcm2nii.exe";
        private const string Dcm2NiiHdrParams = "-n N -f Y -r N";
        private const string Dcm2NiiNiiParams = "-n Y -g N -f Y -r N";
        //private const string BseExe = "bse09e.exe";
        private const string BseExe = "bse.exe";
        private const string BrainSurfaceSuffix = "_brain_surface";
        private const string BrainSurfaceExtSuffix = "_brain_surface_extracted";
        private const string RegistrationExeFileName = "registration.exe";
        private const string ReformatXFileName = "reformatx.exe";
        private const string CmtkParams = "--initxlate --dofs 6 --auto-multi-levels 4 --out-matrix";
        private const string CmtkRawXformFileName = "cmtk_xform_mat.txt";
        private const string CmtkResultXformFileName = "fixed_to_floating_rap_xform.txt";
        private const string CmtkOutputParam = "-o .";
        private const string CmtkOutputDirName = "cmtk_xform";
        private const string MiconvFileName = "miconv.exe";
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

        public ImageProcessor()
        {
            _executablesPath = ImgProc.GetExecutablesPath();
            var javaUtilsPath = ImgProc.GetJavaUtilsPath();
            _javaClassPath = $".;{javaUtilsPath}/PreprocessJavaUtils.jar;{javaUtilsPath}/lib/NICTA.jar;" +
                $"{javaUtilsPath}/lib/vecmath.jar;{javaUtilsPath}/lib/ij.jar";
            _processesRootDir = ImgProc.GetProcessesRootDir();
        }

        public static void RunAll()
        {
            ProcessBuilder.CallExecutableFile($@"{ImgProc.GetProcessesRootDir()}\_runall.bat", "");
        } // TODO3: To be checked if this is stil working

        public string ConvertDicom2Hdr(string dicomPath, string outputPath, string hdrFileNameNoExt)
        {
            try
            {
                // Make sure temp folder exists
                var tmpDir = $"{outputPath}\\tmpDir";
                if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
                Directory.CreateDirectory(tmpDir);

                // Call dcm2nii.exe to perform the conversion
                ProcessBuilder.CallExecutableFile($@"{_executablesPath}\{Dcm2NiiExe}", $"{Dcm2NiiHdrParams} -o {tmpDir} {dicomPath}");

                var hdrFileFullPath = Directory.GetFiles($"{tmpDir}").FirstOrDefault(f => f.EndsWith(".hdr"));
                var imgFileFullPath = Directory.GetFiles($"{tmpDir}").FirstOrDefault(f => f.EndsWith(".img"));
                if (hdrFileFullPath != null) File.Copy(hdrFileFullPath, $"{outputPath}\\{hdrFileNameNoExt}.hdr");
                if (imgFileFullPath != null) File.Copy(imgFileFullPath, $"{outputPath}\\{hdrFileNameNoExt}.img");

                // Remove temp folder
                if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);

                return $"{outputPath}\\{hdrFileNameNoExt}.hdr";
            }
            catch (Exception ex) // TODO1 Exception Handling
            {
                return $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
            }
        }

        public string ConvertDicomToNii(string dicomPath, string outputPath, string niiFileNameNoExt)
        {
            try
            {
                // Make sure temp folder exists
                var tmpDir = $"{outputPath}\\tmpDir";
                if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
                Directory.CreateDirectory(tmpDir);

                // Call dcm2nii.exe to perform the conversion
                var convertFixedDicom2NiiProc = ProcessBuilder.Build(_executablesPath, Dcm2NiiExe, $"{Dcm2NiiNiiParams} -o {tmpDir} {dicomPath}", "");
                convertFixedDicom2NiiProc.Start();
                Logger.ProcessErrorLogWrite(convertFixedDicom2NiiProc, "convertFixedDicom2NiiProc");
                convertFixedDicom2NiiProc.WaitForExit();

                var niiFileFullPath = Directory.GetFiles($"{tmpDir}").FirstOrDefault(f => f.EndsWith(".nii"));
                if (niiFileFullPath != null) File.Copy(niiFileFullPath, $"{outputPath}\\{niiFileFullPath}.nii");

                // Remove temp folder
                if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);

                return $"{outputPath}\\{niiFileNameNoExt}.nii";
            }
            catch (Exception ex) // TODO1 Exception Handling
            {
                return $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
            }
        }

        public void ExtractBrainMask(string inputHdrFullPath, string outputPath, string bseParams,
            out string brainMaskRemoved, out string smoothBrainMask)
        {
            var inputFileName = Path.GetFileNameWithoutExtension(inputHdrFullPath);

            var arguments = $"-i {inputHdrFullPath} " +
                            $"--mask {outputPath}\\{inputFileName}{BrainSurfaceSuffix}.hdr " +
                            $"-o {outputPath}\\{inputFileName}{BrainSurfaceExtSuffix}.hdr {bseParams}";

            ProcessBuilder.CallExecutableFile($@"{_executablesPath}\{BseExe}", arguments);

            brainMaskRemoved = inputFileName + BrainSurfaceExtSuffix + ".hdr";
            smoothBrainMask = inputFileName + BrainSurfaceSuffix + ".hdr";
        }

        public void CopyNiftiImage2PatientTransform(string inputHdrOrNii, string originalHdr)
        {
            var destination = inputHdrOrNii.EndsWith(".hdr")
                ? inputHdrOrNii.Replace(".hdr", ".nii")
                : inputHdrOrNii;

            try
            {
                const string methodName = "au.com.nicta.preprocess.main.CopyNiftiImage2PatientTransform";
                var javaArgument = $"-classpath {_javaClassPath} {methodName} " +
                                   $"\"{originalHdr}\" \"{inputHdrOrNii}\" \"{destination}\"";

                ProcessBuilder.CallJava(javaArgument, methodName);
            }
            catch
            {
                // TODO3: Exception Handling
            }
        }
        private static void RemoveUnnecessaryFiles(string path, string seriesName, string[] exclusions)
        {
            var unused = Directory.GetFiles(path)
                .Select(Path.GetFileName)
                .Where(f => f.ToLower() == $"{seriesName}.hdr" || f.ToLower() == $"{seriesName}.img")
                .Where(f => !exclusions.Contains(f))
                .All(f =>
                {
                    File.Delete($"{path}\\{f}");
                    return true;
                });

            File.Delete($"{path}\\{Fixed}.nii");
            File.Delete($"{path}\\{Floating}.nii");
        }

        public void Registration(string outputPath, string fixedFullPath, string floatingFullPath,
            out string floatingReslicedFullPath, out IFrameOfReference fixedFrameOfRef)
        {
            CreateRawXform(outputPath, fixedFullPath, floatingFullPath);

            CreateResultXform(outputPath, fixedFullPath, floatingFullPath,
                out fixedFrameOfRef);

            ResliceFloatingImages(outputPath, fixedFullPath, floatingFullPath,
                out floatingReslicedFullPath);
        }
        private void CreateRawXform(string outputPath, string fixedHdrFullPath, string floatingHdrFullPath)
        {
            var cmtkOutputDir = $@"{outputPath}\{CmtkOutputDirName}";
            if (Directory.Exists(cmtkOutputDir)) Directory.Delete(cmtkOutputDir);
            Directory.CreateDirectory($@"{outputPath}\{CmtkOutputDirName}");

            var arguments = $@"{CmtkParams} {outputPath}\{CmtkRawXformFileName} {CmtkOutputParam} " +
                            $"{fixedHdrFullPath} {floatingHdrFullPath}";

            ProcessBuilder.CallExecutableFile(
                $@"{_executablesPath}\CMTK\bin\{RegistrationExeFileName}", arguments, cmtkOutputDir);
        }
        private void CreateResultXform(string workingDir, string fixedFullPath, string floatingFullPath,
            out IFrameOfReference fixedFrameOfRef) // Outputs to the same folder as fixed series
        {
            var fixedNoExtension = fixedFullPath.Replace(".nii", "");
            var floatingNoExtension = floatingFullPath.Replace(".nii", "");

            try
            {
                const string methodname = "au.com.nicta.preprocess.main.ConvertCmtkXform"; // TODO3: Hard-coded file name | Hard-coded Java Method Description
                var javaArgument = $"-classpath {_javaClassPath} {methodname} " +
                                   //$@"{srcDir}\{fixedFullPath.Description}.nii {srcDir}\{seriesFloating.Description}.nii "+
                                   $@"{fixedNoExtension}.nii {floatingNoExtension}.nii " +
                                   $@"{workingDir}\{CmtkRawXformFileName} {workingDir}\{CmtkResultXformFileName}";

                ProcessBuilder.CallJava(javaArgument, methodname);

                File.Delete($@"{workingDir}\{CmtkRawXformFileName}");
            }
            catch
            {
                //return null; // TODO3: Exception Handling
            }

            fixedFrameOfRef = GetFrameOfReference(workingDir, CmtkResultXformFileName);
        }
        private void ResliceFloatingImages(string outputPath, string fixedHdrFullPath, string floatingHdrFullPath,
            out string floatingReslicedFullPath)
        {
            var floatingHdrFileNameNoExt = Path.GetFileNameWithoutExtension(floatingHdrFullPath);
            var floatingFolderPath = Path.GetDirectoryName(floatingHdrFullPath);

            floatingReslicedFullPath = $@"{floatingFolderPath}\{floatingHdrFileNameNoExt}_resliced.nii";

            Environment.SetEnvironmentVariable("CMTK_WRITE_UNCOMPRESSED", "1"); // So that output is in nii format instead of nii.gz
            var arguments = $@"-o {floatingReslicedFullPath} " +
                            $@"--floating {floatingHdrFullPath} " +
                            $@"{fixedHdrFullPath} {outputPath}\{CmtkOutputDirName}";

            ProcessBuilder.CallExecutableFile($@"{_executablesPath}\CMTK\bin\{ReformatXFileName}", arguments);
        }

        private IFrameOfReference GetFrameOfReference(string workingDir, string fixedToFloatingRapXformTxt)
        // TODO3: out IFrameOfReference not implemented
        {
            return new FrameOfReference();
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
            ProcessBuilder.CallExecutableFile($"{_executablesPath}\\odin\\{MiconvFileName}", arguments); // TODO3: Hard-coded method name
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