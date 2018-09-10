using CAPI.Agent.Abstractions;
using CAPI.Common.Config;
using CAPI.Dicom.Abstractions;
using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Unity;
using IImageProcessor = CAPI.Agent.Abstractions.IImageProcessor;

namespace CAPI.Tests.Agent
{
    [TestClass]
    public class ImageProcessor
    {
        private IUnityContainer _unity;
        private string _testResourcesPath;
        private string _currentStudyDicomFolder;
        private string _priorStudyDicomFolder;
        private string _lookupTableFile;
        private string _destinationResults;
        private string _destinationPriorResliced;
        private string _tmpFolder;
        private IDicomServices _dicomServices;
        private IImageProcessingFactory _imgProcFactory;
        private IFileSystem _fileSystem;
        private IProcessBuilder _processBuilder;
        private CapiConfig _capiConfig;
        private CAPI.Dicom.Abstractions.IDicomConfig _dicomConfig;
        private ILog _log;

        [TestInitialize]
        public void TestInitialize()
        {
            _unity = Helpers.Unity.CreateContainerCore();
            _testResourcesPath = Helper.GetTestResourcesPath();
            _log = LogHelper.GetLogger();
            _fileSystem = _unity.Resolve<IFileSystem>();
            _processBuilder = _unity.Resolve<IProcessBuilder>();
            _capiConfig = new CapiConfig().GetConfig(new[] { "-dev" });
            _dicomConfig = _unity.Resolve<IDicomConfig>();
            //_dicomConfig.ExecutablesPath = _capiConfig.DicomConfig.DicomServicesExecutablesPath;
            _dicomServices = _unity.Resolve<IDicomFactory>()
                .CreateDicomServices(_dicomConfig, _fileSystem, _processBuilder, _log);
            _imgProcFactory = _unity.Resolve<IImageProcessingFactory>();

            _tmpFolder = $@"{_testResourcesPath}\TempFolder";
            if (Directory.Exists(_tmpFolder)) Directory.Delete(_tmpFolder, true);
            Directory.CreateDirectory(_tmpFolder);
        }

        [TestMethod]
        public void Compare()
        {
            // Arrange
            var agentFactory = _unity.Resolve<IAgentFactory>();
            var agentImgProc = agentFactory.CreateAgentImageProcessor(
                _dicomServices, _imgProcFactory, _fileSystem, _processBuilder, _capiConfig.ImgProcConfig, _log);

            var testFolders = new[] { "01_1323314" };

            foreach (var folder in testFolders)
            {
                _currentStudyDicomFolder = Path.Combine(_tmpFolder, "fixed", "dicom");
                _fileSystem.CopyDirectory($@"{_testResourcesPath}\SeriesToTest\{folder}\fixed\dicom", _currentStudyDicomFolder);
                _priorStudyDicomFolder = Path.Combine(_tmpFolder, "floating", "dicom");
                _fileSystem.CopyDirectory($@"{_testResourcesPath}\SeriesToTest\{folder}\floating\dicom", _priorStudyDicomFolder);

                //_currentStudyDicomFolder = $@"{_testResourcesPath}\SeriesToTest\{folder}\fixed\dicom";
                //_priorStudyDicomFolder = $@"{_testResourcesPath}\SeriesToTest\{folder}\floating\dicom";
                _lookupTableFile = $@"{_testResourcesPath}\LookUpTable.bmp";
                _destinationResults = Path.Combine(_tmpFolder, "Results_dicom"); //$@"{_testResourcesPath}\SeriesToTest\{folder}\Results_dicom";
                _destinationPriorResliced = Path.Combine(_tmpFolder, "Resliced"); //$@"{_testResourcesPath}\SeriesToTest\{folder}\floating\Resliced";

                agentImgProc.CompareAndSendToFilesystem(
                    _currentStudyDicomFolder, _priorStudyDicomFolder, _lookupTableFile, SliceType.Sagittal
                    , true, true, true, _destinationResults, _destinationPriorResliced, "Results", "Prior Resliced");

                // Assert
                Assert.IsTrue(Directory.Exists(_destinationResults));
                Assert.IsTrue(Directory.GetFiles(_destinationResults).Length > 0);
                Assert.IsTrue(Directory.Exists(_destinationPriorResliced));
                Assert.IsTrue(Directory.GetFiles(_destinationPriorResliced).Length > 0);
            }

            // Act
            //brokerImgProc.CompareAndSendToFilesystem(
            //    _currentStudyDicomFolder, _priorStudyDicomFolder, _lookupTableFile, SliceType.Sagittal
            //    , true, true, true, _destinationResults, _destinationPriorResliced);

            //// Assert
            //Assert.IsTrue(Directory.Exists(_destinationResults) && Directory.GetFiles(_destinationResults).Length > 0,
            //    $"CompareAndSendToFilesystem results folder is either empty or contains no files: {_destinationResults}");

            //Assert.IsTrue(Directory.Exists(_destinationPriorResliced) && Directory.GetFiles(_destinationPriorResliced).Length > 0,
            //    $"Prior study resliced folder is either empty or contains no files: {_destinationPriorResliced}");
        }

        [TestMethod]
        public void AddOverLay()
        {
            // Arrange
            var filepath = Path.Combine(_testResourcesPath, "SampleBmps", "ResultSample.bmp");
            var newFilePath = Path.Combine(_tmpFolder, "SampleBmpFile.bmp");
            File.Copy(filepath, newFilePath);
            var overlayText = $"CAPI - Prior re-sliced ({DateTime.Today:dd/MM/yyyy})";
            var imgProcessor = _unity.Resolve<IImageProcessor>();

            // Act
            imgProcessor.AddOverlayToImage(newFilePath, overlayText);

            // Assert
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(_tmpFolder)) Directory.Delete(_tmpFolder, true);
        }
    }
}
