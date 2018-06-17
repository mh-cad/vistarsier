using CAPI.Common.Extensions;
using CAPI.ImageProcessing.Abstraction;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace CAPI.ImageProcessing
{
    /// <summary>
    /// Represents a Nifti-1 type file (reads from  file only if little endian)
    /// </summary>
    public class Nifti : INifti
    {
        public INiftiHeader Header { get; set; }
        public float[] voxels { get; set; }
        public byte[] voxelsBytes { get; set; }

        //#region "Properties"
        //public int sizeof_hdr { get; set; }
        //public string dim_info { get; set; }
        //public short[] dim { get; set; }
        //public float intent_p1 { get; set; }
        //public float intent_p2 { get; set; }
        //public float intent_p3 { get; set; }
        //public short intent_code { get; set; }
        //public short datatype { get; set; }
        //public short bitpix { get; set; }
        //public short slice_start { get; set; }
        //public float[] pix_dim { get; set; }
        //public float vox_offset { get; set; }
        //public float scl_slope { get; set; }
        //public float scl_inter { get; set; }
        //public short slice_end { get; set; }
        //public string slice_code { get; set; }
        //public string xyzt_units { get; set; }
        //public float cal_max { get; set; }
        //public float cal_min { get; set; }
        //public float slice_duration { get; set; }
        //public float toffset { get; set; }
        //public string descrip { get; set; }
        //public string aux_file { get; set; }
        //public short qform_code { get; set; }
        //public short sform_code { get; set; }
        //public float quatern_b { get; set; }
        //public float quatern_c { get; set; }
        //public float quatern_d { get; set; }
        //public float qoffset_x { get; set; }
        //public float qoffset_y { get; set; }
        //public float qoffset_z { get; set; }
        //public float[] srow_x { get; set; }
        //public float[] srow_y { get; set; }
        //public float[] srow_z { get; set; }
        //public string intent_name { get; set; }
        //public string magic { get; set; }
        //#endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Nifti()
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
        }

        /// <summary>
        /// Read header and voxels from the nifti file of type nifti-1 (magic:n+1)
        /// </summary>
        /// <param name="filepath"></param>
        public void ReadNifti(string filepath)
        {
            var fileReader = new FileStream(filepath, FileMode.Open);
            if (!fileReader.CanRead) throw new IOException($"Unable to access file: {filepath}");
            var reader = new BinaryReader(fileReader);

            ReadNiftiHeader(reader);

            var bytesLength = Convert.ToInt32(reader.BaseStream.Length) - Convert.ToInt32(Header.vox_offset);
            voxels = ReadVoxels(reader, bytesLength);

            if (reader.BaseStream.Position != reader.BaseStream.Length)
                throw new Exception("Not all bytes in the stream were read!");
        }
        /// <summary>
        /// Reads only the header of a nifti-1 file
        /// </summary>
        /// <param name="filepath"></param>
        public void ReadNiftiHeader(string filepath)
        {
            var fileReader = new FileStream(filepath, FileMode.Open);
            if (!fileReader.CanRead) throw new IOException($"Unable to access file: {filepath}");
            var reader = new BinaryReader(fileReader);

            ReadNiftiHeader(reader);
        }
        private void ReadNiftiHeader(BinaryReader reader)
        {
            Header.sizeof_hdr = reader.ReadInt32();
            if (Header.sizeof_hdr != 348)
                throw new Exception("Either not a nifti File or type not supported! siezof_hdr should be 348 for a nifti-1 file.");

            for (var i = 0; i < 35; i++) reader.ReadByte();

            Header.dim_info = ReadString(reader, 1);
            for (var i = 0; i < 8; i++) Header.dim[i] = reader.ReadInt16();

            Header.intent_p1 = reader.ReadSingle();
            Header.intent_p2 = reader.ReadSingle();
            Header.intent_p3 = reader.ReadSingle();
            Header.intent_code = reader.ReadInt16();

            Header.datatype = reader.ReadInt16();
            Header.bitpix = reader.ReadInt16();
            Header.slice_start = reader.ReadInt16();

            for (var i = 0; i < 8; i++) Header.pix_dim[i] = reader.ReadSingle();

            Header.vox_offset = reader.ReadSingle();
            Header.scl_slope = reader.ReadSingle();
            Header.scl_inter = reader.ReadSingle();
            Header.slice_end = reader.ReadInt16();
            Header.slice_code = reader.ReadByte().ToString();
            Header.xyzt_units = reader.ReadByte().ToString();
            Header.cal_max = reader.ReadSingle();
            Header.cal_min = reader.ReadSingle();
            Header.slice_duration = reader.ReadSingle();
            Header.toffset = reader.ReadSingle();
            reader.ReadInt32(); // glmax
            reader.ReadInt32(); // glmin

            Header.descrip = ReadString(reader, 80);
            Header.aux_file = ReadString(reader, 24);

            Header.qform_code = reader.ReadInt16();
            Header.sform_code = reader.ReadInt16();

            Header.quatern_b = reader.ReadSingle();
            Header.quatern_c = reader.ReadSingle();
            Header.quatern_d = reader.ReadSingle();
            Header.qoffset_x = reader.ReadSingle();
            Header.qoffset_y = reader.ReadSingle();
            Header.qoffset_z = reader.ReadSingle();

            for (var i = 0; i < 4; i++) Header.srow_x[i] = reader.ReadSingle();
            for (var i = 0; i < 4; i++) Header.srow_y[i] = reader.ReadSingle();
            for (var i = 0; i < 4; i++) Header.srow_z[i] = reader.ReadSingle();

            Header.intent_name = ReadString(reader, 16);
            Header.magic = ReadString(reader, 4);

            for (var i = 0; i < 4; i++) reader.ReadByte(); // 4 bytes gap between header and voxels
        }
        public INiftiHeader ReadHeaderFromFile(string filepath)
        {
            ReadNiftiHeader(filepath);
            return Header;
        }

        public void ExportSlicesToBmps(string folderpath, SliceType sliceType)
        {
            if (Header.dim == null || Header.dim.Length < 4 || Header.dim[3] == 0)
                throw new Exception("dim[3] in nifti file header not valid");
            if (Directory.Exists(folderpath)) throw new Exception($"Folder already exists! [{folderpath}]");
            Directory.CreateDirectory(folderpath);

            for (var i = 0; i < Header.dim[(int)sliceType + 1]; i++)
                GetSlice(i, sliceType).Save($@"{folderpath}\{i}.bmp");
        }

        public INifti Compare(INifti floating, SliceType sliceType, ISubtractionLookUpTable lookUpTable)
        {
            // get FIXED and normalize each slice
            var fixedSlices = GetSlices(sliceType).ToArray();
            if (fixedSlices == null) throw new Exception("No slices found in file being compared");

            for (var i = 0; i < fixedSlices.Length; i++) // Normalize each slice
                fixedSlices[i] = fixedSlices[i].Normalize(byte.MaxValue / 2, byte.MaxValue / 8);

            var fixedVoxels = SlicesToArray(fixedSlices, sliceType); // Return back from slices to a single array
            fixedVoxels.Trim(0, lookUpTable.Width - 1);

            // get FLOATING and normalize each slice
            var floatingSlices = floating.GetSlices(sliceType).ToArray();
            if (floatingSlices == null) throw new Exception("No slices found in file being compared");

            for (var i = 0; i < floatingSlices.Length; i++) // Normalize each slice
                floatingSlices[i] = floatingSlices[i].Normalize(byte.MaxValue / 2, byte.MaxValue / 8);

            var floatingVoxels = SlicesToArray(floatingSlices, sliceType); // Return back from slices to a single array
            floatingVoxels.Trim(0, lookUpTable.Width - 1);

            // COMPARE
            for (var i = 0; i < voxels.Length; i++)
                voxels[i] = lookUpTable.Pixels[(int)fixedVoxels[i], (int)floatingVoxels[i]].ToArgb();

            ConvertHeaderToRgb();

            return this;
        }

        public IEnumerable<float[]> GetSlices(SliceType sliceType)
        {
            GetSagittalDimensions(sliceType, out var width, out var height, out var nSlices);
            var slices = new float[nSlices][];
            for (var z = 0; z < nSlices; z++)
            {
                slices[z] = new float[width * height];
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var pixelIndex = GetVoxelIndex(x, y, z, sliceType);
                        slices[z][x + y * width] = voxels[pixelIndex];
                    }
            }

            return slices;
        }
        public float[] SlicesToArray(float[][] slices, SliceType sliceType)
        {
            var allVoxels = new float[slices.Length * slices[0].Length];

            GetSagittalDimensions(sliceType, out var width, out var height, out var nSlices);

            for (var z = 0; z < slices.Length; z++)
            {

                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                        allVoxels[GetVoxelIndex(x, y, z, sliceType)]
                            = slices[z][x + y * width];
            }

            return allVoxels;
        }

        private static float[] Scale(IEnumerable<float> voxelsArr, int min, int max)
        {
            var vs = voxelsArr as float[] ?? voxelsArr.ToArray();
            var voxelsMax = vs.Max();
            var voxelsMin = vs.Min();
            return vs.ToList()
                .Select(v => (v - voxelsMin) * (max - min) / (voxelsMax - voxelsMin))
                .ToArray();
        }

        //public float[] Normalize(int mean, int stdDev)
        //{
        //    var currMean = voxels.Mean();
        //    var currStdDev = voxels.StandardDeviation();

        //    for (var i = 0; i < voxels.Length; ++i)
        //        voxels[i] = (float)((voxels[i] - currMean) / (currStdDev)) * stdDev + mean;

        //    return voxels; // TODO1: Not Implemented
        //}

        public Bitmap GetSlice(int sliceIndex, SliceType sliceType)
        {
            if (Header.dim == null) throw new NullReferenceException("Nifti file header dim is null");
            if (Header.dim.Length < 8 ||
                Header.dim[0] < 3 ||
                Header.dim[1] < 1 || Header.dim[2] < 1 || Header.dim[3] < 1)
                throw new NullReferenceException("Nifti file header dim 0,1,2,3 are not valid");

            GetSagittalDimensions(sliceType, out var width, out var height, out var nSlices);
            if (sliceIndex >= nSlices) throw new ArgumentOutOfRangeException($"Slice index out of range. No of slices = {nSlices}");

            var slice = new Bitmap(width, height);

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var color = Color.FromArgb(GetPixelColor(x, y, sliceIndex, sliceType));
                    slice.SetPixel(x, y, color);
                }

            return slice;
        }

        public int GetPixelColor(int x, int y, int z, SliceType sliceType)
        {
            var voxelIndex = GetVoxelIndex(x, y, z, sliceType);
            var voxelValue = (int)voxels[voxelIndex];

            switch (Header.datatype)
            {
                case 4: // GrayScale 16bit
                    if (Math.Abs(Header.cal_min - Header.cal_max) < .1)
                    {
                        Header.cal_min = voxels.Min();
                        Header.cal_max = voxels.Max();
                    }
                    // Scale voxel value to 0-255 range
                    var val = (int)((voxelValue - Header.cal_min) * 255 / (Header.cal_max - Header.cal_min));
                    return Color.FromArgb(val, val, val).ToArgb();
                case 128: // RGB 24bit
                case 2304: // ARGB 32bit
                    return voxelValue;
                default:
                    throw new NotImplementedException($"datatype {Header.datatype} not suported!");
            }
        }

        private void GetSagittalDimensions(SliceType sliceType, out int width, out int height, out int nSlices)
        {
            switch (sliceType)
            {
                case SliceType.Sagittal:
                    width = Header.dim[2];
                    height = Header.dim[3];
                    nSlices = Header.dim[1];
                    break;
                case SliceType.Coronal:
                    width = Header.dim[1];
                    height = Header.dim[3];
                    nSlices = Header.dim[2];
                    break;
                case SliceType.Axial:
                    width = Header.dim[1];
                    height = Header.dim[2];
                    nSlices = Header.dim[3];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sliceType), sliceType, null);
            }
        }

        public void WriteNifti(string filepath)
        {
            var buffer = WriteNiftiHeaderBytes();
            buffer = WriteVoxelsBytes(buffer);

            using (var sw = new FileStream(filepath, FileMode.Create))
            using (var wr = new BinaryWriter(sw))
                wr.Write(buffer);
        }

        public void SetPixelRgb(int x, int y, int z, SliceType sliceType, int r, int g, int b)
        {
            var voxelIndex = GetVoxelIndex(x, y, z, sliceType);

            BitConverter.GetBytes(r << 16 | g << 8 | b)
                .Take(3).ToArray()
                .CopyTo(voxelsBytes, voxelIndex * 3);
        }

        public void ConvertHeaderToRgba()
        {
            Header.dim[0] = 5; // RGB and RGBA both have 5 dimensions
            Header.dim[4] = 1; // time
            Header.dim[5] = 4; // 4 channels for RGBA
            Header.bitpix = 32;
            Header.datatype = 2304;
            Header.intent_code = 2004;
        }
        public void ConvertHeaderToRgb()
        {
            Header.dim[0] = 5; // RGB and RGBA both have 5 dimensions
            Header.dim[4] = 1; // time
            Header.dim[5] = 3; // 3 channels for RGB
            Header.bitpix = 24;
            Header.datatype = 128;
            Header.intent_code = 2003;
        }

        /// <summary>
        /// Reorient voxels into new dimensions - settings dim property to new values
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="slices"></param>
        public void Reorient(short width, short height, short slices)
        {
            if (width * height * slices != voxels.Length)
                throw new Exception("number of voxels and new dimensions don't match. " +
                                    $"Width: {width} Height: {height} Slices: {slices} No. of voxels: {voxels.Length}");

            Header.dim[0] = 3;
            Header.dim[1] = width;
            Header.dim[2] = height;
            Header.dim[3] = slices;

            var oldVoxels = voxels;
            voxels = new float[voxels.Length];
            for (var z = 0; z < slices; z++)
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var op = z + (width - 1 - x) * slices + (height - 1 - y) * slices * width;
                        //var op = z + x * slices + y * slices * width;
                        //var op = z + x * slices + y * slices * width;
                        var np = x + y * width + z * width * height;
                        voxels[np] = oldVoxels[op];
                    }
        }

        /// <summary>
        /// Reads voxels from byte 352 for length of bytesLength parameter
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="bytesLength"></param>
        /// <returns>Voxels as an array of float</returns>
        private float[] ReadVoxels(BinaryReader reader, int bytesLength)
        {
            switch (Header.datatype)
            {
                case 1: // 1 bit
                    {
                        voxels = new float[bytesLength];
                        for (var i = 0; i < bytesLength; i++)
                            voxels[i] = reader.ReadBoolean() ? 1 : 0;
                        break;
                    }

                case 2: // 8 bits
                    {
                        voxels = new float[bytesLength];
                        for (var i = 0; i < bytesLength; i++)
                            voxels[i] = reader.ReadByte();
                        break;
                    }

                case 4: // 16 bits
                    {
                        voxels = new float[bytesLength / 2];
                        for (var i = 0; i < bytesLength / 2; i++)
                            voxels[i] = reader.ReadInt16();
                        break;
                    }

                case 8: // 32 bits
                case 2304: // RGBA  (32 bit)
                    {
                        voxels = new float[bytesLength / 4];
                        for (var i = 0; i < bytesLength / 4; i++)
                            voxels[i] = reader.ReadInt32();
                        break;
                    }

                case 16: // float
                    {
                        voxels = new float[bytesLength / 4];
                        for (var i = 0; i < bytesLength / 4; i++)
                            voxels[i] = Convert.ToInt32(reader.ReadSingle());
                        break;
                    }
                case 128: // RGB (24 bit)
                    {
                        voxels = new float[bytesLength / 3];
                        for (var i = 0; i < bytesLength / 3; i++)
                        {
                            var byte1 = reader.ReadByte();
                            var byte2 = reader.ReadByte();
                            var byte3 = reader.ReadByte();
                            voxels[i] = byte3 << 16 | byte2 << 8 | byte1;
                        }
                        break;
                    }

                default:
                    throw new ArgumentException($"Datatype [{Header.datatype}] not supported.");
            }
            return voxels;
        }
        /// <summary>
        /// Read sequence of bytes and convert to string
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="length"></param>
        /// <returns>string read from a sequence of bytes</returns>
        private static string ReadString(BinaryReader reader, int length)
        {
            var buffer = new byte[length];
            reader.Read(buffer, 0, length);
            var chars = Encoding.UTF8.GetChars(buffer);
            var str = new string(chars).Replace('\0', ' ').TrimEnd();
            return str;
        }
        private byte[] WriteNiftiHeaderBytes()
        {
            var bufferSize = GetTotalSize();
            var buffer = new byte[bufferSize];

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.sizeof_hdr)[i], i); // sizeof_hdr

            if (!string.IsNullOrEmpty(Header.dim_info)) buffer.SetValue(BitConverter.GetBytes(Header.dim_info[0])[0], 39); // dim_info
            for (var i = 0; i < 8; i++) // dim[8]
            {
                buffer.SetValue(BitConverter.GetBytes(Header.dim[i])[0], 40 + i * 2);
                buffer.SetValue(BitConverter.GetBytes(Header.dim[i])[1], 41 + i * 2);
            }

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.intent_p1)[i], 56 + i); // intent_p1
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.intent_p2)[i], 60 + i); // intent_p2
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.intent_p3)[i], 64 + i); // intent_p3
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.intent_code)[i], 68 + i); // intent_code
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.datatype)[i], 70 + i); // datatype
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.bitpix)[i], 72 + i); // bitpix
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.slice_start)[i], 74 + i); // slice_start

            for (var i = 0; i < 8; i++)
                for (var j = 0; j < 4; j++)
                    buffer.SetValue(BitConverter.GetBytes(Header.pix_dim[i])[j], 76 + i * 4 + j); // pixdim

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.vox_offset)[i], 108 + i); // vox_offset
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.scl_slope)[i], 112 + i); // scl_slope
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.scl_inter)[i], 116 + i); // scl_inter
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.slice_end)[i], 120 + i); // slice_end

            if (!string.IsNullOrEmpty(Header.slice_code)) buffer.SetValue(BitConverter.GetBytes(Convert.ToByte(Header.slice_code))[0], 122); // slice_code

            if (!string.IsNullOrEmpty(Header.xyzt_units)) buffer.SetValue(BitConverter.GetBytes(Convert.ToByte(Header.xyzt_units))[0], 123); // xyzt_units

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.cal_max)[i], 124 + i); // cal_max
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.cal_min)[i], 128 + i); // cal_min
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.slice_duration)[i], 132 + i); // slice_duration
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.toffset)[i], 136 + i); // toffset

            for (var i = 0; i < 4; i++) buffer.SetValue(Convert.ToByte(0), 140 + i); // glmax
            for (var i = 0; i < 4; i++) buffer.SetValue(Convert.ToByte(0), 144 + i); // glmin

            for (var i = 0; i < Header.descrip.Length; i++) buffer.SetValue(BitConverter.GetBytes(Header.descrip[i])[0], 148 + i); // description
            for (var i = 0; i < Header.aux_file.Length; i++) buffer.SetValue(BitConverter.GetBytes(Header.aux_file[i])[0], 228 + i); // aux_file

            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.qform_code)[i], 252 + i); // qform_code
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.sform_code)[i], 254 + i); // sform_code

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.quatern_b)[i], 256 + i); // quatern_b
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.quatern_c)[i], 260 + i); // quatern_c
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.quatern_d)[i], 264 + i); // quatern_d
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.qoffset_x)[i], 268 + i); // qoffset_x
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.qoffset_y)[i], 272 + i); // qoffset_y
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.qoffset_z)[i], 276 + i); // qoffset_z

            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++) buffer.SetValue(BitConverter.GetBytes(Header.srow_x[i])[j], 280 + i * 4 + j); // srow_x
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++) buffer.SetValue(BitConverter.GetBytes(Header.srow_y[i])[j], 296 + i * 4 + j); // srow_y
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++) buffer.SetValue(BitConverter.GetBytes(Header.srow_z[i])[j], 312 + i * 4 + j); // srow_z

            for (var i = 0; i < Header.intent_name.Length; i++) buffer.SetValue(BitConverter.GetBytes(Header.intent_name[i])[0], 328 + i); // intent_name
            for (var i = 0; i < Header.magic.Length; i++) buffer.SetValue(BitConverter.GetBytes(Header.magic[i])[0], 344 + i); // magic

            return buffer;
        }
        private int GetTotalSize()
        {
            if (voxelsBytes != null && voxelsBytes.Length > 0) return (int)Header.vox_offset + voxelsBytes.Length;
            if (voxels == null || voxels.Length == 0) throw new Exception("Both voxels and voxelsBytes are empty!");
            return (int)Header.vox_offset + voxels.Length * 8 * Header.bitpix;
        }
        private byte[] WriteVoxelsBytes(byte[] buffer)
        {
            if (voxelsBytes == null || voxelsBytes.Length == 0) // If voxelsBytes is empty
                GetVoxelsBytes();

            voxelsBytes.CopyTo(buffer, (int)Header.vox_offset);

            return buffer;
        }
        private void GetVoxelsBytes()
        {
            voxelsBytes = new byte[GetTotalSize()];
            var bytePix = Header.bitpix / 8;

            for (var i = 0; i < voxels.Length; i++)
                for (var j = 0; j < bytePix; j++)
                {
                    var position = Header.sizeof_hdr + i * bytePix + j;
                    switch (bytePix)
                    {
                        case 1 when Header.cal_min >= 0:
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToByte(voxels[i]))[j], position);
                            break;
                        case 1 when Header.cal_min < 0:
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToSByte(voxels[i]))[j], position);
                            break;
                        case 2:
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToInt16(voxels[i]))[j], position);
                            break;
                        case 3 when Header.cal_min >= 0:
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToUInt32(voxels[i]))[j + 1], position);
                            break;
                        case 4 when Header.cal_min >= 0:
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToUInt32(voxels[i]))[j], position);
                            break;
                        case 4 when Header.cal_min < 0:
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToInt32(voxels[i]))[j], position);
                            break;
                        default:
                            throw new Exception($"Bitpix {Header.bitpix} not supported!");
                    }
                }
        }
        private int GetVoxelIndex(int x, int y, int z, SliceType sliceType)
        {
            var ltRt = Header.dim[1];
            var antPos = Header.dim[2];
            var infSup = Header.dim[3];
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