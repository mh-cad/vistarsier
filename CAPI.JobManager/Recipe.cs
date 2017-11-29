using CAPI.JobManager.Abstraction;
using System.Collections.Generic;

namespace CAPI.JobManager
{
    public class Recipe : IRecipe
    {
        public IList<IStudySelectionCriteria> NewStudyCriteria { get; set; }
        public IList<IStudySelectionCriteria> PriorStudyCriteria { get; set; }
        public IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        public IList<IDestination> Destinations { get; set; }
    }
}