using CAPI.DAL.Abstraction;
using CAPI.Dicom.Abstraction;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.DAL
{
    public class DicomNodeRepositoryInMemory : IDicomNodeRepositoryInMemory
    {
        public DicomNodeRepositoryInMemory()
        {

        }

        public bool Add(IDicomNode entity)
        {
            throw new System.NotImplementedException();
        }
        public IDicomNode Get(int id)
        {
            throw new System.NotImplementedException();
        }
        public IQueryable<IDicomNode> GetAll(IDicomFactory dicomiFactory)
        {
            return new List<IDicomNode>
            {
                dicomiFactory.CreateDicomNode("Home PC", "ORTHANC", "127.0.0.1", 4242),
                dicomiFactory.CreateDicomNode("Work PC", "KPSB", "172.28.42.42", 104),
                dicomiFactory.CreateDicomNode("CAPI Server", "VTAIO", "***REMOVED***", 104),
                dicomiFactory.CreateDicomNode("Synapse", "***REMOVED***", "***REMOVED***", 104),
                dicomiFactory.CreateDicomNode("Syn-Mini", "***REMOVED***", "***REMOVED***", 104)
            }.AsQueryable();
        }
        public bool Update(IDicomNode entity)
        {
            throw new System.NotImplementedException();
        }

        public bool SaveChanges()
        {
            throw new System.NotImplementedException();
        }
        public bool Delete(IDicomNode entity)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}