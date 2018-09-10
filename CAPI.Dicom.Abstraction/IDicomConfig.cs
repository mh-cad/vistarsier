namespace CAPI.Dicom.Abstractions
{
    public interface IDicomConfig
    {
        string ExecutablesPath { get; set; }
        string Img2DcmFilePath { get; set; }
    }
}
