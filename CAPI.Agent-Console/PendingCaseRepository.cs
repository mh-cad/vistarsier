using CAPI.Agent_Console.Abstractions;
using System.Linq;

namespace CAPI.Agent_Console
{
    public class PendingCaseRepository : IPendingCaseRepository
    {
        public IPendingCase FindById(string id)
        {
            throw new System.NotImplementedException();
        }

        public IPendingCase FindByAccession(string accession)
        {
            throw new System.NotImplementedException();
        }

        public IQueryable<IPendingCase> FindAll(int count)
        {
            throw new System.NotImplementedException();
        }

        public void Add(IPendingCase pendingCase)
        {
            throw new System.NotImplementedException();
        }

        public void Update(IPendingCase pendingCase)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(IPendingCase pendingCase)
        {
            throw new System.NotImplementedException();
        }
    }
}
