namespace VisTarsier.Dicom.Abstractions
{
    public interface IDicomImage
    {
        string ImageUid { get; set; }
        string LocationOnLocalDisk { get; set; }
    }
}
