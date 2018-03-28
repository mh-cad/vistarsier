using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CAPI.Tests.FileSystem
{
    [TestClass]
    public class FileSystemIntegratedTests
    {
        [TestMethod]
        public void CopyFolderRecursively()
        {
            // Arrange
            var workingDir = Environment.CurrentDirectory;
            var sourcePath = $@"{workingDir}\source-test";
            var targetPath = $@"{workingDir}\target-test";

            if (Directory.Exists(sourcePath)) Directory.Delete(sourcePath, true);
            if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);

            Directory.CreateDirectory(sourcePath);
            Directory.CreateDirectory($@"{sourcePath}\1");
            Directory.CreateDirectory($@"{sourcePath}\1\1-1");
            var filePath = $@"{sourcePath}\1\1-1\test.txt";
            const string fileContent = "This is a test";
            File.AppendAllText(filePath, fileContent);

            // Act
            Common.Services.FileSystem.CopyDirectory(sourcePath, targetPath);

            // Assert
            Assert.IsTrue(File.Exists(filePath));
            var contentFromFile = File.ReadAllText(filePath);
            Assert.IsTrue(contentFromFile == fileContent);

            // Clean up
            if (Directory.Exists(sourcePath)) Directory.Delete(sourcePath, true);
            if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
        }
    }
}