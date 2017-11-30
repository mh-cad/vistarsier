using CAPI.Dicom.Abstraction;
using System.Collections.Generic;

namespace CAPI.JobManager.Abstraction
{
    public interface IJobManagerFactory
    {
        IJob CreateJob();
        IJob CreateJob(IDicomStudy dicomStudyUnderFocus, IDicomStudy dicomStudyBeingComparedTo, 
            IList<IIntegratedProcess> integratedProcesses, IList<IDestination> destinations);

        IRecipe CreateRecipe();

        IIntegratedProcess CreateIntegratedProcess(string id, string version, params string[] parameters);

        IDestination CreateDestination(string id, string folderPath, string aeTitle);
        IStudySelectionCriteria CreateStudySelectionCriteria();
    }
}
