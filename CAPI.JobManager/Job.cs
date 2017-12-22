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

        public string OutputFolderPath { get; set; }
        public IJobSeriesBundle DicomSeriesFixed { get; set; }
        public IJobSeriesBundle DicomSeriesFloating { get; set; }
        public IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        public IList<IDestination> Destinations { get; set; }

        public Job(IJobManagerFactory jobManagerFactory, IDicomFactory dicomFactory
            , IDicomNode localNode, IDicomNode remoteNode, IImageConverter imageConverter)
        {
            _jobManagerFactory = jobManagerFactory;
            _dicomFactory = dicomFactory;
            _localNode = localNode;
            _remoteNode = remoteNode;
            _imageConverter = imageConverter;
        }

        public void Run()
        {
            try
            {
                FetchSeriesAndSaveToDisk();
            }
            catch (Exception ex)
            {
                OnLogContentReady?.Invoke(this, new LogEventArgument { Exception = ex });
            }

            foreach (var integratedProcess in IntegratedProcesses)
            {
                switch (integratedProcess.Type)
                {
                    case IntegratedProcessType.ExtractBrainSurface:
                        DicomSeriesFixed.Original =
                            HdrFileDoesExist(DicomSeriesFixed.Original);

                        DicomSeriesFloating.Original =
                            HdrFileDoesExist(DicomSeriesFloating.Original);

                        RunExtractBrainSurfaceProcess(integratedProcess);
                        break;
                    case IntegratedProcessType.Registeration:
                        DicomSeriesFixed.Transformed =
                            NiiFileDoesExist(DicomSeriesFixed.Transformed);

                        DicomSeriesFloating.Transformed =
                            NiiFileDoesExist(DicomSeriesFloating.Transformed);

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
            }

            // TODO1: Convert to Dicom & Copy headers and send to destinations
        }

        private IJobSeries HdrFileDoesExist(IJobSeries jobSeries)
        {
            var hdrFileName = jobSeries.DicomFolderPath.Split('\\').LastOrDefault();
            var outputPath = jobSeries.DicomFolderPath.Replace("\\" + hdrFileName, "");

            _imageConverter.Dicom2Hdr(jobSeries.DicomFolderPath, outputPath, hdrFileName);

            jobSeries.HdrFileFullPath = outputPath + "\\" + hdrFileName;

            return jobSeries;
        }
        private IJobSeries NiiFileDoesExist(IJobSeries jobSeries)
        {
            _imageConverter.Hdr2Nii(jobSeries.HdrFileFullPath);

            jobSeries.NiiFileFullPath = jobSeries.HdrFileFullPath.Replace(".hdr", ".nii");

            return jobSeries;
        }

        private void FetchSeriesAndSaveToDisk()
        {
            var dicomServices = _dicomFactory.CreateDicomServices();
            dicomServices.CheckRemoteNodeAvailability(_localNode, _remoteNode);

            var patientDetails = DicomSeriesFixed.Original.ParentDicomStudy.PatientsName
                                 + "_" + DicomSeriesFixed.Original.ParentDicomStudy.PatientId;
            OutputFolderPath = $@"{OutputFolderPath}\{patientDetails}";
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
            dicomServices.SaveSeriesToLocalDisk(series, outputPath
                , _localNode, _remoteNode);

            // Rename Study Folder Name - Add Fixed
            var studyFolderPath = $@"{outputPath}\{series?.StudyInstanceUid}";
            var newStudyPath = $@"{outputPath}\{seriesPrefix}-{series?.StudyInstanceUid}";
            if (Directory.Exists(studyFolderPath))
                Directory.Move(studyFolderPath, newStudyPath);

            return newStudyPath + "\\" + series?.SeriesInstanceUid;
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

        private void Process_OnComplete(object sender, IProcessEventArgument e)
        {
            OnEachProcessCompleted?.Invoke(sender, e);
        }

        public event EventHandler<IProcessEventArgument> OnEachProcessCompleted;
        public event EventHandler<ILogEventArgument> OnLogContentReady;
    }
}