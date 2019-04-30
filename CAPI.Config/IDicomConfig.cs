using CAPI.Dicom.Abstractions;
using System.Collections.Generic;

namespace CAPI.Config
{
    public interface IDicomConfig
    {
        string DicomServicesExecutablesPath { get; set; }

        IDicomNode LocalNode { get; set; }

        List<IDicomNode> RemoteNodes { get; set; }
    }
}
