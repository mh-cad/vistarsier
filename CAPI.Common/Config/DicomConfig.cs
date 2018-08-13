using CAPI.Dicom.Model;
using System.Collections.Generic;

namespace CAPI.Common.Config
{
    public class DicomConfig //: IDicomConfig
    {
        public string DicomServicesExecutablesPath { get; set; }
        public DicomNode LocalNode { get; set; }
        public List<DicomNode> RemoteNodes { get; set; }

        public DicomConfig()
        {
            RemoteNodes = new List<DicomNode>();
        }
    }
}