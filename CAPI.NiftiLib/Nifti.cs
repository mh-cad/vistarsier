using CAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace CAPI.NiftiLib
{
    /// <summary>
    /// Represents a Nifti-1 type file (reads from  file only if little endian)
    /// </summary>
    public class Nifti : INifti
    {
        public INiftiHeader Header { get; set; }
        public float[] Voxels { get; set; }
        public byte[] VoxelsBytes { get; set; }
        public Color[] ColorMap { get; set; }

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

            ColorMap = ColorMaps.GreyScale();
        }

        /// <summary>
        /// Read header and voxels from the nifti file of type nifti-1 (magic:n+1)
        /// </summary>
        /// <param name="filepath"></param>
        public INifti ReadNifti(string filepath)
        {
            using (var fileStream = new FileStream(filepath, FileMode.Open))
            {
                if (!fileStream.CanRead) throw new IOException($"Unable to access file: {filepath}");

                var isBigEndian = IsBigEndian(fileStream);
                var reader = new BinaryReaderBigAndLittleEndian(fileStream, isBigEndian);

                ReadNiftiHeader(reader);

                var bytesLength = Convert.ToInt32(reader.BaseStream.Length) - Convert.ToInt32(Header.vox_offset);
                Voxels = ReadVoxels(reader, bytesLength);

                if (reader.BaseStream.Position != reader.BaseStream.Length)
                    throw new Exception("Not all bytes in the stream were read!");
            }

            if (Header.cal_min == 0 && Header.cal_max == 0)
            {
                Header.cal_min = Voxels.Min();
                Header.cal_max = Voxels.Max();
            }

            return this;
        }

        private static bool IsBigEndian(Stream fileStream)
        {
            var reader = new BinaryReader(fileStream);
            var headerSize = reader.ReadInt32();
            if (headerSize != 348) // this means it might be big endian, but let's make sure it is
                                   // (we should be able to check if we're reading 1543569408 and skip the extra read, but this way is less brittle)
            {
                fileStream.Position = 0;
                var beReader = new BinaryReaderBigAndLittleEndian(fileStream, true);
                headerSize = beReader.ReadInt32();
                if (headerSize != 348) throw new Exception("Either not a nifti File or type not supported! sizeof_hdr should be 348 for a nifti-1 file.");
                fileStream.Position = 0;
                return true;
            }
            fileStream.Position = 0;
            return false;
        }

        /// <summary>
        /// Reads only the header of a nifti-1 file
        /// </summary>
        /// <param name="filepath"></param>
        public void ReadNiftiHeader(string filepath)
        {
            using (var fileStream = new FileStream(filepath, FileMode.Open))
            {
                if (!fileStream.CanRead) throw new IOException($"Unable to access file: {filepath}");

                var isBigEndian = IsBigEndian(fileStream);
                var reader = new BinaryReaderBigAndLittleEndian(fileStream, isBigEndian);

                ReadNiftiHeader(reader);
            }
        }

        private void ReadNiftiHeader(BinaryReader reader)
        {

            Header.sizeof_hdr = reader.ReadInt32();
            if (Header.sizeof_hdr != 348)
                throw new Exception("Either not a nifti File or type not supported! sizeof_hdr should be 348 for a nifti-1 file.");

            // TODO: Add a bit of explaination about header format.
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

            try
            {
                for (var i = 0; i < 4; i++) reader.ReadByte(); // 4 bytes gap between header and voxels
            }
            catch (EndOfStreamException)
            {
                // There will not be the 4 byte gap if we're just reading the .hdr, which is fine.
            }
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

        public IEnumerable<float[]> GetSlices(SliceType sliceType)
        {
            GetDimensions(sliceType, out var width, out var height, out var nSlices);
            var slices = new float[nSlices][];
            for (var z = 0; z < nSlices; z++)
            {
                slices[z] = new float[width * height];
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var pixelIndex = GetVoxelIndex(x, y, z, sliceType);
                        slices[z][x + y * width] = Voxels[pixelIndex];
                    }
            }

            return slices;
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

            return slice;
        }


        public int GetPixelColor(int x, int y, int z, SliceType sliceType)
        {
            var voxelIndex = GetVoxelIndex(x, y, z, sliceType);
            var voxelValue = (int)Voxels[voxelIndex];

            switch (Header.datatype)
            {
                case 16:
                case 2: // Standard intensity TODO: Make good. 
                case 4: // GrayScale 16bit
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

                case 128: // RGB 24bit
                case 2304: // ARGB 32bit
                    return voxelValue;
                default:
                    throw new NotImplementedException($"datatype {Header.datatype} not suported!");
            }
        }

        public float GetValue(int x, int y, int z, SliceType sliceType)
        {
            var voxelIndex = GetVoxelIndex(x, y, z, sliceType);
            return Voxels[voxelIndex];
        }

        public void GetDimensions(SliceType sliceType, out int width, out int height, out int nSlices)
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
                .CopyTo(VoxelsBytes, voxelIndex * 3);
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

        public void ConvertHeaderToGrayScale16Bit()
        {
            Header.dim[0] = 4; // 3 spatial and one temporal dimension
            Header.dim[4] = 1; // time
            Header.bitpix = 16;
            Header.datatype = 4;
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
        private float[] ReadVoxels(BinaryReader reader, int bytesLength)
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
        private byte[] WriteVoxelsBytes(byte[] buffer)
        {
            GetVoxelsBytes();
            VoxelsBytes.CopyTo(buffer, (int)Header.vox_offset);

            return buffer;
        }

        private byte[] WriteNiftiHeaderBytesBigEndian()
        {
            var bufferSize = GetTotalSize();
            var buffer = new byte[bufferSize];

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.sizeof_hdr).Reverse().ToArray()[i], i); // sizeof_hdr

            if (!string.IsNullOrEmpty(Header.dim_info)) buffer.SetValue(BitConverter.GetBytes(Header.dim_info[0])[0], 39); // dim_info
            for (var i = 0; i < 8; i++) // dim[8]
            {
                buffer.SetValue(BitConverter.GetBytes(Header.dim[i])[1], 40 + i * 2); // 0 and 1 indices swapped for big endian
                buffer.SetValue(BitConverter.GetBytes(Header.dim[i])[0], 41 + i * 2);
            }

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.intent_p1).Reverse().ToArray()[i], 56 + i); // intent_p1
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.intent_p2).Reverse().ToArray()[i], 60 + i); // intent_p2
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.intent_p3).Reverse().ToArray()[i], 64 + i); // intent_p3
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.intent_code).Reverse().ToArray()[i], 68 + i); // intent_code
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.datatype).Reverse().ToArray()[i], 70 + i); // datatype
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.bitpix).Reverse().ToArray()[i], 72 + i); // bitpix
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.slice_start).Reverse().ToArray()[i], 74 + i); // slice_start

            for (var i = 0; i < 8; i++)
                for (var j = 0; j < 4; j++)
                    buffer.SetValue(BitConverter.GetBytes(Header.pix_dim[i]).Reverse().ToArray()[j], 76 + i * 4 + j); // pixdim

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.vox_offset).Reverse().ToArray()[i], 108 + i); // vox_offset
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.scl_slope).Reverse().ToArray()[i], 112 + i); // scl_slope
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.scl_inter).Reverse().ToArray()[i], 116 + i); // scl_inter
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.slice_end).Reverse().ToArray()[i], 120 + i); // slice_end

            if (!string.IsNullOrEmpty(Header.slice_code)) buffer.SetValue(BitConverter.GetBytes(Convert.ToByte(Header.slice_code))[0], 122); // slice_code

            if (!string.IsNullOrEmpty(Header.xyzt_units)) buffer.SetValue(BitConverter.GetBytes(Convert.ToByte(Header.xyzt_units))[0], 123); // xyzt_units

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.cal_max).Reverse().ToArray()[i], 124 + i); // cal_max
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.cal_min).Reverse().ToArray()[i], 128 + i); // cal_min
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.slice_duration).Reverse().ToArray()[i], 132 + i); // slice_duration
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.toffset).Reverse().ToArray()[i], 136 + i); // toffset

            for (var i = 0; i < 4; i++) buffer.SetValue(Convert.ToByte(0), 140 + i); // glmax | not used in nifti
            for (var i = 0; i < 4; i++) buffer.SetValue(Convert.ToByte(0), 144 + i); // glmin | not used in nifti

            for (var i = 0; i < Header.descrip.Length; i++) buffer.SetValue(BitConverter.GetBytes(Header.descrip[i])[0], 148 + i); // description
            for (var i = 0; i < Header.aux_file.Length; i++) buffer.SetValue(BitConverter.GetBytes(Header.aux_file[i])[0], 228 + i); // aux_file

            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.qform_code).Reverse().ToArray()[i], 252 + i); // qform_code
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(Header.sform_code).Reverse().ToArray()[i], 254 + i); // sform_code

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.quatern_b).Reverse().ToArray()[i], 256 + i); // quatern_b
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.quatern_c).Reverse().ToArray()[i], 260 + i); // quatern_c
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.quatern_d).Reverse().ToArray()[i], 264 + i); // quatern_d
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.qoffset_x).Reverse().ToArray()[i], 268 + i); // qoffset_x
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.qoffset_y).Reverse().ToArray()[i], 272 + i); // qoffset_y
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(Header.qoffset_z).Reverse().ToArray()[i], 276 + i); // qoffset_z

            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++) buffer.SetValue(BitConverter.GetBytes(Header.srow_x[i]).Reverse().ToArray()[j], 280 + i * 4 + j); // srow_x
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++) buffer.SetValue(BitConverter.GetBytes(Header.srow_y[i]).Reverse().ToArray()[j], 296 + i * 4 + j); // srow_y
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++) buffer.SetValue(BitConverter.GetBytes(Header.srow_z[i]).Reverse().ToArray()[j], 312 + i * 4 + j); // srow_z

            for (var i = 0; i < Header.intent_name.Length; i++) buffer.SetValue(BitConverter.GetBytes(Header.intent_name[i])[0], 328 + i); // intent_name
            for (var i = 0; i < Header.magic.Length; i++) buffer.SetValue(BitConverter.GetBytes(Header.magic[i])[0], 344 + i); // magic

            return buffer;
        }

        private int GetTotalSize()
        {
            if (VoxelsBytes != null && VoxelsBytes.Length > 0) return (int)Header.vox_offset + VoxelsBytes.Length;
            if (Voxels == null || Voxels.Length == 0) throw new Exception("Both voxels and voxelsBytes are empty!");
            return (int)Header.vox_offset + Voxels.Length * Header.bitpix / 8;
        }
        private void GetVoxelsBytes()
        {
            VoxelsBytes = new byte[GetTotalSize() - (int)Header.vox_offset];
            var bytePix = Header.bitpix / 8;

            for (var i = 0; i < Voxels.Length; i++)
                for (var j = 0; j < bytePix; j++)
                {
                    var position = i * bytePix + j;
                    switch (bytePix)
                    {
                        case 1 when Header.cal_min >= 0:
                            VoxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToByte(Voxels[i]))[j], position);
                            break;
                        case 1 when Header.cal_min < 0:
                            VoxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToSByte(Voxels[i]))[j], position);
                            break;
                        case 2:
                            VoxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToInt16(Voxels[i])).ToArray()[j], position);
                            break;
                        case 3 when Header.cal_min >= 0:
                            // RGB is 24bit so needs three bytes for each pixel (bytePix=3) hence .Take(3)
                            VoxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToUInt32(Voxels[i])).Take(3).ToArray()[j], position);
                            break;
                        case 4 when Header.cal_min >= 0 && (Header.datatype == 8 || Header.datatype == 768): // signed (8) or unsigned (768) int
                            VoxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToUInt32(Voxels[i])).ToArray()[j], position);
                            break;
                        case 4 when Header.cal_min < 0 && (Header.datatype == 8 || Header.datatype == 768): // signed (8) or unsigned (768) int
                            VoxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToInt32(Voxels[i])).ToArray()[j], position);
                            break;
                        case 4 when Header.datatype == 16: // float
                            VoxelsBytes.SetValue(BitConverter.GetBytes(Voxels[i]).ToArray()[j], position);
                            break;
                        case 4 when Header.datatype == 2304:
                            // This should be RGBA, TODO: Make sure this makes sense (I think it does, but will need a test if it's being used).
                            VoxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToInt32(Voxels[i])).ToArray()[j], position); 
                            break;
                        default:
                            throw new Exception($"Bitpix {Header.bitpix} not supported!");
                    }
                }
        }
        private int GetVoxelIndex(int x, int y, int z, SliceType sliceType)
        {
            if (Header.dim[1] == 0 || Header.dim[2] == 0 || Header.dim[3] == 0)
                throw new Exception("Nifti header dimensions not set!");
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


        public INifti DeepCopy()
        {
            Nifti copy = new Nifti
            {
                Header = Header.DeepCopy(),
                Voxels = new float[Voxels.Length]
            };

            Voxels.CopyTo(copy.Voxels, 0);

            if (VoxelsBytes != null)
            {
                copy.VoxelsBytes = new byte[VoxelsBytes.Length];
                VoxelsBytes.CopyTo(copy.VoxelsBytes, 0);
            }

            copy.ColorMap = new Color[ColorMap.Length];
            ColorMap.CopyTo(copy.ColorMap, 0);

            return copy;
        }

        public INifti AddOverlay(INifti overlay)
        {
            Nifti output = (Nifti)(this.DeepCopy());

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

        public void RecalcHeaderMinMax()
        {
            Header.cal_max = Voxels.Max();
            Header.cal_min = Voxels.Min();
        }
    }
}