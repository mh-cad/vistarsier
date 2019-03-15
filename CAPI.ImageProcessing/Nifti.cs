using CAPI.Extensions;
using CAPI.ImageProcessing.Abstraction;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        public INifti ReadNifti(string filepath)
        {
            using (var fileStream = new FileStream(filepath, FileMode.Open))
            {
                if (!fileStream.CanRead) throw new IOException($"Unable to access file: {filepath}");

                var isBigEndian = IsBigEndian(fileStream);
                var reader = new BinaryReaderBigAndLittleEndian(fileStream, isBigEndian);

                ReadNiftiHeader(reader);

                var bytesLength = Convert.ToInt32(reader.BaseStream.Length) - Convert.ToInt32(Header.vox_offset);
                voxels = ReadVoxels(reader, bytesLength);

                if (reader.BaseStream.Position != reader.BaseStream.Length)
                    throw new Exception("Not all bytes in the stream were read!");
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
            catch(EndOfStreamException)
            {
                // There will not be the 4 byte gap if we're just reading the .hdr, which is fine.
            }
        }
        public INiftiHeader ReadHeaderFromFile(string filepath)
        {
            ReadNiftiHeader(filepath);
            return Header;
        }

        public void ReadVoxelsFromRgb256Bmps(string[] filepaths, SliceType sliceType)
        {
            ConvertHeaderToRgb();
            var w = new Bitmap(filepaths[0]).Width;
            var h = new Bitmap(filepaths[0]).Height;

            SetDimensions((short)filepaths.Length, (short)w, (short)h, sliceType);

            voxels = new float[filepaths.Length * w * h];

            for (var z = 0; z < filepaths.Length; z++)
            {
                var bitmap = new Bitmap(filepaths[z]);
                var sliceVoxels = GetVoxelsFromBitmap(bitmap);
                for (var j = 0; j < sliceVoxels.Length; j++)
                {
                    var index = GetVoxelIndex(j % w, j / w, z, sliceType);
                    voxels[index] = sliceVoxels[j];
                }
            }
        }

        private void SetDimensions(short numberOfFiles, short width, short height, SliceType sliceType)
        {
            switch (sliceType)
            {
                case SliceType.Sagittal:
                    Header.dim[1] = numberOfFiles;
                    Header.dim[2] = width;
                    Header.dim[3] = height;
                    break;
                case SliceType.Coronal:
                    Header.dim[1] = width;
                    Header.dim[2] = numberOfFiles;
                    Header.dim[3] = height;
                    break;
                case SliceType.Axial:
                    Header.dim[1] = width;
                    Header.dim[2] = height;
                    Header.dim[3] = numberOfFiles;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sliceType), sliceType, null);
            }
        }

        private static float[] GetVoxelsFromBitmap(Bitmap bitmap)
        {
            var w = bitmap.Width;
            var h = bitmap.Height;
            var sliceVoxlels = new float[w * h];

            for (var y = 0; y < bitmap.Height; y++)
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    sliceVoxlels[x + y * w] = pixel.B << 16 | pixel.G << 8 | pixel.R; // In bmp files order of colors are BGR
                }

            return sliceVoxlels;
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

        public INifti Compare(INifti current, INifti prior, SliceType sliceType,
                              ISubtractionLookUpTable lookUpTable, string workingDir,
                              INifti currentResliced = null, INifti mask = null)
        {
            mask?.InvertMask();
            currentResliced = currentResliced?.NormalizeNonBrainComponents(currentResliced,
                                  lookUpTable.Width / 2, lookUpTable.Width / 8, mask, 0, lookUpTable.Width - 1);
            mask?.InvertMask();

            var outNifti = currentResliced ?? current;

            EnsureNormalization(current, prior, lookUpTable);
            if (currentResliced != null)
                EnsureNormalization(currentResliced, prior, lookUpTable);


            for (var i = 0; i < current.voxels.Length; i++)
                if (currentResliced != null && mask != null)
                {
                    if (mask.voxels[i] > 0) // brain
                        outNifti.voxels[i] = lookUpTable.Pixels[(int)current.voxels[i],
                            (int)prior.voxels[i]].ToArgb().ToBgr();
                    else if (mask.voxels[i] < 1) // non-brain
                        outNifti.voxels[i] = Color.FromArgb((int)outNifti.voxels[i], (int)outNifti.voxels[i],
                            (int)outNifti.voxels[i]).ToArgb().ToBgr();
                }
                else
                    outNifti.voxels[i] = lookUpTable.Pixels[(int)current.voxels[i],
                        (int)prior.voxels[i]].ToArgb().ToBgr();

            outNifti.Header.cal_min = outNifti.voxels.Min();
            outNifti.Header.cal_max = outNifti.voxels.Max();

            outNifti.ConvertHeaderToRgb();

            return outNifti;
        }

        private static void EnsureNormalization(INifti current, INifti prior, ISubtractionLookUpTable lookUpTable)
        {
            var currentMinVal = current.voxels.Min();
            var currentMaxVal = current.voxels.Max();
            var priorMinVal = prior.voxels.Min();
            var priorMaxVal = prior.voxels.Max();

            if (currentMinVal < 0
                || currentMaxVal > lookUpTable.Width)
                throw new ArgumentOutOfRangeException($"Current nifti file voxels value range does not match with lookup table:{Environment.NewLine}" +
                                                      $"current min={currentMinVal}, current max={currentMaxVal}{Environment.NewLine}" +
                                                      $"lookup Table X min={lookUpTable.Xmin}, lookup Table X max={lookUpTable.Xmax}");

            if (priorMinVal < 0
                || priorMaxVal > lookUpTable.Height)
                throw new ArgumentOutOfRangeException($"Prior nifti file voxels value range does not match with lookup table:{Environment.NewLine}" +
                                                      $"prior min={priorMinVal}, prior max={priorMaxVal}{Environment.NewLine}" +
                                                      $"lookup Table Y min={lookUpTable.Ymin}, lookup Table Y max={lookUpTable.Ymax}");
        }

        public Bitmap GenerateLookupTable(Bitmap currentSlice, Bitmap priorSlice, Bitmap compareResult, Bitmap baseLut = null)
        {
            CheckIfDimensionsMatch(currentSlice, priorSlice, compareResult);

            var lut = baseLut ?? GetBlankLookupTable();
            for (var y = 0; y < currentSlice.Height; y++)
                for (var x = 0; x < currentSlice.Width; x++)
                {
                    var lutX = currentSlice.GetPixel(x, y).R;
                    var lutY = priorSlice.GetPixel(x, y).R;
                    var lutColor = compareResult.GetPixel(x, y);

                    if (IsColor(lut.GetPixel(lutX, lutY), 5)) continue;

                    if (IsColor(lutColor, 5))
                        lut.SetPixel(lutX, lutY, lutColor);
                }

            return lut;
        }

        public void InvertMask()
        {
            var median = (voxels.Max() - voxels.Min()) / 2 + voxels.Min();
            for (var i = 0; i < voxels.Length; i++)
                voxels[i] = 2 * median - voxels[i];
        }

        private static bool IsColor(Color color, int rgbValueGap)
        {
            return Math.Abs(color.R - color.G) > rgbValueGap ||
                   Math.Abs(color.R - color.B) > rgbValueGap ||
                   Math.Abs(color.G - color.B) > rgbValueGap;
        }

        private static Bitmap GetBlankLookupTable()
        {
            var lut = new Bitmap(256, 256);
            for (var y = 0; y < 256; y++)
                for (var x = 0; x < 256; x++)
                    lut.SetPixel(x, y, Color.FromArgb(255, x, x, x));
            return lut;
        }

        private static void CheckIfDimensionsMatch(Image currentSlice, Image priorSlice, Image compareResult)
        {
            if (currentSlice.Width != priorSlice.Width || currentSlice.Width != compareResult.Width)
                throw new ArgumentException(
                    "Width of current, prior or result slices do not match. " +
                    $"Current:[{currentSlice.Width}] Prior:[{priorSlice.Width}] Comparison:[{compareResult.Width}]");

            if (currentSlice.Height != priorSlice.Height || currentSlice.Height != compareResult.Height)
                throw new ArgumentException(
                    "Height of current, prior or result slices do not match. " +
                    $"Current:[{currentSlice.Height}] Prior:[{priorSlice.Height}] Comparison:[{compareResult.Height}]");
        }

        public INifti NormalizeEachSlice(INifti nifti, SliceType sliceType,
                                                        int mean, int std, int rangeWidth, INifti mask)
        {
            var slices = nifti.GetSlices(sliceType).ToArray();
            if (slices == null) throw new Exception("No slices found in file being compared");
            var maskArray = GetArrayFromMask(mask, sliceType);

            for (var i = 0; i < slices.Length; i++)
            {
                if (nifti.Header.datatype == 128) slices[i].RgbValToGrayscale();
                //slices[i].Normalize(mean, std, 10, (int)slices[i].Max()); // Normalize each slice
                //slices[i].Normalize(mean, std); // Normalize each slice
                slices[i].Normalize(mean, std, maskArray[i]); // Normalize each slice with mask
            }

            nifti.voxels = nifti.SlicesToArray(slices, sliceType); // Return back from slices to a single array
            nifti.voxels.Trim(0, rangeWidth - 1);

            return nifti;
        }

        public INifti NormalizeNonBrainComponents(INifti nim, int targetMean, int targetStdDev, INifti mask, int start, int end)
        {
            var nonBrainNonBlackMask = new bool[nim.voxels.Length];
            for (var i = 0; i < nim.voxels.Length; i++) // Create mask for non-brain non-black voxels
                if (mask.voxels[i] < 1 && nim.voxels[i] > 0) nonBrainNonBlackMask[i] = true;

            nim.voxels.Normalize(targetMean, targetStdDev, nonBrainNonBlackMask);

            nim.voxels.Trim(start, end);

            nim.Header.cal_max = nim.voxels.Max();
            nim.Header.cal_min = nim.voxels.Min();

            return nim;
        }

        private static bool[][] GetArrayFromMask(INifti mask, SliceType sliceType)
        {
            var slices = mask.GetSlices(sliceType).ToArray();
            var arr = new bool[slices.Length][];
            for (var i = 0; i < slices.Length; i++)
                arr[i] = slices[i].Select(v => v > 0).ToArray();
            return arr;
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
                        slices[z][x + y * width] = voxels[pixelIndex];
                    }
            }

            return slices;
        }
        public float[] SlicesToArray(float[][] slices, SliceType sliceType)
        {
            var allVoxels = new float[slices.Length * slices[0].Length];

            GetDimensions(sliceType, out var width, out var height, out _);
            //var width = slices[0].;

            for (var z = 0; z < slices.Length; z++)
            {
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                        allVoxels[GetVoxelIndex(x, y, z, sliceType)]
                            = slices[z][x + y * width];
            }

            return allVoxels;
        }

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
                    var color = Color.FromArgb(GetPixelColor(x, y, sliceIndex, sliceType)).SwapRedBlue();
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
                    if (voxelValue < Header.cal_min) voxelValue = (int)Header.cal_min;
                    if (voxelValue > Header.cal_max) voxelValue = (int)Header.cal_max;
                    var val = (int)((voxelValue - Header.cal_min) * 255 / (Header.cal_max - Header.cal_min));
                    return Color.FromArgb(val, val, val).ToArgb();
                case 128: // RGB 24bit
                case 2304: // ARGB 32bit
                    return voxelValue;
                default:
                    throw new NotImplementedException($"datatype {Header.datatype} not suported!");
            }
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
            if (width * height * slices != voxels.Length)
                throw new Exception("number of voxels and new dimensions don't match. " +
                                    $"Width: {width} Height: {height} Slices: {slices} No. of voxels: {voxels.Length}");

            Header.dim[0] = 3;
            Header.dim[1] = width;
            Header.dim[2] = height;
            Header.dim[3] = slices;

            //Header.dim[1] = slices;
            //Header.dim[2] = width;
            //Header.dim[3] = height;

            var oldVoxels = voxels;
            voxels = new float[voxels.Length];
            for (var z = 0; z < slices; z++)
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var op = z + (width - 1 - x) * slices + (height - 1 - y) * slices * width;
                        var np = x + y * width + z * width * height;
                        voxels[np] = oldVoxels[op];
                    }
        }

        public void ReorderVoxelsLpi2Asr()
        {
            Header.dim[0] = 3;
            var d1 = Header.dim[1];
            var d2 = Header.dim[2];
            var d3 = Header.dim[3];

            if (d2 * d3 * d1 != voxels.Length)
                throw new Exception("number of voxels and new dimensions don't match. " +
                                    $"[Assuming Sagittal] Width: {d2} Height: {d3} Slices: {d1} No. of voxels: {voxels.Length}");

            var oldVoxels = voxels;
            voxels = new float[voxels.Length];
            for (var z = 0; z < d3; z++)
                for (var y = 0; y < d2; y++)
                    for (var x = 0; x < d1; x++)
                    {
                        var currentPixelIndex = x + y * d1 + z * d1 * d2;
                        var newI1 = d2 - 1 - y;
                        var newI2 = d3 - 1 - z;
                        var newI3 = d1 - 1 - x;
                        var newPixelIndex = newI1 + newI2 * d2 + newI3 * d2 * d3;

                        voxels[newPixelIndex] = oldVoxels[currentPixelIndex];
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

            if (d2 * d3 * d1 != voxels.Length)
                throw new Exception("number of voxels and new dimensions don't match. " +
                                    $"dim1: {d1} / dim2: {d2} / dim3: {d3} | No. of voxels: {voxels.Length}");

            var oldVoxels = voxels;
            voxels = new float[voxels.Length];
            for (var z = 0; z < d3; z++)
                for (var y = 0; y < d2; y++)
                    for (var x = 0; x < d1; x++)
                    {
                        var currentPixelIndex = x + y * d1 + z * d1 * d2;
                        var newI1 = d2 - 1 - y;
                        var newI2 = z;
                        var newI3 = x;
                        var newPixelIndex = newI1 + newI2 * d2 + newI3 * d2 * d3;

                        voxels[newPixelIndex] = oldVoxels[currentPixelIndex];
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
                            voxels[i] = reader.ReadSingle();
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
                    throw new ArgumentException($"Nifti datatype [{Header.datatype}] not supported.");
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
        private byte[] WriteVoxelsBytes(byte[] buffer)
        {
            if (voxelsBytes == null || voxelsBytes.Length == 0) // If voxelsBytes is empty
                GetVoxelsBytes();

            voxelsBytes.CopyTo(buffer, (int)Header.vox_offset);

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
            if (voxelsBytes != null && voxelsBytes.Length > 0) return (int)Header.vox_offset + voxelsBytes.Length;
            if (voxels == null || voxels.Length == 0) throw new Exception("Both voxels and voxelsBytes are empty!");
            return (int)Header.vox_offset + voxels.Length * Header.bitpix / 8;
        }
        private void GetVoxelsBytes()
        {
            voxelsBytes = new byte[GetTotalSize() - (int)Header.vox_offset];
            var bytePix = Header.bitpix / 8;

            for (var i = 0; i < voxels.Length; i++)
                for (var j = 0; j < bytePix; j++)
                {
                    var position = i * bytePix + j;
                    switch (bytePix)
                    {
                        case 1 when Header.cal_min >= 0:
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToByte(voxels[i]))[j], position);
                            break;
                        case 1 when Header.cal_min < 0:
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToSByte(voxels[i]))[j], position);
                            break;
                        case 2:
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToInt16(voxels[i])).ToArray()[j], position);
                            break;
                        case 3 when Header.cal_min >= 0:
                            // RGB is 24bit so needs three bytes for each pixel (bytePix=3) hence .Take(3)
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToUInt32(voxels[i])).Take(3).ToArray()[j], position);
                            break;
                        case 4 when Header.cal_min >= 0 && (Header.datatype == 8 || Header.datatype == 768): // signed (8) or unsigned (768) int
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToUInt32(voxels[i])).ToArray()[j], position);
                            break;
                        case 4 when Header.cal_min < 0 && (Header.datatype == 8 || Header.datatype == 768): // signed (8) or unsigned (768) int
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToInt32(voxels[i])).ToArray()[j], position);
                            break;
                        case 4 when Header.datatype == 16: // float
                            voxelsBytes.SetValue(BitConverter.GetBytes(voxels[i]).ToArray()[j], position);
                            break;
                        case 4 when Header.datatype == 2304:
                            // This should be RGBA, TODO: Make sure this makes sense (I think it does, but will need a test if it's being used).
                            voxelsBytes.SetValue(BitConverter.GetBytes(Convert.ToInt32(voxels[i])).ToArray()[j], position); 
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
    }
}