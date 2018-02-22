namespace CAPI.Agent_Console.Abstractions
{
    public interface IPendingCaseRepository : IPendingCaseRepositoryReadOnly
    {
        void Add(IPendingCase pendingCase);
        void Update(IPendingCase pendingCase);
        void Delete(IPendingCase pendingCase);
    }
}