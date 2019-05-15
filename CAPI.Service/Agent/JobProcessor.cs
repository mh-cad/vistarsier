using CAPI.Service.Db;
using CAPI.Config;
using CAPI.Dicom.Abstractions;
using CAPI.Common;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SliceType = CAPI.NiftiLib.SliceType;
using CAPI.NiftiLib.Processing;
using CAPI.NiftiLib;
using CAPI.MS;
using System.Drawing;
using CAPI.Service.Agent.Abstractions;
using CAPI.Dicom;

namespace CAPI.Service.Agent
{
    /// <summary>
    /// Compares current and prior sereis and saves results into filesystem or sends off to a dicom node
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class JobProcessor : IJobProcessor
    {
        private readonly ILog _log;
        private readonly DbBroker _dbBroker;

        private const string ResultsFolderName = "Results";
        private const string ImagesFolderSuffix = "_Images";

        public JobProcessor(DbBroker dbBroker)
        {
            _log = Log.GetLogger();
            _dbBroker = dbBroker;
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
            var patientId = DicomFileOps.GetPatientIdFromDicomFile(dicomFilePath);
            var job = _dbBroker.Jobs.LastOrDefault(j => j.PatientId == patientId);


            // Generate Nifti file from Dicom and pass to ProcessNifti Method for current series.
            _log.Info($@"Start converting series dicom files to Nii");
            var task1 = Task.Run(() => { return Tools.Dcm2Nii(currentDicomFolder, "current.nii"); });
            var task2 = Task.Run(() => { return Tools.Dcm2Nii(priorDicomFolder, "prior.nii"); });
            var task3 = Task.Run(() => { return Tools.Dcm2Nii(referenceDicomFolder, "reference.nii"); });
            task1.Wait();
            task2.Wait();
            task3.Wait();

            var currentNifti = task1.Result;
            var priorNifti = task2.Result;
            var referenceNifti = task3.Result;
            _log.Info($@"Finished converting series dicom files to Nii");

            // Process Nifti files.
            var pipe = new MSPipeline(
                currentNifti, priorNifti, referenceNifti,
                extractBrain, register, biasFieldCorrect,
                resultNiis, outPriorReslicedNii);

            var metrics = pipe.Process(); // TODO: use theres.

            
            if (job != null)
            {
                var jobToUpdate = _dbBroker.Jobs.FirstOrDefault(j => j.Id == job.Id);
                if (jobToUpdate == null) throw new Exception($"No job was found in database with id: [{job.Id}]");

                // Check if there is a reference series from last processes for the patient, if not set the current series as reference for future
                GetReferenceSeries(referenceDicomFolder, currentDicomFolder, out var refStudyUid, out var refSeriesUid);
                jobToUpdate.WriteStudyAndSeriesIdsToReferenceSeries(refStudyUid, refSeriesUid);
                _dbBroker.Jobs.Update(jobToUpdate);
                _dbBroker.SaveChanges();
            }

            // "current" study dicom headers are used as the "prior resliced" series gets sent as part of the "current" study
            // prior study date will be added to the end of Series Description tag
            var task = Task.Run(() =>
            {
                _log.Info("Start converting resliced prior series back to Dicom");

                var priorStudyDate = GetStudyDateFromDicomFile(Directory.GetFiles(priorDicomFolder).FirstOrDefault());
                priorStudyDate = FormatDate(priorStudyDate);
                var priorStudyDescBase = string.IsNullOrEmpty(priorReslicedDicomSeriesDescription) ?
                    CapiConfig.GetConfig().ImagePaths.PriorReslicedDicomSeriesDescription :
                    priorReslicedDicomSeriesDescription;
                var priorStudyDescription = $"{priorStudyDescBase} {priorStudyDate}";

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                ConvertNiftiToDicom(outPriorReslicedNii, outPriorReslicedDicom, sliceType,
                                    currentDicomFolder, priorStudyDescription, referenceDicomFolder);

                UpdateSeriesDescriptionForAllFiles(outPriorReslicedDicom, priorStudyDescription);

                stopwatch.Stop();
                _log.Info("Finished Converting resliced prior series back to Dicom in " +
                          $"{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds:D2} minutes.");
            });

            _log.Debug(resultNiis);

            var results = new List<IJobResult>();
            foreach (var resultNii in resultNiis)
            {
                _log.Info("Start converting results back to Dicom");

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var resultsSeriesDescription = string.IsNullOrEmpty(resultsDicomSeriesDescription)
                    ? CapiConfig.GetConfig().ImagePaths.ResultsDicomSeriesDescription
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
                    var description = dicomFolderPath.ToLower().Contains("increase") ? "increase" : "decrease";
                    //lutFilePath = GetLookupTableForResult(resultNii, lookupTablePaths);
                    //var lutFileName = Path.GetFileNameWithoutExtension(lutFilePath);
                    ConvertNiftiToDicom(resultNii, dicomFolderPath, sliceType, currentDicomFolder,
                                        $"{resultsSeriesDescription}-{description}", referenceDicomFolder);
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

        /// <summary>
        /// This is just a dirty method to replace numeric months with letters to avoid confusion.
        /// </summary>
        /// <param name="priorStudyDate"></param>
        /// <returns></returns>
        private string FormatDate(string priorStudyDate)
        {
            // Silly and inefficient but it's not in an inner loop ;)
            priorStudyDate = priorStudyDate.Replace("-01-", "-Jan-");
            priorStudyDate = priorStudyDate.Replace("-02-", "-Feb-");
            priorStudyDate = priorStudyDate.Replace("-03-", "-Mar-");
            priorStudyDate = priorStudyDate.Replace("-04-", "-Apr-");
            priorStudyDate = priorStudyDate.Replace("-05-", "-May-");
            priorStudyDate = priorStudyDate.Replace("-06-", "-Jun-");
            priorStudyDate = priorStudyDate.Replace("-07-", "-Jul-");
            priorStudyDate = priorStudyDate.Replace("-08-", "-Aug-");
            priorStudyDate = priorStudyDate.Replace("-09-", "-Sep-");
            priorStudyDate = priorStudyDate.Replace("-10-", "-Oct-");
            priorStudyDate = priorStudyDate.Replace("-11-", "-Nov-");
            priorStudyDate = priorStudyDate.Replace("-12-", "-Dec-");

            return priorStudyDate;
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
            var dicomFileHeaders = DicomFileOps.GetDicomTags(dicomFilePath);
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

            var dicomTags = DicomFileOps.GetDicomTags(dicomFiles.FirstOrDefault());
            dicomTags.SeriesDescription.Values = new[] { seriesDescription };

            DicomFileOps.GenerateSeriesHeaderForAllFiles(dicomFiles.ToArray(), dicomTags);
            if (!string.IsNullOrEmpty(orientationDicomFolder))
                DicomFileOps.UpdateImagePositionFromReferenceSeries(dicomFiles.ToArray(), orientationDicomFiles);
        }

        private string GetStudyDateFromDicomFile(string dicomFile)
        {
            var headers = DicomFileOps.GetDicomTags(dicomFile);
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

            DicomFileOps.ConvertBmpsToDicom(bmpFolder, outDicomFolder, dicomSliceType,
                                              sourceDicomFolder, matchWithFileNames);

            UpdateSeriesDescriptionForAllFiles(outDicomFolder, seriesDescription);
        }
        private void ConvertNiftiToDicom(string inNiftiFile, string outDicomFolder,
                                         SliceType sliceType, string dicomFolderForReadingHeaders,
                                         string overlayText, string referenceDicomFolder = "")
        {
            var bmpFolder = outDicomFolder + ImagesFolderSuffix;
            _log.Debug("Converting to dicom -- " + inNiftiFile);
            ConvertToBmp(inNiftiFile, bmpFolder, sliceType, overlayText);

            ConvertBmpsToDicom(bmpFolder, outDicomFolder, dicomFolderForReadingHeaders, sliceType, overlayText);

            if (!string.IsNullOrEmpty(referenceDicomFolder) && Directory.Exists(referenceDicomFolder) && Directory.GetFiles(referenceDicomFolder).Length > 0)
                DicomFileOps.UpdateImagePositionFromReferenceSeries(Directory.GetFiles(outDicomFolder), Directory.GetFiles(referenceDicomFolder));

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
            _log.Debug("Converting to bmp -- " + inNiftiFile);
            var nim = new Nifti().ReadNifti(inNiftiFile);

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
                        var y = bitmap.Height - text.Height - 10;
                        graphics.DrawString(overlayText, text, Brushes.Black, new PointF(x+1, y+1));
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
