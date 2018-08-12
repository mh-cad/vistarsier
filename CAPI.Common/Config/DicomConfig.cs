using CAPI.Dicom.Abstraction;
using System.Collections.Generic;
using IDicomConfig = CAPI.Common.Abstractions.Config.IDicomConfig;

namespace CAPI.Common.Config
{
    public class DicomConfig : IDicomConfig
    {
        public string DicomServicesExecutablesPath { get; set; }
        public IDicomNode LocalNode { get; set; }
        public List<IDicomNode> RemoteNodes { get; set; }
    }
}