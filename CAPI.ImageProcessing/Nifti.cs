using CAPI.ImageProcessing.Abstraction;
using System;
using System.IO;
using System.Text;

namespace CAPI.ImageProcessing
{
    /// <summary>
    /// Reads files of type Nifti-1 only if little endian
    /// </summary>
    public class Nifti : INifti
    {
        public int sizeof_hdr { get; set; }
        public string dim_info { get; set; }
        public short[] dim { get; set; }
        public float intent_p1 { get; set; }
        public float intent_p2 { get; set; }
        public float intent_p3 { get; set; }
        public short intent_code { get; set; }
        public short datatype { get; set; }
        public short bitpix { get; set; }
        public short slice_start { get; set; }
        public float[] pix_dim { get; set; }
        public float vox_offset { get; set; }
        public float scl_slope { get; set; }
        public float scl_inter { get; set; }
        public short slice_end { get; set; }
        public string slice_code { get; set; }
        public string xyzt_units { get; set; }
        public float cal_max { get; set; }
        public float cal_min { get; set; }
        public float slice_duration { get; set; }
        public float toffset { get; set; }
        public string descrip { get; set; }
        public string aux_file { get; set; }
        public short qform_code { get; set; }
        public short sform_code { get; set; }
        public float quatern_b { get; set; }
        public float quatern_c { get; set; }
        public float quatern_d { get; set; }
        public float qoffset_x { get; set; }
        public float qoffset_y { get; set; }
        public float qoffset_z { get; set; }
        public float[] srow_x { get; set; }
        public float[] srow_y { get; set; }
        public float[] srow_z { get; set; }
        public string intent_name { get; set; }
        public string magic { get; set; }
        public float[] voxels { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Nifti()
        {
            dim = new short[8];
            pix_dim = new float[8];
            srow_x = new float[4];
            srow_y = new float[4];
            srow_z = new float[4];
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

            sizeof_hdr = reader.ReadInt32();
            if (sizeof_hdr != 348)
                throw new Exception("Either not a nifti File or type not supported! siezof_hdr should be 348 for a nifti-1 file.");

            for (var i = 0; i < 35; i++) reader.ReadByte();

            dim_info = ReadString(reader, 1);
            for (var i = 0; i < 8; i++) dim[i] = reader.ReadInt16();

            intent_p1 = reader.ReadSingle();
            intent_p2 = reader.ReadSingle();
            intent_p3 = reader.ReadSingle();
            intent_code = reader.ReadInt16();

            datatype = reader.ReadInt16();
            bitpix = reader.ReadInt16();
            slice_start = reader.ReadInt16();

            for (var i = 0; i < 8; i++) pix_dim[i] = reader.ReadSingle();

            vox_offset = reader.ReadSingle();
            scl_slope = reader.ReadSingle();
            scl_inter = reader.ReadSingle();
            slice_end = reader.ReadInt16();
            slice_code = reader.ReadByte().ToString();
            xyzt_units = reader.ReadByte().ToString();
            cal_max = reader.ReadSingle();
            cal_min = reader.ReadSingle();
            slice_duration = reader.ReadSingle();
            toffset = reader.ReadSingle();
            reader.ReadInt32(); // glmax
            reader.ReadInt32(); // glmin

            descrip = ReadString(reader, 80);
            aux_file = ReadString(reader, 24);

            qform_code = reader.ReadInt16();
            sform_code = reader.ReadInt16();

            quatern_b = reader.ReadSingle();
            quatern_c = reader.ReadSingle();
            quatern_d = reader.ReadSingle();
            qoffset_x = reader.ReadSingle();
            qoffset_y = reader.ReadSingle();
            qoffset_z = reader.ReadSingle();

            for (var i = 0; i < 4; i++) srow_x[i] = reader.ReadSingle();
            for (var i = 0; i < 4; i++) srow_y[i] = reader.ReadSingle();
            for (var i = 0; i < 4; i++) srow_z[i] = reader.ReadSingle();

            intent_name = ReadString(reader, 16);
            magic = ReadString(reader, 4);

            for (var i = 0; i < 4; i++) reader.ReadByte(); // 4 bytes gap between header and voxels

            var bytesLength = Convert.ToInt32(reader.BaseStream.Length) - Convert.ToInt32(vox_offset);
            voxels = ReadVoxels(reader, Convert.ToInt32(bytesLength));

            if (reader.BaseStream.Position != reader.BaseStream.Length)
                throw new Exception("Not all bytes in the stream were read!");

        }

        public void WriteNifti(string filepath)
        {
            var bufferSize = sizeof_hdr + 4 + (voxels.Length * bitpix / 8);
            var buffer = new byte[bufferSize];

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(sizeof_hdr)[i], i); // sizeof_hdr

            if (!string.IsNullOrEmpty(dim_info)) buffer.SetValue(BitConverter.GetBytes(dim_info[0])[0], 39); // dim_info
            for (var i = 0; i < 8; i++) // dim[8]
            {
                buffer.SetValue(BitConverter.GetBytes(dim[i])[0], 40 + i * 2);
                buffer.SetValue(BitConverter.GetBytes(dim[i])[1], 41 + i * 2);
            }

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(intent_p1)[i], 56 + i); // intent_p1
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(intent_p2)[i], 60 + i); // intent_p2
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(intent_p3)[i], 64 + i); // intent_p3
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(intent_code)[i], 68 + i); // intent_code
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(datatype)[i], 70 + i); // datatype
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(bitpix)[i], 72 + i); // bitpix
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(slice_start)[i], 74 + i); // slice_start

            for (var i = 0; i < 8; i++)
                for (var j = 0; j < 4; j++)
                    buffer.SetValue(BitConverter.GetBytes(pix_dim[i])[j], 76 + i * 4 + j); // pixdim

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(vox_offset)[i], 108 + i); // vox_offset
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(scl_slope)[i], 112 + i); // scl_slope
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(scl_inter)[i], 116 + i); // scl_inter
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(slice_end)[i], 120 + i); // slice_end

            if (!string.IsNullOrEmpty(slice_code)) buffer.SetValue(BitConverter.GetBytes(Convert.ToByte(slice_code))[0], 122); // slice_code

            if (!string.IsNullOrEmpty(xyzt_units)) buffer.SetValue(BitConverter.GetBytes(Convert.ToByte(xyzt_units))[0], 123); // xyzt_units

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(cal_max)[i], 124 + i); // cal_max
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(cal_min)[i], 128 + i); // cal_min
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(slice_duration)[i], 132 + i); // slice_duration
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(toffset)[i], 136 + i); // toffset

            for (var i = 0; i < 4; i++) buffer.SetValue(Convert.ToByte(0), 140 + i); // glmax
            for (var i = 0; i < 4; i++) buffer.SetValue(Convert.ToByte(0), 144 + i); // glmin

            for (var i = 0; i < descrip.Length; i++) buffer.SetValue(BitConverter.GetBytes(descrip[i])[0], 148 + i); // description
            for (var i = 0; i < aux_file.Length; i++) buffer.SetValue(BitConverter.GetBytes(aux_file[i])[0], 228 + i); // aux_file

            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(qform_code)[i], 252 + i); // qform_code
            for (var i = 0; i < 2; i++) buffer.SetValue(BitConverter.GetBytes(sform_code)[i], 254 + i); // sform_code

            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(quatern_b)[i], 256 + i); // quatern_b
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(quatern_c)[i], 260 + i); // quatern_c
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(quatern_d)[i], 264 + i); // quatern_d
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(qoffset_x)[i], 268 + i); // qoffset_x
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(qoffset_y)[i], 272 + i); // qoffset_y
            for (var i = 0; i < 4; i++) buffer.SetValue(BitConverter.GetBytes(qoffset_z)[i], 276 + i); // qoffset_z

            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++) buffer.SetValue(BitConverter.GetBytes(srow_x[i])[j], 280 + i * 4 + j); // srow_x
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++) buffer.SetValue(BitConverter.GetBytes(srow_y[i])[j], 296 + i * 4 + j); // srow_y
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++) buffer.SetValue(BitConverter.GetBytes(srow_z[i])[j], 312 + i * 4 + j); // srow_z

            for (var i = 0; i < intent_name.Length; i++) buffer.SetValue(BitConverter.GetBytes(intent_name[i])[0], 328 + i); // intent_name
            for (var i = 0; i < magic.Length; i++) buffer.SetValue(BitConverter.GetBytes(magic[i])[0], 344 + i); // magic

            buffer = WriteVoxels(buffer);

            using (var sw = new FileStream(filepath, FileMode.Create))
            using (var wr = new BinaryWriter(sw))
                wr.Write(buffer);
        }

        private byte[] WriteVoxels(byte[] buffer)
        {
            var bytePix = bitpix / 8;
            for (var i = 0; i < voxels.Length; i++)
                for (var j = 0; j < bytePix; j++)
                {
                    switch (bytePix)
                    {
                        case 1 when cal_min >= 0:
                            buffer.SetValue(BitConverter.GetBytes(Convert.ToByte(voxels[i]))[j], 352 + i * bytePix + j);
                            break;
                        case 1 when cal_min < 0:
                            buffer.SetValue(BitConverter.GetBytes(Convert.ToSByte(voxels[i]))[j], 352 + i * bytePix + j);
                            break;
                        case 2:
                            buffer.SetValue(BitConverter.GetBytes(Convert.ToInt16(voxels[i]))[j], 352 + i * bytePix + j);
                            break;
                        default:
                            throw new Exception($"Bitpix {bitpix} not supported!");
                    }
                }

            return buffer;
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

            dim[0] = 3;
            dim[1] = width;
            dim[2] = height;
            dim[3] = slices;

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
            if (datatype == 1)
            {
                voxels = new float[bytesLength];
                for (var i = 0; i < bytesLength; i++)
                    voxels[i] = reader.ReadBoolean() ? 1 : 0;
            }
            else if (datatype == 2)
            {
                voxels = new float[bytesLength];
                for (var i = 0; i < bytesLength; i++)
                    voxels[i] = reader.ReadByte();
            }
            else if (datatype == 4)
            {
                voxels = new float[bytesLength / 2];
                for (var i = 0; i < bytesLength / 2; i++)
                    voxels[i] = reader.ReadInt16();
            }
            else if (datatype == 8)
            {
                voxels = new float[bytesLength / 4];
                for (var i = 0; i < bytesLength / 4; i++)
                    voxels[i] = reader.ReadInt32();
            }
            else if (datatype == 16)
            {
                voxels = new float[bytesLength / 4];
                for (var i = 0; i < bytesLength / 4; i++)
                    voxels[i] = reader.ReadSingle();
            }
            else throw new ArgumentException($"Datatype [{datatype}] not supported.");
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
    }
}