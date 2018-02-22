namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessor
    {
        void ExtractBrainMask(string inputHdrFullPath, string outputPath, string bseParams,
            out string brainMaskRemoved, out string smoothBrainMask);

        void Registration(string fixedFullPath, string seriesFloating, string outputPath,
            out string floatingReslicedFullPath, out IFrameOfReference fixedFrameOfRef);

        void TakeDifference(string fixedHdrFullPath, string floatingReslicedNiiFullPath,
            string brainSurfaceNiiFullPath, string outputDir,
            out string darkInFloating2BrightInFixed, out string brightInFloating2DarkInFixed,
            out string brainMask,
            string sliceInset = "0");

        void ColorMap(
            string fixedHdrFullPath, string fixedDicomFolderPath,
            string brainSurfaceNiiFullPath,
            string darkFloatToBrightFixedNiiFullPath,
            string brightFloatToDarkFixedNiiFullPath,
            string outputDir, out string positive, out string negative);

        void ConvertBmpsToDicom(string outputDir);

        void CopyDicomHeaders(string fixedDicomFolderPath, string outputDir
            , out string dicomFolderNewHeaders);
    }
}