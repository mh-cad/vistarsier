using CAPI.Dicom.Abstraction;
using System.Linq;

namespace CAPI.DAL.Abstraction
{
    public interface IDicomNodeRepositoryReadOnly
    {
        IDicomNode Get(int id);
        IQueryable<IDicomNode> GetAll();
    }
}