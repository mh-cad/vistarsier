namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageConverter
    {
        void DicomToNiix(string inDicomDir, string outFile, string @params);
    }
}