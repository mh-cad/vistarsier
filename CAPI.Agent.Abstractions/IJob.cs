using System;

namespace CAPI.Agent.Abstractions
{
    public interface IJob
    {
        int Id { get; set; }
        string PatientId { get; set; }
        string PatientName { get; set; }
        string PatientDob { get; set; }
        string Accession { get; set; }
        string SourceAet { get; set; }
        string DestinationAet { get; set; }
        string Status { get; set; }
        DateTime Start { get; set; }
        DateTime End { get; set; }
    }
}
