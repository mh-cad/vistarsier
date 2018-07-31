using CAPI.Common.Abstractions.Config;

namespace CAPI.Common.Config
{
    public class DicomConfig : IDicomConfig
    {
        public string DicomServicesExecutablesPath { get; set; }
    }
}