using CAPI.Common.Abstractions.Config;
using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CAPI.ImageProcessing
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImageProcessor : IImageProcessor
    {
        private readonly IFileSystem _filesystem;
        private readonly IProcessBuilder _processBuilder;
        private readonly IImgProcConfig _config;
        private readonly ILog _log;
        private string _referenceSeriesDicomFolder;
        private bool _referenceSeriesExists;

        public ImageProcessor(IFileSystem filesystem, IProcessBuilder processBuilder,
                              IImgProcConfig config, ILog log)
        {
            _filesystem = filesystem;
            _processBuilder = processBuilder;
            _config = config;
            _log = log;
        }


        /// <summary>
        /// Entry point to this class
        /// </summary>
        /// <param name="currentDicomFolder">Directory path to current series dicom folder</param>
        /// <param name="priorDicomFolder">Directory path to prior series dicom folder</param>
        /// <param name="lookupTablePaths">array containing filepath to each lookup table used to compare current and prior series</param>
        /// <param name="sliceType">Input set of series can be in either Sagittal, Axial or Coronal plane</param>
        /// <param name="referenceSeriesDicomFolder">Patient has previous processed cases and this series was as reference frame of reference</param>
        /// <param name="extractBrain">true if brain extraction required</param>
        /// <param name="register">true if current and prior are not registered yet and need to be registered</param>
        /// <param name="biasFieldCorrect">true if Bias Field Correction is required</param>
        /// <param name="resultNiis">array including filepath to each result nifti file (Not yet created)</param>
        /// <param name="outPriorReslicedNii">registered prior series which has been resliced to align with current series</param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void CompareDicomInNiftiOut(
            string currentDicomFolder, string priorDicomFolder, string referenceSeriesDicomFolder,
            string[] lookupTablePaths, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string[] resultNiis, string outPriorReslicedNii)
        {
            _referenceSeriesDicomFolder = referenceSeriesDicomFolder ?? string.Empty;
            _referenceSeriesExists = _filesystem.DirectoryIsValidAndNotEmpty(_referenceSeriesDicomFolder);

            foreach (var lookupTablePath in lookupTablePaths)
                if (!File.Exists(lookupTablePath))
                    throw new FileNotFoundException($"Unable to locate Lookup Table in the following path: {lookupTablePath}");

            // Generate Nifti file from Dicom and pass to ProcessNifti Method for current series
            if (!_filesystem.DirectoryIsValidAndNotEmpty(currentDicomFolder))
                throw new DirectoryNotFoundException($"Dicom folder either does not exist or contains no files: {currentDicomFolder}");

            var currentNifti = Path.Combine(Path.GetDirectoryName(currentDicomFolder), "fixed.nii");

            var task1 = Task.Run(() =>
            {
                _log.Info("Start converting current series dicom files to Nii");
                new ImageConverter(_filesystem, _processBuilder, _config, _log).DicomToNiix(currentDicomFolder, currentNifti);
                _log.Info("Finished converting current series dicom files to Nii");
            });

            // Generate Nifti file from Dicom and pass to ProcessNifti Method for prior series
            if (!_filesystem.DirectoryIsValidAndNotEmpty(priorDicomFolder))
                throw new DirectoryNotFoundException($"Dicom folder either does not exist or contains no files: {priorDicomFolder}");

            var priorNifti = Path.Combine(Path.GetDirectoryName(priorDicomFolder), "floating.nii");

            var task2 = Task.Run(() =>
            {
                _log.Info("Start converting prior series dicom files to Nii");
                new ImageConverter(_filesystem, _processBuilder, _config, _log).DicomToNiix(priorDicomFolder, priorNifti);
                _log.Info("Finished converting prior series dicom files to Nii");
            });

            // Generate Nifti file from Dicom and pass to ProcessNifti Method for reference series
            var referenceNifti = _filesystem.DirectoryIsValidAndNotEmpty(referenceSeriesDicomFolder) ?
                                 Path.Combine(Path.GetDirectoryName(_referenceSeriesDicomFolder), "reference.nii") : string.Empty;

            var task3 = Task.Run(() =>
            {
                if (!_filesystem.DirectoryIsValidAndNotEmpty(referenceSeriesDicomFolder))
                {
                    _log.Info("No reference series dicom files exist to convert to Nii");
                    return;
                }
                _log.Info("Start converting reference series dicom files to Nii");
                new ImageConverter(_filesystem, _processBuilder, _config, _log).DicomToNiix(referenceSeriesDicomFolder, referenceNifti);
                _log.Info("Finished converting reference series dicom files to Nii");
            });

            task1.Wait();
            task2.Wait();
            task3.Wait();

            // When dicom files are converted into nifti files they are passed to do the actual processing
            ExtractBrainRegisterAndCompare(currentNifti, priorNifti, referenceNifti, lookupTablePaths, sliceType,
                                           extractBrain, register, biasFieldCorrect,
                                           resultNiis, outPriorReslicedNii);
        }


        /// <summary>
        /// Main method responsible for calling other methods to Extract Brain from current and prior series, Register the two, BFC and Normalize and then compare using a lookup table
        /// </summary>
        /// <param name="currentNii">current series nifti file path</param>
        /// <param name="priorNii">prior series nifti file path</param>
        /// <param name="referenceNii">reference series nifti file path (If exists, used for universal frame of reference)</param>
        /// <param name="lookupTablePaths">bmp files mapping current and prior comparison result colors</param>
        /// <param name="sliceType">Sagittal, Axial or Coronal</param>
        /// <param name="extractBrain">to do skull stripping or not</param>
        /// <param name="register">to register or not</param>
        /// <param name="biasFieldCorrect">to perform bias field correction or not</param>
        /// <param name="resultNiis">end result output nifti files path</param>
        /// <param name="outPriorReslicedNii">resliced prior series nifti file path</param>
        public void ExtractBrainRegisterAndCompare(
            string currentNii, string priorNii, string referenceNii, string[] lookupTablePaths, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string[] resultNiis, string outPriorReslicedNii)
        {
            _filesystem.FilesExist(new[] { currentNii, priorNii });
            _filesystem.FilesExist(lookupTablePaths);

            var fixedFile = currentNii;
            var floatingFile = priorNii;
            var fixedMask = string.Empty; // even in case of no mask, an empty parameter should be passed to bias field correction method - empty param gets handled in bfc
            var floatingMask = string.Empty; // even in case of no mask, an empty parameter should be passed to bias field correction method - empty param gets handled in bfc
            var referenceBrain = string.Empty;
            var referenceMask = string.Empty;

            var stopwatch1 = new Stopwatch();
            var stopwatch2 = new Stopwatch();
            var stopwatch3 = new Stopwatch();

            // Brain Extraction
            if (extractBrain)
            {
                var bseParams = _config.BseParams;

                var task1 = Task.Run(() =>
                {
                    var fixedBrain = currentNii.Replace(".nii", ".brain.nii");
                    fixedMask = currentNii.Replace(".nii", ".mask.nii");
                    _log.Info("Starting EXTRACTING BRAIN for Current series...");
                    stopwatch1.Start();
                    // ReSharper disable once AccessToModifiedClosure

                    ExtractBrainMask(fixedFile, bseParams, fixedBrain, fixedMask);

                    stopwatch1.Stop();
                    _log.Info($"Finished extracting brain for Current series in {stopwatch1.Elapsed.Minutes}:{stopwatch1.Elapsed.Seconds:D2} minutes.");
                    fixedFile = fixedBrain;
                });

                var task2 = Task.Run(() =>
                {
                    var floatingBrain = priorNii.Replace(".nii", ".brain.nii");
                    floatingMask = priorNii.Replace(".nii", ".mask.nii");
                    _log.Info("Starting EXTRACTING BRAIN for Prior series...");
                    stopwatch2.Start();
                    // ReSharper disable once AccessToModifiedClosure

                    ExtractBrainMask(floatingFile, bseParams, floatingBrain, floatingMask);

                    stopwatch2.Stop();
                    _log.Info($"Finished extracting brain for Prior series in {stopwatch2.Elapsed.Minutes}:{stopwatch2.Elapsed.Seconds:D2} minutes.");
                    floatingFile = floatingBrain;
                });

                var task3 = Task.Run(() =>
                {
                    if (string.IsNullOrEmpty(referenceNii) || !File.Exists(referenceNii)) return;
                    referenceBrain = referenceNii.Replace(".nii", ".brain.nii");
                    referenceMask = referenceNii.Replace(".nii", ".mask.nii");
                    _log.Info("Starting EXTRACTING BRAIN for Reference series...");
                    stopwatch3.Start();
                    // ReSharper disable once AccessToModifiedClosure

                    ExtractBrainMask(referenceNii, bseParams, referenceBrain, referenceMask);

                    stopwatch3.Stop();
                    _log.Info($"Finished extracting brain for Reference series in {stopwatch3.Elapsed.Minutes}:{stopwatch3.Elapsed.Seconds:D2} minutes.");
                });

                task1.Wait();
                task2.Wait();
                task3.Wait();
            }

            // Registration
            if (register)
            {
                var refBrain = string.IsNullOrEmpty(referenceBrain) && File.Exists(referenceBrain) ? referenceBrain : fixedFile;
                var refMask = string.IsNullOrEmpty(referenceMask) && File.Exists(referenceMask) ? referenceMask : fixedMask;

                var task1 = Task.Run(() =>
                {
                    if (_referenceSeriesExists)
                    {
                        _log.Info("No reference studies exist for patient to register current series against.");
                        return;
                    };
                    _log.Info("Starting REGISTRATION of current series against reference series...");
                    var reslicedCurrentBrain = fixedFile.Replace(".nii", ".resliced.nii");
                    stopwatch1.Restart();

                    Registration(refBrain, fixedFile, reslicedCurrentBrain, "brain");

                    stopwatch1.Stop();

                    if (!File.Exists(reslicedCurrentBrain))
                        throw new FileNotFoundException(
                            $"Registration process failed to created resliced file {outPriorReslicedNii}");

                    _log.Info($"Finished registration of current series against reference series in {stopwatch1.Elapsed.Minutes}:{stopwatch1.Elapsed.Seconds:D2} minutes.");

                    fixedFile = reslicedCurrentBrain;
                });

                var task2 = Task.Run(() =>
                {
                    if (!_referenceSeriesExists) return;
                    _log.Info("Starting REGISTRATION of current MASK against reference MASK...");
                    var reslicedCurrentMask = fixedMask.Replace(".mask.nii", ".resliced.mask.nii");
                    stopwatch2.Restart();

                    Registration(refMask, fixedMask, reslicedCurrentMask, "mask");

                    stopwatch2.Stop();

                    if (!File.Exists(reslicedCurrentMask))
                        throw new FileNotFoundException($"Registration process failed to created resliced mask file {reslicedCurrentMask}");

                    _log.Info($"Finished registration of current MASK against reference MASK in {stopwatch2.Elapsed.Minutes}:{stopwatch2.Elapsed.Seconds:D2} minutes.");

                    fixedMask = reslicedCurrentMask;
                });

                var task3 = Task.Run(() =>
                {
                    _log.Info("Starting REGISTRATION of prior series against reference series...");
                    var reslicedPriorBrain = priorNii.Replace(".nii", ".resliced.nii");
                    stopwatch1.Restart();

                    Registration(refBrain, floatingFile, reslicedPriorBrain, "brain");

                    stopwatch1.Stop();
                    if (!File.Exists(reslicedPriorBrain))
                        throw new FileNotFoundException(
                            $"Registration process failed to created resliced file {outPriorReslicedNii}");
                    _log.Info(
                        $"Finished registration of prior series against reference series in {stopwatch1.Elapsed.Minutes}:{stopwatch1.Elapsed.Seconds:D2} minutes.");
                    // Move resliced prior to desired out file
                    _filesystem.DirectoryExistsIfNotCreate(Path.GetDirectoryName(outPriorReslicedNii));
                    File.Move(reslicedPriorBrain, outPriorReslicedNii);
                    floatingFile = outPriorReslicedNii;
                });

                var task4 = Task.Run(() =>
                {
                    _log.Info("Starting REGISTRATION of prior MASK against reference MASK...");
                    var reslicedPriorMask = floatingMask.Replace(".mask.nii", ".resliced.mask.nii");
                    stopwatch2.Restart();

                    Registration(refMask, floatingMask, reslicedPriorMask, "mask");

                    stopwatch2.Stop();
                    if (!File.Exists(reslicedPriorMask))
                        throw new FileNotFoundException($"Registration process failed to created resliced mask file {reslicedPriorMask}");
                    _log.Info($"Finished registration of prior MASK against reference MASK in {stopwatch2.Elapsed.Minutes}:{stopwatch2.Elapsed.Seconds:D2} minutes.");

                    floatingMask = reslicedPriorMask;
                });

                task1.Wait();
                task2.Wait();
                task3.Wait();
                task4.Wait();
            }

            // Bias Field Correction
            if (biasFieldCorrect)
            {
                var bfcParams = _config.BfcParams;

                var task1 = Task.Run(() =>
                {
                    var fixedBfc = currentNii.Replace(".nii", ".bfc.nii");
                    _log.Info("Starting Bias Field Correction for Current series...");
                    stopwatch1.Restart();

                    BiasFieldCorrection(fixedFile, fixedMask, bfcParams, fixedBfc);

                    stopwatch1.Stop();
                    _log.Info($"Finished Bias Field Correction of Current series in {stopwatch1.Elapsed.Minutes}:{stopwatch1.Elapsed.Seconds:D2} minutes.");
                    fixedFile = fixedBfc;
                });

                var task2 = Task.Run(() =>
                {
                    var floatingBfc = priorNii.Replace(".nii", ".bfc.nii");
                    _log.Info("Starting Bias Field Correction for Prior series...");
                    stopwatch2.Restart();

                    BiasFieldCorrection(floatingFile, floatingMask, bfcParams, floatingBfc);

                    stopwatch2.Stop();
                    if (File.Exists(outPriorReslicedNii)) File.Move(outPriorReslicedNii, outPriorReslicedNii.Replace(".nii", ".preBfc.nii"));
                    if (File.Exists(floatingBfc) && !File.Exists(outPriorReslicedNii))
                        File.Move(floatingBfc, outPriorReslicedNii);
                    floatingFile = outPriorReslicedNii;

                    _log.Info($"Finished Bias Field Correction for Prior series in {stopwatch2.Elapsed.Minutes}:{stopwatch2.Elapsed.Seconds:D2} minutes.");
                });
                task1.Wait();
                task2.Wait();
            }

            // Normalize
            foreach (var lookupTable in lookupTablePaths)
            {
                const bool normalize = true;
                var lookupTableName = Path.GetFileNameWithoutExtension(lookupTable);
                var fixedPrenorm = fixedFile.EndsWith(".pre-norm.nii") ? fixedFile : fixedFile.Replace(".nii", ".pre-norm.nii");
                var floatingPrenorm = floatingFile.EndsWith(".pre-norm.nii") ? floatingFile : floatingFile.Replace(".nii", ".pre-norm.nii");

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (normalize && extractBrain)
                {
                    var task1 = Task.Run(() =>
                    {
                        if (!File.Exists(fixedPrenorm)) File.Move(fixedFile, fixedPrenorm);
                        if (fixedFile.ToLower().Contains(".pre-norm")) fixedFile = fixedFile.Replace(".pre-norm", "");
                        fixedFile = fixedFile.Replace(".nii", $".norm-{lookupTableName}.nii");
                        File.Copy(fixedPrenorm, fixedFile);
                        _log.Info("Starting NORMALIZATION of current series...");
                        stopwatch1.Restart();
                        Normalize(fixedFile, fixedMask, sliceType, lookupTable);
                        stopwatch1.Stop();
                        _log.Info($"Finished normalization of current series in {stopwatch1.Elapsed.Minutes}:{stopwatch1.Elapsed.Seconds:D2} minutes.");
                    });

                    var task2 = Task.Run(() =>
                    {
                        if (!File.Exists(floatingPrenorm)) File.Move(floatingFile, floatingPrenorm);
                        if (floatingFile.ToLower().Contains(".pre-norm")) floatingFile = floatingFile.Replace(".pre-norm", "");
                        floatingFile = floatingFile.Replace(".nii", $".norm-{lookupTableName}.nii");
                        File.Copy(floatingPrenorm, floatingFile);
                        _log.Info("Starting NORMALIZATION of prior series...");
                        stopwatch2.Restart();
                        Normalize(floatingFile, floatingMask, sliceType, lookupTable);
                        stopwatch2.Stop();
                        _log.Info($"Finished normalization of prior series in {stopwatch2.Elapsed.Minutes}:{stopwatch2.Elapsed.Seconds:D2} minutes.");
                    });
                    task1.Wait();
                    task2.Wait();
                }

                _log.Info($"Starting Comparison of Current and Resliced Prior Series using lookup table {lookupTableName} ...");
                stopwatch1.Restart();

                var resultNii = GetMatchingResultForLut(lookupTable, resultNiis);

                Compare(fixedFile, floatingFile, lookupTable, sliceType, resultNii);
                //Compare(currentNii, fixedFile, fixedMask, floatingFile, lookupTable, sliceType, resultNii);


                stopwatch1.Stop();
                _log.Info($"Finished Comparison of Current and Resliced Prior Series in {stopwatch1.Elapsed.Minutes}:{stopwatch1.Elapsed.Seconds:D2} minutes.");

                // prepare for next comparison
                fixedFile = fixedPrenorm;
                floatingFile = floatingPrenorm;
            }

            // TODO1: Remove when done experimenting
            #region Experimental

            if (false)
            {
                try
                {
                    const bool ignoreErrors = false;
                    var nictaPosResultFilePath = resultNiis.FirstOrDefault(f => f.ToLower().Contains("nictapos"));
                    var nictaNegResultFilePath = resultNiis.FirstOrDefault(f => f.ToLower().Contains("nictaneg"));
                    var colormapConfigFilePath = "D:\\RAPPreprocess\\colormap.config";
                    if (!File.Exists(colormapConfigFilePath)) colormapConfigFilePath = "D:\\Capi-Tests\\colormap.config";
                    CompareUsingNictaCode(fixedFile, floatingFile, fixedMask,
                        nictaPosResultFilePath, nictaNegResultFilePath, colormapConfigFilePath, ignoreErrors);
                }
                catch (Exception ex)
                {
                    // comment out "throw;" to ignore NICTA errors
                    throw;
                }
            }

            #endregion

            File.Move(floatingFile, outPriorReslicedNii);
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

        public void Registration(string refNii, string priorNii, string outPriorReslicedNii, string seriesType)
        {
            var outputPath = Directory.GetParent(Path.GetDirectoryName(refNii)).FullName;
            outputPath = Path.Combine(outputPath, "Registration");
            _filesystem.DirectoryExistsIfNotCreate(outputPath);

            var fixedNiiFileName = Path.GetFileNameWithoutExtension(refNii);
            var cmtkOutputDir = $@"{outputPath}\{_config.CmtkFolderName}-{fixedNiiFileName}";

            Register(refNii, priorNii, cmtkOutputDir);

            Reslice(refNii, priorNii, outPriorReslicedNii, cmtkOutputDir);
        }

        private void Register(string fixedNii, string floatingNii, string cmtkOutputDir)
        {
            var registrationFile = Path.Combine(_config.ImgProcBinFolderPath, _config.RegistrationRelFilePath);

            if (!File.Exists(registrationFile))
                throw new FileNotFoundException($"Unable to find {nameof(registrationFile)} file: [{registrationFile}]");

            var registrationParams = _config.RegistrationParams;

            if (Directory.Exists(cmtkOutputDir)) Directory.Delete(cmtkOutputDir, true);
            _filesystem.DirectoryExistsIfNotCreate(cmtkOutputDir);

            var arguments = $@"{registrationParams} -o . {fixedNii} {floatingNii}";

            _processBuilder.CallExecutableFile(registrationFile, arguments, cmtkOutputDir, OutputDataReceivedInProcess, ErrorOccuredInProcess);
        }
        private void Reslice(string fixedNii, string floatingNii, string floatingResliced, string cmtkOutputDir)
        {
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

            var resultFolderPath = Path.GetDirectoryName(resultNiiFile);
            _filesystem.DirectoryExistsIfNotCreate(resultFolderPath);
            if (!Directory.Exists(resultFolderPath)) throw new DirectoryNotFoundException($"Results folder was not created [{resultFolderPath}]");
            File.Copy(lookupTableFile, Path.Combine(resultFolderPath, Path.GetFileName(lookupTableFile) ?? throw new InvalidOperationException()));

            result.WriteNifti(resultNiiFile);
        }

        // TODO1: Remove when done experimenting
        #region Experimental
        public void CompareUsingNictaCode(string fixedBrainFile, string floatingBrainFile, string fixedMaskFile,
                                          string nictaPosResultFilePath, string nictaNegResultFilePath, string colormapConfigFilePath,
                                          bool ignoreErrors)
        {
            var javaClasspath = _config.JavaClassPath;
            const string methodName = "au.com.nicta.preprocess.main.ColorMap";
            var fixedDicom = Path.Combine(Path.GetDirectoryName(fixedBrainFile) ?? "FixedFileParent", "Dicom");

            var outputDir = Directory.GetParent(Path.GetDirectoryName(fixedBrainFile)).FullName;
            outputDir = Path.Combine(outputDir, "Experimental");
            if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
            _filesystem.DirectoryExistsIfNotCreate(outputDir);

            CreateSubtractFiles(fixedBrainFile, floatingBrainFile, fixedMaskFile, outputDir);

            var subtractDarkToBrightFilePath = Path.Combine(outputDir, "diff_dark_in_floating_to_bright_in_fixed.nii");
            var subtractBrightToDarkFilePath = Path.Combine(outputDir, "diff_bright_in_floating_to_dark_in_fixed.nii");

            var fixedFile = Dicom2NiiUsingDcm2Nii(fixedDicom, fixedBrainFile);
            fixedMaskFile = ReorderVoxelsLpi2Ail(fixedMaskFile);
            subtractDarkToBrightFilePath = ReorderVoxelsLpi2Ail(subtractDarkToBrightFilePath);
            subtractBrightToDarkFilePath = ReorderVoxelsLpi2Ail(subtractBrightToDarkFilePath);

            var nictaPosResultImagesFolderPath = Path.Combine(Path.GetDirectoryName(nictaPosResultFilePath) ?? "ResultParentFolder", "NictaPos_Images");
            var nictaNegResultImagesFolderPath = Path.Combine(Path.GetDirectoryName(nictaNegResultFilePath) ?? "ResultParentFolder", "NictaNeg_Images");

            _filesystem.DirectoryExistsIfNotCreate(Path.GetDirectoryName(nictaPosResultFilePath));
            _filesystem.DirectoryExistsIfNotCreate(Path.GetDirectoryName(nictaNegResultFilePath));

            var javaArgument = $"-Xmx1g -classpath {javaClasspath} {methodName} \"{colormapConfigFilePath}\" \"{nictaNegResultImagesFolderPath}\" " +
                               $"\"{fixedFile}\" \"{fixedDicom}\" \"{fixedMaskFile}\" " +
                               $"\"{subtractDarkToBrightFilePath}\" \"{subtractBrightToDarkFilePath}\" negative";

            _processBuilder.CallJava(_config.JavaExeFilePath, javaArgument, methodName, "", OutputDataReceivedInProcess);

            javaArgument = javaArgument.Replace(" negative", " positive");
            javaArgument = javaArgument.Replace(nictaNegResultImagesFolderPath, nictaPosResultImagesFolderPath);

            _processBuilder.CallJava(_config.JavaExeFilePath, javaArgument, methodName, "", OutputDataReceivedInProcess);
        }

        private string Dicom2NiiUsingDcm2Nii(string fixedDicom, string dcm2NiixOutFilePath)
        {
            var dcm2NiixExe = Path.Combine(_config.ImgProcBinFolderPath, _config.Dcm2NiiExeRelFilePath);
            var dcm2NiiExe = dcm2NiixExe.Replace("dcm2niix", "dcm2nii");

            if (!File.Exists(dcm2NiiExe))
                throw new FileNotFoundException($"Unable to find {nameof(dcm2NiiExe)} file: [{dcm2NiiExe}]");
            if (!File.Exists(dcm2NiixOutFilePath))
                throw new FileNotFoundException($"Unable to find {nameof(dcm2NiixOutFilePath)} file: [{dcm2NiixOutFilePath}]");

            var tmpDir = $@"{Path.GetDirectoryName(dcm2NiixOutFilePath)}\tmp";
            if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            _filesystem.DirectoryExistsIfNotCreate(tmpDir);

            const string options = "-g N - n Y - r N";
            var process = _processBuilder.CallExecutableFile(dcm2NiiExe, $"-o {tmpDir} {options} {fixedDicom}");
            process.OutputDataReceived += OutputDataReceivedInProcess;
            process.ErrorDataReceived += ErrorOccuredInProcess;
            process.WaitForExit();

            if (!Directory.Exists(tmpDir))
                throw new DirectoryNotFoundException("dcm2nii output folder does not exist!");
            var outFiles = Directory.GetFiles(tmpDir);
            var nim = outFiles.Single(f => !Path.GetFileName(f).StartsWith("o"));
            var dcm2niiOutFilePath = dcm2NiixOutFilePath.Replace(".nii", ".dcm2nii.nii");
            dcm2niiOutFilePath = dcm2niiOutFilePath.Replace(".bfc", "");
            File.Move(nim, dcm2niiOutFilePath);

            Directory.Delete(tmpDir, true);

            return dcm2niiOutFilePath;
        }

        private static string ReorderVoxelsLpi2Ail(string niftiFilePath)
        {
            var nim = new Nifti().ReadNifti(niftiFilePath);
            nim.ReorderVoxelsLpi2Ail();
            var reorientedFilepath = niftiFilePath.Replace(".nii", ".ail.nii");
            nim.WriteNifti(reorientedFilepath);
            return reorientedFilepath;
        }

        private void CreateSubtractFiles(string fixedFile, string floatingFile, string fixedMaskFile, string outputDir)
        {
            var javaClasspath = _config.JavaClassPath;
            const string methodName = "au.com.nicta.preprocess.main.MsProgression";

            var javaArgument = $"-Xmx1g -classpath {javaClasspath} {methodName} \"{outputDir}\" \"{fixedFile}\" \"{floatingFile}\" \"{fixedMaskFile}\" 0";

            _processBuilder.CallJava(_config.JavaExeFilePath, javaArgument, methodName, "", OutputDataReceivedInProcess);
        }
        #endregion

        private static string GetMatchingResultForLut(string lookupTablePath, IEnumerable<string> resultNiis)
        {
            var lutName = Path.GetFileNameWithoutExtension(lookupTablePath);
            return resultNiis.FirstOrDefault(r => Directory.GetParent(r).Name
                .Equals(lutName, StringComparison.CurrentCultureIgnoreCase));
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
            normalizedNim.WriteNifti(niftiFilePath);

            // TODO3: Remove when done experimenting
            #region Experimental
            #region Generating bmp files for reverse-generating LUT
            normalizedNim.ExportSlicesToBmps(niftiFilePath.Replace(".nii", "_For_LUT"), sliceType);
            #endregion
            #endregion
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
            // Swallow log4j initialisation warnings
            if (e?.Data == null || string.IsNullOrEmpty(e.Data) || string.IsNullOrWhiteSpace(e.Data) || e.Data.ToLower().Contains("log4j")) return;

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
