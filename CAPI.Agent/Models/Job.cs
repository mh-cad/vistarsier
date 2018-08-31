using CAPI.Agent.Abstractions.Models;
using CAPI.Common.Abstractions.Services;
using CAPI.Common.Config;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CAPI.Agent.Models
{
    public class Job : IJob
    {
        private readonly Recipe _recipe;
        private readonly IDicomServices _dicomServices;
        private readonly IImageProcessingFactory _imgProcFactory;
        private readonly IFileSystem _filesystem;
        private readonly IProcessBuilder _processBuilder;
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
        public bool BiasFieldCorrection { get; set; }
        public string BiasFieldCorrectionParams { get; set; }

        public string Status { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        // Needed for EntityFramework
        public Job() { }

        public Job(Recipe recipe,
                   IDicomServices dicomServices, IImageProcessingFactory imgProcFactory,
                   IFileSystem filesystem, IProcessBuilder processBuilder,
                   CapiConfig capiConfig, ILog log)
        {
            _recipe = recipe;
            _dicomServices = dicomServices;
            _imgProcFactory = imgProcFactory;
            _filesystem = filesystem;
            _processBuilder = processBuilder;
            _capiConfig = capiConfig;
            _log = log;

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

        [NotMapped]
        public string CurrentSeriesDicomFolder { get; set; }
        [NotMapped]
        public string PriorSeriesDicomFolder { get; set; }
        [NotMapped]
        public string ResultSeriesDicomFolder { get; set; }
        [NotMapped]
        public string PriorReslicedSeriesDicomFolder { get; set; }
        [NotMapped]
        public string ProcessingFolder { get; set; }

        public void Process()
        {
            IJob job = this;

            job.Start = DateTime.Now;
            job.Status = "Processing";

            var context = new AgentRepository();

            context.Jobs.Add((Job)job);
            context.SaveChanges();

            _log.Info($"{Environment.NewLine}");
            _log.Info($"Job processing started...{Environment.NewLine}");
            _log.Info($"Job Id: [{job.Id}]");
            _log.Info($"Current Accession: [{CurrentAccession}]");
            _log.Info($"Prior Accession: [{PriorAccession}]");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _capiConfig.ImgProcConfig = UpdateImgProcConfig(_recipe);
            var imageProcessor = new ImageProcessor(_dicomServices, _imgProcFactory,
                                                    _filesystem, _processBuilder,
                                                    _capiConfig.ImgProcConfig, _log);

            var sliceType = GetSliceType(_recipe.SliceType);

            job = imageProcessor.CompareAndSendToFilesystem(job, _recipe, sliceType);

            SendToDestinations();

            Directory.Delete(ProcessingFolder, true);

            End = DateTime.Now;
            Status = "Complete";
            context.Jobs.Update((Job)job);
            context.SaveChanges();

            stopwatch.Stop();
            _log.Info($"Job Id=[{Id}] completed in {stopwatch.Elapsed.Minutes} minutes and " +
                      $"{stopwatch.Elapsed.Seconds} seconds.");
            _log.Info("-------------------------");
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
            if (string.IsNullOrEmpty(ResultSeriesDicomFolder) || Directory.GetFiles(ResultSeriesDicomFolder).Length == 0)
                throw new DirectoryNotFoundException($"No folder found for {nameof(ResultSeriesDicomFolder)} " +
                                                     $"at following path: [{ResultSeriesDicomFolder}] or empty!");

            if (string.IsNullOrEmpty(PriorReslicedSeriesDicomFolder) || Directory.GetFiles(PriorReslicedSeriesDicomFolder).Length == 0)
                throw new DirectoryNotFoundException($"No folder found for {nameof(PriorReslicedSeriesDicomFolder)} " +
                                                     $"at following path: [{PriorReslicedSeriesDicomFolder}] or empty!");

            _log.Info("Sending to destinations...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            if (_recipe.DicomDestinations != null)
                foreach (var dicomDestination in _recipe.DicomDestinations)
                {
                    var localNode = _capiConfig.DicomConfig.LocalNode;//  _capiConfig.DicomConfig.LocalNode;
                    var remoteNode = _capiConfig.DicomConfig.RemoteNodes
                        .SingleOrDefault(n => n.AeTitle.Equals(dicomDestination, StringComparison.InvariantCultureIgnoreCase));

                    _dicomServices.CheckRemoteNodeAvailability(localNode, remoteNode);

                    var resultDicomFiles = Directory.GetFiles(ResultSeriesDicomFolder);
                    foreach (var resultDicomFile in resultDicomFiles)
                        _dicomServices.SendDicomFile(resultDicomFile, localNode.AeTitle, remoteNode);

                    var priorReslicedDicomFiles = Directory.GetFiles(ResultSeriesDicomFolder);
                    foreach (var priorReslicedDicomFile in priorReslicedDicomFiles)
                        _dicomServices.SendDicomFile(priorReslicedDicomFile, localNode.AeTitle, remoteNode);
                }

            if (_recipe.FilesystemDestinations != null)
                foreach (var fsDestination in _recipe.FilesystemDestinations)
                {
                    var jobFolderName = Path.GetFileName(ProcessingFolder) ?? "";
                    var destJobFolderPath = Path.Combine(fsDestination, jobFolderName);

                    if (_recipe.OnlyCopyResults)
                    {
                        var resultsDestFolder = Path.Combine(destJobFolderPath, Path.GetFileName(ResultSeriesDicomFolder));
                        _filesystem.CopyDirectory(ResultSeriesDicomFolder, resultsDestFolder);
                    }
                    else
                    {
                        _filesystem.CopyDirectory(ProcessingFolder, destJobFolderPath);
                    }
                    //var priorReslicedDestFolder = Path.Combine(jobFolderPath, Path.GetFileName(PriorReslicedSeriesDicomFolder));
                    //_filesystem.CopyDirectory(PriorReslicedSeriesDicomFolder, priorReslicedDestFolder);
                }

            stopwatch.Stop();
            _log.Info($"Finished sending to destinations in {Math.Round(stopwatch.Elapsed.TotalSeconds)} seconds");
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
