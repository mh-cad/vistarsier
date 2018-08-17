using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using System.Collections.Generic;

namespace CAPI.Common.Config
{
    public class DicomConfig //: IDicomConfig
    {
        public string DicomServicesExecutablesPath { get; set; }
        public IDicomNode LocalNode { get; set; }
        public List<IDicomNode> RemoteNodes { get; set; }

        public DicomConfig()
        {
            LocalNode = new DicomNode();
            RemoteNodes = new List<IDicomNode>();
        }
    }
}