using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Configs = CAPI.Common.Config;

namespace CAPI.IntegratedTests.Config
{
    [TestClass]
    public class ConfigsIntegratedTests
    {
        private readonly string _executablesPath;

        public ConfigsIntegratedTests()
        {
            _executablesPath = Configs.GetExecutablesPath();
        }

        [TestMethod]
        public void ExecutablesFolderExists()
        {
            // Act
            Assert.IsFalse(string.IsNullOrEmpty(_executablesPath));
            Assert.IsTrue(_executablesPath.Contains(":"));
            var executablesFolderExists = Directory.Exists(_executablesPath);
            // Assert
            Assert.IsTrue(executablesFolderExists);
        }

        [TestMethod]
        public void JavaExeFileExists()
        {
            // Act
            var javaBinPath = Configs.GetJavaExePath();
            var javaBinFileExists = File.Exists(javaBinPath);
            // Assert
            Assert.IsTrue(javaBinFileExists);
        }

        [TestMethod]
        public void ProcessesLogPathExists()
        {
            // Act
            var processesLogPath = Configs.GetProcessesLogPath();
            var procLogPathExists = Directory.Exists(processesLogPath);
            // Assert
            Assert.IsTrue(procLogPathExists);
        }

        [TestMethod]
        public void ImageRepositoryFolderPathExists()
        {
            // Act
            var imgRepositoryPath = Configs.GetProcessesLogPath();
            var imgRepositoryPathExists = Directory.Exists(imgRepositoryPath);
            // Assert
            Assert.IsTrue(imgRepositoryPathExists);
        }

        [TestMethod]
        public void BseExists()
        {
            // Assert
            var bseExeExists = File.Exists(Path.Combine(_executablesPath, "bse09e.exe"));
            Assert.IsTrue(bseExeExists);
        }
    }
}