﻿using VisTarsier.Config;
using VisTarsier.Dicom.Abstractions;
using VisTarsier.Common;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisTarsier.NiftiLib.Processing;
using VisTarsier.NiftiLib;
using System.Drawing;
using VisTarsier.Dicom;
using System.Globalization;
using VisTarsier.Module.Custom;
using Microsoft.EntityFrameworkCore;

namespace VisTarsier.Service
{
    /// <summary>
    /// Compares current and prior sereis and saves results into filesystem or sends off to a dicom node
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class JobProcessor
    {
        private readonly ILog _log;
        private readonly DbBroker _dbBroker;

        private const string ResultsFolderName = "Results";
        private const string ImagesFolderSuffix = "_Images";

        public string SummarySlidePath { get; set; }

        public JobProcessor()
        {
            _log = Log.GetLogger();
            var connectString = CapiConfig.GetConfig()?.AgentDbConnectionString;
            _dbBroker = connectString != null ? new DbBroker(connectString) : new DbBroker();
        }

        public void Process(Job job)
        {

            // We want to update the job in the DB, so we're going to turn it into a new instance
            // straight from the DB to avoid errors.
            if (_dbBroker.Jobs.Find(job.Id) == null) _dbBroker.Jobs.Add(job);
            var @dbjob = _dbBroker.Jobs.Find(job.Id);
            // Update job status in the DB
            @dbjob.Start = DateTime.Now;
            @dbjob.Status = "Processing";
            _dbBroker.Jobs.Update(@dbjob);
            _dbBroker.SaveChanges();
            @dbjob.Attempt = job.Attempt;
            @dbjob.Attempt.JobId = @dbjob.Id;
            _dbBroker.Attempts.Update(@dbjob.Attempt);
            _dbBroker.SaveChanges();

            // Write banner to log file.
            _log.Info($"{Environment.NewLine}");
            _log.Info($"****************  JOB CREATED  **********************************");
            _log.Info($" Job ID               *  {job.Id}");
            _log.Info($" Patient ID           *  {job.Attempt.PatientId}");
            _log.Info($" Patient Name         *  {job.Attempt.PatientFullName}");
            _log.Info($" Patient DOB          *  {job.Attempt.PatientBirthDate}");
            _log.Info($" Current Accession    *  {job.Attempt.CurrentAccession}");
            _log.Info($" Prior Accession      *  {job.Attempt.PriorAccession}");
            _log.Info($"*****************************************************************");
            _log.Info($"{Environment.NewLine}");

            // Start a timer for logging.
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var sliceType = job.Recipe.OutputSettings.SliceType;

            // Create pre-metadata slides.
            try
            {
                var preResults = GenerateMetadataSlides(job);
                SendToDestinations(preResults, job);
            }
            catch (Exception e)
            {
                _log.Error("Failed to generate pre-images. :(");
                _log.Error(e.Message);
                _log.Error(e.StackTrace);
            }

            try
            {
                // This is where the magic happens...
                var results = GetResults(job);
                SendToDestinations(results, job);
            }
            catch (Exception e)
            {
                // Hacky error handling. Attempt to send results slide to DICOM.
                // Obviously this won't work in all cases.
                string metadataPath = Path.Combine(job.ResultSeriesDicomFolder, "metadata");
                FileSystem.DirectoryExistsIfNotCreate(metadataPath);
                var failedResults = GenerateResultsSlide(new Metrics() { Passed = false, Notes = e.Message }, DicomFileOps.GetDicomTags(SummarySlidePath), job, metadataPath, "N/A");
                SendToDestinations(new JobResult[] { new JobResult { DicomFolderPath = metadataPath } }, job);

                _log.Error(e);

                throw e;
            }
            finally
            {
                Directory.Delete(job.ProcessingFolder, true);
            }

            // Update status of job in database.
            @dbjob.End = DateTime.Now;
            @dbjob.Status = "Complete";
            @dbjob = _dbBroker.Jobs.SingleOrDefault(j => j.Id == job.Id);
            if (@dbjob == null) throw new Exception($"Job with id [{job.Id}] not found");
            _dbBroker.Jobs.Update(@dbjob);
            _dbBroker.SaveChanges();

            // Print end banner to log.
            stopwatch.Stop();
            _log.Info($"{Environment.NewLine}");
            _log.Info($"****************  JOB COMPLETE  **********************************");
            _log.Info($" Job ID               *  {job.Id}");
            _log.Info($" Processing Time      *  {job.End - job.Start}");
            _log.Info($"*****************************************************************");
            _log.Info($"{Environment.NewLine}");
            _log.Info($"{Environment.NewLine}");
            _log.Info($"Job Id=[{job.Id}] completed in {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds:D2} minutes.");

        }

        public JobResult[] GetResults(Job job)
        {
            var timer = new Stopwatch();
            timer.Start();
            // Working directory.
            var workingDir = Directory.GetParent(job.PriorReslicedSeriesDicomFolder).FullName;
            // We're getting result files based on paths for look up tables??? //TODO deconvolve...
            //var resultNiis = BuildResultNiftiPathsFromLuts(lookupTablePaths, workingDir).ToArray();
            // Create results folder
            var allResultsFolder = Path.Combine(workingDir, ResultsFolderName);
            FileSystem.DirectoryExistsIfNotCreate(allResultsFolder);
  

            var outPriorReslicedNii = job.PriorReslicedSeriesDicomFolder + ".nii";

            if (Directory.GetFiles(job.CurrentSeriesDicomFolder).Length == 0) throw new ArgumentException($"Dicom folder {job.CurrentSeriesDicomFolder} does not contain any files.");

            var dicomFilePath = Directory.GetFiles(job.CurrentSeriesDicomFolder)[0];
            var patientId = DicomFileOps.GetPatientIdFromDicomFile(dicomFilePath);
            //var job = _dbBroker.Jobs.LastOrDefault(j => j != null && j.Attempt != null && j.Attempt.PatientId == patientId);


            // Generate Nifti file from Dicom and pass to ProcessNifti Method for current series.
            _log.Info($@"Start converting series dicom files to Nii");
            var task1 = Task.Run(() => { return Tools.Dcm2Nii(job.CurrentSeriesDicomFolder, "current.nii"); });
            var task2 = Task.Run(() => { return Tools.Dcm2Nii(job.PriorSeriesDicomFolder, "prior.nii"); });
            var task3 = Task.Run(() => { return Tools.Dcm2Nii(job.ReferenceSeriesDicomFolder, "reference.nii"); });
            task1.Wait();
            task2.Wait();
            task3.Wait();

            var currentNifti = task1.Result;
            var priorNifti = task2.Result;
            var referenceNifti = task3.Result;
            _log.Info($@"Finished converting series dicom files to Nii");

            // Process Nifti files.
            //var pipe = new MSPipeline(
            //    currentNifti, priorNifti, referenceNifti,
            //    extractBrain, register, biasFieldCorrect,
            //    resultNiis, outPriorReslicedNii);
            var pipe = new CustomPipeline(job.Recipe, priorNifti, currentNifti);

            var metrics = pipe.Process();

            var results = new List<JobResult>();

            string metadataPath = Path.Combine(allResultsFolder, "resultmetadata");
            Directory.CreateDirectory(metadataPath);

            if (job != null)
            {
                var jobToUpdate = _dbBroker.Jobs.FirstOrDefault(j => j.Id == job.Id);
                if (jobToUpdate == null) throw new Exception($"No job was found in database with id: [{job.Id}]");

                // Check if there is a reference series from last processes for the patient, if not set the current series as reference for future
                GetReferenceSeries(job.ReferenceSeriesDicomFolder, job.CurrentSeriesDicomFolder, out var refStudyUid, out var refSeriesUid);
                jobToUpdate.WriteStudyAndSeriesIdsToReferenceSeries(refStudyUid, refSeriesUid);
                _dbBroker.Jobs.Update(jobToUpdate);
                _dbBroker.SaveChanges();
            }

            // "current" study dicom headers are used as the "prior resliced" series gets sent as part of the "current" study
            // prior study date will be added to the end of Series Description tag
            //var task = Task.Run(() =>
            //{
            //    _log.Info("Start converting resliced prior series back to Dicom");

            //    var priorStudyDate = GetStudyDateFromDicomFile(Directory.GetFiles(job.PriorSeriesDicomFolder).FirstOrDefault());
            //    priorStudyDate = FormatDate(priorStudyDate);

            //    var priorStudyDescBase = string.IsNullOrEmpty(job.Recipe.OutputSettings.ReslicedDicomSeriesDescription) ?
            //        CapiConfig.GetConfig().ImagePaths.PriorReslicedDicomSeriesDescription
            //        : job.Recipe.OutputSettings.ReslicedDicomSeriesDescription;

            //    var priorStudyDescription = $"{priorStudyDescBase} {priorStudyDate}";

            //    var stopwatch = new Stopwatch();
            //    stopwatch.Start();

            //    ConvertNiftiToDicom(outPriorReslicedNii, job.PriorReslicedSeriesDicomFolder, job.Recipe.OutputSettings.SliceType,
            //                        job.CurrentSeriesDicomFolder, priorStudyDescription, job.ReferenceSeriesDicomFolder);

            //    UpdateSeriesDescriptionForAllFiles(job.PriorReslicedSeriesDicomFolder, priorStudyDescription);

            //    stopwatch.Stop();
            //    _log.Info("Finished Converting resliced prior series back to Dicom in " +
            //              $"{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds:D2} minutes.");

            //    results.Add(new JobResult()
            //    {
            //        DicomFolderPath = job.PriorReslicedSeriesDicomFolder,
            //        NiftiFilePath = outPriorReslicedNii,
            //        ImagesFolderPath = job.PriorReslicedSeriesDicomFolder + ImagesFolderSuffix,
            //    });
            //});

            List<Task> tasks = new List<Task>();

            foreach (var resultNii in metrics.ResultFiles)
            {
                tasks.Add(Task.Run(() =>
                {
                    _log.Info("Start converting results back to Dicom");

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var resultsSeriesDescription = string.IsNullOrEmpty(job.Recipe.OutputSettings.ResultsDicomSeriesDescription)
                        ? CapiConfig.GetConfig().ImagePaths.ResultsDicomSeriesDescription
                        : job.Recipe.OutputSettings.ResultsDicomSeriesDescription;

                    string dicomFolderPath;

                    dicomFolderPath = resultNii.FilePath.Replace(".nii", ".dicom");
                    var priorDate = GetStudyDateFromDicomFile(Directory.GetFiles(job.PriorSeriesDicomFolder).FirstOrDefault());
                    var currentDate = GetStudyDateFromDicomFile(Directory.GetFiles(job.CurrentSeriesDicomFolder).FirstOrDefault());
                    var description = resultNii.Description;
                    //lutFilePath = GetLookupTableForResult(resultNii, lookupTablePaths);
                    //var lutFileName = Path.GetFileNameWithoutExtension(lutFilePath);
                    ConvertNiftiToDicom(resultNii.FilePath, dicomFolderPath, job.Recipe.OutputSettings.SliceType, job.CurrentSeriesDicomFolder,
                                        $"{resultsSeriesDescription}-{description}\n {FormatDate(priorDate)} -> {FormatDate(currentDate)}", job.ReferenceSeriesDicomFolder);


                    stopwatch.Stop();
                    _log.Info("Finished converting results back to Dicom in " +
                              $"{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds:D2} minutes.");

                    results.Add(new JobResult
                    {
                        DicomFolderPath = dicomFolderPath,
                        NiftiFilePath = resultNii.FilePath,
                        ImagesFolderPath = dicomFolderPath + ImagesFolderSuffix,
                    });

                }));
            }

            Task.WaitAll(tasks.ToArray());
            //task.Wait();
            //task.Dispose();

            GenerateResultsSlide(metrics, DicomFileOps.GetDicomTags(SummarySlidePath), job, metadataPath, timer.Elapsed.ToString(@"m\:ss"));

            results.Add(new JobResult
            {
                DicomFolderPath = metadataPath,
            });

            return results.ToArray();
        }

        public JobResult[] GenerateMetadataSlides(Job job)
        {
            // Create results folder
            var allResultsFolder = job.ResultSeriesDicomFolder;
            FileSystem.DirectoryExistsIfNotCreate(allResultsFolder);

            // Make our metadata...
            string metadataPath = Path.Combine(allResultsFolder, "metadata");
            FileSystem.DirectoryExistsIfNotCreate(metadataPath); 

            string summary = GenerateSummarySlide(
                Directory.GetFiles(job.PriorSeriesDicomFolder).FirstOrDefault(),
                Directory.GetFiles(job.CurrentSeriesDicomFolder).FirstOrDefault(),
                $"{job.Id}",
                metadataPath);

            GenerateMetadataSlide(
                Directory.GetFiles(job.PriorSeriesDicomFolder).FirstOrDefault(),
                Directory.GetFiles(job.CurrentSeriesDicomFolder).FirstOrDefault(),
                DicomFileOps.GetDicomTags(summary),
                metadataPath);

            SummarySlidePath = summary;

            return new JobResult[] {
                new JobResult()
                {
                    DicomFolderPath = metadataPath
                }
            };
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
            DicomFileOps.ConvertBmpsToDicom(bmpFolder, outDicomFolder, sliceType,
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

        private void ConvertToBmp(string inNiftiFile, string bmpFolder, SliceType sliceType, string overlayText)
        {
            _log.Debug("Converting to bmp -- " + inNiftiFile);
            var nim = (NiftiFloat32)new NiftiFloat32().ReadNifti(inNiftiFile);

            nim.ExportSlicesToBmps(bmpFolder, sliceType);

            foreach (var bmpFilePath in Directory.GetFiles(bmpFolder))
                AddOverlayToImage(bmpFilePath, overlayText);
        }

        public void AddOverlayToImage(string bmpFilePath, string overlayText)
        {
            if (string.IsNullOrEmpty(overlayText) || string.IsNullOrWhiteSpace(overlayText)) return;
            Bitmap bmpWithOverlay;
            var lines = overlayText.Split('\n');

            using (var fs = new FileStream(bmpFilePath, FileMode.Open))
            {
                var bitmap = (Bitmap)Image.FromStream(fs);

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    using (var text = new Font("Tahoma", 9))
                    {
                        var x = (float)(bitmap.Width - lines[0].Length * 5.4) / 2;
                        var y = bitmap.Height - text.Height - 20;
                        graphics.DrawString(lines[0], text, Brushes.Black, new PointF(x+1, y+1));
                        graphics.DrawString(lines[0], text, Brushes.White, new PointF(x, y));
                    }

                    if(lines.Length > 1)
                    {
                        using (var text = new Font("Tahoma", 7))
                        {
                            var x = (float)(bitmap.Width - lines[0].Length * 5.4) / 2;
                            var y = bitmap.Height - text.Height - 10;
                            graphics.DrawString(lines[1], text, Brushes.Black, new PointF(x + 1, y + 1));
                            graphics.DrawString(lines[1], text, Brushes.White, new PointF(x, y));
                        }
                    }
                }
                bmpWithOverlay = bitmap;
            }
            if (File.Exists(bmpFilePath)) File.Delete(bmpFilePath);
            bmpWithOverlay.Save(bmpFilePath);
        }

        public string GenerateSummarySlide(string priorDcm, string currentDcm, string jobId, string outfolder)
        {
            var patientIdField = new Point(183, 177);
            var patientNameField = new Point(205, 200);
            var patientDobField = new Point(194, 221);
            var patientSexField = new Point(194, 243);
            var priorAccessionField = new Point(260, 358);
            var priorDateField = new Point(204, 379);
            var priorDescField = new Point(175, 418);
            var currentAccessionField = new Point(260, 539);
            var currentDateField = new Point(204, 557);
            var currentDescField = new Point(175, 597);

            var outfile = Path.Combine(outfolder, "summary.dcm");

            var priorTags = DicomFileOps.GetDicomTags(priorDcm);
            var currentTags = DicomFileOps.GetDicomTags(currentDcm);

            var bmppath = Path.Combine(".", "resources", "templates", "summary.bmp");
            Bitmap slide = new Bitmap(bmppath);
            using (var g = Graphics.FromImage(slide))
            {
                var font = new Font("Courier New", 12);
                var brush = Brushes.White;
                var priorStudyDate = priorTags.StudyDate?.Values?.Length > 0 ? DateTime.ParseExact(priorTags.StudyDate?.Values?[0], "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy") : null;
                var dob = priorTags.PatientBirthDate?.Values.Length > 0 ? DateTime.ParseExact(priorTags.PatientBirthDate?.Values?[0], "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy"): null;
                var currentStudyDate = currentTags.StudyDate?.Values.Length > 0 ? DateTime.ParseExact(currentTags.StudyDate?.Values?[0], "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy") :  null;

                if (priorTags.PatientId?.Values?.Length > 0) g.DrawString(priorTags.PatientId?.Values?[0], font, Brushes.White, patientIdField);
                if (priorTags.PatientName?.Values?.Length > 0) g.DrawString(priorTags.PatientName?.Values?[0], font, Brushes.White, patientNameField);
                g.DrawString(dob, font, Brushes.White, patientDobField);
                if (priorTags.PatientSex?.Values?.Length > 0) g.DrawString(priorTags.PatientSex?.Values?[0], font, Brushes.White, patientSexField);
                if (priorTags.StudyAccessionNumber?.Values?.Length > 0) g.DrawString(priorTags.StudyAccessionNumber?.Values?[0], font, Brushes.White, priorAccessionField);
                g.DrawString(priorStudyDate, font, Brushes.White, priorDateField);
                if (priorTags.StudyDescription?.Values?.Length > 0) g.DrawString(priorTags.StudyDescription?.Values?[0], font, Brushes.White, priorDescField);
                if (currentTags.StudyAccessionNumber?.Values?.Length > 0) g.DrawString(currentTags.StudyAccessionNumber?.Values?[0], font, Brushes.White, currentAccessionField);
                g.DrawString(currentStudyDate, font, Brushes.White, currentDateField);
                if (currentTags.StudyDescription?.Values?.Length > 0) g.DrawString(currentTags.StudyDescription?.Values?[0], font, Brushes.White, currentDescField);
                g.DrawString($"[{jobId}]", new Font("Courier New", 14, FontStyle.Bold), Brushes.White, new Point(524,112));
            }

            try
            {
                new Bitmap(slide).Save($"{outfile}.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }
            catch (Exception e)
            {
                _log.Debug("Threw an error while trying to save bitmap because GDI+ is dumb.");
                _log.Debug($"Tried to save bmp to {outfile}.bmp");
                throw e;
            }

            DicomFileOps.ConvertBmpToDicom($"{outfile}.bmp", outfile);
            File.Delete($"{outfile}.bmp");

            // Generate a bunch of metadata for the outgoing DICOM...
            var metatags = DicomFileOps.GetDicomTags(outfile);
            metatags.InstitutionAddress.Values = currentTags.InstitutionAddress.Values;
            metatags.InstitutionalDepartmentName.Values = currentTags.InstitutionalDepartmentName.Values;
            metatags.InstitutionName.Values = currentTags.InstitutionName.Values;
            metatags.PatientId.Values = currentTags.PatientId.Values;
            metatags.PatientName.Values = currentTags.PatientName.Values;
            metatags.PatientSex.Values = currentTags.PatientSex.Values;
            metatags.PerformingPhysiciansName.Values = currentTags.PerformingPhysiciansName.Values;
            metatags.PhysiciansOfRecord.Values = currentTags.PhysiciansOfRecord.Values;
            metatags.ReferringPhysician.Values = currentTags.ReferringPhysician.Values;
            metatags.RequestingPhysician.Values = currentTags.RequestingPhysician.Values;
            metatags.SeriesInstanceUid.Values = new string[] { DicomFileOps.GenerateNewSeriesUid() };
            metatags.SeriesDescription.Values = new string[] { $"VisTarsier Metadata Job [{jobId}]" };
            metatags.StudyAccessionNumber.Values = currentTags.StudyAccessionNumber.Values;
            metatags.StudyDate.Values = currentTags.StudyDate.Values;
            metatags.StudyDescription.Values = currentTags.StudyDescription.Values;
            metatags.StudyInstanceUid.Values = currentTags.StudyInstanceUid.Values;
            metatags.InstanceNumber.Values = new string[] { "20" };

            DicomFileOps.ForceUpdateDicomHeaders(outfile, metatags);


            return outfile;
        }

        public string GenerateMetadataSlide(string priorDcm, string currentDcm, IDicomTagCollection metatags, string outFolder)
        {
            var priorTags = DicomFileOps.GetDicomTags(priorDcm);
            var currentTags = DicomFileOps.GetDicomTags(currentDcm);

            var outpath = Path.Combine(outFolder, "metadata.dcm");

            // References for bitmap template (TODO: Maybe we could put these in a file for easy changing)
            //var col1X = 77;
            var col2X = 295;
            var col3X = 530;
            var rowHeight = 22;
            var rowstartY = 213;

            // Lots of ugly code to position text.
            Bitmap slide = new Bitmap(Path.Combine("resources", "templates", "metadata.bmp"));
            using (var g = Graphics.FromImage(slide))
            {
                var font = new Font("Courier New", 12);
                var brush = Brushes.White;

                var priorModality = priorTags.Modality?.Values?.Length > 0 ? priorTags.Modality?.Values?[0].ToString() : "";
                var currentModality = currentTags.Modality?.Values?.Length > 0 ? currentTags.Modality?.Values?[0].ToString() : "";
                brush = priorModality.Equals(currentModality) ? Brushes.White : Brushes.Red;
                g.DrawString(priorModality, font, brush, new Point(col2X, rowstartY + rowHeight * 0));
                g.DrawString(currentModality, font, brush, new Point(col3X, rowstartY + rowHeight * 0));

                var priorProtocol = priorTags.ProtocolName?.Values?.Length > 0 ? priorTags.ProtocolName?.Values?[0].ToString() : "";
                var currentProtocol = currentTags.ProtocolName?.Values?.Length > 0 ? currentTags.ProtocolName?.Values?[0].ToString() : "";
                brush = priorProtocol.Equals(currentProtocol) ? Brushes.White : Brushes.Red;
                if (priorProtocol.Length > 20) priorProtocol = priorProtocol.Substring(0, 17) + "...";
                if (currentProtocol.Length > 20) currentProtocol = currentProtocol.Substring(0, 17) + "...";

                g.DrawString(priorProtocol, font, brush, new Point(col2X, rowstartY + rowHeight * 1));
                g.DrawString(currentProtocol, font, brush, new Point(col3X, rowstartY + rowHeight * 1));

                var priorOptions = "";
                foreach (var val in priorTags.ScanOptions?.Values) priorOptions += val + "/";
                var currentOptions = "";
                foreach (var val in currentTags.ScanOptions?.Values) currentOptions += val + "/";
                brush = priorOptions.Equals(currentOptions) ? Brushes.White : Brushes.Orange;
                g.DrawString(priorOptions, font, brush, new Point(col2X, rowstartY + rowHeight * 2));
                g.DrawString(currentOptions, font, brush, new Point(col3X, rowstartY + rowHeight * 2));

                var priorScanner = priorTags.Manufacturer?.Values?.Length > 0 && priorTags.ManufacturersModelName?.Values?.Length > 0 ? $"{priorTags.Manufacturer?.Values?[0].ToString()} {priorTags.ManufacturersModelName?.Values?[0].ToString()}" : "";
                var currentScanner = currentTags.Manufacturer?.Values?.Length > 0 && currentTags.ManufacturersModelName?.Values?.Length > 0 ? $"{currentTags.Manufacturer?.Values?[0].ToString()} {currentTags.ManufacturersModelName?.Values?[0].ToString()}" : "";
                brush = priorScanner.Equals(currentScanner) ? Brushes.White : Brushes.Orange;
                g.DrawString(priorScanner, font, brush, new Point(col2X, rowstartY + rowHeight * 3));
                g.DrawString(currentScanner, font, brush, new Point(col3X, rowstartY + rowHeight * 3));

                var priorSerial = priorTags.DeviceSerialNumber?.Values?.Length > 0 ? priorTags.DeviceSerialNumber?.Values?[0].ToString() : "";
                var currentSerial = currentTags.DeviceSerialNumber?.Values?.Length > 0 ? currentTags.DeviceSerialNumber?.Values?[0].ToString(): "";
                brush = priorSerial.Equals(currentSerial) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorSerial, font, brush, new Point(col2X, rowstartY + rowHeight * 4));
                g.DrawString(currentSerial, font, brush, new Point(col3X, rowstartY + rowHeight * 4));

                var priorSoftware = "";
                foreach (var val in priorTags.SoftwareVersion?.Values) priorSoftware += val + " ";
                var currentSoftware = "";
                foreach (var val in currentTags.SoftwareVersion?.Values) currentSoftware += val + " ";
                brush = priorSoftware.Equals(currentSoftware) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorSoftware, font, brush, new Point(col2X, rowstartY + rowHeight * 5));
                g.DrawString(currentSoftware, font, brush, new Point(col3X, rowstartY + rowHeight * 5));

                var priorEcho = priorTags.EchoTime?.Values?.Length > 0 ? priorTags.EchoTime?.Values?[0].ToString() : "";
                var currentEcho = currentTags.EchoTime?.Values?.Length > 0 ? currentTags.EchoTime?.Values?[0].ToString() : "";
                brush = priorEcho.Equals(currentEcho) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorEcho, font, brush, new Point(col2X, rowstartY + rowHeight * 6));
                g.DrawString(currentEcho, font, brush, new Point(col3X, rowstartY + rowHeight * 6));

                var priorIt = priorTags.InversionTime?.Values?.Length > 0 ? priorTags.InversionTime?.Values?[0].ToString() : "";
                var currentIt = currentTags.InversionTime?.Values?.Length > 0 ? currentTags.InversionTime?.Values?[0].ToString() : "";
                brush = priorIt.Equals(currentIt) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorIt, font, brush, new Point(col2X, rowstartY + rowHeight * 7));
                g.DrawString(currentIt, font, brush, new Point(col3X, rowstartY + rowHeight * 7));

                var priorIn = priorTags.ImagedNucleus?.Values?.Length > 0 ? priorTags.ImagedNucleus?.Values?[0].ToString() : "";
                var currentIn = currentTags.ImagedNucleus?.Values?.Length > 0 ? currentTags.ImagedNucleus?.Values?[0].ToString() : "";
                brush = priorIn.Equals(currentIn) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorIn, font, brush, new Point(col2X, rowstartY + rowHeight * 8));
                g.DrawString(currentIn, font, brush, new Point(col3X, rowstartY + rowHeight * 8));

                var priorTeslas = priorTags.MagneticFieldStrength?.Values?.Length > 0 ? priorTags.MagneticFieldStrength?.Values?[0].ToString() + "T" : "";
                var currentTeslas = currentTags.MagneticFieldStrength?.Values?.Length > 0 ? currentTags.MagneticFieldStrength?.Values?[0].ToString() + "T" : "";
                brush = priorTeslas.Equals(currentTeslas) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorTeslas, font, brush, new Point(col2X, rowstartY + rowHeight * 9));
                g.DrawString(currentTeslas, font, brush, new Point(col3X, rowstartY + rowHeight * 9));

                var priorEt = priorTags.EchoTrainLength?.Values?.Length > 0 ? priorTags.EchoTrainLength?.Values?[0].ToString() : "";
                var currentEt = currentTags.EchoTrainLength?.Values?.Length > 0 ? currentTags.EchoTrainLength?.Values?[0].ToString() : "";
                brush = priorEt.Equals(currentEt) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorEt, font, brush, new Point(col2X, rowstartY + rowHeight * 10));
                g.DrawString(currentEt, font, brush, new Point(col3X, rowstartY + rowHeight * 10));

                var priorTc = priorTags.TransmitCoilName?.Values?.Length > 0 ? priorTags.TransmitCoilName?.Values?[0].ToString() : "";
                var currentTc = currentTags.TransmitCoilName?.Values?.Length > 0 ? currentTags.TransmitCoilName?.Values?[0].ToString() : "";
                brush = priorTc.Equals(currentTc) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorTc, font, brush, new Point(col2X, rowstartY + rowHeight * 11));
                g.DrawString(currentTc, font, brush, new Point(col3X, rowstartY + rowHeight * 11));
            }


            try
            {
                new Bitmap(slide).Save($"{outpath}.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }
            catch (Exception e)
            {
                _log.Debug("Threw an error while trying to save bitmap because GDI+ is dumb.");
                _log.Debug($"Tried to save bmp to {outpath}.bmp");
                throw e;
            }

            DicomFileOps.ConvertBmpToDicom($"{outpath}.bmp", outpath);
            File.Delete($"{outpath}.bmp");

            metatags = DicomFileOps.UpdateUidsForNewImage(metatags);
            metatags.InstanceNumber.Values = new string[] { "40" };
            DicomFileOps.ForceUpdateDicomHeaders(outpath, metatags);

           // DicomFileOps.UpdateDicomHeaders(outpath, metatags, DicomNewObjectType.NewImage);

            return outpath;
        }

        public string[] GenerateResultsSlide(Metrics results, IDicomTagCollection metatags, Job job, string outFolder, string time)
        {
            var output = new List<string>();

            var resultFile = Path.Combine(outFolder, "resultsSummary.dcm");
            var bmppath = Path.Combine(".", "resources", "templates", "results.bmp");
            var comment = results.Notes;

            // Do some line splitting on the comments.
            var lines = comment?.Split('\n');
            for (int i = 0; i < lines?.Length; ++i)
            {
                var len = 0;
                while (lines[i].Length - len > 48)
                {
                    len += 48;
                    lines[i] = lines[i].Insert(len, "\n");
                }
            }
            comment = "";
            if (lines != null) foreach (var line in lines) comment += line + '\n';

            // Create out results slide.
            Bitmap slide = File.Exists(bmppath) ? new Bitmap(bmppath) : new Bitmap(1024, 1024);

            using (var g = Graphics.FromImage(slide))
            {
                var font = new Font("Courier New", 12);
                var brush = Brushes.White;

                if (results.Passed) g.DrawString("PASSED", new Font("Courier New", 48, FontStyle.Bold), Brushes.LightGreen, new Point(250, 155));
                else g.DrawString("FAILED", new Font("Courier New", 48, FontStyle.Bold), Brushes.OrangeRed, new Point(250, 155));
                g.DrawString(comment, new Font("Courier New", 12), Brushes.White, new Point(141, 289));
                g.DrawString($"Time to process : {time}", new Font("Courier New", 12), Brushes.White, new Point(147, 475));
                for (int i = 0; i < results.Stats?.Count; ++i)
                {
                    g.DrawString(results.Stats[i], new Font("Courier New", 12), Brushes.White, new Point(147, 497 + 22*i));
                }
                g.DrawString($"[{job.Id}]", new Font("Courier New", 14, FontStyle.Bold), Brushes.White, new Point(524, 112));
            }

            try
            {
                new Bitmap(slide).Save($"{resultFile}.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }
            catch (Exception e)
            {
                _log.Debug("Threw an error while trying to save bitmap because GDI+ is dumb.");
                _log.Debug($"Tried to save bmp to {resultFile}.bmp");
                throw e;
            }

            // Convert to DICOM file
            DicomFileOps.ConvertBmpToDicom($"{resultFile}.bmp", resultFile);
            File.Delete($"{resultFile}.bmp");
            output.Add(resultFile);
            _log.Info($"Written DICOM: {resultFile}");
            metatags = DicomFileOps.UpdateUidsForNewImage(metatags);
            metatags.InstanceNumber.Values = new string[] { $"1" };
            
            DicomFileOps.ForceUpdateDicomHeaders(resultFile, metatags);

            // Process pipeline's results slides...
            if (results.ResultsSlides == null)
            {
                _log.Debug("No results slides to be output.");

                return output.ToArray();
            }


            for (int i = 0; i < results.ResultsSlides.Length; ++i)
            {
                var outpath = Path.Combine(outFolder, $"results{i+60}.dcm");
                var bmp = new Bitmap(results.ResultsSlides[i]);
                bmp.Save($"{outpath}.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                _log.Info($"Written: {outpath}.bmp");

                DicomFileOps.ConvertBmpToDicom($"{outpath}.bmp", outpath);
                File.Delete($"{outpath}.bmp");
                _log.Info($"Written DICOM: {outpath}");

                metatags = DicomFileOps.UpdateUidsForNewImage(metatags);
                metatags.InstanceNumber.Values = new string[] { $"{i+60}" };
                DicomFileOps.ForceUpdateDicomHeaders(outpath, metatags);

                output.Add(outpath);
            }
         
            // ... done.
            return output.ToArray();
        }

        private void SendToDestinations(JobResult[] results, Job job)
        {
            // TODO :: I'm note sure that we should be throwing errors here, since the job results should be 
            // detemining where we're getting files from 
            //if (string.IsNullOrEmpty(ResultSeriesDicomFolder) ||
            //    Directory.GetFiles(ResultSeriesDicomFolder).Length == 0 &&
            //    Directory.GetDirectories(ResultSeriesDicomFolder).Length == 0)
            //    throw new DirectoryNotFoundException($"No folder found for {nameof(ResultSeriesDicomFolder)} " +
            //                                         $"at following path: [{ResultSeriesDicomFolder}] or empty!");

            //if (string.IsNullOrEmpty(PriorReslicedSeriesDicomFolder) || Directory.GetFiles(PriorReslicedSeriesDicomFolder).Length == 0)
            //    throw new DirectoryNotFoundException($"No folder found for {nameof(PriorReslicedSeriesDicomFolder)} " +
            //                                         $"at following path: [{PriorReslicedSeriesDicomFolder}] or empty!");

            _log.Info("Sending to destinations...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Sending to Dicom Destinations
            SendToDicomDestinations(results, job);
            // Sending to Filesystem Destinations
            SendToFilesystemDestinations(results, job);

            stopwatch.Stop();
            _log.Info($"Finished sending to destinations in {Math.Round(stopwatch.Elapsed.TotalSeconds)} seconds");
        }
        private void SendToFilesystemDestinations(JobResult[] results, Job job)
        {
            if (job.Recipe.OutputSettings.FilesystemDestinations == null) return;
            foreach (var fsDestination in job.Recipe.OutputSettings.FilesystemDestinations)
            {
                var jobFolderName = Path.GetFileName(job.ProcessingFolder) ?? throw new InvalidOperationException();
                var destJobFolderPath = Path.Combine(fsDestination, jobFolderName);

                _log.Info($"Sending to folder [{destJobFolderPath}]...");
                if (job.Recipe.OutputSettings.OnlyCopyResults)
                {
                    foreach (var result in results)
                    {
                        var resultParentFolderPath = Path.GetDirectoryName(result.NiftiFilePath);
                        var resultParentFolderName = Path.GetFileName(resultParentFolderPath);
                        var resultDestinationFolder = Path.Combine(destJobFolderPath, resultParentFolderName ?? throw new InvalidOperationException());
                        FileSystem.CopyDirectory(resultParentFolderPath, resultDestinationFolder);
                    }

                    //var priorFolderName = Path.GetFileName(PriorReslicedSeriesDicomFolder);
                    //var priorDestinationFolder = Path.Combine(destJobFolderPath, priorFolderName ?? throw new InvalidOperationException());
                    //FileSystem.CopyDirectory(PriorReslicedSeriesDicomFolder, priorDestinationFolder);
                }
                else
                {
                    FileSystem.CopyDirectory(job.ProcessingFolder, destJobFolderPath);
                }
            }
        }
        private void SendToDicomDestinations(JobResult[] results, Job job)
        {
            if (job.Recipe.OutputSettings.DicomDestinations == null) return;
            foreach (var dicomDestination in job.Recipe.OutputSettings.DicomDestinations)
            {
                var cfg = CapiConfig.GetConfig();
                var localNode = cfg.DicomConfig.LocalNode;
                IDicomNode remoteNode;
                try
                {
                    remoteNode = cfg.DicomConfig.RemoteNodes
                        .SingleOrDefault(n => n.AeTitle.Equals(dicomDestination, StringComparison.CurrentCultureIgnoreCase));
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed at getting remote node defined in recipe from config file. AET in recipe: [{dicomDestination}]", ex);
                    throw new Exception($"Remote node not found in config file [{dicomDestination}]");
                }
                if (remoteNode == null)
                    throw new Exception($"Remote node not found in config file [{dicomDestination}]");

                var dicomServices = new DicomService(localNode, remoteNode);

                _log.Info($"Establishing connection to AET [{remoteNode.AeTitle}]...");
                dicomServices.CheckRemoteNodeAvailability();

                _log.Info($"Sending results to AET [{remoteNode.AeTitle}]...");

                foreach (var result in results)
                {
                    // NOTE If there are non-dicom files in the directory you'll get an exception 
                    // That looks like -- ClearCanvas.Dicom.DicomException: Invalid abstract syntax for presentation context, UID is zero length. 
                    // TODO: Handle this better.
                    var resultDicomFiles = Directory.GetFiles(result.DicomFolderPath);
                    dicomServices.SendDicomFiles(resultDicomFiles);
                }
                _log.Info($"Finished sending results to AET [{remoteNode.AeTitle}]");

                //_log.Info($"Sending resliced prior series to AET [{remoteNode.AeTitle}]...");
                //var priorReslicedDicomFiles = Directory.GetFiles(PriorReslicedSeriesDicomFolder);
                //dicomServices.SendDicomFiles(priorReslicedDicomFiles);
                //_log.Info($"Finished sending resliced prior series to AET [{remoteNode.AeTitle}]");
            }
        }
    }
}
