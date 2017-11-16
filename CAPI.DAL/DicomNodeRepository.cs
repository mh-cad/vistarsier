using System.Collections.Generic;
using System.Linq;
using CAPI.Dicom.Model;

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
                new DicomNode { AeTitle = "ORTHANC", IpAddress = "127.0.0.1", Port = 4242 },
                new DicomNode { AeTitle = "KPSB", IpAddress = "172.28.42.42", Port = 104 },
                new DicomNode { AeTitle = "VTAIO", IpAddress = "172.28.43.65", Port = 104 }
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