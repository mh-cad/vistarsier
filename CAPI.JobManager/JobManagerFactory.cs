using CAPI.Dicom.Abstraction;
using CAPI.JobManager.Abstraction;
using System.Collections.Generic;

namespace CAPI.JobManager
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class JobManagerFactory : IJobManagerFactory
    {
        public IJob<IRecipe> CreateJob()
        {
            return new Job<IRecipe>(this);
        }

        public IJob<IRecipe> CreateJob(IDicomStudy dicomStudyUnderFocus, IDicomStudy dicomStudyBeingComparedTo,
            IList<IIntegratedProcess> integratedProcesses, IList<IDestination> destinations)
        {
            return new Job<IRecipe>(this)
            {
                DicomStudyUnderFocus = dicomStudyUnderFocus,
                DicomStudyBeingComparedTo = dicomStudyBeingComparedTo,
                IntegratedProcesses = integratedProcesses,
                Destinations = destinations
            };
        }

        public IRecipe CreateRecipe()
        {
            return new Recipe();
        }

        public IIntegratedProcess CreateExtractBrinSurfaceIntegratedProcess(string version, params string[] parameters)
        {
            return new ExtractBrainSurface(parameters) { Version = version };
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

        public IStudySelectionCriteria CreateStudySelectionCriteria()
        {
            return new StudySelectionCriteria();
        }
    }
}