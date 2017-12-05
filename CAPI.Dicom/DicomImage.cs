using CAPI.Dicom.Abstraction;

namespace CAPI.Dicom
{
    public class DicomImage : IDicomImage
    {
        public string ImageUid { get; set; }
        public string LocationOnLocalDisk { get; set; }
    }
}