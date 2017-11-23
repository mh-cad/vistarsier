using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CAPI.BLL.Model;
using CAPI.Common;
using CAPI.Common.Services;
using SeriesHdr = CAPI.BLL.Model.SeriesHdr;
using SeriesNii = CAPI.BLL.Model.SeriesNii;

namespace CAPI.ImageProcessing
{
    public class ImageProcessor
    {
        private readonly string _executablesPath;
        private readonly string _javaClassPath;
        private const string Fixed = "fixed"; // TODO3: Hard-coded name
        private const string Floating = "floating";
        private readonly string _fixedDicomPath;
        private readonly string _processesRootDir;

        private const string FlippedSuffix = "_flipped";
        private const string Dcm2NiiExe = "dcm2nii.exe";
        private const string Dcm2NiiHdrParams = "-n N -f Y -r N";
        private const string Dcm2NiiNiiParams = "-n Y -g N -f Y -r N";
        private const string BseExe = "bse09e.exe";
        private const string BseParams = "-n 3 -d 25 -s 0.64 -r 1 --trim";
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
            _executablesPath = Config.GetExecutablesPath();
            var javaUtilsPath = Config.GetJavaUtilsPath();
            _javaClassPath = $".;{javaUtilsPath}/*";
            _fixedDicomPath = Config.GetFixedDicomDir();
            _processesRootDir = Config.GetProcessesRootDir();
        }

        public ImageProcessor(string executablesPath) : this()
        {
            if (!string.IsNullOrEmpty(executablesPath)) _executablesPath = Config.GetExecutablesPath();
        }

        public static void RunAll()
        {
            ProcessBuilder.CallExecutableFile($@"{Config.GetProcessesRootDir()}\_runall.bat", "");
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

        public void ExtractBrainMask(SeriesHdr hdrSeries, string outputPath, out SeriesHdr brainMaskRemoved, out SeriesHdr smoothBrainMask)
        {
            var arguments = $"-i {hdrSeries.FileFullPath} --mask {outputPath}\\{hdrSeries.Description}{BrainSurfaceSuffix} " +
                            $"-o {outputPath}\\{hdrSeries.Description}{BrainSurfaceExtSuffix} {BseParams}";
            ProcessBuilder.CallExecutableFile($@"{_executablesPath}\{BseExe}", arguments);

            brainMaskRemoved = new SeriesHdr(hdrSeries.Description, $"{outputPath}\\{hdrSeries.Description}{BrainSurfaceExtSuffix}.hdr", hdrSeries.NumberOfImages);
            smoothBrainMask = new SeriesHdr(hdrSeries.Description, $"{outputPath}\\{hdrSeries.Description}{BrainSurfaceSuffix}.hdr", hdrSeries.NumberOfImages);
        }

        public SeriesNii ConvertHdrToNii(SeriesHdr seriesHdr, SeriesHdr originalHdr, string seriesName)
        {
            try
            {
                const string methodName = "au.com.nicta.preprocess.main.CopyNiftiImage2PatientTransform";
                var javaArgument = $"-classpath {_javaClassPath} {methodName} " + // TODO3: Hard-coded file name | Hard-coded Java Method Description
                                   $"\"{seriesHdr.FolderPath}\\{originalHdr.Description}.hdr\" \"{seriesHdr.FolderPath}\\{seriesName}.hdr\" \"{seriesHdr.FolderPath}\\{seriesName}.nii\"";

                ProcessBuilder.CallJava(javaArgument, methodName);

                RemoveUnnecessaryFiles(
                    originalHdr.FolderPath,
                    seriesHdr.Description,
                    new[] { $"{Fixed.ToLower()}.hdr", $"{Fixed.ToLower()}.img", $"{Floating.ToLower()}.hdr", $"{Floating.ToLower()}.img" });

                return new SeriesNii(seriesName, seriesHdr.FileFullPath.Replace("hdr", "nii"), seriesHdr.NumberOfImages);
            }
            catch
            {
                return null; // TODO3: Exception Handling
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

        public void Registration(ISeries seriesFixed, ISeries seriesFloating, string outputPath)
        {
            CreateRawXform(seriesFixed, seriesFloating, outputPath);
            CreateResultXform(seriesFixed, seriesFloating);
            ResliceFloatingImages(outputPath);
        }
        private void CreateRawXform(ISeries seriesFixed, ISeries seriesFloating, string outputPath)
        {
            var cmtkOutputDir = $@"{outputPath}\{CmtkOutputDirName}";
            if (Directory.Exists(cmtkOutputDir)) Directory.Delete(cmtkOutputDir);
            Directory.CreateDirectory($@"{outputPath}\{CmtkOutputDirName}");

            var arguments = $@"{CmtkParams} {outputPath}\{CmtkRawXformFileName} {CmtkOutputParam} {seriesFixed.FileFullPath} {seriesFloating.FileFullPath}";

            ProcessBuilder.CallExecutableFile($@"{_executablesPath}\CMTK\bin\{RegistrationExeFileName}", arguments, cmtkOutputDir);
        }
        private void CreateResultXform(ISeries seriesFixed, ISeries seriesFloating) // Outputs to the same folder as fixed series
        {
            try
            {
                var srcDir = seriesFixed.FolderPath;
                const string methodname = "au.com.nicta.preprocess.main.ConvertCmtkXform"; // TODO3: Hard-coded file name | Hard-coded Java Method Description
                var javaArgument = $"-classpath {_javaClassPath} {methodname} " + 
                                   $@"{srcDir}\{seriesFixed.Description}.nii {srcDir}\{seriesFloating.Description}.nii {srcDir}\{CmtkRawXformFileName} {srcDir}\{CmtkResultXformFileName}";

                ProcessBuilder.CallJava(javaArgument, methodname);

                File.Delete($@"{srcDir}\{CmtkRawXformFileName}");
            }
            catch
            {
                //return null; // TODO3: Exception Handling
            }
        }
        private void ResliceFloatingImages(string outputPath)
        {
            Environment.SetEnvironmentVariable("CMTK_WRITE_UNCOMPRESSED", "1"); // So that output is in nii format instead of nii.gz
            var arguments = $@"-o {outputPath}\{Floating}_resliced.nii --floating {outputPath}\{Floating}.hdr {outputPath}\{Fixed}.hdr {outputPath}\{CmtkOutputDirName}";

            ProcessBuilder.CallExecutableFile($@"{_executablesPath}\CMTK\bin\{ReformatXFileName}", arguments);
        }

        public void TakeDifference(ISeries seriesFixedHdr, ISeries seriesFloatingReslicedNii, ISeries seriesBrainSurfaceNii, string outputDir, string sliceInset) // Outputs to the same folder as fixed series
        {
            try
            {
                const string methodName = "au.com.nicta.preprocess.main.MsProgression"; // TODO3: Hard-coded method name
                ProcessBuilder.CallJava(
                    $"-classpath {_javaClassPath} {methodName} " +
                    $@"{outputDir} {outputDir}\{seriesFixedHdr.Description}.hdr {outputDir}\{seriesFloatingReslicedNii.Description}.nii {outputDir}\{seriesBrainSurfaceNii.Description}.nii {sliceInset}"
                    , methodName);

                CreatePropertiesFiles(outputDir);
                RenameDiffNiiFiles(outputDir);
            }
            catch
            {
                //return; // TODO3: Exception Handling
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
                File.Move($"{outputDir}\\{fileToBeRenamed.Key}", $"{outputDir}\\{fileToBeRenamed.Value}");
        }

        public void FlipAndConvertFloatingToDicom(SeriesNii seriesNii)
        {
            var reslicedFloatingName = seriesNii.Description;
            var outputDir = seriesNii.FolderPath;

            FlipFloatingReslicedImages(reslicedFloatingName, outputDir);
            ConvertNii2Dicom(reslicedFloatingName, outputDir);
            MatchDicom2Nii(reslicedFloatingName, outputDir);
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
        
        public void ColorMap(string outputDir)
        {
            const string methodName = "au.com.nicta.preprocess.main.ColorMap";
            var arguments =
                $"-classpath {_javaClassPath} {methodName} {_processesRootDir}/colormap.config {outputDir}/{DstPrefixPositive} " +
                $"{outputDir}/{Fixed}.hdr {_fixedDicomPath} {outputDir}/{Fixed}{BrainSurfaceSuffix}.nii " +
                $"{outputDir}/{StructChangesDarkFloat2BrightFixed}.nii {outputDir}/{StructChangesBrightFloat2DarkFixed}.nii positive"; // TODO3: Hard-coded method name
            ProcessBuilder.CallJava(arguments, methodName);
            arguments =
                $"-classpath {_javaClassPath} {methodName} {_processesRootDir}/colormap.config {outputDir}/{DstPrefixNegative} " +
                $"{outputDir}/{Fixed}.hdr {_fixedDicomPath} {outputDir}/{Fixed}{BrainSurfaceSuffix}.nii " +
                $"{outputDir}/{StructChangesDarkFloat2BrightFixed}.nii {outputDir}/{StructChangesBrightFloat2DarkFixed}.nii negative"; // TODO3: Hard-coded method name
            ProcessBuilder.CallJava(arguments, methodName);
        }

        public void ConvertBmpsToDicom(string outputDir)
        {
            var folders = new [] { $"{outputDir}\\{DstPrefixNegative}", $"{outputDir}\\{DstPrefixPositive}" };
            foreach (var folder in folders)
            {
                if (!Directory.Exists($"{folder}_dcm")) Directory.CreateDirectory($"{folder}_dcm");
                var files = Directory.GetFiles(folder);
                foreach (var file in files)
                {
                    var filenameNoExt = Path.GetFileNameWithoutExtension(file);
                    var arguments = $"-i BMP {filenameNoExt}.bmp {folder}_dcm\\{filenameNoExt}"; // TODO3: Hard-coded method name
                    ProcessBuilder.CallExecutableFile($@"{_executablesPath}\{DcmtkFolderName}\{Img2DcmFileName}", arguments, folder);
                }
            }
        }

        public void CopyDicomHeaders(string outputDir)
        {
            var fixedFiles = Directory.GetFiles(_fixedDicomPath);
            var destinationFolder = $"{outputDir}\\{DicomFilesWithNewHeadersFolder}";
            if (!Directory.Exists(destinationFolder)) Directory.CreateDirectory(destinationFolder);
            var keys = new[] { "(0020,0032)", "(0020,0037)" }; // TODO3: Hard-coded data

            foreach (var fixedFileFullPath in fixedFiles)
            {
                var filenameWithExt = Path.GetFileName(fixedFileFullPath);
                var copiedFileFullPath = $"{destinationFolder}\\{filenameWithExt}";
                File.Copy(fixedFileFullPath, copiedFileFullPath, true);
                foreach (var key in keys)
                {
                    var arguments = $"+L -M {fixedFileFullPath}";
                    var stdout = ProcessBuilder.CallExecutableFile($"{_executablesPath}\\{DcmtkFolderName}\\{DcmdumpFileName}", arguments);

                    var match = Regex.Match(stdout, $"{key}.*").Value;
                    var value = Regex.Match(match, @"\[(.*)\]").Value.Replace("[","").Replace("]", "");

                    arguments = $"--no-backup -m {key}={value} {copiedFileFullPath}";
                    ProcessBuilder.CallExecutableFile($"{_executablesPath}\\{DcmtkFolderName}\\{DcmodifyFileName}", arguments);
                }
            }
        }
    }
}