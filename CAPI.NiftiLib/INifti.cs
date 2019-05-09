using System.Collections.Generic;
using System.Drawing;

namespace CAPI.NiftiLib
{
    public interface INifti
    {
        /// <summary>
        /// The Header for this Nifti object.
        /// </summary>
        INiftiHeader Header { get; set; }
        
        /// <summary>
        /// The underlying voxels for the Nifti object.
        /// </summary>
        float[] voxels { get; set; }
        
        /// <summary>
        /// Cache of the voxel bytes(?) TODO: Should this be exposed in the interface?
        /// </summary>
        byte[] voxelsBytes { get; set; }
        
        /// <summary>
        /// The ColorMap, which is used to map intensity values to a given color range.
        /// </summary>
        Color[] ColorMap { get; set; }

        /// <summary>
        /// Reads Nifti from a given file name.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns>This Nifti object as populated from the file.</returns>
        INifti ReadNifti(string filepath);
        
        /// <summary>
        /// Reads the header for this Nifti object from the given header file path.
        /// </summary>
        /// <param name="filepath"></param>
        void ReadNiftiHeader(string filepath);
        
        /// <summary>
        /// Same as ReadNiftiHeader but returns the header as well (TODO: Why don't we just have this version?)
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        INiftiHeader ReadHeaderFromFile(string filepath);
        
        /// <summary>
        /// Write this Nifti object to a Nifti file.
        /// </summary>
        /// <param name="filepath"></param>
        void WriteNifti(string filepath);
        
        /// <summary>
        ///  Reorient voxels into new dimensions - setting dimension property to new values
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="slices"></param>
        void Reorient(short width, short height, short slices);
        
        /// <summary>
        /// Reorder voxels from Left-to-right, posterior-to-anterior, inferior-to-superior  -- to -- Anterior-to-posterior, superior-to-inferior, right-to-left
        /// </summary>
        void ReorderVoxelsLpi2Asr();
        
        /// <summary>
        /// Reorder voxels from Left-to-right, posterior-to-anterior, inferior-to-superior  -- to -- Anterior-to-posterior, inferior-to-superior, left-to-right
        /// </summary>
        void ReorderVoxelsLpi2Ail();
        
        /// <summary>
        /// Convert the header of this Nifti object to the RGB datatype.
        /// </summary>
        void ConvertHeaderToRgb();
        
        /// <summary>
        /// Convert the header of this Nifti object to the RGBA datatype.
        /// </summary>
        void ConvertHeaderToRgba();
        
        /// <summary>
        /// Set voxel RGB value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="sliceType"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        void SetPixelRgb(int x, int y, int z, SliceType sliceType, int r, int g, int b);
        
        /// <summary>
        /// Get the RGBA value for a given voxel.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="sliceType"></param>
        /// <returns></returns>
        int GetPixelColor(int x, int y, int z, SliceType sliceType);
        
        /// <summary>
        /// Get the raw voxel value for a given coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="sliceType"></param>
        /// <returns></returns>
        float GetValue(int x, int y, int z, SliceType sliceType);
        
        /// <summary>
        /// Populate voxels from a series of BMP image files.
        /// </summary>
        /// <param name="filepaths"></param>
        /// <param name="sliceType"></param>
        void ReadVoxelsFromRgb256Bmps(string[] filepaths, SliceType sliceType);
        
        /// <summary>
        /// Get the specified slice as a BMP.
        /// </summary>
        /// <param name="sliceIndex"></param>
        /// <param name="sliceType"></param>
        /// <returns></returns>
        Bitmap GetSlice(int sliceIndex, SliceType sliceType);
        
        /// <summary>
        /// Gets the dimensions for the Nifti file when sliced using the given sliceType.
        /// </summary>
        /// <param name="sliceType"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="nSlices"></param>
        void GetDimensions(SliceType sliceType, out int width, out int height, out int nSlices);
        
        /// <summary>
        /// Creates an enumerable of float arrays which each contained a flattened slice of voxel values.
        /// </summary>
        /// <param name="sliceType"></param>
        /// <returns></returns>
        IEnumerable<float[]> GetSlices(SliceType sliceType);
        
        /// <summary>
        /// Flattens the given 2-D array to a 1-D array of voxel values.
        /// </summary>
        /// <param name="slices"></param>
        /// <param name="sliceType"></param>
        /// <returns></returns>
        float[] SlicesToArray(float[][] slices, SliceType sliceType);
        
        /// <summary>
        /// Exports each slice to a bitmap file in the given folder path.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="sliceType"></param>
        void ExportSlicesToBmps(string folderPath, SliceType sliceType);
        
        /// <summary>
        /// Normalises each slice individually to the given mean, standard deviation, and range. Ignoring values where the mask is false.
        /// </summary>
        /// <param name="nifti"></param>
        /// <param name="sliceType"></param>
        /// <param name="mean"></param>
        /// <param name="std"></param>
        /// <param name="rangeWidth"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        INifti NormalizeEachSlice(INifti nifti, SliceType sliceType, int mean, int std,
                                  int rangeWidth, INifti mask);

        /// <summary>
        /// Normalises non-zero components which fall outside of the given mask.
        /// </summary>
        /// <param name="nim"></param>
        /// <param name="targetMean"></param>
        /// <param name="targetStdDev"></param>
        /// <param name="mask"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        INifti NormalizeNonBrainComponents(INifti nim, int targetMean, int targetStdDev, INifti mask, int start,
            int end);

        /// <summary>
        /// Generates a lookup table. TODO: How? Why?
        /// </summary>
        /// <param name="currentSlice"></param>
        /// <param name="priorSlice"></param>
        /// <param name="compareResult"></param>
        /// <param name="baseLut"></param>
        /// <returns></returns>
        Bitmap GenerateLookupTable(Bitmap currentSlice, Bitmap priorSlice, Bitmap compareResult, Bitmap baseLut = null);

        /// <summary>
        /// Assuming that if this Nifti object is being used as a mask, it will be inverted. The actual operation is converting each voxel value to : 2 * median - voxelValue
        /// </summary>
        void InvertMask();

        /// <summary>
        /// Creates a deep copy of this INifti instance.
        /// </summary>
        /// <returns></returns>
        INifti DeepCopy();

        /// <summary>
        /// Adds the given overlay to this nifti file, returning a deep copy which has been converted to RGB.
        /// </summary>
        /// <param name="overlay"></param>
        /// <returns></returns>
        INifti AddOverlay(INifti overlay);

        /// <summary>
        /// Recalculate the minimum and maximum value to be displayed based on current voxels.
        /// </summary>
        void RecalcHeaderMinMax();

    }
}