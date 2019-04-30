using CAPI.Dicom.Abstractions;
using CAPI.Dicom.Model;
using System.Collections.Generic;

namespace CAPI.Config
{
    public class DicomConfig //: IDicomConfig
    {
        //public string DicomServicesExecutablesPath { get; set; }
        public string Img2DcmFilePath { get; set; }
        public IDicomNode LocalNode { get; set; }
        public List<IDicomNode> RemoteNodes { get; set; }

        public DicomConfig()
        {
            LocalNode = new DicomNode();
            RemoteNodes = new List<IDicomNode>();
        }
    }
}