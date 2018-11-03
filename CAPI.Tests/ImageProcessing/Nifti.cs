using CAPI.Common.Config;
using CAPI.ImageProcessing.Abstraction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
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
        private string _rgbBmpsFolder;
        private ISubtractionLookUpTable _lookUpTable;
        private string _fixedShades;
        private string _floatingShades;
        private string _sampleRgbBitmapsFolder;
        private string _tmpFolder;
        private string _lutGeneratorCurrent;
        private string _lutGeneratorPrior;
        private string _lutGeneratorResult;

        [TestInitialize]
        public void TestInit()
        {

            _unity = Helpers.Unity.CreateContainerCore();
            _testResourcesPath = Helper.GetTestResourcesPath();

            _fixed = $@"{_testResourcesPath}\SeriesToTest\01_1323314\Fixed\fixed.bfc.nii";
            _outfile = $@"{_testResourcesPath}\Fixed\fixed.noro.nii";
            _fixedBrain = $@"{_testResourcesPath}\Fixed\fixed.brain.bfc.nii";
            _rgbafile = $@"{_testResourcesPath}\rgba-test.nii";
            _rgbfile = $@"{_testResourcesPath}\rgb-test.nii";
            _rgbBmpsFolder = $@"{_testResourcesPath}\RgbBmps";
            _lookUpTable = _unity.Resolve<ISubtractionLookUpTable>();
            _lookUpTable.LoadImage($@"{_testResourcesPath}\LookupTable.bmp");
            _sampleRgbBitmapsFolder = $@"{_testResourcesPath}\SampleBmps";
            _fixedShades = $@"{_testResourcesPath}\fixedShades.nii";
            _floatingShades = $@"{_testResourcesPath}\floatingShades.nii";

            _lutGeneratorCurrent = $@"{_testResourcesPath}\LookupTableGenerator\01\Current_with_xtra_lesions.bmp";
            _lutGeneratorPrior = $@"{_testResourcesPath}\LookupTableGenerator\01\Prior_with_xtra_lesions.bmp";
            _lutGeneratorResult = $@"{_testResourcesPath}\LookupTableGenerator\01\Comparison_with_xtra_lesions.bmp";

            _tmpFolder = $@"{_testResourcesPath}\TempFolder";
            if (Directory.Exists(_tmpFolder)) Directory.Delete(_tmpFolder, true);
            Directory.CreateDirectory(_tmpFolder);
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
            nim.ExportSlicesToBmps(_rgbBmpsFolder, SliceType.Sagittal);

            //Assert.Fail(); //TODO3: Not Implemented
        }

        [TestMethod]
        public void ReadVoxelsFromRgbBmps()
        {
            // Arrange
            if (File.Exists($@"{_sampleRgbBitmapsFolder}\nim.nii")) File.Delete($@"{_sampleRgbBitmapsFolder}\nim.nii");
            var files = Directory.GetFiles(_sampleRgbBitmapsFolder)
                .Where(f => Path.GetExtension(f) == ".bmp").ToArray();
            var nim = _unity.Resolve<INifti>();
            nim.Header = nim.ReadHeaderFromFile(_fixedBrain);

            // Act
            nim.ReadVoxelsFromRgb256Bmps(files, SliceType.Sagittal);
            nim.WriteNifti($@"{_sampleRgbBitmapsFolder}\nim.nii");
        }

        [TestMethod]
        public void Compare()
        {
            // Arrange
            var fixedBrain = _unity.Resolve<INifti>().ReadNifti(_fixedShades);
            var floatingResliced = _unity.Resolve<INifti>().ReadNifti(_floatingShades);
            var result = _unity.Resolve<INifti>()
                .Compare(fixedBrain, floatingResliced, SliceType.Sagittal, _lookUpTable, "C:\\temp\\Capi-out\\tmp");
            if (Directory.Exists(_rgbBmpsFolder)) Directory.Delete(_rgbBmpsFolder, true);

            // Act
            result.ExportSlicesToBmps(_rgbBmpsFolder, SliceType.Sagittal);

            // Assert
            Assert.IsTrue(Directory.GetFiles(_rgbBmpsFolder).Length > 0, "No bmp files were found exported from result");
        }

        [TestMethod]
        public void GenerateLookupTable()
        {
            // Arrange
            var current = Image.FromFile(_lutGeneratorCurrent) as Bitmap;
            var prior = Image.FromFile(_lutGeneratorPrior) as Bitmap;
            var result = Image.FromFile(_lutGeneratorResult) as Bitmap;

            // Act
            var nifti = _unity.Resolve<INifti>();
            var lut = nifti.GenerateLookupTable(current, prior, result);
            var lutFilePath = $@"{_tmpFolder}\lut.bmp";
            lut.Save(lutFilePath);

            // Assert
            Assert.IsTrue(File.Exists(lutFilePath));
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
            //nifti.ReadNifti(_fixed);
            //nifti.ReadNifti(@"D:\Capi-Files\Experimental\output\fixed.bfc.nii");
            //nifti.Reorient(nifti.Header.dim[2], nifti.Header.dim[3], nifti.Header.dim[1]);
            //nifti.WriteNifti(@"D:\Capi-Files\Experimental\output\fixed.bfc.ro.nii");

            //nifti.ReadNifti(@"D:\Capi-Files\Experimental\output\fixed_brain_surface.nii");
            //nifti.Reorient(nifti.Header.dim[2], nifti.Header.dim[3], nifti.Header.dim[1]);
            //nifti.WriteNifti(@"D:\Capi-Files\Experimental\output\fixed_brain_surface.ro.nii");

            //nifti.ReadNifti(@"D:\Capi-Files\Experimental\output\floating_resliced.bfc.nii");
            //nifti.Reorient(nifti.Header.dim[2], nifti.Header.dim[3], nifti.Header.dim[1]);
            //nifti.WriteNifti(@"D:\Capi-Files\Experimental\output\floating_resliced.bfc.ro.nii");

            nifti.ReadNifti(@"D:\Capi-Files\Experimental\output\current.mod.nii");
            nifti.Reorient(nifti.Header.dim[2], nifti.Header.dim[3], nifti.Header.dim[1]);
            nifti.WriteNifti(@"D:\Capi-Files\Experimental\output\current.mod.ro.nii");
        }

        [TestMethod]
        public void ReorderVoxelsLpi2Ail()
        {
            var nim = _unity.Resolve<INifti>();
            //nim.ReadNifti(@"D:\temp\tst1\output\fixed.nii");
            //nim.ReorderVoxelsLPI2ASL();
            //nim.WriteNifti(@"D:\temp\tst1\output\fixed.roASR.nii");

            //nim = _unity.Resolve<INifti>();
            //nim.ReadNifti(@"D:\temp\tst1\output\current.mask.nii");
            //nim.ReorderVoxelsLpi2Ail();
            //nim.WriteNifti(@"D:\temp\tst1\output\current.mask.roAIL.nii");

            nim = _unity.Resolve<INifti>();
            nim.ReadNifti(@"D:\temp\tst1\output\diff_dark_in_floating_to_bright_in_fixed.nii");
            nim.ReorderVoxelsLpi2Ail();
            nim.WriteNifti(@"D:\temp\tst1\output\diff_dark_in_floating_to_bright_in_fixed.roAIL.nii");

            nim = _unity.Resolve<INifti>();
            nim.ReadNifti(@"D:\temp\tst1\output\diff_bright_in_floating_to_dark_in_fixed.nii");
            nim.ReorderVoxelsLpi2Ail();
            nim.WriteNifti(@"D:\temp\tst1\output\diff_bright_in_floating_to_dark_in_fixed.roAIL.nii");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(_tmpFolder)) Directory.Delete(_tmpFolder, true);
        }
    }
}