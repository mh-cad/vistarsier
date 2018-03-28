using CAPI.Agent_Console.Abstractions;
using System;

namespace CAPI.Agent_Console
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class VerifiedMri : IVerifiedMri
    {
        public string Id { get; set; }
        public string Accession { get; set; }
        public string Status { get; set; }
        public string AdditionMethod { get; set; }
        public DateTime AdditionTime { get; set; }
        public DateTime LastModified { get; set; }
        public Exception Exception { get; set; }

        public VerifiedMri()
        {
            AdditionTime = DateTime.Now;
            LastModified = DateTime.Now;
        }
    }
}