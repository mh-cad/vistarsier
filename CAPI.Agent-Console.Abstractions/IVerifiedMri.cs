using System;

namespace CAPI.Agent_Console.Abstractions
{
    public interface IVerifiedMri
    {
        string Id { get; set; }
        string Accession { get; set; }
        string Status { get; set; }
        string Note { get; set; }
        string AdditionMethod { get; set; }
        DateTime AdditionTime { get; set; }
        DateTime LastModified { get; set; }
        Exception Exception { get; set; }
    }
}