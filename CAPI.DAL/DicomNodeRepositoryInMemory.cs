using CAPI.DAL.Abstraction;
using CAPI.Dicom.Abstraction;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.DAL
{
    public class DicomNodeRepositoryInMemory : IDicomNodeRepositoryInMemory
    {
        private readonly IDicomFactory _dicomFactory;

        public DicomNodeRepositoryInMemory(IDicomFactory dicomFactory)
        {
            _dicomFactory = dicomFactory;
        }

        public bool Add(IDicomNode entity)
        {
            throw new System.NotImplementedException();
        }
        public IDicomNode Get(int id)
        {
            throw new System.NotImplementedException();
        }
        public IQueryable<IDicomNode> GetAll()
        {
            return new List<IDicomNode>
            {
                _dicomFactory.CreateDicomNode("Home PC", "ORTHANC", "127.0.0.1", 4242),
                _dicomFactory.CreateDicomNode("Work PC", "KPSB", "172.28.42.42", 104),
                _dicomFactory.CreateDicomNode("CAPI Server", "VTAIO", "***REMOVED***", 104),
                _dicomFactory.CreateDicomNode("Synapse", "***REMOVED***", "***REMOVED***", 104),
                _dicomFactory.CreateDicomNode("Syn-Mini", "***REMOVED***", "***REMOVED***", 104)
            }.AsQueryable();
        }
        public bool Update(IDicomNode dicomNode)
        {
            throw new System.NotImplementedException();
        }
        public bool SaveChanges()
        {
            throw new System.NotImplementedException();
        }
        public bool Delete(IDicomNode dicomNode)
        {
            throw new System.NotImplementedException();
        }
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}