using CAPI.JobManager.Abstraction;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CAPI.JobManager
{
    [JsonConverter(typeof(JsonConverterRecipe))]
    public class Recipe : IRecipe
    {
        public IList<IStudySelectionCriteria> NewStudyCriteria { get; set; }
        public IList<IStudySelectionCriteria> PriorStudyCriteria { get; set; }
        public IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        public IList<IDestination> Destinations { get; set; }

        public Recipe()
        {
            NewStudyCriteria = new List<IStudySelectionCriteria>();
            PriorStudyCriteria = new List<IStudySelectionCriteria>();
            IntegratedProcesses = new List<IIntegratedProcess>();
            Destinations = new List<IDestination>();
        }

        public Recipe(IList<IStudySelectionCriteria> newStudyCriteria,
            IList<IStudySelectionCriteria> priorStudyCriterias,
            IList<IIntegratedProcess> integratedProcesses,
            IList<IDestination> destinations)
        {
            NewStudyCriteria = newStudyCriteria;
            PriorStudyCriteria = priorStudyCriterias;
            IntegratedProcesses = integratedProcesses;
            Destinations = destinations;
        }
    }
}