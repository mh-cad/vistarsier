using CAPI.Dicom.Abstraction;

namespace CAPI.DAL.Abstraction
{
    public interface IDicomNodeRepository
        : IDicomNodeRepositoryReadOnly, IRepository<IDicomNode>
    {
    }
}