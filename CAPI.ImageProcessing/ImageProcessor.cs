using CAPI.Common.Abstractions.Config;
using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;

namespace CAPI.ImageProcessing
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImageProcessor : IImageProcessor
    {
        private readonly IFileSystem _filesystem;
        private readonly IProcessBuilder _processBuilder;
        private readonly IImgProcConfig _config;
        private readonly ILog _log;

        public ImageProcessor(IFileSystem filesystem, IProcessBuilder processBuilder,
                              IImgProcConfig config, ILog log)
        {
            _filesystem = filesystem;
            _processBuilder = processBuilder;
            _config = config;
            _log = log;
        }

        public void ExtractBrainMask(string inNii, string bseParams, string outBrainNii, string outMaskNii)
        {
            var bseExe = Path.Combine(_config.ImgProcBinFolderPath, _config.BseExeRelFilePath);

            if (!File.Exists(bseExe))
                throw new FileNotFoundException($"Unable to find {nameof(bseExe)} file: [{bseExe}]");

            var arguments = $"-i {inNii} --mask {outMaskNii} -o {outBrainNii} {bseParams}";

            if (!Directory.Exists(Path.GetDirectoryName(outBrainNii))) throw new DirectoryNotFoundException();
            if (!Directory.Exists(Path.GetDirectoryName(outMaskNii))) throw new DirectoryNotFoundException();

            _processBuilder.CallExecutableFile(bseExe, arguments, "", OutputDataReceivedInProcess, ErrorOccuredInProcess);

            if (!File.Exists(outBrainNii) || !File.Exists(outMaskNii))
                throw new FileNotFoundException("Brain surface removal failed to create brain/mask.");
        }

        public void Registration(string currentNii, string priorNii, string outPriorReslicedNii)
        {
            var outputPath = Directory.GetParent(Path.GetDirectoryName(currentNii)).FullName;

            CreateRawXform(outputPath, currentNii, priorNii);

            CreateResultXform(outputPath, currentNii, priorNii);

            ResliceFloatingImages(outputPath, currentNii, priorNii, outPriorReslicedNii);
        }

        private void CreateRawXform(string outputPath, string fixedNii, string floatingNii)
        {
            var registrationFile = Path.Combine(_config.ImgProcBinFolderPath, _config.RegistrationRelFilePath);

            if (!File.Exists(registrationFile))
                throw new FileNotFoundException($"Unable to find {nameof(registrationFile)} file: [{registrationFile}]");

            var registrationParams = _config.RegistrationParams;
            var cmtkOutputDir = $@"{outputPath}\{_config.CmtkFolderName}";
            var rawForm = $@"{outputPath}\{_config.CmtkRawxformFile}";

            if (Directory.Exists(cmtkOutputDir)) Directory.Delete(cmtkOutputDir, true);
            _filesystem.DirectoryExistsIfNotCreate(cmtkOutputDir);

            var arguments = $@"{registrationParams} --out-matrix {rawForm} -o . {fixedNii} {floatingNii}";

            _processBuilder.CallExecutableFile(registrationFile, arguments, cmtkOutputDir, OutputDataReceivedInProcess, ErrorOccuredInProcess);
        }
        private void CreateResultXform(string workingDir, string fixedNii, string floatingNii) // Outputs to the same folder as fixed series
        {
            var rawForm = $@"{workingDir}\{_config.CmtkRawxformFile}";
            var resultForm = $@"{workingDir}\{_config.CmtkResultxformFile}";
            if (File.Exists(resultForm)) File.Delete(resultForm);

            var javaClasspath = _config.JavaClassPath;

            var methodname = Properties.Settings.Default.javaClassConvertCmtkXform;

            var javaArgument = $"-classpath {javaClasspath} {methodname} {fixedNii} {floatingNii} {rawForm} {resultForm}";

            _processBuilder.CallJava(_config.JavaExeFilePath, javaArgument, methodname, "", OutputDataReceivedInProcess, ErrorOccuredInProcess);

            File.Delete(rawForm);
        }
        private void ResliceFloatingImages(string outputPath, string fixedNii, string floatingNii, string floatingResliced)
        {
            var cmtkOutputDir = $@"{outputPath}\{_config.CmtkFolderName}";

            Environment.SetEnvironmentVariable("CMTK_WRITE_UNCOMPRESSED", "1"); // So that output is in nii format instead of nii.gz

            var arguments = $@"-o {floatingResliced} --floating {floatingNii} {fixedNii} {cmtkOutputDir}";

            var reformatxFilePath = Path.Combine(_config.ImgProcBinFolderPath, _config.ReformatXRelFilePath);

            if (!File.Exists(reformatxFilePath)) throw new
                FileNotFoundException($"Unable to find {nameof(reformatxFilePath)} file: [{reformatxFilePath}]");

            _processBuilder.CallExecutableFile(reformatxFilePath, arguments, "", OutputDataReceivedInProcess, ErrorOccuredInProcess);
        }

        public void BiasFieldCorrection(string inNii, string mask, string bfcParams, string outNii)
        {
            var bfcExe = Path.Combine(_config.ImgProcBinFolderPath, _config.BfcExeRelFilePath);

            if (!File.Exists(inNii))
                throw new FileNotFoundException($"Unable to find {nameof(inNii)} file: [{inNii}]");
            if (!File.Exists(bfcExe))
                throw new FileNotFoundException($"Unable to find {nameof(bfcExe)} file: [{bfcExe}]");

            var arguments = $"-i \"{inNii}\" -o \"{outNii}\" {bfcParams}";
            if (!string.IsNullOrEmpty(mask) && File.Exists(mask))
                arguments += $" -m \"{mask}\"";

            _processBuilder.CallExecutableFile(bfcExe, arguments, "", OutputDataReceivedInProcess, ErrorOccuredInProcess);
        }

        public void Compare(
            string currentNiiFile, string priorNiiFile, string lookupTableFile,
            SliceType sliceType, string resultNiiFile)
        {
            var currentNii = new Nifti().ReadNifti(currentNiiFile);
            var priorNii = new Nifti().ReadNifti(priorNiiFile);

            var lookupTable = new SubtractionLookUpTable();
            lookupTable.LoadImage(lookupTableFile);

            var workingDir = Directory.GetParent(currentNiiFile).FullName;

            var result = new Nifti().Compare(currentNii, priorNii, sliceType, lookupTable, workingDir);

            _filesystem.DirectoryExistsIfNotCreate(Path.GetDirectoryName(resultNiiFile));

            result.WriteNifti(resultNiiFile);
        }

        public void CompareBrainNiftiWithReslicedBrainNifti_OutNifti(
            string currentNii, string priorNii, string lookupTable, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string resultNii, string outPriorReslicedNii)
        {
            _filesystem.FilesExist(new[] { currentNii, priorNii, lookupTable });

            var fixedFile = currentNii;
            var floatingFile = priorNii;
            var fixedMask = ""; // even in case of no mask, an empty parameter should be passed to bias field correction method - empty param gets handled in bfc
            var floatingMask = ""; // even in case of no mask, an empty parameter should be passed to bias field correction method - empty param gets handled in bfc

            var stopwatch = new Stopwatch();

            if (extractBrain)
            {
                var bseParams = _config.BseParams;
                var fixedBrain = currentNii.Replace(".nii", ".brain.nii");
                fixedMask = currentNii.Replace(".nii", ".mask.nii");
                _log.Info("Starting Extraction of Brain Surface for Current series...");
                stopwatch.Start();

                ExtractBrainMask(fixedFile, bseParams, fixedBrain, fixedMask);

                stopwatch.Stop();
                _log.Info($"Finished Extracting Brain Surface for Current series in {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds} minutes.");
                fixedFile = fixedBrain;

                var floatingBrain = priorNii.Replace(".nii", ".brain.nii");
                floatingMask = priorNii.Replace(".nii", ".mask.nii");
                _log.Info("Starting Extraction of Brain Surface for Prior series...");
                stopwatch.Restart();

                ExtractBrainMask(floatingFile, bseParams, floatingBrain, floatingMask);

                floatingFile = floatingBrain;
                stopwatch.Stop();
                _log.Info($"Finished Extracting Brain Surface for Prior series in {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds} minutes.");
            }

            if (register)
            {
                _log.Info("Starting Registration of Current and Prior Series...");
                stopwatch.Restart();
                var resliced = priorNii.Replace(".nii", ".resliced.nii");
                Registration(fixedFile, floatingFile, resliced);
                if (!File.Exists(resliced))
                    throw new FileNotFoundException($"Registration process failed to created resliced file {outPriorReslicedNii}");
                _filesystem.DirectoryExistsIfNotCreate(Path.GetDirectoryName(outPriorReslicedNii));
                File.Move(resliced, outPriorReslicedNii);
                floatingFile = outPriorReslicedNii;
                stopwatch.Stop();
                _log.Info($"Finished Registration of Current and Prior in {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds} minutes.");
            }

            if (biasFieldCorrect)
            {

                var bfcParams = _config.BfcParams;
                var fixedBfc = currentNii.Replace(".nii", ".bfc.nii");
                _log.Info("Starting Bias Field Correction for Current series...");
                stopwatch.Restart();

                BiasFieldCorrection(fixedFile, fixedMask, bfcParams, fixedBfc);

                stopwatch.Stop();
                _log.Info($"Finished Bias Field Correction of Current series in {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds} minutes.");
                fixedFile = fixedBfc;

                var floatingBfc = priorNii.Replace(".nii", ".bfc.nii");
                _log.Info("Starting Bias Field Correction for Prior series...");
                stopwatch.Restart();

                BiasFieldCorrection(floatingFile, fixedMask, bfcParams, floatingBfc);

                if (File.Exists(outPriorReslicedNii)) File.Move(outPriorReslicedNii, outPriorReslicedNii.Replace(".nii", ".preBfc.nii"));
                if (File.Exists(floatingBfc) && !File.Exists(outPriorReslicedNii))
                    File.Move(floatingBfc, outPriorReslicedNii);
                floatingFile = outPriorReslicedNii;

                stopwatch.Stop();
                _log.Info($"Finished Bias Field Correction for Prior series in {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds} minutes.");
            }

            const bool normalize = true;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (normalize && extractBrain)
            {
                Normalize(fixedFile, fixedMask, sliceType, lookupTable);
                Normalize(floatingFile, floatingMask, sliceType, lookupTable);
            }

            _log.Info("Starting Comparison of Current and Resliced Prior Series...");
            stopwatch.Restart();

            Compare(fixedFile, floatingFile, lookupTable, sliceType, resultNii);

            stopwatch.Stop();
            _log.Info($"Finished Comparison of Current and Resliced Prior Series in {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds} minutes.");
        }

        public void Normalize(string niftiFilePath, string maskFilePath, SliceType sliceType, string lookupTable)
        {
            var lut = new Bitmap(lookupTable);
            Normalize(niftiFilePath, maskFilePath, sliceType, lut.Width / 2, lut.Width / 8, lut.Width);
        }

        public void Normalize(string niftiFilePath, string maskFilePath, SliceType sliceType, int mean, int std, int widthRange)
        {
            var nim = new Nifti().ReadNifti(niftiFilePath);
            var mask = new Nifti().ReadNifti(maskFilePath);
            var normalizedNim = nim.NormalizeEachSlice(nim, sliceType, mean, std, widthRange, mask);
            File.Move(niftiFilePath, niftiFilePath.Replace(".nii", ".preNormalization.nii"));
            normalizedNim.WriteNifti(niftiFilePath);

            // TODO3: Remove when done testing
            #region Generating bmp files for create LUT
            normalizedNim.ExportSlicesToBmps(niftiFilePath.Replace(".nii", "_For_LUT"), sliceType);
            #endregion
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void CompareDicomInNiftiOut(
            string currentDicomFolder, string priorDicomFolder,
            string lookupTable, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string resultNii, string outPriorReslicedNii)
        {
            if (!File.Exists(lookupTable))
                throw new FileNotFoundException($"Unable to locate Lookup Table in the following path: {lookupTable}");

            // Generate Nifti file from Dicom and pass to ProcessNifti Method for current series
            if (!_filesystem.DirectoryIsValidAndNotEmpty(currentDicomFolder))
                throw new DirectoryNotFoundException($"Dicom folder either does not exist or contains no files: {currentDicomFolder}");

            var currentNifti = Path.Combine(Path.GetDirectoryName(currentDicomFolder), "fixed.nii");

            _log.Info("Start converting current series dicom files to Nii");
            new ImageConverter(_filesystem, _processBuilder, _config, _log).DicomToNiix(currentDicomFolder, currentNifti);
            _log.Info("Finished converting current series dicom files to Nii");

            // Generate Nifti file from Dicom and pass to ProcessNifti Method for prior series
            if (!_filesystem.DirectoryIsValidAndNotEmpty(priorDicomFolder))
                throw new DirectoryNotFoundException($"Dicom folder either does not exist or contains no files: {priorDicomFolder}");

            var priorNifti = Path.Combine(Path.GetDirectoryName(priorDicomFolder), "floating.nii");

            _log.Info("Start converting prior series dicom files to Nii");
            new ImageConverter(_filesystem, _processBuilder, _config, _log).DicomToNiix(priorDicomFolder, priorNifti);
            _log.Info("Finished converting prior series dicom files to Nii");

            CompareBrainNiftiWithReslicedBrainNifti_OutNifti(currentNifti, priorNifti, lookupTable, sliceType,
                                                             extractBrain, register, biasFieldCorrect,
                                                             resultNii, outPriorReslicedNii);
        }

        private void OutputDataReceivedInProcess(object sender, DataReceivedEventArgs e)
        {
            var consoleColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (!string.IsNullOrEmpty(e.Data) && !string.IsNullOrWhiteSpace(e.Data))
                    _log.Info($"Process stdout:{Environment.NewLine}{e.Data}");
            }
            finally
            {
                Console.ForegroundColor = consoleColor;
            }
        }
        private void ErrorOccuredInProcess(object sender, DataReceivedEventArgs e)
        {
            var consoleColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                if (!string.IsNullOrEmpty(e.Data) && !string.IsNullOrWhiteSpace(e.Data))
                    _log.Error($"Process error:{Environment.NewLine}{e.Data}");
            }
            finally
            {
                Console.ForegroundColor = consoleColor;
            }
        }
    }
}