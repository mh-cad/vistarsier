using System.Collections.Generic;
using System.Drawing;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface INifti
    {
        INiftiHeader Header { get; set; }
        float[] voxels { get; set; }
        byte[] voxelsBytes { get; set; }

        void ReadNifti(string filepath);
        void ReadNiftiHeader(string filepath);
        void WriteNifti(string filepath);
        void Reorient(short width, short height, short slices);
        void ConvertHeaderToRgb();
        void ConvertHeaderToRgba();
        void SetPixelRgb(int x, int y, int z, SliceType sliceType, int r, int g, int b);
        int GetPixelColor(int x, int y, int z, SliceType sliceType);
        INiftiHeader ReadHeaderFromFile(string filepath);
        Bitmap GetSlice(int sliceIndex, SliceType sliceType);
        IEnumerable<float[]> GetSlices(SliceType sliceType);
        float[] SlicesToArray(float[][] slices, SliceType sliceType);
        void ExportSlicesToBmps(string folderpath, SliceType sliceType);
        INifti Compare(INifti floatingResliced, SliceType sliceType, ISubtractionLookUpTable lookUpTable);
    }
}