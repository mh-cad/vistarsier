namespace CAPI.Dicom.Abstraction
{
    public interface IDicomPatient
    {
        string PatientId { get; set; }
        string PatientFullName { get; set; }
        string PatientBirthDate { get; set; }
    }
}