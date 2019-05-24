using VisTarsier.Dicom.Abstractions;

namespace VisTarsier.Dicom
{
    public class DicomImage : IDicomImage
    {
        public string ImageUid { get; set; }
        public string LocationOnLocalDisk { get; set; }
    }
}