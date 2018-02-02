using CAPI.Common.Services;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CAPI.JobManager
{
    public class Job<T> : IJob<T>
    {
        private readonly IJobManagerFactory _jobManagerFactory;
        private readonly IDicomFactory _dicomFactory;
        private readonly IDicomNode _localNode;
        private readonly IDicomNode _remoteNode;
        private readonly IImageConverter _imageConverter;
        private readonly IImageProcessor _imageProcessor;

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

        public Job(IJobManagerFactory jobManagerFactory, IDicomFactory dicomFactory,
            IDicomNode localNode, IDicomNode remoteNode,
            IImageConverter imageConverter, IImageProcessor imageProcessor)
        {
            _jobManagerFactory = jobManagerFactory;
            _dicomFactory = dicomFactory;
            _localNode = localNode;
            _remoteNode = remoteNode;
            _imageConverter = imageConverter;
            _imageProcessor = imageProcessor;

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
            try
            {
                FetchSeriesAndSaveToDisk();
            }
            catch (Exception ex)
            {
                OnLogContentReady?.Invoke(this, new LogEventArgument
                {
                    LogContent = "Failure in retreiving images from PACS!",
                    Exception = ex
                });
            }

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
            }

            _imageProcessor.ConvertBmpsToDicom(OutputFolderPath);

            //_imageProcessor.CopyDicomHeaders(DicomSeriesFixed.Original.DicomFolderPath,
            //OutputFolderPath, out var dicomFolderNewHeaders);

            UpdateDicomHeaders(); // TODO1: Implement

            //SendDicomFilesToDestinations(); // TODO1: Implement
        }

        private void FetchSeriesAndSaveToDisk()
        {
            var dicomServices = _dicomFactory.CreateDicomServices();
            dicomServices.CheckRemoteNodeAvailability(_localNode, _remoteNode);

            var patientFullName = DicomSeriesFixed.Original.ParentDicomStudy.PatientsName.Replace("^", "_");
            var jobId = $"{DateTime.Now:yyyyMMddHHmmss}_{patientFullName}";
            OutputFolderPath = $@"{OutputFolderPath}\{jobId}";
            FileSystem.DirectoryExists(OutputFolderPath);

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
            dicomServices.SaveSeriesToLocalDisk(series, outputPath, _localNode, _remoteNode);

            // Rename Study Folder Name to Fixed/Floating (Series Prefix)
            var studyFolderPath = $@"{outputPath}\{series?.StudyInstanceUid}";
            var newStudyPath = $@"{outputPath}\{seriesPrefix}";
            if (Directory.Exists(studyFolderPath))
                Directory.Move(studyFolderPath, newStudyPath);

            // Rename Dicom Series Folder Name
            var dicomFolderPath = $@"{newStudyPath}\{series?.SeriesInstanceUid}";
            var newDicomFolderPath = $@"{newStudyPath}\Dicom";
            if (Directory.Exists(dicomFolderPath))
                Directory.Move(dicomFolderPath, newDicomFolderPath);

            return newDicomFolderPath;
        }

        private IJobSeries HdrFileDoesExist(IJobSeries jobSeries, string studyName)
        {
            if (!string.IsNullOrEmpty(jobSeries.HdrFileFullPath)) return jobSeries;

            var fileNameNoExt = jobSeries.DicomFolderPath.Split('\\').LastOrDefault();
            var outputPath = jobSeries.DicomFolderPath.Replace("\\" + fileNameNoExt, "");

            _imageConverter.Dicom2Hdr(jobSeries.DicomFolderPath, outputPath, studyName);

            jobSeries.HdrFileFullPath = $@"{outputPath}\{studyName}.hdr";

            return jobSeries;
        }
        private IJobSeries NiiFileDoesExist(ISeries originalJobSeries, IJobSeries jobSeries)
        {
            if (!string.IsNullOrEmpty(jobSeries.NiiFileFullPath)) return jobSeries;
            //if (string.IsNullOrEmpty(jobSeries.DicomFolderPath))
            //throw new NullReferenceException();
            _imageConverter.Hdr2Nii(originalJobSeries.HdrFileFullPath,
                jobSeries.HdrFileFullPath, out var niiFileFullPath);

            //var dicomFolderName = jobSeries.DicomFolderPath.Split('\\').LastOrDefault();
            //var hdrFileNameNoExt = dicomFolderName + ".nii";

            //_imageConverter.DicomToNii(
            //   jobSeries.DicomFolderPath, OutputFolderPath, hdrFileNameNoExt);

            jobSeries.NiiFileFullPath = niiFileFullPath;

            return jobSeries;
        }

        private void RunExtractBrainSurfaceProcess(IIntegratedProcess integratedProcess)
        {
            var extractBrainSurfaceProcess = _jobManagerFactory
                .CreateExtractBrinSurfaceIntegratedProcess(
                integratedProcess.Version, integratedProcess.Parameters);

            extractBrainSurfaceProcess.OnComplete += Process_OnComplete;

            extractBrainSurfaceProcess.Run(this as IJob<IRecipe>);
        }
        private void RunRegistrationProcess(IIntegratedProcess integratedProcess)
        {
            var registrationProcess = _jobManagerFactory
                .CreateRegistrationIntegratedProcess(
                    integratedProcess.Version, integratedProcess.Parameters);

            registrationProcess.OnComplete += Process_OnComplete;

            registrationProcess.Run(this as IJob<IRecipe>);
        }
        private void RunTakeDifference(IIntegratedProcess integratedProcess)
        {
            var takeDifferenceProcess = _jobManagerFactory
                .CreateTakeDifferenceIntegratedProcess(
                    integratedProcess.Version, integratedProcess.Parameters);

            takeDifferenceProcess.OnComplete += Process_OnComplete;

            takeDifferenceProcess.Run(this as IJob<IRecipe>);
        }
        private void RunColorMap(IIntegratedProcess integratedProcess)
        {
            var colorMapProcess = _jobManagerFactory
                .CreateColorMapIntegratedProcess(
                    integratedProcess.Version, integratedProcess.Parameters);

            colorMapProcess.OnComplete += Process_OnComplete;

            colorMapProcess.Run(this as IJob<IRecipe>);
        }

        private void UpdateDicomHeaders()
        {
            var dicomServices = _dicomFactory.CreateDicomServices();

            // Update Negative Folder
            var seriesDescription = "VT Decreased Signal";
            var negativeFiles =
                Directory.GetFiles($@"{OutputFolderPath}\{"flair_new_with_changes_overlay_negative_dcm"}");

            var negDicomTags = _dicomFactory.CreateDicomTagCollection();
            negDicomTags.SeriesDescription.Values = new[] { seriesDescription };
            dicomServices.UpdateSeriesHeadersForAllFiles(negativeFiles, negDicomTags);

            // Update Positive Folder
            seriesDescription = "VT Increased Signal";
            var positiveFiles =
                Directory.GetFiles($@"{OutputFolderPath}\{"flair_new_with_changes_overlay_positive_dcm"}");

            var posDicomTags = _dicomFactory.CreateDicomTagCollection();
            posDicomTags.SeriesDescription.Values = new[] { seriesDescription };
            dicomServices.UpdateSeriesHeadersForAllFiles(positiveFiles, posDicomTags);
        }
        private void SendDicomFilesToDestinations()
        {
            throw new NotImplementedException();
        }

        private void Process_OnComplete(object sender, IProcessEventArgument e)
        {
            OnEachProcessCompleted?.Invoke(sender, e);
        }

        public event EventHandler<IProcessEventArgument> OnEachProcessCompleted;
        public event EventHandler<ILogEventArgument> OnLogContentReady;
    }
}