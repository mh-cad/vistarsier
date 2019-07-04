using VisTarsier.Extensions;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using VisTarsier.Config;

namespace VisTarsier.NiftiLib
{
    public abstract class NiftiBase<T> : INifti<T>
    {
        public INiftiHeader Header { get; set; }
        public T[] Voxels { get; set; }
        public Color[] ColorMap { get; set; }


        public abstract INifti<T> DeepCopy();
        public abstract T[] ReadVoxels(BinaryReader reader, int bytesLength);
        public abstract INifti<T> AddOverlay(INifti<T> overlay);
        public abstract byte[] GetVoxelsBytes();
        public abstract int GetVoxelIndex(int x, int y, int z, SliceType sliceType);


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

        public T GetValue(int x, int y, int z, SliceType sliceType)
        {
            var voxelIndex = GetVoxelIndex(x, y, z, sliceType);
            return Voxels[voxelIndex];
        }

        public INifti<T> ReadNifti(string filepath)
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

            return this;
        }

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

        public abstract void RecalcHeaderMinMax();

        public void WriteNifti(string filepath)
        {
            var buffer = WriteNiftiHeaderBytes();
            buffer = WriteVoxelsBytes(buffer);

            using (var sw = new FileStream(filepath, FileMode.Create))
            using (var wr = new BinaryWriter(sw))
                wr.Write(buffer);
        }
        private byte[] WriteVoxelsBytes(byte[] buffer)
        {
            GetVoxelsBytes().CopyTo(buffer, (int)Header.vox_offset);

            return buffer;
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

        public int GetTotalSize()
        {
            if (Voxels == null || Voxels.Length == 0) throw new Exception("Both voxels and voxelsBytes are empty!");
            return (int)Header.vox_offset + Voxels.Length * Header.bitpix / 8;
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
    }
}
