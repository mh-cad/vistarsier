using CAPI.Common.Abstractions.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Unity;

namespace CAPI.Tests.FileSystem
{
    [TestClass]
    public class FileSystem
    {
        private string _workingDir;
        private string _sourcePath;
        private string _targetPath;
        private IFileSystem _filesystem;
        private IUnityContainer _unity;

        [TestInitialize]
        public void TestInit()
        {
            _unity = Helpers.Unity.CreateContainerCore();
            _filesystem = _unity.Resolve<IFileSystem>();

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
            _filesystem.CopyDirectory(_sourcePath, _targetPath);

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