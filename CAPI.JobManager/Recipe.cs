using CAPI.JobManager.Abstraction;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CAPI.JobManager
{
    [JsonConverter(typeof(JsonConverterRecipe))]
    public class Recipe : IRecipe
    {
        public string SourceAet { get; set; }
        public string PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientBirthDate { get; set; }
        public IList<ISeriesSelectionCriteria> NewStudyCriteria { get; set; }
        public string NewStudyAccession { get; set; }
        public IList<ISeriesSelectionCriteria> PriorStudyCriteria { get; set; }
        public string PriorStudyAccession { get; set; }
        public IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        public IList<IDestination> Destinations { get; set; }

        public Recipe()
        {
            PatientId = "";
            PatientFullName = "";
            PatientBirthDate = "";
            NewStudyCriteria = new List<ISeriesSelectionCriteria>();
            NewStudyAccession = "";
            PriorStudyCriteria = new List<ISeriesSelectionCriteria>();
            PriorStudyAccession = "";
            IntegratedProcesses = new List<IIntegratedProcess>();
            Destinations = new List<IDestination>();
        }

        public Recipe(IList<ISeriesSelectionCriteria> newStudyCriteria,
            IList<ISeriesSelectionCriteria> priorStudyCriterias,
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