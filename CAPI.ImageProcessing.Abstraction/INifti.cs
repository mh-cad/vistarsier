using System.Collections.Generic;
using System.Drawing;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface INifti
    {
        INiftiHeader Header { get; set; }
        float[] voxels { get; set; }
        byte[] voxelsBytes { get; set; }

        INifti ReadNifti(string filepath);
        void ReadNiftiHeader(string filepath);
        void WriteNifti(string filepath);
        void Reorient(short width, short height, short slices);
        void ReorderVoxelsLpi2Asr();
        void ReorderVoxelsLpi2Ail();
        void ConvertHeaderToRgb();
        void ConvertHeaderToRgba();
        void SetPixelRgb(int x, int y, int z, SliceType sliceType, int r, int g, int b);
        int GetPixelColor(int x, int y, int z, SliceType sliceType);
        INiftiHeader ReadHeaderFromFile(string filepath);
        void ReadVoxelsFromRgb256Bmps(string[] filepaths, SliceType sliceType);
        Bitmap GetSlice(int sliceIndex, SliceType sliceType);
        void GetDimensions(SliceType sliceType, out int width, out int height, out int nSlices);
        IEnumerable<float[]> GetSlices(SliceType sliceType);
        float[] SlicesToArray(float[][] slices, SliceType sliceType);
        void ExportSlicesToBmps(string folderPath, SliceType sliceType);

        INifti Compare(INifti current, INifti prior, SliceType sliceType,
            ISubtractionLookUpTable lookUpTable, string workingDir, INifti currentResliced = null, INifti mask = null);

        INifti NormalizeEachSlice(INifti nifti, SliceType sliceType, int mean, int std,
                                  int rangeWidth, INifti mask);

        INifti NormalizeNonBrainComponents(INifti nim, int targetMean, int targetStdDev, INifti mask, int start,
            int end);

        Bitmap GenerateLookupTable(Bitmap currentSlice, Bitmap priorSlice, Bitmap compareResult, Bitmap baseLut = null);
        void InvertMask();

    }
}