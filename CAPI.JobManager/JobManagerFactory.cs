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
        private readonly IDicomServices _dicomServices;
        private readonly IImageConverter _imageConverter;

        public JobManagerFactory(IImageProcessor imageProcessor, IDicomServices dicomServices, IImageConverter imageConverter)
        {
            _imageProcessor = imageProcessor;
            _dicomServices = dicomServices;
            _imageConverter = imageConverter;
        }

        public IJob<IRecipe> CreateJob()
        {
            return new Job<IRecipe>(this, _dicomServices);
        }

        public IJob<IRecipe> CreateJob(IDicomStudy dicomStudyUnderFocus, IDicomStudy dicomStudyBeingComparedTo,
            IList<IIntegratedProcess> integratedProcesses, IList<IDestination> destinations)
        {
            return new Job<IRecipe>(this, _dicomServices)
            {
                DicomStudyFixed = dicomStudyUnderFocus,
                DicomStudyFloating = dicomStudyBeingComparedTo,
                IntegratedProcesses = integratedProcesses,
                Destinations = destinations
            };
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
            return new Registration(parameters) { Version = version };
        }

        public IIntegratedProcess CreateTakeDifferenceIntegratedProcess(string version, params string[] parameters)
        {
            return new TakeDifference(parameters) { Version = version };
        }

        public IIntegratedProcess CreateColorMapIntegratedProcess(string version, params string[] parameters)
        {
            return new ColorMap(parameters) { Version = version };
        }

        public IDestination CreateDestination(string id, string folderPath, string aeTitle)
        {
            return new Destination(id, folderPath, aeTitle);
        }

        public ISeriesSelectionCriteria CreateStudySelectionCriteria()
        {
            return new SeriesSelectionCriteria();
        }
    }
}