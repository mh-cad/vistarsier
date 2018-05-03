using System.Collections.Generic;
using System.Linq;

namespace CAPI.Agent_Console.Abstractions
{
    public interface IAgentConsoleRepository
    {
        // Database Check Methods
        bool DbIsAvailable();
        bool DbTableVerifiedMriExists();

        // Verified Mri Methods
        IEnumerable<IVerifiedMri> GetRecentVerifiedMris(int numbersToCheck);
        IQueryable<IVerifiedMri> GetAllCases();
        IEnumerable<IVerifiedMri> GetPendingCases();
        IEnumerable<IVerifiedMri> GetProcessingCases();
        IEnumerable<IVerifiedMri> GetQueuedCases();
        IEnumerable<IVerifiedMri> GetAllManualCases();
        IEnumerable<IVerifiedMri> GetAllHl7Cases();
        IVerifiedMri GetVerifiedMriByAccession(string accession);
        IVerifiedMri GetVerifiedMriById(string id);
        bool AccessionExistsInDb(string accession);

        void UpdateVerifiedMri(IVerifiedMri verifiedMri);
        void SetVerifiedMriStatus(string accession, string statusText);

        void InsertVerifiedMriIntoDb(IVerifiedMri verifiedMri);
        void DeleteInDbByAccession(string accession);

        void DeleteInDbById(string id);

        
    }
}