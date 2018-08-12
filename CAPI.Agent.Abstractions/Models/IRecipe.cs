namespace CAPI.Agent.Abstractions.Models
{
    public interface IRecipe
    {
        string SourceAet { get; set; }

        string PatientId { get; set; }
        string PatientFullName { get; set; }
        string PatientBirthDate { get; set; }

        string CurrentSeriesDicomFolder { get; set; }
        string CurrentAccession { get; set; }
        //IList<SeriesSelectionCriteria> CurrentStudyCriteria { get; set; }

        string PriorSeriesDicomFolder { get; set; }
        string PriorAccession { get; set; }
        //IList<ISeriesSelectionCriteria> PriorStudyCriteria { get; set; }

        bool ExtractBrain { get; set; }
        string ExtractBrainParams { get; set; }
        bool Register { get; set; }
        bool BiasFieldCorrection { get; set; }
        string BiasFieldCorrectionParams { get; set; }

        //IList<IDestination> Destinations { get; set; }
    }
}
