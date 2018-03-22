namespace CAPI.ImageProcessing.Abstraction.ImageProcessor
{
    public interface IBrainMaskExtractor
    {
        void ExtractBrainMask(string inputFileFullPath, string outputPath, string bseParams,
            out string brainMaskRemoved, out string brainMask);
    }
}
