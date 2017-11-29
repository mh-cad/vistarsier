using CAPI.Dicom.Abstraction;
using CAPI.JobManager.Abstraction;
using System.Collections.Generic;

namespace CAPI.JobManager
{
    public class JobManagerFactory : IJobManagerFactory
    {
        public IJob CreateJob()
        {
            return new Job();
        }

        public IJob CreateJob(IStudy studyUnderFocus, IStudy studyBeingComparedTo, 
            IList<IIntegratedProcess> integratedProcesses, IList<IDestination> destinations)
        {
            return new Job
            {
                StudyUnderFocus = studyUnderFocus,
                StudyBeingComparedTo = studyBeingComparedTo,
                IntegratedProcesses = integratedProcesses,
                Destinations = destinations
            };
        }

        public IRecipe CreateRecipe()
        {
            return new Recipe();
        }

        public IIntegratedProcess CreateIntegratedProcess(string id, string version, params string[] parameters)
        {
            return new IntegratedProcess(id, version, parameters);
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