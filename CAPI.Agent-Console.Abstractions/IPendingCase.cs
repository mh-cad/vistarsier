using System;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.Agent_Console.Abstractions
{
    public interface IPendingCase
    {
        // Properties
        string Id { get; set; }
        string Accession { get; set; }
        string Status { get; set; }
        string AdditionMethod { get; set; }
        Exception Exception { get; set; }

        // Public Methods
        IEnumerable<IPendingCase> GetVtCases(int count);

        IEnumerable<IPendingCase> GetCapiCases(int count);
        IEnumerable<IPendingCase> GetRecentPendingCapiCases(bool manual, int numOfcasesToCheckInDb = 1000);
        IEnumerable<IPendingCase> GetProcessingCapiCases(int numOfcasesToCheckInDb = 1000);
        IEnumerable<IPendingCase> GetQueuedCapiCases(int numOfcasesToCheckInDb = 1000);

        void AddToCapiDb(bool manual = false);

        void SetStatus(string statusText);

        void UpdateAdditionMethodToManual(bool manual);

        IQueryable<IPendingCase> GetAllCapiCases();
    }
}