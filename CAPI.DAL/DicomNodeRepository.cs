using CAPI.Dicom.Model;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.DAL
{
    public class DicomNodeRepository : IDicomNodeRepository
    {
        public bool Add(DicomNode entity)
        {
            throw new System.NotImplementedException();
        }

        public DicomNode Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public IQueryable<DicomNode> GetAll()
        {
            return new List<DicomNode>
            {
                new DicomNode { LogicalName = "Home PC", AeTitle = "ORTHANC", IpAddress = "127.0.0.1", Port = 4242 },
                new DicomNode { LogicalName = "Work PC", AeTitle = "KPSB", IpAddress = "172.28.42.42", Port = 104 },
                new DicomNode { LogicalName = "CAPI Server", AeTitle = "VTAIO", IpAddress = "***REMOVED***", Port = 104 },
                new DicomNode { LogicalName = "Synapse", AeTitle = "***REMOVED***", IpAddress = "***REMOVED***", Port = 104 },
                new DicomNode { LogicalName = "Syn Mini", AeTitle = "***REMOVED***", IpAddress = "***REMOVED***", Port = 104 }
            }.AsQueryable();
        }

        public bool Update(DicomNode entity)
        {
            throw new System.NotImplementedException();
        }

        public bool SaveChanges()
        {
            throw new System.NotImplementedException();
        }

        public bool Delete(DicomNode entity)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}