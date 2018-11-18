using CAPI.Common.Config;
using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using log4net;
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
        private string _fixedBfcNiiFile;
        private string _floatingBfcNiiFile;
        private string _fixedBrainNiiFile;
        private string _reslicedfloatingNiiFile;
        private IUnityContainer _unity;
        private IImageProcessor _imageProcessor;
        private IFileSystem _filesystem;
        private string _lookupTable;
        private string _compareResult;
        private string _fixedDicomFolder;
        private string _floatingDicomFolder;
        private CapiConfig _capiConfig;
        private string _fixedMaskFile;
        private string _fixedNiiFile;
        private IProcessBuilder _processBuilder;
        private ILog _log;
        private string _fixedBrainFile;
        private string _floatingBrainFile;
        private string _floatingMaskFile;
        private string _resultsFolder;
        private string _fixedNormalizedNiiFile;
        private string _floatingNormalizedNiiFile;

        [TestInitialize]
        public void TestInit()
        {
            _unity = Helpers.Unity.CreateContainerCore();

            _log = LogHelper.GetLogger();
            _filesystem = _unity.Resolve<IFileSystem>();
            _processBuilder = _unity.Resolve<IProcessBuilder>();
            _capiConfig = new CapiConfig().GetConfig(new[] { "-dev" });
            _testResourcesPath = _capiConfig.TestsConfig.TestResourcesPath;  //Helper.GetTestResourcesPath();
            var imgProcFactory = _unity.Resolve<IImageProcessingFactory>();
            _imageProcessor = imgProcFactory.CreateImageProcessor(_filesystem, _processBuilder, _capiConfig.ImgProcConfig, _log);

            const string testFolderName = "sample_series";
            _outputFolder = $@"{_testResourcesPath}\Output";
            _resultsFolder = $@"{_outputFolder}\Results";
            if (Directory.Exists(_outputFolder)) Directory.Delete(_outputFolder, true);
            _filesystem.DirectoryExistsIfNotCreate(_outputFolder);
            if (Directory.Exists(_resultsFolder)) Directory.Delete(_resultsFolder, true);
            _filesystem.DirectoryExistsIfNotCreate(_resultsFolder);

            var fixedFilepath = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\Current\fixed.nii";
            var fixedBrainFilepath = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\Current\fixed.brain.nii";
            var fixedMaskFilepath = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\Current\fixed.mask.nii";
            var floatingBrainFilepath = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\Prior\floating.brain.nii";
            var floatingMaskFilepath = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\Prior\floating.mask.nii";
            var fixedBfcFilepath = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\Current\fixed.bfc.nii";
            var floatingBfcFilePath = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\prior.resliced.bfc.nii";
            var fixedNormalizedFilePath = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\Current\fixed.normalized.nii";
            var floatingNormalizedFilePath = $@"{_testResourcesPath}\SeriesToTest\{testFolderName}\PriorResliced.nii";

            _lookupTable = $@"{_testResourcesPath}\LookUpTable.bmp";
            _compareResult = $@"{_testResourcesPath}\compareResult.nii";

            _fixedNiiFile = Path.Combine(_outputFolder, "current.nii");
            _fixedBrainFile = Path.Combine(_outputFolder, "current.brain.nii");
            _fixedMaskFile = Path.Combine(_outputFolder, "current.mask.nii");
            _floatingBrainFile = Path.Combine(_outputFolder, "prior.brain.nii");
            _floatingMaskFile = Path.Combine(_outputFolder, "prior.mask.nii");
            _fixedBfcNiiFile = Path.Combine(_outputFolder, "current.bfc.nii");
            _floatingBfcNiiFile = Path.Combine(_outputFolder, "prior.bfc.nii");
            _fixedNormalizedNiiFile = Path.Combine(_outputFolder, "current.normalized.nii");
            _floatingNormalizedNiiFile = Path.Combine(_outputFolder, "prior.normalized.nii");

            File.Copy(fixedFilepath, _fixedNiiFile);
            File.Copy(fixedBrainFilepath, _fixedBrainFile);
            File.Copy(fixedMaskFilepath, _fixedMaskFile);
            File.Copy(floatingBrainFilepath, _floatingBrainFile);
            File.Copy(floatingMaskFilepath, _floatingMaskFile);
            File.Copy(fixedBfcFilepath, _fixedBfcNiiFile);
            File.Copy(floatingBfcFilePath, _floatingBfcNiiFile);
            File.Copy(fixedNormalizedFilePath, _fixedNormalizedNiiFile);
            File.Copy(floatingNormalizedFilePath, _floatingNormalizedNiiFile);

            ClearFilesAndFolders();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(_outputFolder)) Directory.Delete(_outputFolder, true);

            ClearFilesAndFolders();
        }

        private void ClearFilesAndFolders()
        {
            var cmtkRaw = _capiConfig.ImgProcConfig.CmtkRawxformFile;
            if (File.Exists($@"{_testResourcesPath}\{cmtkRaw}")) File.Delete($@"{_testResourcesPath}\{cmtkRaw}");
            var cmtkResult = _capiConfig.ImgProcConfig.CmtkResultxformFile;
            if (File.Exists($@"{_testResourcesPath}\{cmtkResult}")) File.Delete($@"{_testResourcesPath}\{cmtkResult}");
            var cmtkFolder = _capiConfig.ImgProcConfig.CmtkFolderName;
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
            var bseParams = _capiConfig.ImgProcConfig.BseParams;
            _filesystem.DirectoryExistsIfNotCreate(_outputFolder);

            // Act
            _imageProcessor.ExtractBrainMask(_floatingBfcNiiFile, bseParams, brain, mask);

            // Assert
            Assert.IsTrue(File.Exists(brain), $"Skull stripped brain file does not exist [{brain}]");
            Assert.IsTrue(File.Exists(mask), $"Brain mask file does not exist [{mask}]");
        }

        [TestMethod]
        public void Registration()
        {
            // Arrange
            var floatingResliced = $@"{_resultsFolder}\prior.resliced.nii";
            var maskResliced = $@"{_resultsFolder}\prior.resliced.mask.nii";
            _filesystem.DirectoryExistsIfNotCreate(_resultsFolder);

            // Act
            _imageProcessor.Registration(_fixedBrainFile, _floatingBrainFile, floatingResliced, "brain");
            _imageProcessor.Registration(_fixedMaskFile, _floatingMaskFile, maskResliced, "mask");

            // Assert
            Assert.IsTrue(File.Exists(floatingResliced), $"Resliced floating file does not exist [{floatingResliced}]");
            Assert.IsTrue(File.Exists(maskResliced), $"Resliced floating mask file does not exist [{maskResliced}]");
        }

        [TestMethod]
        public void BiasFieldCorrection()
        {
            // Arrange
            var outNii = $@"{_resultsFolder}\prior.brain.bfc.nii";
            const string bseParams = "--iterate --ellipse --timer";
            _filesystem.DirectoryExistsIfNotCreate($"{_resultsFolder}");

            // Act
            _imageProcessor.BiasFieldCorrection(_floatingBrainFile, _floatingMaskFile, bseParams, outNii);

            // Assert
            Assert.IsTrue(File.Exists(outNii), $"Bias Field Correction output file does not exist [{outNii}]");
        }

        [TestMethod]
        public void Normalize()
        {
            // Arrange
            var lookupTable = _lookupTable;

            // Act
            _imageProcessor.Normalize(_fixedBfcNiiFile, _fixedMaskFile, SliceType.Sagittal, 128, 32, 256);
            _imageProcessor.Normalize(_floatingBfcNiiFile, _floatingMaskFile, SliceType.Sagittal, 128, 32, 256);

            // Assert
            // pre-normalized nifti files get renamed to %currentNii%.preNormalized.nii - normalized files keep original file names
            Assert.IsTrue(File.Exists(_fixedBfcNiiFile));
            Assert.IsTrue(File.Exists(_floatingBfcNiiFile));
        }

        [TestMethod]
        public void TempNormalize()
        {
            // Arrange
            var currentBfc = @"C:\temp\Capi-out\Normalization\fixed.bfc.nii";
            var currentMask = @"C:\temp\Capi-out\Normalization\fixed.mask.nii";
            var priorBfc = @"C:\temp\Capi-out\Normalization\PriorResliced.bfc.nii";
            var priorMask = @"C:\temp\Capi-out\Normalization\floating.mask.nii";

            // Act
            _imageProcessor.Normalize(currentBfc, currentMask, SliceType.Sagittal, 128, 32, 256);
            _imageProcessor.Normalize(priorBfc, priorMask, SliceType.Sagittal, 128, 32, 256);

            // Assert
            // pre-normalized nifti files get renamed to %currentNii%.preNormalized.nii - normalized files keep original file names
            Assert.IsTrue(File.Exists(_fixedBfcNiiFile));
            Assert.IsTrue(File.Exists(_floatingBfcNiiFile));
        }

        [TestMethod]
        public void Compare()
        {
            var currentNii = _fixedNormalizedNiiFile;
            var priorNii = _floatingNormalizedNiiFile;
            var lookupTable = _lookupTable;
            var resultNii = _compareResult;

            _imageProcessor.Compare(currentNii, priorNii, lookupTable, SliceType.Sagittal, resultNii);
        }

        //CompareUsingNictaCode
        [TestMethod]
        public void CompareUsingNictaCode()
        {
            var currentBfc = @"C:\temp\Capi-out\Normalization\fixed.bfc.nii";
            var currentMask = @"C:\temp\Capi-out\Normalization\fixed.mask.nii";
            var priorBfc = @"C:\temp\Capi-out\Normalization\PriorResliced.bfc.nii";
            var nictaPosResultsFilePath = @"C:\temp\Capi-out\Normalization\NictaPosResult.nii";
            var nictaNegResultsFilePath = @"C:\temp\Capi-out\Normalization\NictaNegResult.nii";
            var colorMapConfigFilePath = @"D:\Capi-Tests\colormap.config";

            _imageProcessor.CompareUsingNictaCode(currentBfc, priorBfc, currentMask,
                                                  nictaPosResultsFilePath, nictaNegResultsFilePath,
                                                  colorMapConfigFilePath, false);
        }
    }
}