using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace VisTarsier.Tests.Common
{
    [TestClass]
    public class FileSystemTests
    {
        private string _workingDir;
        private string _sourcePath;
        private string _targetPath;

        [TestInitialize]
        public void TestInit()
        {
            _workingDir = Environment.CurrentDirectory;
            _sourcePath = $@"{_workingDir}\source-test";
            _targetPath = $@"{_workingDir}\target-test";

            if (Directory.Exists(_sourcePath)) Directory.Delete(_sourcePath, true);
            if (Directory.Exists(_targetPath)) Directory.Delete(_targetPath, true);
        }

        [TestMethod]
        public void CopyFolderRecursively()
        {
            // Arrange
            Directory.CreateDirectory(_sourcePath);
            Directory.CreateDirectory($@"{_sourcePath}\1");
            Directory.CreateDirectory($@"{_sourcePath}\1\1-1");
            var filePath = $@"{_sourcePath}\1\1-1\test.txt";
            const string fileContent = "This is a test";
            File.AppendAllText(filePath, fileContent);

            // Act
            VisTarsier.Common.FileSystem.CopyDirectory(_sourcePath, _targetPath);

            // Assert
            Assert.IsTrue(File.Exists(filePath));
            var contentFromFile = File.ReadAllText(filePath);
            Assert.IsTrue(contentFromFile == fileContent);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            if (Directory.Exists(_sourcePath)) Directory.Delete(_sourcePath, true);
            if (Directory.Exists(_targetPath)) Directory.Delete(_targetPath, true);
        }
    }
}