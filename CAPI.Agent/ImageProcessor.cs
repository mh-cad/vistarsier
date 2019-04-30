﻿using CAPI.Agent.Abstractions.Models;
using CAPI.Agent.Models;
using CAPI.Config;
using CAPI.Dicom.Abstractions;
using CAPI.Common;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IImageProcessor = CAPI.ImageProcessing.Abstraction.IImageProcessor;
using SliceType = CAPI.NiftiLib.SliceType;

namespace CAPI.Agent
{
    /// <summary>
    /// Compares current and prior sereis and saves results into filesystem or sends off to a dicom node
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImageProcessor : Abstractions.IImageProcessor
    {
        private readonly IDicomServices _dicomServices;
        private readonly IImageProcessor _imgProc;
        private readonly IImageProcessingFactory _imgProcFactory;
        private readonly ILog _log;
        private readonly IImgProcConfig _imgProcConfig;
        private readonly AgentRepository _context;

        private const string ResultsFolderName = "Results";
        private const string ResultsFileName = "result.nii";
        private const string ImagesFolderSuffix = "_Images";

        public ImageProcessor(IDicomServices dicomServices, IImageProcessingFactory imgProcFactory,
                              IImgProcConfig imgProcConfig, AgentRepository context)
        {
            _dicomServices = dicomServices;
            _imgProcFactory = imgProcFactory;
            _log = Log.GetLogger();
            _imgProcConfig = imgProcConfig;
            _imgProc = imgProcFactory.CreateImageProcessor(imgProcConfig);
            _context = context;
        }

        public IJobResult[] CompareAndSaveLocally(
            string currentDicomFolder, string priorDicomFolder, string referenceDicomFolder,
            SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string outPriorReslicedDicom,
            string resultsDicomSeriesDescription, string priorReslicedDicomSeriesDescription)
        {
            // Working directory.
            var workingDir = Directory.GetParent(outPriorReslicedDicom).FullName;
            // We're getting result files based on paths for look up tables??? //TODO deconvolve...
            //var resultNiis = BuildResultNiftiPathsFromLuts(lookupTablePaths, workingDir).ToArray();
            // Create results folder
            var allResultsFolder = Path.Combine(workingDir, ResultsFolderName);
            Directory.CreateDirectory(allResultsFolder);
            string[] resultNiis = { Path.Combine(allResultsFolder, "increase.nii"), Path.Combine(allResultsFolder, "decrease.nii") };


            var outPriorReslicedNii = outPriorReslicedDicom + ".nii";

            var dicomFilePath = Directory.GetFiles(currentDicomFolder)[0];
            var patientId = _dicomServices.GetPatientIdFromDicomFile(dicomFilePath);
            var job = _context.Jobs.LastOrDefault(j => j.PatientId == patientId);


            // Generate Nifti file from Dicom and pass to ProcessNifti Method for current series.
            _log.Info($@"Start converting series dicom files to Nii");
            var task1 = Task.Run(() => { return _imgProc.DicomToNifti(currentDicomFolder); });
            var task2 = Task.Run(() => { return _imgProc.DicomToNifti(priorDicomFolder); });
            var task3 = Task.Run(() => { return _imgProc.DicomToNifti(referenceDicomFolder); });
            task1.Wait();
            task2.Wait();
            task3.Wait();

            var currentNifti = task1.Result;
            var priorNifti = task1.Result;
            var referenceNifti = task1.Result;
            _log.Info($@"Finished converting series dicom files to Nii");

            // Process Nifti files.
            _imgProc.MSLesionCompare(
                currentNifti, priorNifti, referenceNifti,
                extractBrain, register, biasFieldCorrect,
                resultNiis, outPriorReslicedNii);

            
            if (job != null)
            {
                var jobToUpdate = _context.Jobs.FirstOrDefault(j => j.Id == job.Id);
                if (jobToUpdate == null) throw new Exception($"No job was found in database with id: [{job.Id}]");

                // Check if there is a reference series from last processes for the patient, if not set the current series as reference for future
                GetReferenceSeries(referenceDicomFolder, currentDicomFolder, out var refStudyUid, out var refSeriesUid);
                jobToUpdate.WriteStudyAndSeriesIdsToReferenceSeries(refStudyUid, refSeriesUid);
                _context.Jobs.Update(jobToUpdate);
                _context.SaveChanges();
            }

            // "current" study dicom headers are used as the "prior resliced" series gets sent as part of the "current" study
            // prior study date will be added to the end of Series Description tag
            var task = Task.Run(() =>
            {
                _log.Info("Start converting resliced prior series back to Dicom");

                var priorStudyDate = GetStudyDateFromDicomFile(Directory.GetFiles(priorDicomFolder).FirstOrDefault());
                var priorStudyDescBase = string.IsNullOrEmpty(priorReslicedDicomSeriesDescription) ?
                    _imgProcConfig.PriorReslicedDicomSeriesDescription :
                    priorReslicedDicomSeriesDescription;
                var priorStudyDescription = $"{priorStudyDescBase} {priorStudyDate}";

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                ConvertNiftiToDicom(referenceNifti, outPriorReslicedDicom, sliceType,
                                    currentDicomFolder, priorStudyDescription, "", referenceDicomFolder);

                UpdateSeriesDescriptionForAllFiles(outPriorReslicedDicom, priorStudyDescription);

                stopwatch.Stop();
                _log.Info("Finished Converting resliced prior series back to Dicom in " +
                          $"{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds:D2} minutes.");
            });

            // TODO1: Remove when done experimenting
            #region "Experimental"
            //ConvertCurrentBfcedToDicom(outPriorReslicedNiiFile, currentDicomFolder, sliceType, referenceDicomFolder);
            #endregion

            var results = new List<IJobResult>();
            foreach (var resultNii in resultNiis)
            {
                _log.Info("Start converting results back to Dicom");

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var resultsSeriesDescription = string.IsNullOrEmpty(resultsDicomSeriesDescription)
                    ? _imgProcConfig.ResultsDicomSeriesDescription
                    : resultsDicomSeriesDescription;

                string dicomFolderPath;
                var lutFilePath = string.Empty;

                if (resultNii.ToLower().Contains("nicta"))
                {
                    var resultFolder = Path.GetDirectoryName(resultNii);
                    dicomFolderPath = Path.Combine(resultFolder ?? throw new InvalidOperationException($"unable to get folder [{resultNii}] resides in."),
                        "Result_Dicom");
                    var imagesFolder = Directory.GetDirectories(resultFolder)
                        .FirstOrDefault(d => d.ToLower().Contains("images"));

                    var seriesDescription = resultNii.ToLower().Contains("nictapos") ? "Nicta Increased Signal" : "Nicta Decreased Signal";
                    ConvertBmpsToDicom(imagesFolder, dicomFolderPath, currentDicomFolder, sliceType, seriesDescription, true);
                }
                else
                {
                    dicomFolderPath = resultNii.Replace(".nii", "");
                    var description = dicomFolderPath.Contains("increase") ? "increase" : "decrease";
                    //lutFilePath = GetLookupTableForResult(resultNii, lookupTablePaths);
                    //var lutFileName = Path.GetFileNameWithoutExtension(lutFilePath);
                    ConvertNiftiToDicom(resultNii, dicomFolderPath, sliceType, currentDicomFolder,
                                        $"{resultsSeriesDescription}-{description}", "", referenceDicomFolder);
                }

                stopwatch.Stop();
                _log.Info("Finished converting results back to Dicom in " +
                          $"{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds:D2} minutes.");

                results.Add(new JobResult
                {
                    DicomFolderPath = dicomFolderPath,
                    NiftiFilePath = resultNii,
                    ImagesFolderPath = dicomFolderPath + ImagesFolderSuffix,
                    LutFilePath = lutFilePath
                });
            }

            task.Wait();
            task.Dispose();

            return results.ToArray();
        }

        private void GetReferenceSeries(string referenceDicomFolder, string currentDicomFolder,
                                        out string refStudyUId, out string refSeriesUid)
        {
            // Check if a reference series exists from previous process(es)
            var refStudyExists =
                (!string.IsNullOrEmpty(referenceDicomFolder) && Directory.Exists(referenceDicomFolder) &&
                 Directory.GetFiles(referenceDicomFolder).Length > 0);

            var referenceForFutureProcessesDicomFolder = refStudyExists ? referenceDicomFolder : currentDicomFolder;

            var dicomFilePath = Directory.GetFiles(referenceForFutureProcessesDicomFolder).FirstOrDefault();
            var dicomFileHeaders = _dicomServices.GetDicomTags(dicomFilePath);
            refStudyUId = dicomFileHeaders.StudyInstanceUid.Values[0];
            refSeriesUid = dicomFileHeaders.SeriesInstanceUid.Values[0];
        }

        public IJobResult[] CompareAndSaveLocally(IJob job, IRecipe recipe, SliceType sliceType)
        {
            return CompareAndSaveLocally(
                job.CurrentSeriesDicomFolder, job.PriorSeriesDicomFolder, job.ReferenceSeriesDicomFolder,
                sliceType,
                job.ExtractBrain, job.Register, job.BiasFieldCorrection,
                job.PriorReslicedSeriesDicomFolder,
                recipe.ResultsDicomSeriesDescription, recipe.PriorReslicedDicomSeriesDescription
            );
        }

        private void UpdateSeriesDescriptionForAllFiles(string dicomFolder, string seriesDescription,
                                                        string orientationDicomFolder = "")
        {
            var dicomFiles = Directory.GetFiles(dicomFolder);
            if (dicomFiles.FirstOrDefault() == null)
                throw new FileNotFoundException($"Dicom folder contains no files: [{dicomFolder}]");

            var orientationDicomFiles = string.IsNullOrEmpty(orientationDicomFolder) ? null : Directory.GetFiles(orientationDicomFolder);
            if (!string.IsNullOrEmpty(orientationDicomFolder) &&
                (!Directory.Exists(orientationDicomFolder) || Directory.GetFiles(orientationDicomFolder).Length == 0))
                throw new FileNotFoundException($"Orientation dicom folder contains no files: [{orientationDicomFolder}]");

            var dicomTags = _dicomServices.GetDicomTags(dicomFiles.FirstOrDefault());
            dicomTags.SeriesDescription.Values = new[] { seriesDescription };

            _dicomServices.UpdateSeriesHeadersForAllFiles(dicomFiles.ToArray(), dicomTags);
            if (!string.IsNullOrEmpty(orientationDicomFolder))
                _dicomServices.UpdateImagePositionFromReferenceSeries(dicomFiles.ToArray(), orientationDicomFiles);
        }

        private string GetStudyDateFromDicomFile(string dicomFile)
        {
            var headers = _dicomServices.GetDicomTags(dicomFile);
            var studyDateVal = headers.StudyDate.Values[0];
            var year = studyDateVal.Substring(0, 4);
            var month = studyDateVal.Substring(4, 2);
            var day = studyDateVal.Substring(6, 2);
            return $"{year}-{month}-{day}";
        }

        private void ConvertBmpsToDicom(string bmpFolder, string outDicomFolder, string sourceDicomFolder,
                                        SliceType sliceType, string seriesDescription, bool matchWithFileNames = false)
        {
            var dicomSliceType = GetDicomSliceType(sliceType);

            _dicomServices.ConvertBmpsToDicom(bmpFolder, outDicomFolder, dicomSliceType,
                                              sourceDicomFolder, matchWithFileNames);

            UpdateSeriesDescriptionForAllFiles(outDicomFolder, seriesDescription);
        }
        private void ConvertNiftiToDicom(string inNiftiFile, string outDicomFolder,
                                         SliceType sliceType, string dicomFolderForReadingHeaders,
                                         string overlayText, string lookupTableFilePath = "", string referenceDicomFolder = "")
        {
            var bmpFolder = outDicomFolder + ImagesFolderSuffix;

            ConvertToBmp(inNiftiFile, bmpFolder, sliceType, overlayText);

            ConvertBmpsToDicom(bmpFolder, outDicomFolder, dicomFolderForReadingHeaders, sliceType, overlayText);

            if (!string.IsNullOrEmpty(referenceDicomFolder) && Directory.Exists(referenceDicomFolder) && Directory.GetFiles(referenceDicomFolder).Length > 0)
                _dicomServices.UpdateImagePositionFromReferenceSeries(Directory.GetFiles(outDicomFolder), Directory.GetFiles(referenceDicomFolder));

            //if (!string.IsNullOrEmpty(lookupTableFilePath) &&
            //    File.Exists(lookupTableFilePath))
            //    _dicomServices.ConvertBmpToDicomAndAddToExistingFolder(lookupTableFilePath, outDicomFolder);
        }

        private Dicom.Abstractions.SliceType GetDicomSliceType(SliceType sliceType)
        {
            switch (sliceType)
            {
                case SliceType.Sagittal:
                    return Dicom.Abstractions.SliceType.Sagittal;
                case SliceType.Coronal:
                    return Dicom.Abstractions.SliceType.Coronal;
                case SliceType.Axial:
                    return Dicom.Abstractions.SliceType.Axial;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sliceType), sliceType, null);
            }
        }

        private void ConvertToBmp(string inNiftiFile, string bmpFolder, SliceType sliceType, string overlayText)
        {
            var nim = _imgProcFactory.CreateNifti().ReadNifti(inNiftiFile);

            nim.ExportSlicesToBmps(bmpFolder, sliceType);

            foreach (var bmpFilePath in Directory.GetFiles(bmpFolder))
                AddOverlayToImage(bmpFilePath, overlayText);
        }

        public void AddOverlayToImage(string bmpFilePath, string overlayText)
        {
            if (string.IsNullOrEmpty(overlayText) || string.IsNullOrWhiteSpace(overlayText)) return;
            Bitmap bmpWithOverlay;
            using (var fs = new FileStream(bmpFilePath, FileMode.Open))
            {
                var bitmap = (Bitmap)Image.FromStream(fs);

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    using (var text = new Font("Tahoma", 9))
                    {
                        var x = (float)(bitmap.Width - overlayText.Length * 5.4) / 2;
                        var y = bitmap.Height - text.Height - 5;
                        graphics.DrawString(overlayText, text, Brushes.White, new PointF(x, y));
                    }
                }
                bmpWithOverlay = bitmap;
            }
            if (File.Exists(bmpFilePath)) File.Delete(bmpFilePath);
            bmpWithOverlay.Save(bmpFilePath);
        }
    }
}
