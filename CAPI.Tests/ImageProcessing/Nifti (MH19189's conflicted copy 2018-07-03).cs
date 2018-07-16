using CAPI.ImageProcessing.Abstraction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using Unity;

namespace CAPI.Tests.ImageProcessing
{
    [TestClass]
    public class Nifti
    {
        private IUnityContainer _unity;
        private string _testResourcesPath;
        private string _fixed;
        private string _outfile;
        private string _rgbafile;
        private string _rgbfile;
        private string _fixedBrain;
        private string _floatingResliced;
        private string _rgbBmpsFolder;
        private ISubtractionLookUpTable _lookUpTable;
        private string _fixedBrain1;
        private string _slice;
        private string _sliceCopy;

        [TestInitialize]
        public void TestInit()
        {
            _unity = Helpers.Unity.CreateContainerCore();
            _testResourcesPath = CAPI.Common.Config.Helper.GetTestResourcesPath();
            _fixed = $@"{_testResourcesPath}\Fixed\fixed.nii";
            _outfile = $@"{_testResourcesPath}\Fixed\fixed.noro.nii";
            _fixedBrain = $@"{_testResourcesPath}\Fixed\fixed.brain.bfc.nii";
            _fixedBrain1 = $@"{_testResourcesPath}\Fixed\fixed.brain.bfc1.nii";
            _slice = $@"{_testResourcesPath}\slice.nii";
            _sliceCopy = $@"{_testResourcesPath}\sliceCopy.nii";
            _floatingResliced = $@"{_testResourcesPath}\Floating\floating.resliced.bfc.nii";
            _rgbafile = $@"{_testResourcesPath}\rgba-test.nii";
            _rgbfile = $@"{_testResourcesPath}\rgb-test.nii";
            _rgbBmpsFolder = $@"{_testResourcesPath}\RgbBmps";
            _lookUpTable = _unity.Resolve<ISubtractionLookUpTable>();
            _lookUpTable.LoadImage($@"{_testResourcesPath}\LookupTable.bmp");
        }

        [TestMethod]
        public void Read()
        {
            var nifti = _unity.Resolve<INifti>();
            nifti.ReadNifti(_fixed);

            Assert.Fail(); //TODO3: Not Implemented
        }

        [TestMethod]
        public void Write()
        {
            var nifti = _unity.Resolve<INifti>();
            nifti.ReadNifti(_fixed);
            //nifti.Reorient(256, 256, 160);
            nifti.WriteNifti(_outfile);

            Assert.Fail(); //TODO3: Not Implemented
        }

        [TestMethod]
        public void WriteRgba()
        {
            var nifti = _unity.Resolve<INifti>();
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

            nifti.WriteNifti(_rgbafile);

            Assert.Fail(); //TODO3: Not Implemented
        }

        [TestMethod]
        public void WriteRgb()
        {
            if (File.Exists(_rgbfile)) File.Delete(_rgbfile);
            var nifti = _unity.Resolve<INifti>();

            // Read pixdim from sample file
            var header = nifti.ReadHeaderFromFile(_fixedBrain);
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

            nifti.WriteNifti(_rgbfile);

            //Assert.Fail(); //TODO3: Not Implemented
        }

        [TestMethod]
        public void ExportToBmps()
        {
            var nim = _unity.Resolve<INifti>();
            nim.ReadNifti(_fixed);
            if (Directory.Exists(_rgbBmpsFolder)) Directory.Delete(_rgbBmpsFolder, true);
            nim.ExportSlicesToBmps(_rgbBmpsFolder, SliceType.Axial);

            //Assert.Fail(); //TODO3: Not Implemented
        }

        [TestMethod]
        public void Compare()
        {
            var fixedBrain = _unity.Resolve<INifti>();
            fixedBrain.ReadNifti(_slice);

            var floatingResliced = _unity.Resolve<INifti>();
            //floatingResliced.ReadNifti(_floatingResliced);
            floatingResliced.ReadNifti(_sliceCopy);

            var result = fixedBrain.Compare(floatingResliced, SliceType.Sagittal, _lookUpTable);

            if (Directory.Exists(_rgbBmpsFolder)) Directory.Delete(_rgbBmpsFolder, true);
            result.ExportSlicesToBmps(_rgbBmpsFolder, SliceType.Sagittal);

            //Assert.Fail(); //TODO3: Not Implemented
        }

        [TestMethod]
        public void SlicesSagittalToArray()
        {
            // Arrange
            var nim = _unity.Resolve<INifti>();
            nim.voxels = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
            nim.Header.dim = new short[] { 3, 2, 3, 4 };
            var slices = nim.GetSlices(SliceType.Sagittal).ToArray();

            // Act
            var arr = nim.SlicesToArray(slices, SliceType.Sagittal);

            // Assert
            for (var i = 0; i < arr.Length; i++)
                Assert.AreEqual(arr[i], nim.voxels[i]);
        }
        [TestMethod]
        public void SlicesAxialToArray()
        {
            // Arrange
            var nim = _unity.Resolve<INifti>();
            nim.voxels = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
            nim.Header.dim = new short[] { 3, 2, 3, 4 };
            var slices = nim.GetSlices(SliceType.Axial).ToArray();

            // Act
            var arr = nim.SlicesToArray(slices, SliceType.Axial);

            // Assert
            for (var i = 0; i < arr.Length; i++)
                Assert.AreEqual(arr[i], nim.voxels[i]);
        }
        [TestMethod]
        public void SlicesCoronalToArray()
        {
            // Arrange
            var nim = _unity.Resolve<INifti>();
            nim.voxels = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
            nim.Header.dim = new short[] { 3, 2, 3, 4 };
            var slices = nim.GetSlices(SliceType.Coronal).ToArray();

            // Act
            var arr = nim.SlicesToArray(slices, SliceType.Coronal);

            // Assert
            for (var i = 0; i < arr.Length; i++)
                Assert.AreEqual(arr[i], nim.voxels[i]);
        }

        [TestMethod]
        public void Reorient()
        {
            var nifti = _unity.Resolve<INifti>();
            nifti.ReadNifti(_fixed);
            nifti.Reorient(256, 256, 160);
        }
    }
}