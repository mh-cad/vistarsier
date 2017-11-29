using System.Linq;
using CAPI.Dicom.Abstraction;

namespace CAPI.DAL.Abstraction
{
    public interface IDicomNodeRepository : IRepository <IDicomNode>
    {
        IDicomNode Get(int id);
        IQueryable<IDicomNode> GetAll(IDicomFactory dicomFactory);
    }
}