using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace VisTarsier.Tests.Agent
{
    [TestClass]
    public class JobProcessorTest
    {
        private string _testResourcesPath;
        private string _tmpFolder;

        [TestInitialize]
        public void TestInitialize()
        {
            _testResourcesPath = Helper.GetTestResourcesPath();
           // _dicomServices = new DicomServices();
            

            _tmpFolder = $@"{_testResourcesPath}\TempFolder";
            if (Directory.Exists(_tmpFolder)) Directory.Delete(_tmpFolder, true);
            Directory.CreateDirectory(_tmpFolder);
        }

        [TestMethod]
        public void AddOverLay()
        {
            // Arrange
            var filepath = Path.Combine(_testResourcesPath, "bmp", "test.bmp");
            var newFilePath = Path.Combine(_tmpFolder, "test.bmp");
            File.Copy(filepath, newFilePath);
            var overlayText = $"CAPI - Prior re-sliced ({DateTime.Today:dd/MM/yyyy})";
            //var jobProcessor = new JobProcessor();

            // Act
            //jobProcessor.AddOverlayToImage(newFilePath, overlayText);

            // Assert
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(_tmpFolder)) Directory.Delete(_tmpFolder, true);
        }
    }
}
