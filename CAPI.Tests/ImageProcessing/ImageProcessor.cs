using CAPI.Common.Abstractions.Services;
using CAPI.Common.Config;
using CAPI.ImageProcessing.Abstraction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Unity;
#pragma warning disable 169

namespace CAPI.Tests.ImageProcessing
{
    [TestClass]
    public class ImageProcessor
    {
        private string _testResourcesPath;
        private string _outputFolder;
        private string _fixedNiiFile;
        private string _floatingNiiFile;
        private string _fixedBrainNiiFile;
        private string _reslicedfloatingNiiFile;
        private IUnityContainer _unity;
        private IImageProcessorNew _imageProcessor;
        private IFileSystem _filesystem;
        private string _lookupTable;
        private string _compareResult;
        private string _fixedDicomFolder;
        private string _floatingDicomFolder;

        [TestInitialize]
        public void TestInit()
        {
            _filesystem = _unity.Resolve<IFileSystem>();
            _testResourcesPath = Helper.GetTestResourcesPath();

            const string testFolderName = "01_1323314";
            _outputFolder = $@"{_testResourcesPath}\Output";
            if (Directory.Exists(_outputFolder)) Directory.Delete(_outputFolder, true);
            _fixedNiiFile = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\fixed\fixed.bfc.nii";
            _floatingNiiFile = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\floating\floating.bfc.nii";
            //_fixedBrainNiiFile = $@"{_testResourcesPath}\Fixed2\fixed.bfc.nii";
            //_reslicedfloatingNiiFile = $@"{_testResourcesPath}\Floating2\floating.bfc.nii";
            _lookupTable = $@"{_testResourcesPath}\LookUpTable.bmp";
            _compareResult = $@"{_testResourcesPath}\compareResult.nii";

            //_fixedDicomFolder = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\fixed";
            //_floatingDicomFolder = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\floating";

            _unity = Helpers.Unity.CreateContainerCore();
            _imageProcessor = _unity.Resolve<IImageProcessorNew>();

            ClearFilesAndFolders();
        }

        private void ClearFilesAndFolders()
        {
            var cmtkRaw = CAPI.ImageProcessing.ImgProcConfig.GetCmtkRawxformFile();
            if (File.Exists($@"{_testResourcesPath}\{cmtkRaw}")) File.Delete($@"{_testResourcesPath}\{cmtkRaw}");
            var cmtkResult = CAPI.ImageProcessing.ImgProcConfig.GetCmtkResultxformFile();
            if (File.Exists($@"{_testResourcesPath}\{cmtkResult}")) File.Delete($@"{_testResourcesPath}\{cmtkResult}");
            var cmtkFolder = CAPI.ImageProcessing.ImgProcConfig.GetCmtkFolderName();
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
            var brain = $@"{_outputFolder}\floating.brain.nii";
            var mask = $@"{_outputFolder}\floating.mask.nii";
            var bseParams = CAPI.ImageProcessing.ImgProcConfig.GetBseParams();
            _filesystem.DirectoryExistsIfNotCreate(_outputFolder);

            // Act
            _imageProcessor.ExtractBrainMask(_floatingNiiFile, bseParams, brain, mask);

            // Assert
            Assert.IsTrue(File.Exists(brain), $"Skull stripped brain file does not exist [{brain}]");
            Assert.IsTrue(File.Exists(mask), $"Brain mask file does not exist [{mask}]");
        }

        [TestMethod]
        public void Registration()
        {
            // Arrange
            var fixedBrain = $@"{_testResourcesPath}\Fixed2\fixed.brain.nii";
            var floatingBrain = $@"{_testResourcesPath}\Floating2\floating.brain.nii";
            var floatingResliced = $@"{_testResourcesPath}\Floating2\floating.resliced.nii";
            _filesystem.DirectoryExistsIfNotCreate(_outputFolder);

            // Act
            _imageProcessor.Registration(fixedBrain, floatingBrain, floatingResliced);

            // Assert
            Assert.IsTrue(File.Exists(floatingResliced), $"Resliced floating file does not exist [{floatingResliced}]");
        }

        [TestMethod]
        public void BiasFieldCorrection()
        {
            // Arrange
            var inNii = $@"{_testResourcesPath}\Fixed2\fixed.brain.nii";
            var outNii = $@"{_outputFolder}\fixed.brain.bfc.nii";
            var bseParams = CAPI.ImageProcessing.ImgProcConfig.GetBfcParams();
            _filesystem.DirectoryExistsIfNotCreate(_outputFolder);

            // Act
            _imageProcessor.BiasFieldCorrection(inNii, bseParams, outNii);

            // Assert
            Assert.IsTrue(File.Exists(outNii), $"Bias Field Correction output file does not exist [{outNii}]");
        }

        //[TestMethod]
        //public void TakeDifference()
        //{
        //    // Arrange
        //    var fixedBrainNii = $@"{_testResourcesPath}\Fixed\fixed.brain.bfc.nii";
        //    var fixedMaskNii = $@"{_testResourcesPath}\Fixed\fixed.mask.nii";
        //    var floatingReslicedNii = $@"{_testResourcesPath}\Floating\floating.resliced.bfc.nii";

        //    var subtractPositive = $@"{_outputFolder}\sub.pos.nii";
        //    var subtractNegative = $@"{_outputFolder}\sub.neg.nii";
        //    var subtractMask = $@"{_outputFolder}\sub.mask.nii";

        //    CAPI.Common.Services.FileSystem.DirectoryExistsIfNotCreate(_outputFolder);

        //    // Act
        //    ImageProcessorNew.TakeDifference(fixedBrainNii, floatingReslicedNii, fixedMaskNii,
        //        subtractPositive, subtractNegative, subtractMask);

        //    // Assert
        //    Assert.IsTrue(File.Exists(subtractPositive), $"Positive structural changes file does not exist [{subtractPositive}]");
        //    Assert.IsTrue(File.Exists(subtractPositive), $"Negative structural changes file does not exist [{subtractNegative}]");
        //    Assert.IsTrue(File.Exists(subtractPositive), $"Structural changes mask file does not exist [{subtractMask}]");
        //}

        //[TestMethod]
        //public void Colormap()
        //{
        //    // Arrange
        //    var fixedDicomFolder = $@"{_testResourcesPath}\Fixed\Dicom";
        //    var fixedMaskNii = $@"{_testResourcesPath}\Fixed\fixed.mask.nii";

        //    var subtractPositive = $@"{_testResourcesPath}\sub.pos.nii";
        //    var subtractNegative = $@"{_testResourcesPath}\sub.neg.nii";

        //    var positiveImagesFolder = $@"{_outputFolder}\{ImgProcConfig.GetSubtractPositiveImgFolder()}";
        //    var negativeImagesFolder = $@"{_outputFolder}\{ImgProcConfig.GetSubtractNegativeImgFolder()}";

        //    CAPI.Common.Services.FileSystem.DirectoryExistsIfNotCreate(_outputFolder);

        //    // Act
        //    ImageProcessorNew.ColorMap(_fixedNiiFile, fixedDicomFolder, fixedMaskNii,
        //        subtractPositive, subtractNegative, positiveImagesFolder, negativeImagesFolder);

        //    // Assert
        //    Assert.IsTrue(File.Exists(positiveImagesFolder) && Directory.GetFiles(positiveImagesFolder).Length > 0,
        //        $"Positive changes images folder does not exist/contains no files in following path: [{positiveImagesFolder}]");
        //    Assert.IsTrue(File.Exists(negativeImagesFolder) && Directory.GetFiles(negativeImagesFolder).Length > 0,
        //        $"Negative changes images folder does not exist/contains no files in following path: [{negativeImagesFolder}]");
        //}

        [TestMethod]
        public void Compare()
        {
            var currentNii = _fixedNiiFile;
            var priorNii = _floatingNiiFile;
            var lookupTable = _lookupTable;
            var resultNii = _compareResult;

            _imageProcessor.Compare(currentNii, priorNii, lookupTable, SliceType.Sagittal, resultNii);
        }

        //[TestMethod]
        //public void RunWholeProcess()
        //{
        //    var currentDicomFolder = _fixedDicomFolder;
        //    var priorDicomFolder = _floatingDicomFolder;
        //    var lookupTable = _lookupTable;
        //    const bool extractBrain = true;
        //    const bool register = true;
        //    const bool biasFieldCorrect = true;
        //    var resultNiiFile = _compareResult;
        //    var outPriorReslicedNii = _reslicedfloatingNiiFile;

        //    _imageProcessor.CompareDicomInNiftiOut(
        //        currentDicomFolder, priorDicomFolder, lookupTable, SliceType.Sagittal,
        //        extractBrain, register, biasFieldCorrect, resultNiiFile, outPriorReslicedNii);

        //    var resultNii = _unity.Resolve<INifti>().ReadNifti(resultNiiFile);

        //    var bmpFolder = Path.Combine(Path.GetDirectoryName(resultNiiFile), "CompareResultBmpFiles");
        //    resultNii.ExportSlicesToBmps(bmpFolder, SliceType.Sagittal);
        //}

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(_outputFolder)) Directory.Delete(_outputFolder, true);

            ClearFilesAndFolders();
        }
    }
}