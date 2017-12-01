using CAPI.Domain.Model;
using CAPI.ImageProcessing.Abstraction;

namespace CAPI.ImageProcessing.Mock
{
    public class ImageProcessor : IImageProcessor
    {
        public void ExtractBrainMask(SeriesHdr hdrSeries, string outputPath, out SeriesHdr brainMaskRemoved,
            out SeriesHdr smoothBrainMask)
        {
            brainMaskRemoved = new SeriesHdr("", "", 0);
            smoothBrainMask = new SeriesHdr("", "", 0);
        }

        public void Registration(ISeries seriesFixed, ISeries seriesFloating, string outputPath)
        {

        }

        public void TakeDifference(ISeries seriesFixedHdr, ISeries seriesFloatingReslicedNii, ISeries seriesBrainSurfaceNii,
            string outputDir, string sliceInset)
        {

        }

        public void FlipAndConvertFloatingToDicom(SeriesNii seriesNii)
        {

        }

        public void ColorMap(string outputDir)
        {

        }
    }
}