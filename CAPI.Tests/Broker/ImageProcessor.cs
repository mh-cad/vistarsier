using CAPI.ImageProcessing.Abstraction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;
using IImageProcessor = CAPI.Agent.Abstractions.IImageProcessor;

namespace CAPI.Tests.Broker
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


        [TestInitialize]
        public void TestInitialize()
        {
            _unity = Helpers.Unity.CreateContainerCore();
            _testResourcesPath = CAPI.Common.Config.Helper.GetTestResourcesPath();
            //const string testFolder = ""05_K87728981";
            //_currentStudyDicomFolder = $@"{_testResourcesPath}\SeriesToTest\{testFolder}\fixed\dicom";
            //_priorStudyDicomFolder = $@"{_testResourcesPath}\SeriesToTest\{testFolder}\floating\dicom";
            //_lookupTableFile = $@"{_testResourcesPath}\LookUpTable.bmp";
            //_destinationResults = $@"{_testResourcesPath}\SeriesToTest\{testFolder}\Results_dicom";
            //_destinationPriorResliced = $@"{_testResourcesPath}\SeriesToTest\{testFolder}\floating\Resliced";
        }

        [TestMethod]
        public void Compare()
        {
            // Arrange
            var brokerImgProc = _unity.Resolve<IImageProcessor>();

            var testFolders = new[] { "01_1323314" };

            foreach (var folder in testFolders)
            {
                _currentStudyDicomFolder = $@"{_testResourcesPath}\SeriesToTest\{folder}\fixed\dicom";
                _priorStudyDicomFolder = $@"{_testResourcesPath}\SeriesToTest\{folder}\floating\dicom";
                _lookupTableFile = $@"{_testResourcesPath}\LookUpTable.bmp";
                _destinationResults = $@"{_testResourcesPath}\SeriesToTest\{folder}\Results_dicom";
                _destinationPriorResliced = $@"{_testResourcesPath}\SeriesToTest\{folder}\floating\Resliced";

                brokerImgProc.CompareAndSendToFilesystem(
                    _currentStudyDicomFolder, _priorStudyDicomFolder, _lookupTableFile, SliceType.Sagittal
                    , true, true, true, _destinationResults, _destinationPriorResliced);
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

        [TestCleanup]
        public void TestCleanup()
        {

        }
    }
}
