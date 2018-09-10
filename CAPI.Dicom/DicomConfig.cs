using CAPI.Dicom.Abstractions;

namespace CAPI.Dicom
{
    public class DicomConfig : IDicomConfig
    {
        public string ExecutablesPath { get; set; }
        public string Img2DcmFilePath { get; set; }
    }
}
