using System.Collections.Generic;

namespace CAPI.Agent_Console.Abstractions
{
    public interface IAgentConsoleRepository
    {
        // Database Check Methods
        bool DbIsAvailable();
        bool DbTableVerifiedMriExists();

        // Verified Mri Methods
        IEnumerable<IVerifiedMri> GetRecentVerifiedMris(int numbersToCheck);
        IEnumerable<IVerifiedMri> GetPendingCases();
        IEnumerable<IVerifiedMri> GetProcessingCases();
        IEnumerable<IVerifiedMri> GetQueuedCases();
        IVerifiedMri GetVerifiedMriByAccession(string accession);
        IVerifiedMri GetVerifiedMriById(string id);
        void SetVerifiedMriStatus(string accession, string statusText);
        void InsertVerifiedMriIntoDb(IVerifiedMri verifiedMri);
        void DeleteInDbByAccession(string accession);
        void DeleteInDbById(string id);
        bool VerifiedMriExistsInDb(string accession);
    }
}