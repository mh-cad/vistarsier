namespace CAPI.DAL.Abstraction
{
    public interface IRepositoryFactory
    {
        IDicomNodeRepository CreateDicomNodeRepository();
    }
}
