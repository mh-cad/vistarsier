﻿using CAPI.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CAPI.Tests.Config
{
    [TestClass]
    public class Config
    {
        private CapiConfig _config;

        [TestInitialize]
        public void TestInit()
        {
            _config = CapiConfig.GetConfig();
        }

        [TestMethod]
        public void GetConfigFile()
        {
            // Arrange
            // Act


            // Assert

        }

        [TestMethod]
        public void ExecutablesFolderExists()
        {
            // Act
            var executablesPath = _config.ImgProcConfig.ImgProcBinPath;
            var executablesFolderExists = Directory.Exists(executablesPath);

            // Assert
            Assert.IsTrue(executablesFolderExists, $"Executables path does not exist: {executablesPath}");
        }

        [TestMethod]
        public void JavaExeFileExists()
        {
            // Act
            var javaBinPath = _config.ImgProcConfig.JavaExeBin;
            var javaBinFileExists = File.Exists(javaBinPath);
            // Assert
            Assert.IsTrue(javaBinFileExists, $"Java exe file does not exist: {javaBinPath}");
        }

        [TestMethod]
        public void JavaClasspathContainsValue()
        {
            // Act
            var javaclasspath = _config.ImgProcConfig.JavaClassPath;
            // Assert
            Assert.IsTrue(!string.IsNullOrEmpty(javaclasspath), "Java classpath has no values.");
        }

        [TestMethod]
        public void ProcessesLogPathExists()
        {
            // Act
            var processesLogPath = _config.ImgProcConfig.ProcessesLogPath;
            var procLogPathExists = Directory.Exists(processesLogPath);
            // Assert
            Assert.IsTrue(procLogPathExists, $"Processes log path does not exist: {processesLogPath}");
        }

        [TestMethod]
        public void ImageRepositoryFolderPathExists()
        {
            // Act
            var imgRepositoryPath = _config.ImgProcConfig.ImageRepositoryPath;
            var imgRepositoryPathExists = Directory.Exists(imgRepositoryPath);

            // Assert
            Assert.IsTrue(imgRepositoryPathExists, $"Image repository path does not exist: {imgRepositoryPath}");
        }

        [TestMethod]
        public void Dcm2NiiExeExists()
        {
            var filepath = _config.ImgProcConfig.Dcm2NiiExeFilePath;

            //Assert
            Assert.IsTrue(File.Exists(filepath));
        }

        [TestMethod]
        public void BseExists()
        {
            // Arrange
            // Act
            var bseFile = _config.ImgProcConfig.BseExeFilePath;

            // Assert
            Assert.IsTrue(File.Exists(bseFile));
        }

        [TestMethod]
        public void BfcExists()
        {
            // Arrange
            // Act
            var bfcFile = _config.ImgProcConfig.BfcExeFilePath;

            // Assert
            Assert.IsTrue(File.Exists(bfcFile));
        }
    }
}