using System.Collections.Generic;

namespace CAPI.JobManager.Abstraction
{
    public interface IRecipe
    {
        IList<IStudySelectionCriteria> NewStudyCriteria { get; set; }
        IList<IStudySelectionCriteria> PriorStudyCriteria { get; set; }
        IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        IList<IDestination> Destinations { get; set; }
    }
}