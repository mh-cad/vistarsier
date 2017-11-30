using CAPI.DAL.Abstraction;

namespace CAPI.DAL
{
    public class RepositoryFactory : IRepositoryFactory
    {
        public IDicomNodeRepository CreateDicomNodeRepository()
        {
            return new DicomNodeRepositoryInMemory(null);
        }
    }
}