using System;
using System.IO;
using System.Text;

namespace VisTarsier.Extensions
{
    public class BinaryReaderBigAndLittleEndian : BinaryReader
    {
        private readonly bool _bigEndian;

        public BinaryReaderBigAndLittleEndian(Stream input, bool bigEndian = false) : base(input)
        {
            _bigEndian = bigEndian;
        }

        public BinaryReaderBigAndLittleEndian(Stream input) : base(input)
        {
        }

        public BinaryReaderBigAndLittleEndian(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public BinaryReaderBigAndLittleEndian(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public override int ReadInt32()
        {
            var data = base.ReadBytes(4);
            if (_bigEndian) Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public override float ReadSingle()
        {
            var data = base.ReadBytes(4);
            if (_bigEndian) Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }

        public override short ReadInt16()
        {
            var data = base.ReadBytes(2);
            if (_bigEndian) Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public override long ReadInt64()
        {
            var data = base.ReadBytes(8);
            if (_bigEndian) Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }
    }
}
