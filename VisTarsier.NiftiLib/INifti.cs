using System.Collections.Generic;
using System.Drawing;
using VisTarsier.Config;

namespace VisTarsier.NiftiLib
{
    public interface INifti<T>
    {
        /// <summary>
        /// The Header for this Nifti object.
        /// </summary>
        INiftiHeader Header { get; set; }
        
        /// <summary>
        /// The underlying voxels for the Nifti object.
        /// </summary>
        T[] Voxels { get; set; }
        
        /// <summary>
        /// The ColorMap, which is used to map intensity values to a given color range.
        /// </summary>
        Color[] ColorMap { get; set; }

        /// <summary>
        /// Reads Nifti from a given file name.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns>This Nifti object as populated from the file.</returns>
        INifti<T> ReadNifti(string filepath);

        /// <summary>
        /// Reads the header for this Nifti object from the given header file path.
        /// </summary>
        /// <param name="filepath"></param>
        void ReadNiftiHeader(string filepath);
        
        /// <summary>
        /// Write this Nifti object to a Nifti file.
        /// </summary>
        /// <param name="filepath"></param>
        void WriteNifti(string filepath);
        
        /// <summary>
        /// Get the raw voxel value for a given coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="sliceType"></param>
        /// <returns></returns>
        T GetValue(int x, int y, int z, SliceType sliceType);

        
        /// <summary>
        /// Gets the dimensions for the Nifti file when sliced using the given sliceType.
        /// </summary>
        /// <param name="sliceType"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="nSlices"></param>
        void GetDimensions(SliceType sliceType, out int width, out int height, out int nSlices);
       
        /// <summary>
        /// Creates a deep copy of this INifti instance.
        /// </summary>
        /// <returns></returns>
        INifti<T> DeepCopy();

        /// <summary>
        /// Adds the given overlay to this nifti file, returning a deep copy which has been converted to RGB.
        /// </summary>
        /// <param name="overlay"></param>
        /// <returns></returns>
        INifti<T> AddOverlay(INifti<T> overlay);

        /// <summary>
        /// Recalculate the minimum and maximum value to be displayed based on current voxels.
        /// </summary>
        void RecalcHeaderMinMax();

    }
}