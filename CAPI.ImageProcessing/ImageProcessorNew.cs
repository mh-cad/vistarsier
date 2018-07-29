using CAPI.Common.Services;
using CAPI.ImageProcessing.Abstraction;
using System;
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

            new ImageConverter().DicomToNiix(currentDicomFolder, currentNifti);

            // Generate Nifti file from Dicom and pass to ProcessNifti Method for prior seires
            if (!FileSystem.DirectoryIsValidAndNotEmpty(priorDicomFolder))
                throw new DirectoryNotFoundException($"Dicom folder either does not exist or contains no files: {priorDicomFolder}");

            var priorNifti = Path.Combine(Path.GetDirectoryName(priorDicomFolder), "floating.nii");

            new ImageConverter().DicomToNiix(priorDicomFolder, priorNifti);

            CompareBrainNiftiWithReslicedBrainNifti_OutNifti(currentNifti, priorNifti, lookupTable, sliceType,
                extractBrain, register, biasFieldCorrect,
                resultNii, outPriorReslicedNii);
        }
    }
}