namespace CAPI.Dicom.Abstraction
{
    public interface IDicomImage
    {
        string ImageUid { get; set; }
        string LocationOnLocalDisk { get; set; }
    }
}
