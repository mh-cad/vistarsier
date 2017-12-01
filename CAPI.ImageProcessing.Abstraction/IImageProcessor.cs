using CAPI.Domain.Model;
using SeriesHdr = CAPI.Domain.Model.SeriesHdr;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessor
    {
        void ExtractBrainMask(SeriesHdr hdrSeries, string outputPath, out SeriesHdr brainMaskRemoved,
            out SeriesHdr smoothBrainMask);

        void Registration(ISeries seriesFixed, ISeries seriesFloating, string outputPath);

        void TakeDifference(ISeries seriesFixedHdr, ISeries seriesFloatingReslicedNii, ISeries seriesBrainSurfaceNii,
            string outputDir, string sliceInset);

        void FlipAndConvertFloatingToDicom(SeriesNii seriesNii);

        void ColorMap(string outputDir);
    }
}
