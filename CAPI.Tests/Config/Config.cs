using CAPI.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CAPI.Tests.Config
{
    [TestClass]
    public class Config
    {
        private string _executablesPath;

        [TestInitialize]
        public void TestInit()
        {
            _executablesPath = ImgProc.GetExecutablesPath();
        }

        [TestMethod]
        public void ExecutablesFolderExists()
        {
            // Act
            Assert.IsFalse(string.IsNullOrEmpty(_executablesPath));
            Assert.IsTrue(_executablesPath.Contains(":"));
            var executablesFolderExists = Directory.Exists(_executablesPath);

            // Assert
            Assert.IsTrue(executablesFolderExists, $"Executables path does not exist: {_executablesPath}");
        }

        [TestMethod]
        public void JavaExeFileExists()
        {
            // Act
            var javaBinPath = ImgProc.GetJavaExePath();
            var javaBinFileExists = File.Exists(javaBinPath);
            // Assert
            Assert.IsTrue(javaBinFileExists, $"Java exe file does not exist: {javaBinPath}");
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
        public void BseExists()
        {
            // Arrange
            var bseFilepath = Path.Combine(_executablesPath, "bse09e.exe");
            var bseExeExists = File.Exists(bseFilepath);

            // Assert
            Assert.IsTrue(bseExeExists, $"BSE exe file does not exist: {bseFilepath}");
        }
    }
}