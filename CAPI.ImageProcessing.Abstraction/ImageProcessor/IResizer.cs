namespace CAPI.ImageProcessing.Abstraction.ImageProcessor
{
    public interface IResizer
    {
        string ResizeToDestWidth(string hdrFileFullPath, int destinationWidth);
        string ResizeNiiToSameSize(string resizedTargetHdr, string originalHdrFileFullPath);
    }
}
