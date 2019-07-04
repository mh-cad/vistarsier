using System.Collections.Generic;

namespace VisTarsier.Config
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Recipe
    {
        public enum RegisterToOption { PRIOR, CURRENT }
        public Recipe()
        {
            CurrentSeriesCriteria = new List<SeriesSelectionCriteria>();
            PriorSeriesCriteria = new List<SeriesSelectionCriteria>();
        }

        public string SourceAet { get; set; }

        public string PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientBirthDate { get; set; }

        public string CurrentSeriesDicomFolder { get; set; }
        public string CurrentAccession { get; set; }
        public List<SeriesSelectionCriteria> CurrentSeriesCriteria { get; set; }

        public string PriorSeriesDicomFolder { get; set; }
        public string PriorAccession { get; set; }
        public List<SeriesSelectionCriteria> PriorSeriesCriteria { get; set; }

        public bool ExtractBrain { get; set; }
        public RegisterToOption RegisterTo { get; set; }
        public bool BiasFieldCorrection { get; set; }
        public CompareSettings CompareSettings { get; set; }
        public OutputSettings OutputSettings { get; set; }
    }
}
