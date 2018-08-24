using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
using CAPI.Common.Config;
using CAPI.ImageProcessing.Abstraction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Unity;

namespace CAPI.Tests.ImageProcessing
{
    [TestClass]
    public class ImageConverter
    {
        private IUnityContainer _unity;
        private IFileSystem _filesystem;
        private IProcessBuilder _processBuilder;
        private IImageProcessingFactory _imageProcessingFactory;
        private IImgProcConfig _imgProcConfig;

        private string _testResourcesPath;
        private string _fixedDicomFolder;
        private string _floatingDicomFolder;
        private string _outputFolder;


        [TestInitialize]
        public void TestInit()
        {
            _unity = Helpers.Unity.CreateContainerCore();
            _filesystem = _unity.Resolve<IFileSystem>();
            _processBuilder = _unity.Resolve<IProcessBuilder>();
            _imageProcessingFactory = _unity.Resolve<IImageProcessingFactory>();
            _imgProcConfig = _unity.Resolve<IImgProcConfig>();

            _testResourcesPath = Helper.GetTestResourcesPath();
            _fixedDicomFolder = $@"{_testResourcesPath}\Fixed2\Dicom";
            _floatingDicomFolder = $@"{_testResourcesPath}\Floating2\Dicom";
            _outputFolder = $@"{_testResourcesPath}\Output";

            if (Directory.Exists(_outputFolder)) Directory.Delete(_outputFolder, true);
        }

        [TestMethod]
        public void TestResourcesExist()
        {
            Assert.IsTrue(Directory.Exists(_testResourcesPath), $"Test Resources folder does not exist [{_testResourcesPath}]");
            Assert.IsTrue(Directory.Exists(_fixedDicomFolder), $"Fixed dicom folder does not exist: [{_fixedDicomFolder}]");
            Assert.IsTrue(Directory.GetFiles(_fixedDicomFolder).Length > 0, $"No files were found in test fixed folder [{_fixedDicomFolder}]");
            Assert.IsTrue(Directory.Exists(_floatingDicomFolder), $"Floating dicom folder does not exist: [{_floatingDicomFolder}]");
            Assert.IsTrue(Directory.GetFiles(_floatingDicomFolder).Length > 0, $"No files were found in test floating folder [{_floatingDicomFolder}]");
        }

        [TestMethod]
        public void ConvertDicom2Nii()
        {
            // Arrange
            _filesystem.DirectoryExistsIfNotCreate(_outputFolder);
            var outfile = $@"{_outputFolder}\floating2.nii";

            // Act
            var imageConverter =
                _imageProcessingFactory.CreateImageConverter(_filesystem, _processBuilder, _imgProcConfig);
            imageConverter.DicomToNiix(_floatingDicomFolder, outfile);

            // Assert
            Assert.IsTrue(File.Exists(outfile), "dcm2niix failed to convert dicom to nii");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(_outputFolder)) Directory.Delete(_outputFolder, true);
        }
    }
}