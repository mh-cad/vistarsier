using CAPI.Domain.Model;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessor
    {
        void ExtractBrainMask(ISeriesHdr hdrSeries, string outputPath, out ISeriesHdr brainMaskRemoved,
            out ISeriesHdr smoothBrainMask);

        void ExtractBrainMask(string inputHdrFullPath, string outputPath, out string brainMaskRemoved,
            out string smoothBrainMask);
    }
}