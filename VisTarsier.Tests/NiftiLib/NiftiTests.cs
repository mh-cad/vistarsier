
using VisTarsier.Config;
using VisTarsier.NiftiLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace VisTarsier.Tests.NiftiLib
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
            string testResourcesPath = Path.Combine(Helper.GetTestResourcesPath(), "nifti");
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
            var nifti = new NiftiFloat32();
            nifti.ReadNifti(_minimalNiftiPath);

            // Check that the dimensions are correct.
            nifti.GetDimensions(SliceType.Axial, out var width, out var height, out var nSlices);
            Assert.AreEqual(height, 64);
            Assert.AreEqual(width, 64);
            Assert.AreEqual(nSlices, 10);

            //// Read the minimal nifti file.
            //var nifti2 = new NiftiRgb48();
            //nifti2.ReadNifti(_minimalNiftiPath);

            //// Check that the dimensions are correct.
            //nifti2.GetDimensions(SliceType.Axial, out var width2, out var height2, out var nSlices2);
            //Assert.AreEqual(height2, 64);
            //Assert.AreEqual(width2, 64);
            //Assert.AreEqual(nSlices2, 10);
            //Assert.IsFalse(nifti2.Voxels[10] == 0);
        }

        [TestMethod]
        public void Reorient()
        {
            var nifti = new NiftiFloat32();

            nifti.ReadNifti(_minimalNiftiPath);
            nifti.Reorient(nifti.Header.dim[2], nifti.Header.dim[3], nifti.Header.dim[1]);
            nifti.GetDimensions(SliceType.Axial, out var width, out var height, out var nSlices);

            // TODO: Check that this is the expected result.
            Assert.AreEqual(height, 10);
            Assert.AreEqual(width, 64);
            Assert.AreEqual(nSlices, 64);
        }

        [TestMethod]
        public void Write()
        {
            // Read our minimal Nifti file
            var nifti = new NiftiFloat32();
            nifti.ReadNifti(_minimalNiftiPath);

            // Write the minimal Nifti file.
            nifti.WriteNifti(_outfile);
            // Check that we've written something.
            Assert.IsTrue(File.Exists(_outfile), "Nifti file does not exist");

            // Read our nifti file back in.
            var nifti2 = new NiftiFloat32();
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
            var nifti = new NiftiFloat32();
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
            nifti.Voxels = new float[voxelsSize];
            for (var i = 0; i < voxelsSize; i++)
                nifti.Voxels[i] = 255;

            if (File.Exists(_rgbafile)) File.Delete(_rgbafile);
            nifti.WriteNifti(_rgbafile);
            Assert.IsTrue(File.Exists(_rgbafile)); //TODO more meaningful asserts.
            File.Delete(_rgbafile);
        }

        [TestMethod]
        public void WriteRgb()
        {
            if (File.Exists(_rgbfile)) File.Delete(_rgbfile);
            var nifti = new NiftiFloat32();
            nifti.ReadNifti(_minimalNiftiPath);

            // Read pixdim from sample file
            nifti.ReadNiftiHeader(_minimalNiftiHdrPath);

            // set dimensions for new file
            nifti.ConvertHeaderToRgb();

            nifti.Header.cal_min = 0;
            nifti.Header.cal_max = uint.MaxValue * (3 / 4);

            // write voxels
            //var voxelsSize = nifti.Header.dim[1] * nifti.Header.dim[2] * nifti.Header.dim[3];
            //var bytepix = nifti.Header.bitpix / 8;
            //nifti.VoxelsBytes = new byte[voxelsSize * bytepix];

            const int r = 255;
            const int g = 51;
            const int b = 204;

            for (var x = 0; x < nifti.Header.dim[1]; x++)
                for (var y = 0; y < nifti.Header.dim[2]; y++)
                    for (var z = 0; z < nifti.Header.dim[3]; z++)
                        nifti.SetPixelRgb(x, y, z, SliceType.Axial, r, g, b);

            if (File.Exists(_rgbfile)) File.Delete(_rgbfile);
            nifti.WriteNifti(_rgbfile);

            Assert.IsTrue(File.Exists(_rgbfile)); //TODO: More meaningful asserts
            File.Delete(_rgbfile);
        }

        [TestMethod]
        public void DeepCopy()
        {
            NiftiFloat32 nifti = (NiftiFloat32)new NiftiFloat32().ReadNifti(_minimalNiftiPath); // TODO <<-- This is a nasty interface

            var niftiB = (NiftiFloat32)nifti.DeepCopy();
            // Check voxel copy
            Assert.IsTrue(nifti.Voxels[0] == niftiB.Voxels[0]);
            // Check header copy
            Assert.IsTrue(nifti.Header.intent_code == niftiB.Header.intent_code);
            // Check that we're not just doing a shallow copy
            niftiB.Voxels[0] = nifti.Voxels[0] + 1;
            niftiB.Header.intent_code = (short)(nifti.Header.intent_code + 1);
            // Now they should be different...
            Assert.IsFalse(nifti.Voxels[0] == niftiB.Voxels[0]);
            Assert.IsFalse(nifti.Header.intent_code == niftiB.Header.intent_code);
        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }
    }
}
