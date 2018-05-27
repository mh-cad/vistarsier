using CAPI.ImageProcessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CAPI.Tests.ImageProcessing
{
    [TestClass]
    public class ImageProcessor
    {
        private string _testResourcesPath;
        private string _outputFolder;
        private string _fixedNiiFile;
        private string _floatingNiiFile;

        [TestInitialize]
        public void TestInit()
        {
            _testResourcesPath = Common.Config.Helper.GetTestResourcesPath();
            _outputFolder = $@"{_testResourcesPath}\Output";
            if (Directory.Exists(_outputFolder)) Directory.Delete(_outputFolder, true);
            _fixedNiiFile = $@"{_testResourcesPath}\Fixed\fixed.nii";
            _floatingNiiFile = $@"{_testResourcesPath}\Floating\floating.nii";

            ClearFilesAndFolders();
        }

        private void ClearFilesAndFolders()
        {
            var cmtkRaw = ImgProcConfig.GetCmtkRawxformFile();
            if (File.Exists($@"{_testResourcesPath}\{cmtkRaw}")) File.Delete($@"{_testResourcesPath}\{cmtkRaw}");
            var cmtkResult = ImgProcConfig.GetCmtkResultxformFile();
            if (File.Exists($@"{_testResourcesPath}\{cmtkResult}")) File.Delete($@"{_testResourcesPath}\{cmtkResult}");
            var cmtkFolder = ImgProcConfig.GetCmtkFolderName();
            if (Directory.Exists($@"{_testResourcesPath}\{cmtkFolder}")) Directory.Delete($@"{_testResourcesPath}\{cmtkFolder}", true);
        }

        [TestMethod]
        public void TestResourcesExist()
        {
            var fixedFolder = $@"{_testResourcesPath}\Fixed";
            var floatingFolder = $@"{_testResourcesPath}\Floating";

            Assert.IsTrue(Directory.Exists(_testResourcesPath), $"Test Resources folder does not exist [{_testResourcesPath}]");
            Assert.IsTrue(Directory.Exists(fixedFolder), $"Fixed dicom folder does not exist: [{fixedFolder}]");
            Assert.IsTrue(Directory.GetFiles(fixedFolder).Length > 0, $"No files were found in test fixed folder [{fixedFolder}]");
            Assert.IsTrue(Directory.Exists(floatingFolder), $"Floating dicom folder does not exist: [{floatingFolder}]");
            Assert.IsTrue(Directory.GetFiles(floatingFolder).Length > 0, $"No files were found in test floating folder [{floatingFolder}]");
        }

        [TestMethod]
        public void ExtractBrainMask()
        {
            // Arrange
            var brain = $@"{_outputFolder}\fixed.brain.nii";
            var mask = $@"{_outputFolder}\fixed.mask.nii";
            var bseParams = ImgProcConfig.GetBseParams();
            Common.Services.FileSystem.DirectoryExists(_outputFolder);

            // Act
            ImageProcessorNew.ExtractBrainMask(_fixedNiiFile, bseParams, brain, mask);

            // Assert
            Assert.IsTrue(File.Exists(brain), $"Skull stripped brain file does not exist [{brain}]");
            Assert.IsTrue(File.Exists(mask), $"Brain mask file does not exist [{mask}]");
        }

        [TestMethod]
        public void Registration()
        {
            // Arrange
            var fixedBrain = $@"{_testResourcesPath}\Fixed\fixed.brain.nii";
            var floatingBrain = $@"{_testResourcesPath}\Floating\floating.brain.nii";
            var floatingResliced = $@"{_testResourcesPath}\Floating\floating.resliced.nii";
            Common.Services.FileSystem.DirectoryExists(_outputFolder);

            // Act
            ImageProcessorNew.Registration(fixedBrain, floatingBrain, floatingResliced);

            // Assert
            Assert.IsTrue(File.Exists(floatingResliced), $"Resliced floating file does not exist [{floatingResliced}]");
        }

        [TestMethod]
        public void BiasFieldCorrection()
        {
            // Arrange
            var inNii = $@"{_testResourcesPath}\Fixed\fixed.brain.nii";
            var outNii = $@"{_outputFolder}\fixed.brain.bfc.nii";
            var bseParams = ImgProcConfig.GetBfcParams();
            Common.Services.FileSystem.DirectoryExists(_outputFolder);

            // Act
            ImageProcessorNew.BiasFieldCorrection(inNii, bseParams, outNii);

            // Assert
            Assert.IsTrue(File.Exists(outNii), $"Bias Field Correction output file does not exist [{outNii}]");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(_outputFolder)) Directory.Delete(_outputFolder, true);

            ClearFilesAndFolders();
        }
    }
}