using CAPI.JobManager.Abstraction;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CAPI.JobManager
{
    public class Recipe : IRecipe
    {
        [JsonConverter(typeof(RecipeJsonConverter<Recipe,Destination,IntegratedProcess,StudySelectionCriteria>))]
        public IList<IStudySelectionCriteria> NewStudyCriteria { get; set; }
        [JsonConverter(typeof(RecipeJsonConverter<Recipe, Destination, IntegratedProcess, StudySelectionCriteria>))]
        public IList<IStudySelectionCriteria> PriorStudyCriteria { get; set; }
        [JsonConverter(typeof(RecipeJsonConverter<Recipe, Destination, IntegratedProcess, StudySelectionCriteria>))]
        public IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        [JsonConverter(typeof(RecipeJsonConverter<Recipe, Destination, IntegratedProcess, StudySelectionCriteria>))]
        public IList<IDestination> Destinations { get; set; }

        public Recipe()
        {

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