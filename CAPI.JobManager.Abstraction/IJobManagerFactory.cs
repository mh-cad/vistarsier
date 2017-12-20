using CAPI.Dicom.Abstraction;
using System.Collections.Generic;

namespace CAPI.JobManager.Abstraction
{
    public interface IJobManagerFactory
    {
        IJob<IRecipe> CreateJob(IDicomNode localNode, IDicomNode remoteNode);
        IJob<IRecipe> CreateJob(IJobSeriesBundle dicomStudyUnderFocus, IJobSeriesBundle dicomStudyBeingComparedTo,
            IList<IIntegratedProcess> integratedProcesses, IList<IDestination> destinations,
            string outputFolderPath, IDicomNode localNode, IDicomNode remoteNode);

        IRecipe CreateRecipe();

        IIntegratedProcess CreateExtractBrinSurfaceIntegratedProcess(string version, params string[] parameters);
        IIntegratedProcess CreateRegistrationIntegratedProcess(string version, params string[] integratedProcessParameters);
        IIntegratedProcess CreateTakeDifferenceIntegratedProcess(string version, params string[] integratedProcessParameters);
        IIntegratedProcess CreateColorMapIntegratedProcess(string version, params string[] integratedProcessParameters);

        IDestination CreateDestination(string id, string folderPath, string aeTitle);
        ISeriesSelectionCriteria CreateStudySelectionCriteria();
        IJobSeriesBundle CreateJobSeriesBundle();
    }
}