using System.Collections.Generic;

namespace CAPI.JobManager.Abstraction
{
    public interface IRecipe
    {
        string SourceAet { get; set; }
        string PatientId { get; set; }
        string PatientFullName { get; set; }
        string PatientBirthDate { get; set; }
        IList<ISeriesSelectionCriteria> NewStudyCriteria { get; set; }
        string NewStudyAccession { get; set; }
        IList<ISeriesSelectionCriteria> PriorStudyCriteria { get; set; }
        string PriorStudyAccession { get; set; }
        IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        IList<IDestination> Destinations { get; set; }
    }
}