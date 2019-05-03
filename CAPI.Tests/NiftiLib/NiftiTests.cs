
using CAPI.Config;
using CAPI.NiftiLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace CAPI.Tests.NiftiLib
{
    [TestClass]
    public class NiftiTests
    {
        string _minimalNiftiPath;
        string _minimalNiftiHdrPath;
        string _outfile;
        string _rgbafile;
        string _rgbfile;

        [TestInitialize]
        public void TestInit()
        {
            string testResourcesPath = Helper.GetTestResourcesPath() + "/nifti";
            _minimalNiftiPath = $@"{testResourcesPath}/minimal.nii";
            _minimalNiftiHdrPath = $@"{testResourcesPath}/minimal.hdr";
            _outfile = $@"{testResourcesPath}/out.nii";
            _rgbafile = $@"{testResourcesPath}/rgba-test.nii";
            _rgbfile = $@"{testResourcesPath}/rgb-test.nii";
        }

        [TestMethod]
        public void ReadNifti()
        {
            // Read the minimal nifti file.
            var nifti = new Nifti();
            nifti.ReadNifti(_minimalNiftiPath);

            // Check that the dimensions are correct.
            nifti.GetDimensions(SliceType.Axial, out var width, out var height, out var nSlices);
            Assert.AreEqual(height, 64);
            Assert.AreEqual(width, 64);
            Assert.AreEqual(nSlices, 10);
        }

        [TestMethod]
        public void Reorient()
        {
            var nifti = new Nifti();

            nifti.ReadNifti(_minimalNiftiPath);
            nifti.Reorient(nifti.Header.dim[2], nifti.Header.dim[3], nifti.Header.dim[1]);
            nifti.GetDimensions(SliceType.Axial, out var width, out var height, out var nSlices);

            // TODO: Check that this is the expected result.
            Assert.AreEqual(height, 10);
            Assert.AreEqual(width, 64);
            Assert.AreEqual(nSlices, 64);
        }

        [TestMethod]
        public void SlicesAxialToArray()
        {
            var nim = new Nifti();
            nim.voxels = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
            nim.Header.dim = new short[] { 3, 2, 3, 4 };
            var slices = nim.GetSlices(SliceType.Axial).ToArray();

            var arr = nim.SlicesToArray(slices, SliceType.Axial);

            for (var i = 0; i < arr.Length; i++) Assert.AreEqual(arr[i], nim.voxels[i]);
        }
        [TestMethod]
        public void SlicesCoronalToArray()
        {
            var nim = new Nifti();
            nim.voxels = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
            nim.Header.dim = new short[] { 3, 2, 3, 4 };
            var slices = nim.GetSlices(SliceType.Coronal).ToArray();

            var arr = nim.SlicesToArray(slices, SliceType.Coronal);

            for (var i = 0; i < arr.Length; i++) Assert.AreEqual(arr[i], nim.voxels[i]);
        }

        [TestMethod]
        public void SlicesSagittalToArray()
        {
            var nim = new Nifti();
            nim.voxels = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
            nim.Header.dim = new short[] { 3, 2, 3, 4 };
            var slices = nim.GetSlices(SliceType.Sagittal).ToArray();

            var arr = nim.SlicesToArray(slices, SliceType.Sagittal);

            for (var i = 0; i < arr.Length; i++)
                Assert.AreEqual(arr[i], nim.voxels[i]);
        }

        [TestMethod]
        public void Write() // TODO: This is currently failing (I'm assuming we're not releasing the lock on the file when we write)
        {
            // Read our minimal Nifti file
            var nifti = new Nifti();
            nifti.ReadNifti(_minimalNiftiPath);

            // Write the minimal Nifti file.
            nifti.WriteNifti(_outfile);
            // Check that we've written something.
            Assert.IsTrue(File.Exists(_outfile), "Nifti file does not exist");

            // Read our nifti file back in.
            var nifti2 = new Nifti();
            nifti2.ReadNifti(_outfile);
            // Delete the old file.
            File.Delete(_outfile);
            Assert.IsFalse(File.Exists(_outfile), "Nifti file could not be deleted.");

            // Check that the dimensions match the expected Nifti file.
            nifti.GetDimensions(SliceType.Axial, out var width, out var height, out var nSlices);
            Assert.AreEqual(height, 64);
            Assert.AreEqual(width, 64);
            Assert.AreEqual(nSlices, 10);
        }

        [TestMethod]
        public void WriteRgba()
        // TODO: This is currently unsupported (technically RGBA doesn't seem to be part
        // of the NifTI 1.1 format, so it will throw an error on the datatype
        {
            var nifti = new Nifti();
            nifti.ReadNifti(_minimalNiftiPath);
            nifti.ConvertHeaderToRgba();

            // Read pixdim from sample file

            // set dimensions for new file
            nifti.Header.dim[1] = 100;
            nifti.Header.dim[2] = 80;
            nifti.Header.dim[3] = 50;

            nifti.Header.cal_min = 0;
            nifti.Header.cal_max = uint.MaxValue;

            // write voxels
            var voxelsSize = nifti.Header.dim[1] * nifti.Header.dim[2] * nifti.Header.dim[3];
            nifti.voxels = new float[voxelsSize];
            for (var i = 0; i < voxelsSize; i++)
                nifti.voxels[i] = 255;

            if (File.Exists(_rgbafile)) File.Delete(_rgbafile);
            nifti.WriteNifti(_rgbafile);
            Assert.IsTrue(File.Exists(_rgbafile)); //TODO more meaningful asserts.
            File.Delete(_rgbafile);
        }

        [TestMethod]
        public void WriteRgb()
        {
            if (File.Exists(_rgbfile)) File.Delete(_rgbfile);
            var nifti = new Nifti();
            nifti.ReadNifti(_minimalNiftiPath);

            // Read pixdim from sample file
            var header = nifti.ReadHeaderFromFile(_minimalNiftiHdrPath);
            nifti.Header = header;

            // set dimensions for new file
            nifti.ConvertHeaderToRgb();

            nifti.Header.dim[1] = 160;
            nifti.Header.dim[2] = 256;
            nifti.Header.dim[3] = 256;

            nifti.Header.cal_min = 0;
            nifti.Header.cal_max = uint.MaxValue * (3 / 4);

            // write voxels
            var voxelsSize = nifti.Header.dim[1] * nifti.Header.dim[2] * nifti.Header.dim[3];
            var bytepix = nifti.Header.bitpix / 8;
            nifti.voxelsBytes = new byte[voxelsSize * bytepix];

            const int r = 255;
            const int g = 51;
            const int b = 204;

            for (var x = 0; x < nifti.Header.dim[1]; x++)
                for (var y = 0; y < nifti.Header.dim[2]; y++)
                    for (var z = 0; z < nifti.Header.dim[3]; z++)
                        if ((x - 128) * (x - 128) + (y - 128) * (y - 128) + (z) * (z) < 60 * 60) // Sphere
                            nifti.SetPixelRgb(x, y, z, SliceType.Axial, r, g, b);

            if (File.Exists(_rgbfile)) File.Delete(_rgbfile);
            nifti.WriteNifti(_rgbfile);

            Assert.IsTrue(File.Exists(_rgbfile)); //TODO: More meaningful asserts
            File.Delete(_rgbfile);
        }

        [TestMethod]
        public void DeepCopy()
        {
            var nifti = new Nifti().ReadNifti(_minimalNiftiPath);

            var niftiB = nifti.DeepCopy();
            // Check voxel copy
            Assert.IsTrue(nifti.voxels[0] == niftiB.voxels[0]);
            // Check header copy
            Assert.IsTrue(nifti.Header.intent_code == niftiB.Header.intent_code);
            // Check that we're not just doing a shallow copy
            niftiB.voxels[0] = nifti.voxels[0] + 1;
            niftiB.Header.intent_code = (short)(nifti.Header.intent_code + 1);
            // Now they should be different...
            Assert.IsFalse(nifti.voxels[0] == niftiB.voxels[0]);
            Assert.IsFalse(nifti.Header.intent_code == niftiB.Header.intent_code);
        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }
    }
}
