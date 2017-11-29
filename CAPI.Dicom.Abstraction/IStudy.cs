namespace CAPI.Dicom.Abstraction
{
    public interface IStudy
    {
        string AccessionNumber { get; set; }
        string Region { get; set; }
        string StudyDescription { get; set; }
        string StudyUid { get; set; }
    }
}
