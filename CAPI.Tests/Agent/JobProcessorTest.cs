using CAPI.Config;
using CAPI.Dicom;
using CAPI.Dicom.Abstractions;
using CAPI.Service.Agent;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using SliceType = CAPI.NiftiLib.SliceType;

namespace CAPI.Tests.Agent
{
    [TestClass]
    public class JobProcessorTest
    {
        private string _testResourcesPath;
        private string _tmpFolder;
        private IDicomServices _dicomServices;

        [TestInitialize]
        public void TestInitialize()
        {
            _testResourcesPath = Helper.GetTestResourcesPath();
            _dicomServices = new DicomServices();
            

            _tmpFolder = $@"{_testResourcesPath}\TempFolder";
            if (Directory.Exists(_tmpFolder)) Directory.Delete(_tmpFolder, true);
            Directory.CreateDirectory(_tmpFolder);
        }

        [TestMethod]
        public void AddOverLay()
        {
            // Arrange
            var filepath = Path.Combine(_testResourcesPath,"test.bmp");
            var newFilePath = Path.Combine(_tmpFolder, "test.bmp");
            File.Copy(filepath, newFilePath);
            var overlayText = $"CAPI - Prior re-sliced ({DateTime.Today:dd/MM/yyyy})";
            var jobProcessor = new JobProcessor(null);

            // Act
            jobProcessor.AddOverlayToImage(newFilePath, overlayText);

            // Assert
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(_tmpFolder)) Directory.Delete(_tmpFolder, true);
        }
    }
}
