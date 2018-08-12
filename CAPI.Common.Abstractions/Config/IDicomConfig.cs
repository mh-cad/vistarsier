using CAPI.Dicom.Abstraction;
using System.Collections.Generic;

namespace CAPI.Common.Abstractions.Config
{
    public interface IDicomConfig
    {
        string DicomServicesExecutablesPath { get; set; }

        IDicomNode LocalNode { get; set; }

        List<IDicomNode> RemoteNodes { get; set; }
    }
}
