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

        IIntegratedProcess CreateExtractBrinSurfaceIntegratedProcess(string version, params string[] parameters);
        IIntegratedProcess CreateRegistrationIntegratedProcess(string version, params string[] integratedProcessParameters);
        IIntegratedProcess CreateTakeDifferenceIntegratedProcess(string version, params string[] integratedProcessParameters);
        IIntegratedProcess CreateColorMapIntegratedProcess(string version, params string[] integratedProcessParameters);

        IDestination CreateDestination(string id, string folderPath, string aeTitle);
        IStudySelectionCriteria CreateStudySelectionCriteria();
    }
}