using System;

namespace CAPI.Agent.Abstractions.Models
{
    public interface IJob
    {
        long Id { get; set; }
        string SourceAet { get; set; }
        string PatientId { get; set; }
        string PatientFullName { get; set; }
        string PatientBirthDate { get; set; }
        string CurrentAccession { get; set; }
        string PriorAccession { get; set; }
        string DefaultDestination { get; set; }
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
        string ResultSeriesDicomFolder { get; set; }
        string PriorReslicedSeriesDicomFolder { get; set; }
        string ProcessingFolder { get; set; }

        void Process();
    }
}
