using VisTarsier.Config;
using VisTarsier.Common;
using log4net;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SliceType = VisTarsier.NiftiLib.SliceType;
using VisTarsier.Dicom;
using System.ComponentModel.DataAnnotations;

namespace VisTarsier.Service
{
    public class Job
    {
        private readonly CapiConfig _capiConfig;
        private readonly ILog _log;


        public readonly Recipe Recipe;
        public long Id { get; set; }
        public string Status { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string RecipeString { get; set; }
        public string DbExt { get; set; }

        [NotMapped]
        public Attempt Attempt { get; set; }
        [NotMapped]
        public string CurrentSeriesDicomFolder { get; set; }
        [NotMapped]
        public string PriorSeriesDicomFolder { get; set; }
        [NotMapped]
        public string ReferenceSeriesDicomFolder { get; set; }
       // [NotMapped]
        //public IJobResult[] Results { get; set; }
        [NotMapped]
        public string ResultSeriesDicomFolder { get; set; }
        [NotMapped]
        public string PriorReslicedSeriesDicomFolder { get; set; }
        [NotMapped]
        public string ProcessingFolder { get; set; }
        [NotMapped]
        public string DefaultDestination { get; set; }

        // Needed for EntityFramework
        public Job() { }

        public Job(Recipe recipe, Attempt attempt)
        {
            Recipe = recipe;
            _capiConfig = CapiConfig.GetConfig();
            _log = Log.GetLogger();
            Attempt = attempt;

            Attempt.SourceAet = recipe.SourceAet;
            Attempt.PatientId = recipe.PatientId;
            Attempt.PatientFullName = recipe.PatientFullName;
            Attempt.PatientBirthDate = recipe.PatientBirthDate;
            Attempt.CurrentAccession = recipe.CurrentAccession;
            Attempt.PriorAccession = recipe.PriorAccession;

            DefaultDestination =
                recipe.DicomDestinations != null && !string.IsNullOrEmpty(recipe.DicomDestinations.FirstOrDefault()) ?
                recipe.DicomDestinations.FirstOrDefault() :
                recipe.FilesystemDestinations.FirstOrDefault();
        }

        public void Process(DbBroker dbBroker)
        {
            var @job = this;

            @job.Start = DateTime.Now;
            @job.Status = "Processing";
            
            if (dbBroker.Jobs.Find(@job.Id) == null) dbBroker.Jobs.Add(@job);
            else dbBroker.Jobs.Update(@job);
            dbBroker.SaveChanges();
            job.Attempt.JobId = @job.Id;
            dbBroker.Attempts.Update(Attempt);
            dbBroker.SaveChanges();

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

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _capiConfig.ImagePaths = UpdateImgProcConfig(Recipe);
            var jobProcessor = new JobProcessor(dbBroker);

            var sliceType = GetSliceType(Recipe.SliceType);

            try
            {
                var preResults = jobProcessor.GenerateMetadataSlides(job);
                SendToDestinations(preResults);
            }
            catch (Exception e)
            {
                _log.Error("Failed to generate pre-images. :(");
                _log.Error(e.Message);
                _log.Error(e.StackTrace);
            }

            var results = jobProcessor.CompareAndSaveLocally(job, Recipe, sliceType);
            SendToDestinations(results);

            Directory.Delete(ProcessingFolder, true);

            End = DateTime.Now;
            Status = "Complete";
            var jobToUpdate = dbBroker.Jobs.SingleOrDefault(j => j.Id == Id);
            if (jobToUpdate == null) throw new Exception($"Job with id [{Id}] not found");
            dbBroker.Jobs.Update(jobToUpdate);
            dbBroker.SaveChanges();

            stopwatch.Stop();

            _log.Info($"{Environment.NewLine}");
            _log.Info($"****************  JOB COMPLETE  **********************************");
            _log.Info($" Job ID               *  {job.Id}");
            _log.Info($" Processing Time      *  {job.End - job.Start}");
            _log.Info($"*****************************************************************");
            _log.Info($"{Environment.NewLine}");
            _log.Info($"{Environment.NewLine}");
            _log.Info($"Job Id=[{Id}] completed in {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds:D2} minutes.");
        }

        public string GetStudyIdFromReferenceSeries()
        {
            return Attempt.ReferenceSeries.Split('|')[0];
        }

        public string GetSeriesIdFromReferenceSeries()
        {
            return Attempt.ReferenceSeries.Split('|').Length < 2 ? string.Empty :
                                                           Attempt.ReferenceSeries.Split('|')[1];
        }

        public void WriteStudyAndSeriesIdsToReferenceSeries(string studyId, string seriesId)
        {
            Attempt.ReferenceSeries = string.Join("|", studyId, seriesId);
        }

        private ImagePaths UpdateImgProcConfig(IRecipe recipe)
        {
            var imgProcConfig = _capiConfig.ImagePaths;

            if (!string.IsNullOrEmpty(recipe.ResultsDicomSeriesDescription.Trim()))
                imgProcConfig.ResultsDicomSeriesDescription = recipe.ResultsDicomSeriesDescription;

            if (!string.IsNullOrEmpty(recipe.PriorReslicedDicomSeriesDescription.Trim()))
                imgProcConfig.PriorReslicedDicomSeriesDescription = recipe.PriorReslicedDicomSeriesDescription;

            return imgProcConfig;
        }

        private void SendToDestinations(JobResult[] results)
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
            SendToDicomDestinations(results);
            // Sending to Filesystem Destinations
            SendToFilesystemDestinations(results);

            stopwatch.Stop();
            _log.Info($"Finished sending to destinations in {Math.Round(stopwatch.Elapsed.TotalSeconds)} seconds");
        }
        private void SendToFilesystemDestinations(JobResult[] results)
        {
            if (Recipe.FilesystemDestinations == null) return;
            foreach (var fsDestination in Recipe.FilesystemDestinations)
            {
                var jobFolderName = Path.GetFileName(ProcessingFolder) ?? throw new InvalidOperationException();
                var destJobFolderPath = Path.Combine(fsDestination, jobFolderName);

                _log.Info($"Sending to folder [{destJobFolderPath}]...");
                if (Recipe.OnlyCopyResults)
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
                    FileSystem.CopyDirectory(ProcessingFolder, destJobFolderPath);
                }
            }
        }
        private void SendToDicomDestinations(JobResult[] results)
        {
            if (Recipe.DicomDestinations == null) return;
            foreach (var dicomDestination in Recipe.DicomDestinations)
            {
                var localNode = _capiConfig.DicomConfig.LocalNode;
                IDicomNode remoteNode;
                try
                {
                    remoteNode = _capiConfig.DicomConfig.RemoteNodes
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

        private SliceType GetSliceType(string sliceType)
        {
            switch (sliceType)
            {
                case "Sag":
                    return SliceType.Sagittal;
                case "Ax":
                    return SliceType.Axial;
                case "Cor":
                    return SliceType.Coronal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sliceType), "SliceType should be either [Sag], [Ax] or [Cor]");
            }
        }
    }
}
