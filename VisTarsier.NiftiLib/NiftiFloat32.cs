using VisTarsier.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using VisTarsier.Common;
using VisTarsier.Config;

namespace VisTarsier.NiftiLib
{
    /// <summary>
    /// Represents a Nifti-1 type file (reads from  file only if little endian)
    /// </summary>
    public class NiftiFloat32 : NiftiBase<float>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NiftiFloat32()
        {
            Header = new NiftiHeader
            {
                dim = new short[8],
                pix_dim = new float[8],
                srow_x = new float[4],
                srow_y = new float[4],
                srow_z = new float[4],
                sizeof_hdr = 348,
                vox_offset = 352,
                dim_info = string.Empty,
                slice_code = string.Empty,
                xyzt_units = string.Empty,
                descrip = string.Empty,
                aux_file = string.Empty,
                intent_name = string.Empty,
                magic = "n+1"
            };

            ColorMap = ColorMaps.GreyScale();
        }

        // TODO: Avoid using SetPixel (since it takes out a lock on the image, changes pixel, unlocks.
        // This is super slow and we can just take out the lock once. Also there's a bunch of stuff we're doing
        // in the inner loop which we can just do up front.
        // Example code so I remember what to try again:
        //    var bmpData = slice.LockBits(new Rectangle(0, 0, slice.Width, slice.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        //    unsafe
        //    {
        //        var u = (int*)bmpData.Scan0;
        //        ...
        //    }
        //    slick.UnlockBits(bmpData);
        public Bitmap GetSlice(int sliceIndex, SliceType sliceType)
        {
            if (Header.dim == null) throw new NullReferenceException("Nifti file header dim is null");
            if (Header.dim.Length < 8 ||
                Header.dim[0] < 3 ||
                Header.dim[1] < 1 || Header.dim[2] < 1 || Header.dim[3] < 1)
                throw new NullReferenceException("Nifti file header dim 0,1,2,3 are not valid");

            GetDimensions(sliceType, out var width, out var height, out var nSlices);
            if (sliceIndex >= nSlices) throw new ArgumentOutOfRangeException($"Slice index out of range. No of slices = {nSlices}");

            var slice = new Bitmap(width, height);

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    //var color = Color.FromArgb(GetPixelColor(x, y, sliceIndex, sliceType)).SwapRedBlue();
                    slice.SetPixel(x, y, Color.FromArgb(GetPixelColor(x, y, sliceIndex, sliceType)));
                }

            //var bmp = new Bitmap(slice.Bitmap);
            //slice.Dispose();
            return slice;
        }


        public int GetPixelColor(int x, int y, int z, SliceType sliceType)
        {
            var voxelIndex = GetVoxelIndex(x, y, z, sliceType);
            var voxelValue = (int)Voxels[voxelIndex];

            switch (Header.datatype)
            {
                case NiftiType.FLOAT32:
                case NiftiType.UINT8: // Standard intensity TODO: Make good. 
                case NiftiType.INT16: // GrayScale 16bit
                    if (Math.Abs(Header.cal_min - Header.cal_max) < .1)
                    {
                        Header.cal_min = Voxels.Min();
                        Header.cal_max = Voxels.Max();
                    }

                    var range = Header.cal_max - Header.cal_min;
                    var scale = ColorMap.Length / range;
                    var bias = -(Header.cal_min);

                    int idx = (int)((GetValue(x, y, z, sliceType) + bias) * scale);
                    if (idx < 0) idx = 0;
                    else if (idx > ColorMap.Length - 1) idx = ColorMap.Length - 1;

                    return ColorMap[idx].ToArgb();

                case NiftiType.RGB24: // RGB 24bit
                case NiftiType.RGBA32: // ARGB 32bit
                    return voxelValue;
                default:
                    throw new NotImplementedException($"datatype {Header.datatype} not suported!");
            }
        }

        public void SetPixelRgb(int x, int y, int z, SliceType sliceType, int r, int g, int b)
        {
            var voxelIndex = GetVoxelIndex(x, y, z, sliceType);
            Voxels[voxelIndex] = r << 16 | g << 8 | b;
        }

        // TODO: I understand why we're just using float32 to hold everything in the background, 
        // but it may be worth storing these in the way that we're saying just for readability?
        // Would have to check on the performance hit and if it's worth potentially introducing 
        // even more bugs.
        public void ConvertHeaderToRgba()
        {
            Header.dim[0] = 5; // RGB and RGBA both have 5 dimensions
            Header.dim[4] = 1; // time
            Header.dim[5] = 4; // 4 channels for RGBA
            Header.bitpix = 32;
            Header.datatype = NiftiType.RGBA32;
            Header.intent_code = 2004;
        }

        public void ConvertHeaderToRgb()
        {
            Header.dim[0] = 5; // RGB and RGBA both have 5 dimensions
            Header.dim[4] = 1; // time
            Header.dim[5] = 3; // 3 channels for RGB
            Header.bitpix = 24;
            Header.datatype = NiftiType.RGB24;
            Header.intent_code = 2003;
        }

        public void ConvertHeaderToGrayScale16Bit()
        {
            Header.dim[0] = 4; // 3 spatial and one temporal dimension
            Header.dim[4] = 1; // time
            Header.bitpix = 16;
            Header.datatype = NiftiType.INT16;
        }

        /// <summary>
        /// Reorient voxels into new dimensions - settings dim property to new values
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="slices"></param>
        public void Reorient(short width, short height, short slices)
        {
            if (width * height * slices != Voxels.Length)
                throw new Exception("number of voxels and new dimensions don't match. " +
                                    $"Width: {width} Height: {height} Slices: {slices} No. of voxels: {Voxels.Length}");

            Header.dim[0] = 3;
            Header.dim[1] = width;
            Header.dim[2] = height;
            Header.dim[3] = slices;

            //Header.dim[1] = slices;
            //Header.dim[2] = width;
            //Header.dim[3] = height;

            var oldVoxels = Voxels;
            Voxels = new float[Voxels.Length];
            for (var z = 0; z < slices; z++)
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var op = z + (width - 1 - x) * slices + (height - 1 - y) * slices * width;
                        var np = x + y * width + z * width * height;
                        Voxels[np] = oldVoxels[op];
                    }
        }

        public void ReorderVoxelsLpi2Asr()
        {
            Header.dim[0] = 3;
            var d1 = Header.dim[1];
            var d2 = Header.dim[2];
            var d3 = Header.dim[3];

            if (d2 * d3 * d1 != Voxels.Length)
                throw new Exception("number of voxels and new dimensions don't match. " +
                                    $"[Assuming Sagittal] Width: {d2} Height: {d3} Slices: {d1} No. of voxels: {Voxels.Length}");

            var oldVoxels = Voxels;
            Voxels = new float[Voxels.Length];
            for (var z = 0; z < d3; z++)
                for (var y = 0; y < d2; y++)
                    for (var x = 0; x < d1; x++)
                    {
                        var currentPixelIndex = x + y * d1 + z * d1 * d2;
                        var newI1 = d2 - 1 - y;
                        var newI2 = d3 - 1 - z;
                        var newI3 = d1 - 1 - x;
                        var newPixelIndex = newI1 + newI2 * d2 + newI3 * d2 * d3;

                        Voxels[newPixelIndex] = oldVoxels[currentPixelIndex];
                    }

            Header.dim[1] = d2;
            Header.dim[2] = d3;
            Header.dim[3] = d1;
        }

        public void ReorderVoxelsLpi2Ail()
        {
            Header.dim[0] = 3;
            var d1 = Header.dim[1];
            var d2 = Header.dim[2];
            var d3 = Header.dim[3];

            if (d2 * d3 * d1 != Voxels.Length)
                throw new Exception("number of voxels and new dimensions don't match. " +
                                    $"dim1: {d1} / dim2: {d2} / dim3: {d3} | No. of voxels: {Voxels.Length}");

            var oldVoxels = Voxels;
            Voxels = new float[Voxels.Length];
            for (var z = 0; z < d3; z++)
                for (var y = 0; y < d2; y++)
                    for (var x = 0; x < d1; x++)
                    {
                        var currentPixelIndex = x + y * d1 + z * d1 * d2;
                        var newI1 = d2 - 1 - y;
                        var newI2 = z;
                        var newI3 = x;
                        var newPixelIndex = newI1 + newI2 * d2 + newI3 * d2 * d3;

                        Voxels[newPixelIndex] = oldVoxels[currentPixelIndex];
                    }

            Header.dim[1] = d2;
            Header.dim[2] = d3;
            Header.dim[3] = d1;
        }

        /// <summary>
        /// Reads voxels from byte 352 for length of bytesLength parameter
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="bytesLength"></param>
        /// <returns>Voxels as an array of float</returns>
        public override float[] ReadVoxels(BinaryReader reader, int bytesLength)
        {
            switch (Header.datatype)
            {
                case 1: // 1 bit
                    {
                        Voxels = new float[bytesLength];
                        for (var i = 0; i < bytesLength; i++)
                            Voxels[i] = reader.ReadBoolean() ? 1 : 0;
                        break;
                    }

                case 2: // 8 bits
                    {
                        Voxels = new float[bytesLength];
                        for (var i = 0; i < bytesLength; i++)
                            Voxels[i] = reader.ReadByte();
                        break;
                    }

                case 4: // 16 bits
                    {
                        Voxels = new float[bytesLength / 2];
                        for (var i = 0; i < bytesLength / 2; i++)
                            Voxels[i] = reader.ReadInt16();
                        break;
                    }

                case 8: // 32 bits
                case 2304: // RGBA  (32 bit)
                    {
                        Voxels = new float[bytesLength / 4];
                        for (var i = 0; i < bytesLength / 4; i++)
                            Voxels[i] = reader.ReadInt32();
                        break;
                    }

                case 16: // float
                    {
                        Voxels = new float[bytesLength / 4];
                        for (var i = 0; i < bytesLength / 4; i++)
                            Voxels[i] = reader.ReadSingle();
                        break;
                    }
                case 128: // RGB (24 bit)
                    {
                        Voxels = new float[bytesLength / 3];
                        for (var i = 0; i < bytesLength / 3; i++)
                        {
                            var byte1 = reader.ReadByte();
                            var byte2 = reader.ReadByte();
                            var byte3 = reader.ReadByte();
                            Voxels[i] = byte3 << 16 | byte2 << 8 | byte1;
                        }
                        break;
                    }

                default:
                    throw new ArgumentException($"Nifti datatype [{Header.datatype}] not supported.");
            }
            return Voxels;
        }

        public override byte[] GetVoxelsBytes()
        {
            byte[] voxelsBytes = new byte[GetTotalSize() - (int)Header.vox_offset];
            var bytePix = Header.bitpix / 8;

            for (var i = 0; i < Voxels.Length; i++)
            {
                for (var j = 0; j < bytePix; j++)
                {
                    var position = i * bytePix + j;
                    switch (bytePix)
                    {
                        case 1 when Header.cal_min >= 0:
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToByte(Voxels[i]))[j], position);
                            break;
                        case 1 when Header.cal_min < 0:
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToSByte(Voxels[i]))[j], position);
                            break;
                        case 2:
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToInt16(Voxels[i])).ToArray()[j], position);
                            break;
                        case 3 when Header.cal_min >= 0:
                            // RGB is 24bit so needs three bytes for each pixel (bytePix=3) hence .Take(3)
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToUInt32(Voxels[i])).Take(3).ToArray()[j], position);
                            break;
                        case 4 when Header.cal_min >= 0 && (Header.datatype == 8 || Header.datatype == 768): // signed (8) or unsigned (768) int
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToUInt32(Voxels[i])).ToArray()[j], position);
                            break;
                        case 4 when Header.cal_min < 0 && (Header.datatype == 8 || Header.datatype == 768): // signed (8) or unsigned (768) int
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToInt32(Voxels[i])).ToArray()[j], position);
                            break;
                        case 4 when Header.datatype == 16: // float
                            voxelsBytes.SetValue(BitConverter.GetBytes(Voxels[i]).ToArray()[j], position);
                            break;
                        case 4 when Header.datatype == 2304:
                            // This should be RGBA, TODO: Make sure this makes sense (I think it does, but will need a test if it's being used).
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToInt32(Voxels[i])).ToArray()[j], position);
                            break;
                        default:
                            throw new Exception($"Bitpix {Header.bitpix} not supported!");
                    }
                }

            }
            return voxelsBytes;
        }

        public void ExportSlicesToBmps(string folderPath, SliceType sliceType)
        {
            if (Header.dim == null || Header.dim.Length < 4 || Header.dim[3] == 0)
                throw new Exception("dim[3] in nifti file header not valid");
            if (Directory.Exists(folderPath)) throw new Exception($"Folder already exists! [{folderPath}]");
            Directory.CreateDirectory(folderPath);

            var sliceCount = Header.dim[(int)sliceType + 1];
            var digits = (int)Math.Log10(sliceCount) + 1;
            for (var i = 0; i < sliceCount; i++)
                GetSlice(i, sliceType).Save($@"{folderPath}\{(i + 1).ToString($"D{digits}")}.bmp", ImageFormat.Bmp);
        }

        public override INifti<float> AddOverlay(INifti<float> overlay)
        {
            NiftiFloat32 output = (NiftiFloat32)(this.DeepCopy());

            // Caclulate conversion to colour map.
            var range = Header.cal_max - Header.cal_min;
            var scale = ColorMap.Length / range;
            var bias = -(Header.cal_min);

            // Caclulate conversion to colour map.
            var rangeOverlay = overlay.Header.cal_max - overlay.Header.cal_min;
            var scaleOverlay = overlay.ColorMap.Length / rangeOverlay;
            var biasOverlay = -(overlay.Header.cal_min);

            for (int i = 0; i < Voxels.Length; ++i)
            {
                // Get the colour map index for our value.
                int idx = (int)((Voxels[i] + bias) * scale);
                if (idx < 0) idx = 0;
                else if (idx > ColorMap.Length - 1) idx = ColorMap.Length - 1;

                // Get the colour map value for the overlay
                int idxOverlay = (int)((overlay.Voxels[i] + biasOverlay) * scaleOverlay);
                if (idxOverlay < 0) idxOverlay = 0;
                else if (idxOverlay > overlay.ColorMap.Length - 1) idxOverlay = overlay.ColorMap.Length - 1;

                var red = (overlay.ColorMap[idxOverlay].R * overlay.ColorMap[idxOverlay].A + ColorMap[idx].R * (255 - overlay.ColorMap[idxOverlay].A)) / 255;
                var green = (overlay.ColorMap[idxOverlay].G * overlay.ColorMap[idxOverlay].A + ColorMap[idx].G * (255 - overlay.ColorMap[idxOverlay].A)) / 255;
                var blue = (overlay.ColorMap[idxOverlay].B * overlay.ColorMap[idxOverlay].A + ColorMap[idx].B * (255 - overlay.ColorMap[idxOverlay].A)) / 255;

                output.Voxels[i] = Convert.ToUInt32((byte)red << 16 | (byte)green << 8 | (byte)blue);
            }

            output.ConvertHeaderToRgb();
            output.RecalcHeaderMinMax();

            return output;
        }

        public override INifti<float> DeepCopy()
        {
            var copy = new NiftiFloat32()
            {
                Header = Header.DeepCopy(),
                Voxels = new float[Voxels.Length]
            };

            Voxels.CopyTo(copy.Voxels, 0);

            copy.ColorMap = new Color[ColorMap.Length];
            ColorMap.CopyTo(copy.ColorMap, 0);

            return copy;
        }

        public override void RecalcHeaderMinMax()
        {
            Header.cal_min = Voxels.Min();
            Header.cal_max = Voxels.Max();
        }

        public override int GetVoxelIndex(int x, int y, int z, SliceType sliceType)
        {
            if (Header.dim[1] == 0 || Header.dim[2] == 0 || Header.dim[3] == 0)
                throw new Exception("Nifti header dimensions not set!");
            var ltRt = Header.dim[1];
            var antPos = Header.dim[2];
            var infSup = Header.dim[3];
            // TODO : This is the problem with having a header that doesn't reflect our
            // data storage in memory. We should be able to index in based on the header. 
            
            switch (sliceType)
            {
                case SliceType.Axial:
                    return (ltRt - 1 - x) + ltRt * (antPos - 1 - y) + ltRt * antPos * z;
                case SliceType.Sagittal:
                    return z + ltRt * (antPos - 1 - x) + ltRt * antPos * (infSup - 1 - y);
                case SliceType.Coronal:
                    return (ltRt - 1 - x) + ltRt * z + ltRt * antPos * (infSup - 1 - y);
                default:
                    throw new Exception("Slice Type not supported");
            }
        }
    }
}