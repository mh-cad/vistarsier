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

        public Job(string outputFolderPath, IJobManagerFactory jobManagerFactory,
            IDicomFactory dicomFactory, IDicomNode localNode, IDicomNode remoteNode, IImageConverter imageConverter)
            : this(jobManagerFactory, dicomFactory, localNode, remoteNode, imageConverter)
        {
            OutputFolderPath = outputFolderPath;
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
                        DicomSeriesFixed.Original.HdrFileFullPath =
                            CreateHdrFiles(DicomSeriesFixed.Original.DicomFolderPath);

                        DicomSeriesFloating.Original.HdrFileFullPath =
                            CreateHdrFiles(DicomSeriesFloating.Original.DicomFolderPath);

                        RunExtractBrainSurfaceProcess(integratedProcess);
                        break;
                    case IntegratedProcessType.Registeration:
                        //RunProcess(_jobManagerFactory
                        //    .CreateRegistrationIntegratedProcess(
                        //        integratedProcess.Version, integratedProcess.Parameters));
                        break;
                    case IntegratedProcessType.TakeDifference:
                        //RunProcess(_jobManagerFactory
                        //    .CreateTakeDifferenceIntegratedProcess(
                        //        integratedProcess.Version, integratedProcess.Parameters));
                        break;
                    case IntegratedProcessType.ColorMap:
                        //RunProcess(_jobManagerFactory
                        //    .CreateColorMapIntegratedProcess(
                        //        integratedProcess.Version, integratedProcess.Parameters));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private string CreateHdrFiles(string dicomFolderPath)
        {
            var hdrFileName = dicomFolderPath.Split('\\').LastOrDefault();
            var outputPath = dicomFolderPath.Replace("\\" + hdrFileName, "");

            _imageConverter.Dicom2Hdr(dicomFolderPath, outputPath, hdrFileName);

            return outputPath + "\\" + hdrFileName;
        }

        private void FetchSeriesAndSaveToDisk()
        {
            var dicomServices = _dicomFactory.CreateDicomServices();
            dicomServices.CheckRemoteNodeAvailability(_localNode, _remoteNode);

            var patientDetails = DicomSeriesFixed.Original.ParentDicomStudy.PatientsName
                                 + "_" + DicomSeriesFixed.Original.ParentDicomStudy.PatientId;
            var patientFolderPath = $@"{OutputFolderPath}\{patientDetails}";
            FileSystem.DirectoryExists(patientFolderPath);

            DicomSeriesFixed.Original.DicomFolderPath =
                SaveSeriesToDisk(patientFolderPath, "Fixed",
                    DicomSeriesFixed.Original.ParentDicomStudy, dicomServices);

            DicomSeriesFloating.Original.DicomFolderPath =
                SaveSeriesToDisk(patientFolderPath, "Floating",
                    DicomSeriesFloating.Original.ParentDicomStudy, dicomServices);
        }

        private string SaveSeriesToDisk(string patientFolderPath, string seriesPrefix,
            IDicomStudy study, IDicomServices dicomServices)
        {
            var series = study.Series.ToList().FirstOrDefault();

            // Retrieve Series From Pacs
            dicomServices.SaveSeriesToLocalDisk(series, patientFolderPath
                , _localNode, _remoteNode);

            // Rename Study Folder Name - Add Fixed
            var studyFolderPath = $@"{patientFolderPath}\{series?.StudyInstanceUid}";
            var newStudyPath = $@"{patientFolderPath}\{seriesPrefix}-{series?.StudyInstanceUid}";
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

        private void Process_OnComplete(object sender, IProcessEventArgument e)
        {
            OnEachProcessCompleted?.Invoke(sender, e);
        }

        public event EventHandler<IProcessEventArgument> OnEachProcessCompleted;
        public event EventHandler<ILogEventArgument> OnLogContentReady;
    }
}