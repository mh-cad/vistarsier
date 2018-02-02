using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager.Abstraction;
using System.Collections.Generic;

namespace CAPI.JobManager
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class JobManagerFactory : IJobManagerFactory
    {
        private readonly IImageProcessor _imageProcessor;
        private readonly IImageConverter _imageConverter;
        private readonly IDicomFactory _dicomFactory;

        public JobManagerFactory(IImageProcessor imageProcessor, IDicomFactory dicomFactory, IImageConverter imageConverter)
        {
            _imageProcessor = imageProcessor;
            _dicomFactory = dicomFactory;
            _imageConverter = imageConverter;
        }

        public IJob<IRecipe> CreateJob(IDicomNode localNode, IDicomNode remoteNode)
        {
            return new Job<IRecipe>(
                this, _dicomFactory, localNode, remoteNode, _imageConverter, _imageProcessor);
        }

        public IJob<IRecipe> CreateJob(
            IJobSeriesBundle dicomStudyUnderFocus, IJobSeriesBundle dicomStudyBeingComparedTo,
            IList<IIntegratedProcess> integratedProcesses, IList<IDestination> destinations,
            string outputFolderPath, IDicomNode localNode, IDicomNode remoteNode)
        {
            var job = CreateJob(localNode, remoteNode);

            job.DicomSeriesFixed = dicomStudyUnderFocus;
            job.DicomSeriesFloating = dicomStudyBeingComparedTo;
            job.IntegratedProcesses = integratedProcesses;
            job.Destinations = destinations;
            job.OutputFolderPath = outputFolderPath;

            return job;
        }

        public IRecipe CreateRecipe()
        {
            return new Recipe();
        }

        public IIntegratedProcess CreateExtractBrinSurfaceIntegratedProcess(string version, string[] parameters)
        {
            return new ExtractBrainSurface(_imageProcessor)
            {
                Version = version,
                Parameters = parameters
            };
        }

        public IIntegratedProcess CreateRegistrationIntegratedProcess(string version, params string[] parameters)
        {
            return new Registration(_imageProcessor)
            {
                Version = version,
                Parameters = parameters
            };
        }

        public IIntegratedProcess CreateTakeDifferenceIntegratedProcess(string version, params string[] parameters)
        {
            return new TakeDifference(_imageProcessor)
            {
                Version = version,
                Parameters = parameters
            };
        }

        public IIntegratedProcess CreateColorMapIntegratedProcess(string version, params string[] parameters)
        {
            return new ColorMap(_imageProcessor)
            {
                Version = version,
                Parameters = parameters
            };
        }

        public IDestination CreateDestination(string id, string folderPath, string aeTitle)
        {
            return new Destination(id, folderPath, aeTitle);
        }

        public ISeriesSelectionCriteria CreateStudySelectionCriteria()
        {
            return new SeriesSelectionCriteria();
        }

        public IJobSeriesBundle CreateJobSeriesBundle()
        {
            return new JobSeriesBundle();
        }
    }
}