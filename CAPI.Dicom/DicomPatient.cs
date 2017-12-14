using CAPI.Dicom.Abstraction;

namespace CAPI.Dicom
{
    public class DicomPatient : IDicomPatient
    {
        public string PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientBirthDate { get; set; }
    }
}
