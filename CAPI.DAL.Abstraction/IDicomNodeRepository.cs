using CAPI.Dicom.Abstraction;
using System.Linq;

namespace CAPI.DAL.Abstraction
{
    public interface IDicomNodeRepository : IRepository<IDicomNode>
    {
        IDicomNode Get(int id);
        IQueryable<IDicomNode> GetAll();
    }
}