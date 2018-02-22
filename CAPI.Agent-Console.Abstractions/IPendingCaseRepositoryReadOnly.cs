using System.Linq;

namespace CAPI.Agent_Console.Abstractions
{
    public interface IPendingCaseRepositoryReadOnly
    {
        IPendingCase FindById(string id);
        IPendingCase FindByAccession(string accession);
        IQueryable<IPendingCase> FindAll(int count);
    }
}
