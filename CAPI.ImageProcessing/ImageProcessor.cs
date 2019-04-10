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
        private string _currentResliced;

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
            //_filesystem.FilesExist(lookupTablePaths);

            //var fixedFile = currentNii;
            //var floatingFile = priorNii;
            //var fixedMask = string.Empty; // even in case of no mask, an empty parameter should be passed to bias field correction method - empty param gets handled in bfc
            //var floatingMask = string.Empty; // even in case of no mask, an empty parameter should be passed to bias field correction method - empty param gets handled in bfc
            //var referenceBrain = string.Empty;
            //var referenceMask = string.Empty;

            var stopwatch1 = new Stopwatch();

            // BiasCorrection
            if (biasFieldCorrect)
            {
                _log.Info("Starting bias correction...");
                stopwatch1.Start();
                var bias1 = Task.Run(() => { return BiasCorrection.AntsN4(currentNii); });
                var bias2 = Task.Run(() => { return BiasCorrection.AntsN4(priorNii); });
                bias1.Wait();
                bias2.Wait();
                currentNii = bias1.Result;
                priorNii = bias2.Result;
                _log.Info($@"..done. [{stopwatch1.Elapsed}]");
                stopwatch1.Restart();
            }

            var currentWithSkull = currentNii;
            var priorWithSkull = priorNii;

            // Brain extraction
            if (extractBrain)
            {
                // Brain Extraction
                _log.Info("Starting brain extraction...");
                var brain1 = Task.Run(() => { return BrainExtraction.BrainSuiteBSE(currentNii); });
                var brain2 = Task.Run(() => { return BrainExtraction.BrainSuiteBSE(priorNii); });
                var brain3 = Task.Run(() => { return BrainExtraction.BrainSuiteBSE(referenceNii); }); // TODO: This can be null.
                brain1.Wait();
                brain2.Wait();
                brain3.Wait();
                currentNii = brain1.Result;
                priorNii = brain2.Result;
                referenceNii = brain3.Result;
                _log.Info($@"..done. [{stopwatch1.Elapsed}]");
                stopwatch1.Restart();
            }

            // Registration
            if (register)
            {
                var refBrain = string.IsNullOrEmpty(referenceNii) && File.Exists(referenceNii) ? referenceNii : currentNii;

                // Registration
                _log.Info("Starting registration...");
                priorNii = ImageProcessing.Registration.CMTKRegistration(priorNii, refBrain);
                if (!refBrain.Equals(currentNii)) // If we're using a third file for registration...
                {
                    priorNii = ImageProcessing.Registration.CMTKRegistration(currentNii, refBrain);
                }
                _log.Info($@"..done. [{stopwatch1.Elapsed}]");
                stopwatch1.Restart();

                priorWithSkull = Registration.CMTKResliceUsingPrevious(priorWithSkull, refBrain);
            }

            // Convert files to INifti, now that we're done with pre-processing.
            INifti currentNifti = new Nifti().ReadNifti(currentNii);
            INifti priorNifti = new Nifti().ReadNifti(priorNii);
            INifti currentNiftiWithSkull = new Nifti().ReadNifti(currentWithSkull);
            INifti priorNiftiWithSkull = new Nifti().ReadNifti(priorWithSkull);

            // Normalize
            stopwatch1.Restart();
            _log.Info("Starting normalization...");
            currentNifti = Normalization.ZNormalize(currentNifti, priorNifti);
            _log.Info($@"..done. [{stopwatch1.Elapsed}]");
            stopwatch1.Restart();

            // Compare 
            _log.Info("Starting compare...");
            var increaseTask = Task.Run(() => { return Compare.CompareMSLesionIncrease(currentNifti, priorNifti); });
            var decreaseTask = Task.Run(() => { return Compare.CompareMSLesionDecrease(currentNifti, priorNifti); });
            increaseTask.Wait();
            decreaseTask.Wait();
            var increaseNifti = increaseTask.Result;
            var decreaseNifti = decreaseTask.Result;
            _log.Info($@"..done. [{stopwatch1.Elapsed}]");
            stopwatch1.Restart();

            // Write the prior resliced file.
            priorNiftiWithSkull.WriteNifti(outPriorReslicedNii);

            //Overlay increase and decrease values:
            _log.Info("Generating RGB overlays...");
            var overlayTask1 = Task.Run(() => { return currentNiftiWithSkull.AddOverlay(increaseNifti); });
            var overlayTask2 = Task.Run(() => { return currentNiftiWithSkull.AddOverlay(decreaseNifti); });
            overlayTask1.Wait();
            overlayTask2.Wait();
            var increaseNiftiRGB = overlayTask1.Result;
            var decreaseNiftiRGB = overlayTask2.Result;

            // Write files out to disk.
            var writeTask1 = Task.Run(() => { increaseNiftiRGB.WriteNifti(resultNiis[0]); });
            var writeTask2 = Task.Run(() => { decreaseNiftiRGB.WriteNifti(resultNiis[1]); });
            writeTask1.Wait();
            writeTask2.Wait();

            _log.Info($@"..done. [{stopwatch1.Elapsed}]");

        }

        private static Bitmap[] getSlices(INifti mainNifti, INifti overlayNifti, SliceType sliceType)
        {
            mainNifti.GetDimensions(sliceType, out int width, out int height, out int nSlices);
            Bitmap[] output = new Bitmap[nSlices];

            for (int i = 0; i < nSlices; ++i)
            {
                // Draw overlay nifti over main nifti
                Bitmap slice = mainNifti.GetSlice(i, sliceType);
                Bitmap overlay = overlayNifti.GetSlice(i, sliceType);
                Graphics g = Graphics.FromImage((Image)slice);
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.DrawImage(overlay, new Point(0, 0));
                g.Save();
                output[i] = slice;
            }
            return output;
        }

    }
}
