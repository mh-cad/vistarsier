using System;

namespace CAPI.Agent.Abstractions.Models
{
    public interface IJob
    {
        int Id { get; set; }
        string SourceAet { get; set; }
        string PatientId { get; set; }
        string PatientFullName { get; set; }
        string PatientBirthDate { get; set; }
        string CurrentAccession { get; set; }
        string PriorAccession { get; set; }
        string ResultDestination { get; set; }
        bool ExtractBrain { get; set; }
        string ExtractBrainParams { get; set; }
        bool Register { get; set; }
        bool BiasFieldCorrection { get; set; }
        string BiasFieldCorrectionParams { get; set; }
        string Status { get; set; }
        DateTime Start { get; set; }
        DateTime End { get; set; }
        string CurrentSeriesDicomFolder { get; set; }
        string PriorSeriesDicomFolder { get; set; }

        void Process();
    }
}
