using System;
using CAPI.Agent.Abstractions;

namespace CAPI.Agent.Models
{
    public class Job : IJob
    {
        public int Id { get; set; }
        public string PatientId { get; set; }
        public string PatientName { get; set; }
        public string PatientDob { get; set; }
        public string Accession { get; set; }
        public string SourceAet { get; set; }
        public string DestinationAet { get; set; }
        public string Status { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
