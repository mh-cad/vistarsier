namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessor
    {
        void ExtractBrainMask(string inputHdrFullPath, string outputPath,
            out string brainMaskRemoved, out string smoothBrainMask);

        void Registration(string fixedFullPath, string seriesFloating, string outputPath,
            out string floatingReslicedFullPath, out IFrameOfReference fixedFrameOfRef);
    }
}