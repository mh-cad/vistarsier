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
            FilesystemDestinations = new List<string>();
            DicomDestinations = new List<string>();
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

        public string SliceType { get; set; } // Ref: Sag=Sagittal | Ax=Axial | Cor=Coronal
        public string[] LookUpTablePaths { get; set; }

        public string ResultsDicomSeriesDescription { get; set; }
        public string PriorReslicedDicomSeriesDescription { get; set; }

        public List<string> FilesystemDestinations { get; set; }
        public bool OnlyCopyResults { get; set; }
        public List<string> DicomDestinations { get; set; }
    }
}
