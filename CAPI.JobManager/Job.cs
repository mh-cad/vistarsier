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
    public class Job<T> : IJob<T>
    {
        private readonly IJobManagerFactory _jobManagerFactory;
        private readonly IDicomFactory _dicomFactory;
        private readonly IDicomNode _localNode;
        private readonly IDicomNode _remoteNode;
        private readonly IImageConverter _imageConverter;
        private readonly IDicomNodeRepository _dicomNodeRepository;
        private readonly IDicomConfig _dicomConfig;
        private readonly ILog _log;

        private const string NegativeDcmFolderName = "flair_new_with_changes_overlay_negative_dcm";
        private const string PositiveDcmFolderName = "flair_new_with_changes_overlay_positive_dcm";
        private const string IncreasedSignalSeriesName = "CAPI Increased Signal";
        private const string DecreasedSignalSeriesName = "CAPI Decreased Signal";

        public string OutputFolderPath { get; set; }
        public IJobSeriesBundle DicomSeriesFixed { get; set; }
        public IJobSeriesBundle DicomSeriesFloating { get; set; }
        public IJobSeries StructChangesDarkInFloating2BrightInFixed { get; set; }
        public IJobSeries StructChangesBrightInFloating2DarkInFixed { get; set; }
        public IJobSeries StructChangesBrainMask { get; set; }
        public IJobSeries PositiveOverlay { get; set; }
        public IJobSeries NegativeOverlay { get; set; }
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
        /// <param name="dicomConfig">Dicom Configuration</param>
        public Job(IJobManagerFactory jobManagerFactory, IDicomFactory dicomFactory,
            IDicomNode localNode, IDicomNode remoteNode,
            IImageConverter imageConverter,
            IDicomNodeRepository dicomNodeRepository,
            IDicomConfig dicomConfig)
        {
            _jobManagerFactory = jobManagerFactory;
            _dicomFactory = dicomFactory;
            _localNode = localNode;
            _remoteNode = remoteNode;
            _imageConverter = imageConverter;
            _dicomNodeRepository = dicomNodeRepository;
            _dicomConfig = dicomConfig;
            _log = LogHelper.GetLogger();

            DicomSeriesFixed = new JobSeriesBundle();
            DicomSeriesFloating = new JobSeriesBundle();
            StructChangesDarkInFloating2BrightInFixed = new JobSeries();
            StructChangesBrightInFloating2DarkInFixed = new JobSeries();
            StructChangesBrainMask = new JobSeries();
            PositiveOverlay = new JobSeries();
            NegativeOverlay = new JobSeries();
            IntegratedProcesses = new List<IIntegratedProcess>();
            Destinations = new List<IDestination>();
        }

        public void Run()
        {
            var accession = DicomSeriesFixed.Original.ParentDicomStudy.AccessionNumber;

            _log.Info($"Started processing job for accession [{accession}] ...");

            try
            {
                FetchSeriesAndSaveToDisk();

                // Process each integrated process
                foreach (var integratedProcess in IntegratedProcesses)
                {
                    switch (integratedProcess.Type)
                    {
                        case IntegratedProcessType.ExtractBrainSurface:
                            DicomSeriesFixed.Original =
                                HdrFileDoesExist(DicomSeriesFixed.Original, "Fixed");

                            DicomSeriesFloating.Original =
                                HdrFileDoesExist(DicomSeriesFloating.Original, "Floating");

                            RunExtractBrainSurfaceProcess(integratedProcess);
                            break;
                        case IntegratedProcessType.Registeration:
                            DicomSeriesFixed.Transformed =
                                NiiFileDoesExist(DicomSeriesFixed.Original,
                                    DicomSeriesFixed.Transformed);

                            DicomSeriesFloating.Transformed =
                                NiiFileDoesExist(DicomSeriesFloating.Original,
                                    DicomSeriesFloating.Transformed);

                            RunRegistrationProcess(integratedProcess);
                            break;
                        case IntegratedProcessType.TakeDifference:
                            // Brain Mask Nii Exists!
                            DicomSeriesFixed.BrainMask = NiiFileDoesExist(
                                DicomSeriesFixed.Original, DicomSeriesFixed.BrainMask);

                            DicomSeriesFloating.BrainMask = NiiFileDoesExist(
                                DicomSeriesFloating.Original, DicomSeriesFloating.BrainMask);

                            RunTakeDifference(integratedProcess);
                            break;
                        case IntegratedProcessType.ColorMap:
                            RunColorMap(integratedProcess);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                } // TODO2: Add logging to integrated processes

                var dicomServices = _dicomFactory.CreateDicomServices(_dicomConfig);

                NegativeOverlay.DicomFolderPath = NegativeOverlay.BmpFolderPath + "_dcm";
                dicomServices.ConvertBmpsToDicom(
                    NegativeOverlay.BmpFolderPath, $@"{NegativeOverlay.DicomFolderPath}",
                    DicomSeriesFixed.Original.DicomFolderPath);

                PositiveOverlay.DicomFolderPath = PositiveOverlay.BmpFolderPath + "_dcm";
                dicomServices.ConvertBmpsToDicom(
                    PositiveOverlay.BmpFolderPath, $@"{PositiveOverlay.DicomFolderPath}",
                    DicomSeriesFixed.Original.DicomFolderPath);

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
            var dicomServices = _dicomFactory.CreateDicomServices(_dicomConfig);
            dicomServices.CheckRemoteNodeAvailability(_localNode, _remoteNode);

            var patientFullName = DicomSeriesFixed.Original.ParentDicomStudy.PatientsName.Replace("^", "_");
            var jobId = $"{DateTime.Now:yyyyMMddHHmmss}_{patientFullName}";
            OutputFolderPath = $@"{OutputFolderPath}\{jobId}";
            FileSystem.DirectoryExistsIfNotCreate(OutputFolderPath);

            DicomSeriesFixed.Original.DicomFolderPath =
                SaveSeriesToDisk(OutputFolderPath, "Fixed",
                    DicomSeriesFixed.Original.ParentDicomStudy, dicomServices);

            DicomSeriesFloating.Original.DicomFolderPath =
                SaveSeriesToDisk(OutputFolderPath, "Floating",
                    DicomSeriesFloating.Original.ParentDicomStudy, dicomServices);

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

        //private IJobSeries HdrFileDoesExist(IJobSeries jobSeries, string studyName)
        //{
        //    if (!string.IsNullOrEmpty(jobSeries.HdrFileFullPath)) return jobSeries;

        //    var fileNameNoExt = jobSeries.DicomFolderPath.Split('\\').LastOrDefault();
        //    var outputPath = jobSeries.DicomFolderPath.Replace("\\" + fileNameNoExt, "");

        //    _imageConverter.Dicom2Hdr(jobSeries.DicomFolderPath, outputPath, studyName);

        //    jobSeries.HdrFileFullPath = $@"{outputPath}\{studyName}.hdr";

        //    return jobSeries;
        //}
        //private IJobSeries NiiFileDoesExist(ISeries originalJobSeries, IJobSeries jobSeries)
        //{
        //    if (!string.IsNullOrEmpty(jobSeries.NiiFileFullPath)) return jobSeries;
        //    //if (string.IsNullOrEmpty(jobSeries.DicomFolderPath))
        //    //throw new NullReferenceException();
        //    _imageConverter.Hdr2Nii(originalJobSeries.HdrFileFullPath,
        //        jobSeries.HdrFileFullPath, out var niiFileFullPath);

        //    //var dicomFolderName = jobSeries.DicomFolderPath.Split('\\').LastOrDefault();
        //    //var hdrFileNameNoExt = dicomFolderName + ".nii";

        //    //_imageConverter.DicomToNii(
        //    //   jobSeries.DicomFolderPath, OutputFolderPath, hdrFileNameNoExt);

        //    jobSeries.NiiFileFullPath = niiFileFullPath;

        //    return jobSeries;
        //}

        private void RunExtractBrainSurfaceProcess(IIntegratedProcess integratedProcess)
        {
            var extractBrainSurfaceProcess = _jobManagerFactory
                .CreateExtractBrinSurfaceIntegratedProcess(
                integratedProcess.Version, integratedProcess.Parameters);

            extractBrainSurfaceProcess.OnStart += Process_OnStart;
            extractBrainSurfaceProcess.OnComplete += Process_OnComplete;

            extractBrainSurfaceProcess.Run(this as IJob<IRecipe>);
        }
        private void RunRegistrationProcess(IIntegratedProcess integratedProcess)
        {
            var registrationProcess = _jobManagerFactory
                .CreateRegistrationIntegratedProcess(
                    integratedProcess.Version, integratedProcess.Parameters);

            registrationProcess.OnStart += Process_OnStart;
            registrationProcess.OnComplete += Process_OnComplete;

            registrationProcess.Run(this as IJob<IRecipe>);
        }
        private void RunTakeDifference(IIntegratedProcess integratedProcess)
        {
            var takeDifferenceProcess = _jobManagerFactory
                .CreateTakeDifferenceIntegratedProcess(
                    integratedProcess.Version, integratedProcess.Parameters);

            takeDifferenceProcess.OnStart += Process_OnStart;
            takeDifferenceProcess.OnComplete += Process_OnComplete;

            takeDifferenceProcess.Run(this as IJob<IRecipe>);
        }
        private void RunColorMap(IIntegratedProcess integratedProcess)
        {
            var colorMapProcess = _jobManagerFactory
                .CreateColorMapIntegratedProcess(
                    integratedProcess.Version, integratedProcess.Parameters);

            colorMapProcess.OnStart += Process_OnStart;
            colorMapProcess.OnComplete += Process_OnComplete;

            colorMapProcess.Run(this as IJob<IRecipe>);
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
            var dicomServices = _dicomFactory.CreateDicomServices(_dicomConfig);

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

                        var dicomServices = _dicomFactory.CreateDicomServices(_dicomConfig);

                        var negativeFiles = Directory.GetFiles(NegativeOverlay.DicomFolderPath);
                        foreach (var negativeFile in negativeFiles)
                            dicomServices.SendDicomFile(negativeFile, _localNode.AeTitle, destination.DicomNode);

                        var positiveFiles = Directory.GetFiles(PositiveOverlay.DicomFolderPath);
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