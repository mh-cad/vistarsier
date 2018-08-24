﻿using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CAPI.ImageProcessing
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImageProcessorNew : IImageProcessorNew
    {
        private readonly IFileSystem _filesystem;
        private readonly IProcessBuilder _processBuilder;
        private readonly IImgProcConfig _config;


        public ImageProcessorNew(IFileSystem filesystem, IProcessBuilder processBuilder, IImgProcConfig config)
        {
            _filesystem = filesystem;
            _processBuilder = processBuilder;
            _config = config;
        }

        public void ExtractBrainMask(string inNii, string bseParams, string outBrainNii, string outMaskNii)
        {
            var bseExe = Path.Combine(_config.ImgProcBinFolderPath, _config.BseExeRelFilePath);

            if (!File.Exists(bseExe))
                throw new FileNotFoundException($"Unable to find {nameof(bseExe)} file: [{bseExe}]");

            var arguments = $"-i {inNii} --mask {outMaskNii} -o {outBrainNii} {bseParams}";

            if (!Directory.Exists(Path.GetDirectoryName(outBrainNii))) throw new DirectoryNotFoundException();
            if (!Directory.Exists(Path.GetDirectoryName(outMaskNii))) throw new DirectoryNotFoundException();

            _processBuilder.CallExecutableFile(bseExe, arguments);

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

        private void CreateRawXform(string outputPath, string fixedNii, string floatingNii)
        {
            var registrationFile = Path.Combine(_config.ImgProcBinFolderPath, _config.RegistrationRelFilePath);

            if (!File.Exists(registrationFile))
                throw new FileNotFoundException($"Unable to find {nameof(registrationFile)} file: [{registrationFile}]");

            var registrationParams = _config.RegistrationParams;
            var cmtkOutputDir = $@"{outputPath}\{_config.CmtkFolderName}";
            var rawForm = $@"{outputPath}\{_config.CmtkRawxformFile}";

            if (Directory.Exists(cmtkOutputDir)) Directory.Delete(cmtkOutputDir);
            _filesystem.DirectoryExistsIfNotCreate(cmtkOutputDir);

            var arguments = $@"{registrationParams} --out-matrix {rawForm} -o . {fixedNii} {floatingNii}";

            _processBuilder.CallExecutableFile(registrationFile, arguments, cmtkOutputDir);
        }
        private void CreateResultXform(string workingDir, string fixedNii, string floatingNii) // Outputs to the same folder as fixed series
        {
            var rawForm = $@"{workingDir}\{_config.CmtkRawxformFile}";
            var resultForm = $@"{workingDir}\{_config.CmtkResultxformFile}";

            var javaClasspath = _config.JavaClassPath;

            var methodname = Properties.Settings.Default.javaClassConvertCmtkXform;

            var javaArgument = $"-classpath {javaClasspath} {methodname} {fixedNii} {floatingNii} {rawForm} {resultForm}";

            _processBuilder.CallJava(javaArgument, methodname);

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

            _processBuilder.CallExecutableFile(reformatxFilePath, arguments);
        }

        public void BiasFieldCorrection(string inNii, string bfcParams, string outNii)
        {
            var bfcExe = Path.Combine(_config.ImgProcBinFolderPath, _config.BfcExeRelFilePath);

            if (!File.Exists(bfcExe))
                throw new FileNotFoundException($"Unable to find {nameof(bfcExe)} file: [{bfcExe}]");

            var arguments = $"-i {inNii} -o {outNii} {bfcParams}";

            _processBuilder.CallExecutableFile(bfcExe, arguments);
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

            if (extractBrain)
            {
                var bseParams = _config.BseParams;
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
                _filesystem.DirectoryExistsIfNotCreate(Path.GetDirectoryName(outPriorReslicedNii));
                File.Move(resliced, outPriorReslicedNii);
                floatingFile = outPriorReslicedNii;
            }

            if (biasFieldCorrect)
            {
                var bfcParams = _config.BfcParams;

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
            if (!_filesystem.DirectoryIsValidAndNotEmpty(currentDicomFolder))
                throw new DirectoryNotFoundException($"Dicom folder either does not exist or contains no files: {currentDicomFolder}");

            var currentNifti = Path.Combine(Path.GetDirectoryName(currentDicomFolder), "fixed.nii");

            new ImageConverter(_filesystem, _processBuilder, _config).DicomToNiix(currentDicomFolder, currentNifti);

            // Generate Nifti file from Dicom and pass to ProcessNifti Method for prior seires
            if (!_filesystem.DirectoryIsValidAndNotEmpty(priorDicomFolder))
                throw new DirectoryNotFoundException($"Dicom folder either does not exist or contains no files: {priorDicomFolder}");

            var priorNifti = Path.Combine(Path.GetDirectoryName(priorDicomFolder), "floating.nii");

            new ImageConverter(_filesystem, _processBuilder, _config).DicomToNiix(priorDicomFolder, priorNifti);

            CompareBrainNiftiWithReslicedBrainNifti_OutNifti(currentNifti, priorNifti, lookupTable, sliceType,
                extractBrain, register, biasFieldCorrect,
                resultNii, outPriorReslicedNii);
        }
    }
}