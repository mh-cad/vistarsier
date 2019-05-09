﻿using CAPI.Service.Db;
using CAPI.Config;
using CAPI.Dicom.Abstractions;
using CAPI.Common;
using log4net;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SliceType = CAPI.NiftiLib.SliceType;
using CAPI.Service.Agent;

namespace CAPI.Service.Db
{
    public class Job : IJob
    {
        private readonly Recipe _recipe;
        private readonly IDicomServices _dicomServices;
        private readonly CapiConfig _capiConfig;
        private readonly ILog _log;

        public long Id { get; set; }
        public string SourceAet { get; set; }
        public string PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientBirthDate { get; set; }
        public string CurrentAccession { get; set; }
        public string PriorAccession { get; set; }
        public string DefaultDestination { get; set; }
        public bool ExtractBrain { get; set; }
        public string ExtractBrainParams { get; set; }
        public bool Register { get; set; }
        //public string RegistrationData { get; set; }
        public string ReferenceSeries { get; set; }
        public bool BiasFieldCorrection { get; set; }
        public string BiasFieldCorrectionParams { get; set; }

        public string Status { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        [NotMapped]
        public string CurrentSeriesDicomFolder { get; set; }
        [NotMapped]
        public string PriorSeriesDicomFolder { get; set; }
        [NotMapped]
        public string ReferenceSeriesDicomFolder { get; set; }
        [NotMapped]
        public IJobResult[] Results { get; set; }
        [NotMapped]
        public string ResultSeriesDicomFolder { get; set; }
        [NotMapped]
        public string PriorReslicedSeriesDicomFolder { get; set; }
        [NotMapped]
        public string ProcessingFolder { get; set; }

        // Needed for EntityFramework
        public Job() { }

        public Job(Recipe recipe,
                   IDicomServices dicomServices,
                   CapiConfig capiConfig)
        {
            _recipe = recipe;
            _dicomServices = dicomServices;
            _capiConfig = capiConfig;
            _log = Log.GetLogger();

            SourceAet = recipe.SourceAet;
            PatientId = recipe.PatientId;
            PatientFullName = recipe.PatientFullName;
            PatientBirthDate = recipe.PatientBirthDate;
            CurrentAccession = recipe.CurrentAccession;
            PriorAccession = recipe.PriorAccession;

            ExtractBrain = recipe.ExtractBrain;
            ExtractBrainParams = recipe.ExtractBrainParams;
            Register = recipe.Register;
            BiasFieldCorrection = recipe.BiasFieldCorrection;
            BiasFieldCorrectionParams = recipe.BiasFieldCorrectionParams;

            DefaultDestination =
                recipe.DicomDestinations != null && !string.IsNullOrEmpty(recipe.DicomDestinations.FirstOrDefault()) ?
                recipe.DicomDestinations.FirstOrDefault() :
                recipe.FilesystemDestinations.FirstOrDefault();
        }

        public void Process()
        {
            var job = this;

            job.Start = DateTime.Now;
            job.Status = "Processing";

            var context = new DbBroker(_capiConfig.AgentDbConnectionString);

            context.Jobs.Add(job);
            context.SaveChanges();

            _log.Info($"{Environment.NewLine}");
            _log.Info($"Job processing started...{Environment.NewLine}");
            _log.Info($"Job Id: [{job.Id}]");
            _log.Info($"Current Accession: [{CurrentAccession}]");
            _log.Info($"Prior Accession: [{PriorAccession}]");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _capiConfig.ImgProcConfig = UpdateImgProcConfig(_recipe);
            var imageProcessor = new JobProcessor(_dicomServices,
                                                    _capiConfig.ImgProcConfig, context);

            var sliceType = GetSliceType(_recipe.SliceType);

            Results = imageProcessor.CompareAndSaveLocally(job, _recipe, sliceType);

            SendToDestinations();

            Directory.Delete(ProcessingFolder, true);

            End = DateTime.Now;
            Status = "Complete";
            var jobToUpdate = context.Jobs.SingleOrDefault(j => j.Id == Id);
            if (jobToUpdate == null) throw new Exception($"Job with id [{Id}] not found");
            context.Jobs.Update(jobToUpdate);
            context.SaveChanges();

            stopwatch.Stop();
            _log.Info($"Job Id=[{Id}] completed in {stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds:D2} minutes.");
        }

        public string GetStudyIdFromReferenceSeries()
        {
            return ReferenceSeries.Split('|')[0];
        }

        public string GetSeriesIdFromReferenceSeries()
        {
            return ReferenceSeries.Split('|').Length < 2 ? string.Empty :
                                                           ReferenceSeries.Split('|')[1];
        }

        public void WriteStudyAndSeriesIdsToReferenceSeries(string studyId, string seriesId)
        {
            ReferenceSeries = string.Join("|", studyId, seriesId);
        }

        private ImgProcConfig UpdateImgProcConfig(IRecipe recipe)
        {
            var imgProcConfig = _capiConfig.ImgProcConfig;

            if (!string.IsNullOrEmpty(recipe.ExtractBrainParams))
                imgProcConfig.BseParams = recipe.ExtractBrainParams;

            if (!string.IsNullOrEmpty(recipe.BiasFieldCorrectionParams))
                imgProcConfig.BfcParams = recipe.BiasFieldCorrectionParams;

            if (!string.IsNullOrEmpty(recipe.ResultsDicomSeriesDescription.Trim()))
                imgProcConfig.ResultsDicomSeriesDescription = recipe.ResultsDicomSeriesDescription;

            if (!string.IsNullOrEmpty(recipe.PriorReslicedDicomSeriesDescription.Trim()))
                imgProcConfig.PriorReslicedDicomSeriesDescription = recipe.PriorReslicedDicomSeriesDescription;

            return imgProcConfig;
        }

        private void SendToDestinations()
        {
            if (string.IsNullOrEmpty(ResultSeriesDicomFolder) ||
                Directory.GetFiles(ResultSeriesDicomFolder).Length == 0 &&
                Directory.GetDirectories(ResultSeriesDicomFolder).Length == 0)
                throw new DirectoryNotFoundException($"No folder found for {nameof(ResultSeriesDicomFolder)} " +
                                                     $"at following path: [{ResultSeriesDicomFolder}] or empty!");

            if (string.IsNullOrEmpty(PriorReslicedSeriesDicomFolder) || Directory.GetFiles(PriorReslicedSeriesDicomFolder).Length == 0)
                throw new DirectoryNotFoundException($"No folder found for {nameof(PriorReslicedSeriesDicomFolder)} " +
                                                     $"at following path: [{PriorReslicedSeriesDicomFolder}] or empty!");

            _log.Info("Sending to destinations...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Sending to Dicom Destinations
            SendToDicomDestinations();
            // Sending to Filesystem Destinations
            SendToFilesystemDestinations();

            stopwatch.Stop();
            _log.Info($"Finished sending to destinations in {Math.Round(stopwatch.Elapsed.TotalSeconds)} seconds");
        }
        private void SendToFilesystemDestinations()
        {
            if (_recipe.FilesystemDestinations == null) return;
            foreach (var fsDestination in _recipe.FilesystemDestinations)
            {
                var jobFolderName = Path.GetFileName(ProcessingFolder) ?? throw new InvalidOperationException();
                var destJobFolderPath = Path.Combine(fsDestination, jobFolderName);

                _log.Info($"Sending to folder [{destJobFolderPath}]...");
                if (_recipe.OnlyCopyResults)
                {
                    foreach (var result in Results)
                    {
                        var resultParentFolderPath = Path.GetDirectoryName(result.NiftiFilePath);
                        var resultParentFolderName = Path.GetFileName(resultParentFolderPath);
                        var resultDestinationFolder = Path.Combine(destJobFolderPath, resultParentFolderName ?? throw new InvalidOperationException());
                        FileSystem.CopyDirectory(resultParentFolderPath, resultDestinationFolder);
                    }

                    var priorFolderName = Path.GetFileName(PriorReslicedSeriesDicomFolder);
                    var priorDestinationFolder = Path.Combine(destJobFolderPath, priorFolderName ?? throw new InvalidOperationException());
                    FileSystem.CopyDirectory(PriorReslicedSeriesDicomFolder, priorDestinationFolder);
                }
                else
                {
                    FileSystem.CopyDirectory(ProcessingFolder, destJobFolderPath);
                }
            }
        }
        private void SendToDicomDestinations()
        {
            if (_recipe.DicomDestinations == null) return;
            foreach (var dicomDestination in _recipe.DicomDestinations)
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

                _log.Info($"Establishing connection to AET [{remoteNode.AeTitle}]...");
                _dicomServices.CheckRemoteNodeAvailability(localNode, remoteNode);

                _log.Info($"Sending results to AET [{remoteNode.AeTitle}]...");
                foreach (var result in Results)
                {
                    var resultDicomFiles = Directory.GetFiles(result.DicomFolderPath);
                    _dicomServices.SendDicomFiles(resultDicomFiles, localNode.AeTitle, remoteNode);
                }
                _log.Info($"Finished sending results to AET [{remoteNode.AeTitle}]");

                _log.Info($"Sending resliced prior series to AET [{remoteNode.AeTitle}]...");
                var priorReslicedDicomFiles = Directory.GetFiles(PriorReslicedSeriesDicomFolder);
                _dicomServices.SendDicomFiles(priorReslicedDicomFiles, localNode.AeTitle, remoteNode);
                _log.Info($"Finished sending resliced prior series to AET [{remoteNode.AeTitle}]");
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