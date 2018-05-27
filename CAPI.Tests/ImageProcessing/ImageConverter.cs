﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CAPI.Tests.ImageProcessing
{
    [TestClass]
    public class ImageConverter
    {
        private string _testResourcesPath;
        private string _fixedDicomFolder;
        private string _floatingDicomFolder;
        private string _outputFolder;

        [TestInitialize]
        public void TestInit()
        {
            _testResourcesPath = Common.Config.Helper.GetTestResourcesPath();
            _fixedDicomFolder = $@"{_testResourcesPath}\Fixed\Dicom";
            _floatingDicomFolder = $@"{_testResourcesPath}\Floating\Dicom";
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
            Common.Services.FileSystem.DirectoryExists(_outputFolder);
            var outfile = $@"{_outputFolder}\fixed.nii";

            // Act
            CAPI.ImageProcessing.ImageConverter.DicomToNiix(_fixedDicomFolder, outfile);

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