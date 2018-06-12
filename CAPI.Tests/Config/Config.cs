using CAPI.Common.Config;
using CAPI.ImageProcessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CAPI.Tests.Config
{
    [TestClass]
    public class Config
    {
        [TestInitialize]
        public void TestInit()
        {
        }

        [TestMethod]
        public void ExecutablesFolderExists()
        {
            // Act
            var executablesPath = ImgProcConfig.GetImgProcBinPath();
            var executablesFolderExists = Directory.Exists(executablesPath);

            // Assert
            Assert.IsTrue(executablesFolderExists, $"Executables path does not exist: {executablesPath}");
        }

        [TestMethod]
        public void JavaExeFileExists()
        {
            // Act
            var javaBinPath = ImgProcConfig.GetJavaExeBin();
            var javaBinFileExists = File.Exists(javaBinPath);
            // Assert
            Assert.IsTrue(javaBinFileExists, $"Java exe file does not exist: {javaBinPath}");
        }

        [TestMethod]
        public void JavaClasspathContainsValue()
        {
            // Act
            var javaclasspath = ImgProcConfig.GetJavaClassPath();
            // Assert
            Assert.IsTrue(!string.IsNullOrEmpty(javaclasspath), "Java classpath has no values.");
        }

        [TestMethod]
        public void ProcessesLogPathExists()
        {
            // Act
            var processesLogPath = ImgProc.GetProcessesLogPath();
            var procLogPathExists = Directory.Exists(processesLogPath);
            // Assert
            Assert.IsTrue(procLogPathExists, $"Processes log path does not exist: {processesLogPath}");
        }

        [TestMethod]
        public void ImageRepositoryFolderPathExists()
        {
            // Act
            var imgRepositoryPath = ImgProc.GetImageRepositoryPath();
            var imgRepositoryPathExists = Directory.Exists(imgRepositoryPath);

            // Assert
            Assert.IsTrue(imgRepositoryPathExists, $"Image repository path does not exist: {imgRepositoryPath}");
        }

        [TestMethod]
        public void Dcm2NiiExeExists()
        {
            var filepath = ImgProcConfig.GetDcm2NiiExeFilePath();

            //Assert
            Assert.IsTrue(File.Exists(filepath));
        }

        [TestMethod]
        public void BseExists()
        {
            ImgProcConfig.GetBseExeFilePath();
        }

        [TestMethod]
        public void BfcExists()
        {
            ImgProcConfig.GetBfcExeFilePath();
        }

        [TestMethod]
        public void ColorMapConfigExists()
        {
            ImgProcConfig.GetColorMapConfigFile();
        }
    }
}