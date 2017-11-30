using CAPI.Dicom.Abstraction;

namespace CAPI.Dicom
{
    public class DicomStudy : IDicomStudy
    {
        public string AccessionNumber { get; set; }
        public string Region { get; set; }
        public string StudyDescription { get; set; }
        public string StudyUid { get; set; }
    }
}