using CAPI.Agent.Abstractions.Models;
using System.Collections.Generic;

namespace CAPI.Agent.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Recipe : IRecipe
    {
        public Recipe()
        {
            CurrentSeriesCriteria = new List<SeriesSelectionCriteria>();
            PriorSeriesCriteria = new List<SeriesSelectionCriteria>();
            Destinations = new List<Destination>();
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
        public string ExtractBrainParams { get; set; }
        public bool Register { get; set; }
        public bool BiasFieldCorrection { get; set; }
        public string BiasFieldCorrectionParams { get; set; }

        public string SliceType { get; set; }
        public string LookUpTablePath { get; set; }


        public List<Destination> Destinations { get; set; }
    }
}
