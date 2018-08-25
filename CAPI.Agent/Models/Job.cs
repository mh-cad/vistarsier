using CAPI.Agent.Abstractions.Models;
using CAPI.Common.Abstractions.Services;
using CAPI.Common.Config;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using System;
using System.ComponentModel.DataAnnotations.Schema;
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

        public string Id { get; set; }
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

            DefaultDestination = recipe.DicomDestinations != null ?
                recipe.DicomDestinations.FirstOrDefault() :
                recipe.FilesystemDestinations.FirstOrDefault();
            //Destinations.FirstOrDefault()?.DisplayName;
        }

        [NotMapped]
        public string CurrentSeriesDicomFolder { get; set; }
        [NotMapped]
        public string PriorSeriesDicomFolder { get; set; }
        [NotMapped]
        public string ResultSeriesDicomFolder { get; set; }
        [NotMapped]
        public string PriorReslicedSeriesDicomFolder { get; set; }

        public void Process()
        {
            Start = DateTime.Now;
            Status = "Processing";

            var context = new AgentRepository();
            context.Jobs.Add(this);
            context.SaveChanges();

            _log.Info($"{Environment.NewLine}");
            _log.Info($"Job processing started...{Environment.NewLine}");
            _log.Info($"Job Id: [{Id}]");
            _log.Info($"Current Accession: [{CurrentAccession}]");
            _log.Info($"Prior Accession: [{PriorAccession}]");

            var imageProcessor = new ImageProcessor(_dicomServices, _imgProcFactory,
                                                    _filesystem, _processBuilder, _capiConfig.ImgProcConfig, _log);

            var sliceType = GetSliceType(_recipe.SliceType);
            imageProcessor.CompareAndSendToFilesystem(
                CurrentSeriesDicomFolder, PriorSeriesDicomFolder, _recipe.LookUpTablePath, sliceType,
                ExtractBrain, Register, BiasFieldCorrection,
                ResultSeriesDicomFolder, PriorReslicedSeriesDicomFolder);

            SendToDestinations();

            End = DateTime.Now;
            Status = "Complete";
            context.Jobs.Update(this);
            context.SaveChanges();

            _log.Info($"Job Id=[{Id}] completed.");
            _log.Info("-------------------------");
        }

        private void SendToDestinations()
        {
            if (string.IsNullOrEmpty(ResultSeriesDicomFolder) || Directory.GetFiles(ResultSeriesDicomFolder).Length == 0)
                throw new DirectoryNotFoundException($"No folder found for {nameof(ResultSeriesDicomFolder)} " +
                                                     $"at following path: [{ResultSeriesDicomFolder}] or empty!");

            if (string.IsNullOrEmpty(PriorReslicedSeriesDicomFolder) || Directory.GetFiles(PriorReslicedSeriesDicomFolder).Length == 0)
                throw new DirectoryNotFoundException($"No folder found for {nameof(PriorReslicedSeriesDicomFolder)} " +
                                                     $"at following path: [{PriorReslicedSeriesDicomFolder}] or empty!");

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
                foreach (var fsDestinations in _recipe.FilesystemDestinations)
                {
                    var resultsDestFolder = Path.Combine(fsDestinations, Path.GetFileName(ResultSeriesDicomFolder));
                    _filesystem.CopyDirectory(ResultSeriesDicomFolder, resultsDestFolder);

                    var priorReslicedDestFolder = Path.Combine(fsDestinations, Path.GetFileName(PriorReslicedSeriesDicomFolder));
                    _filesystem.CopyDirectory(PriorReslicedSeriesDicomFolder, priorReslicedDestFolder);
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
