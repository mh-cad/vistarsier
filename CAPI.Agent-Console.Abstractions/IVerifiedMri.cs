using System;
using System.Collections.Generic;

namespace CAPI.Agent_Console.Abstractions
{
    public interface IVerifiedMri
    {
        string Id { get; set; }
        string Accession { get; set; }
        string Status { get; set; }
        string AdditionMethod { get; set; }
        DateTime AdditionTime { get; set; }
        IEnumerable<IVerifiedMri> GetRecentVerifiedMris(int numbersToCheck);

        void InsertIntoDb();
        void DeleteInDb();
        bool AccessionExistsInDb();
        bool DbIsAvailable();
        bool DbTableVerifiedMriExists();
    }
}