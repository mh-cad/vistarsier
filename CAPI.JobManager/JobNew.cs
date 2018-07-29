using CAPI.Common.Services;
using CAPI.DAL.Abstraction;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager.Abstraction;
using log4net;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Linq;
using System.Threading;

namespace CAPI.JobManager
{
    public class JobNew<T> : IJobNew<T>
    {
        private readonly IJobManagerFactory _jobManagerFactory;
        private readonly IDicomFactory _dicomFactory;
        private readonly IDicomNode _localNode;
        private readonly IDicomNode _remoteNode;
        private readonly IImageConverter _imageConverter;
        private readonly IDicomNodeRepository _dicomNodeRepository;
        private readonly ILog _log;

        private const string FixedTitle = "Fixed";
        private const string FloatingTitle = "Floating";
        private const string PositiveDcmFolderName = "flair_new_with_changes_overlay_positive_dcm";
        private const string NegativeDcmFolderName = "flair_new_with_changes_overlay_negative_dcm";
        private const string IncreasedSignalSeriesName = "CAPI Increased Signal";
        private const string DecreasedSignalSeriesName = "CAPI Decreased Signal";
        private const string DicomFolderSuffix = "_dcm";

        public string OutputFolderPath { get; set; }
        public IJobSeriesBundleNew Fixed { get; set; }
        public IJobSeriesBundleNew Floating { get; set; }
        public string ChangesDarkInFloating2BrightInFixed { get; set; }
        public string ChangesBrightInFloating2DarkInFixed { get; set; }
        public string ChangesBrainMask { get; set; }
        public string PositiveOverlayImageFolder { get; set; }
        public string PositiveOverlayDicomFolder { get; set; }
        public string NegativeOverlayImageFolder { get; set; }
        public string NegativeOverlayDicomFolder { get; set; }
        public IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        public IList<IDestination> Destinations { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="jobManagerFactory"></param>
        /// <param name="dicomFactory"></param>
        /// <param name="localNode"></param>
        /// <param name="remoteNode"></param>
        /// <param name="imageConverter"></param>
        /// <param name="dicomNodeRepository"></param>
        public JobNew(IJobManagerFactory jobManagerFactory, IDicomFactory dicomFactory,
            IDicomNode localNode, IDicomNode remoteNode,
            IImageConverter imageConverter,
            IDicomNodeRepository dicomNodeRepository)
        {
            _jobManagerFactory = jobManagerFactory;
            _dicomFactory = dicomFactory;
            _localNode = localNode;
            _remoteNode = remoteNode;
            _imageConverter = imageConverter;
            _dicomNodeRepository = dicomNodeRepository;
            _log = LogHelper.GetLogger();

            Fixed = new JobSeriesBundleNew { Title = FixedTitle };
            Floating = new JobSeriesBundleNew { Title = FloatingTitle };
            IntegratedProcesses = new List<IIntegratedProcess>();
            Destinations = new List<IDestination>();
        }

        public void Run()
        {
            var accession = Fixed.ParentDicomStudy.AccessionNumber;

            _log.Info($"Started processing job for accession [{accession}] ...");

            try
            {
                FetchSeriesAndSaveToDisk();

                ConvertDicomToNii();

                // Process each integrated process
                foreach (var integratedProcess in IntegratedProcesses)
                {
                    switch (integratedProcess.Type)
                    {
                        case IntegratedProcessType.ExtractBrainSurface:
                            RunExtractBrainSurfaceProcess(integratedProcess);
                            break;
                        case IntegratedProcessType.Registeration:
                            RunRegistrationProcess(integratedProcess);
                            break;
                        case IntegratedProcessType.TakeDifference:
                            RunTakeDifference(integratedProcess);
                            break;
                        case IntegratedProcessType.ColorMap:
                            RunColorMap(integratedProcess);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                } // TODO2: Add logging to integrated processes

                var dicomServices = _dicomFactory.CreateDicomServices();

                NegativeOverlayDicomFolder = NegativeOverlayImageFolder + DicomFolderSuffix;
                dicomServices.ConvertBmpsToDicom(
                    NegativeOverlayImageFolder, $@"{NegativeOverlayDicomFolder}", Fixed.DicomFolderPath);

                PositiveOverlayDicomFolder = PositiveOverlayImageFolder + DicomFolderSuffix;
                dicomServices.ConvertBmpsToDicom(
                    PositiveOverlayImageFolder, $@"{PositiveOverlayDicomFolder}", Fixed.DicomFolderPath);

                UpdateDicomHeaders();

                SendDicomFilesToDestinations();

                DeleteDirectoryInImageRepo(OutputFolderPath);

                _log.Info($"Finished processing job for accession [{accession}] successfully.");
            }
            catch (Exception ex)
            {
                _log.Error($"Job failed for accession [{accession}] ...", ex);
                throw;
            }
        }

        private void FetchSeriesAndSaveToDisk()
        {
            _log.Info("Receiving dicom files from source");

            var dicomServices = _dicomFactory.CreateDicomServices();
            dicomServices.CheckRemoteNodeAvailability(_localNode, _remoteNode);

            var patientFullName = Fixed.ParentDicomStudy.PatientsName.Replace("^", "_");
            var jobId = $"{DateTime.Now:yyyyMMddHHmmss}_{patientFullName}";
            OutputFolderPath = $@"{OutputFolderPath}\{jobId}";
            FileSystem.DirectoryExistsIfNotCreate(OutputFolderPath);

            Fixed.DicomFolderPath =
                SaveSeriesToDisk(OutputFolderPath, Fixed.Title, Fixed.ParentDicomStudy, dicomServices);

            Floating.DicomFolderPath =
                SaveSeriesToDisk(OutputFolderPath, Floating.Title, Floating.ParentDicomStudy, dicomServices);
        }
        private string SaveSeriesToDisk(string outputPath, string seriesPrefix,
            IDicomStudy study, IDicomServices dicomServices)
        {
            var series = study.Series.ToList().FirstOrDefault();

            // Retrieve Series From Pacs
            try
            {
                dicomServices.SaveSeriesToLocalDisk(series, outputPath, _localNode, _remoteNode);
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to retrieve images from PACS for series [{series?.SeriesDescription}]", ex);
                throw;
            }

            // Rename folders
            string newDicomFolderPath;
            try
            {
                newDicomFolderPath = RenameFolders(outputPath, series, seriesPrefix, 0);
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to rename folders for series [{series?.SeriesDescription}]", ex);
                throw;
            }

            return newDicomFolderPath;
        }
        private static string RenameFolders(string outputPath, IDicomSeries series,
            string seriesPrefix, int tryCount)
        {
            if (tryCount > 2) throw new DirectoryException("Failed to rename folders");

            var newDicomFolderPath = string.Empty;
            try
            {
                // Rename Study Folder Name to Fixed/Floating (Series Prefix)
                var studyFolderPath = $@"{outputPath}\{series?.StudyInstanceUid}";
                var newStudyPath = $@"{outputPath}\{seriesPrefix}";
                if (Directory.Exists(studyFolderPath))
                    Directory.Move(studyFolderPath, newStudyPath);

                // Rename Dicom Series Folder Name
                var dicomFolderPath = $@"{newStudyPath}\{series?.SeriesInstanceUid}";
                newDicomFolderPath = $@"{newStudyPath}\Dicom";
                if (Directory.Exists(dicomFolderPath))
                    Directory.Move(dicomFolderPath, newDicomFolderPath);
            }
            catch
            {
                tryCount++;
                Thread.Sleep(1000);
                RenameFolders(outputPath, series, seriesPrefix, tryCount);
                return newDicomFolderPath;
            }

            return newDicomFolderPath;
        }

        public void ConvertDicomToNii()
        {
            _imageConverter.DicomToNii(Fixed.DicomFolderPath, $@"{OutputFolderPath}\{Fixed.Title}", Fixed.Title);
            _imageConverter.DicomToNii(Floating.DicomFolderPath, $@"{OutputFolderPath}\{Floating.Title}", Floating.Title);
        }

        public void RunExtractBrainSurfaceProcess(IIntegratedProcess integratedProcess)
        {
            var extractBrainSurfaceProcess = _jobManagerFactory
                .CreateExtractBrinSurfaceIntegratedProcess(
                integratedProcess.Version, integratedProcess.Parameters);

            extractBrainSurfaceProcess.OnStart += Process_OnStart;
            extractBrainSurfaceProcess.OnComplete += Process_OnComplete;

            extractBrainSurfaceProcess.Run(this as IJobNew<IRecipe>);
        }

        public void RunBiasFieldCorrectionProcess(IIntegratedProcess integratedProcess)
        {
            throw new NotImplementedException();
        }

        public void RunRegistrationProcess(IIntegratedProcess integratedProcess)
        {
            var registrationProcess = _jobManagerFactory
                .CreateRegistrationIntegratedProcess(
                    integratedProcess.Version, integratedProcess.Parameters);

            registrationProcess.OnStart += Process_OnStart;
            registrationProcess.OnComplete += Process_OnComplete;

            registrationProcess.Run(this as IJobNew<IRecipe>);
        }
        public void RunTakeDifference(IIntegratedProcess integratedProcess)
        {
            var takeDifferenceProcess = _jobManagerFactory
                .CreateTakeDifferenceIntegratedProcess(
                    integratedProcess.Version, integratedProcess.Parameters);

            takeDifferenceProcess.OnStart += Process_OnStart;
            takeDifferenceProcess.OnComplete += Process_OnComplete;

            takeDifferenceProcess.Run(this as IJobNew<IRecipe>);
        }
        public void RunColorMap(IIntegratedProcess integratedProcess)
        {
            var colorMapProcess = _jobManagerFactory
                .CreateColorMapIntegratedProcess(
                    integratedProcess.Version, integratedProcess.Parameters);

            colorMapProcess.OnStart += Process_OnStart;
            colorMapProcess.OnComplete += Process_OnComplete;

            colorMapProcess.Run(this as IJobNew<IRecipe>);
        }

        private void UpdateDicomHeaders()
        {
            _log.Info("Updating Dicom Headers");

            // Update Negative Folder
            UpdateDicomHeaders(OutputFolderPath, NegativeDcmFolderName, DecreasedSignalSeriesName);
            _log.Info("Finished updating decreased signal dicom headers");

            // Update Positive Folder
            UpdateDicomHeaders(OutputFolderPath, PositiveDcmFolderName, IncreasedSignalSeriesName);
            _log.Info("Finished updating increased signal dicom headers");
        }
        private void UpdateDicomHeaders(string outputFolderPath, string dicomFolderName, string seriesName)
        {
            var dicomServices = _dicomFactory.CreateDicomServices();

            var dicomFiles = Directory.GetFiles($@"{outputFolderPath}\{dicomFolderName}");

            var dicomTags = _dicomFactory.CreateDicomTagCollection();
            dicomTags.SeriesDescription.Values = new[] { seriesName };

            dicomServices.UpdateSeriesHeadersForAllFiles(dicomFiles, dicomTags);
        }

        private void SendDicomFilesToDestinations()
        {
            foreach (var destination in Destinations)
            {
                try
                {
                    destination.DicomNode = _dicomNodeRepository.GetAll()
                        .FirstOrDefault(n => n.AeTitle == destination.AeTitle);

                    if (string.IsNullOrEmpty(destination.AeTitle))
                    // AET is not defined
                    {
                        var directoryName = Path.GetFileName(OutputFolderPath);
                        var targetDirectory = $@"{destination.FolderPath}\{directoryName}";
                        _log.Info($"Copying files to {targetDirectory}...");
                        FileSystem.CopyDirectory(OutputFolderPath, targetDirectory);
                        _log.Info($"Finished copying files to destination successfully [{targetDirectory}]");
                    }
                    else
                    // AET is defined
                    {
                        _log.Info($"Sending files to Dicom node AET: [{destination.AeTitle}] ...");

                        var dicomServices = _dicomFactory.CreateDicomServices();

                        var negativeFiles = Directory.GetFiles(NegativeOverlayDicomFolder);
                        foreach (var negativeFile in negativeFiles)
                            dicomServices.SendDicomFile(negativeFile, _localNode.AeTitle, destination.DicomNode);

                        var positiveFiles = Directory.GetFiles(PositiveOverlayDicomFolder);
                        foreach (var positiveFile in positiveFiles)
                            dicomServices.SendDicomFile(positiveFile, _localNode.AeTitle, destination.DicomNode);

                        _log.Info($"Finished sending files to Dicom node AET: [{destination.AeTitle}]");
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(
                        !string.IsNullOrEmpty(destination.AeTitle)
                            ? $"Failed to send images to AET [{destination.AeTitle}]"
                            : $"Failed to copy files to folder [{destination.FolderPath}]", ex);
                    throw;
                }
            }

        }

        private void DeleteDirectoryInImageRepo(string outputFolderPath)
        {
            try
            {
                Directory.Delete(OutputFolderPath, true);
            }
            catch
            {
                _log.Error($"Failed to delete folder: [{outputFolderPath}]");
                throw;
            }
        }

        private void Process_OnStart(object sender, IProcessEventArgument e)
        {
            OnEachProcessStarted?.Invoke(sender, e);
        }
        private void Process_OnComplete(object sender, IProcessEventArgument e)
        {
            OnEachProcessCompleted?.Invoke(sender, e);
        }

        public event EventHandler<IProcessEventArgument> OnEachProcessStarted;
        public event EventHandler<IProcessEventArgument> OnEachProcessCompleted;
        public event EventHandler<ILogEventArgument> OnLogContentReady;
    }
}