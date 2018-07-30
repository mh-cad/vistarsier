using CAPI.Common.Abstractions.Config;
using CAPI.JobManager;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Unity;

namespace CAPI.Tests.Job
{
    [TestClass]
    public class Job
    {
        private IUnityContainer _unity;
        private ICapiConfig _capiConfig;

        private const string FixedDicomFolderPath = @"D:\Capi-Tests\TestsResources\Fixed\Dicom";
        private const string FloatingDicomFolderPath = @"D:\Capi-Tests\TestsResources\Floating\Dicom";
        private const string Destination = @"D:\temp\Capi-Tests-Output";
        private const string FixedTest = "Fixed";
        private const string FloatingTest = "Floating";

        private static readonly ILog Log = LogHelper.GetLogger();
        private string _imageRepoFolder;

        [TestInitialize]
        public void TestInit()
        {
            _unity = Helpers.Unity.CreateContainerCore();
            _capiConfig = _unity.Resolve<ICapiConfig>().GetConfig(new[] { "-dev" });
            _imageRepoFolder = _capiConfig.ImgProcConfig.ImageRepositoryPath;

            ClearFoldersAndFiles();
        }

        private void ClearFoldersAndFiles()
        {
            var fixedTestFolder = $@"{_imageRepoFolder}\{FixedTest}";
            var floatingTestFolder = $@"{_imageRepoFolder}\{FloatingTest}";
            if (Directory.Exists(fixedTestFolder)) Directory.Delete(fixedTestFolder, true);
            if (Directory.Exists(floatingTestFolder)) Directory.Delete(floatingTestFolder, true);

            if (Directory.Exists(Destination)) Directory.Delete(Destination, true);
            Directory.CreateDirectory(Destination);
        }

        [TestMethod]
        public void ConvertDicomToNii()
        {
            // Arrange
            if (!Directory.Exists(FixedDicomFolderPath) || Directory.GetFiles(FixedDicomFolderPath).Length == 0)
                Assert.Fail($"No files found in {FixedDicomFolderPath}");
            if (!Directory.Exists(FloatingDicomFolderPath) || Directory.GetFiles(FloatingDicomFolderPath).Length == 0)
                Assert.Fail($"No files found in {FloatingDicomFolderPath}");

            var job = Helpers.JobBuilder.GetTestJob();
            job.Fixed = new JobSeriesBundleNew
            {
                Title = FixedTest,
                DicomFolderPath = FixedDicomFolderPath
            };
            job.Floating = new JobSeriesBundleNew
            {
                Title = FloatingTest,
                DicomFolderPath = FloatingDicomFolderPath
            };

            // Act
            job.ConvertDicomToNii();

            // Assert
            Assert.IsTrue(File.Exists($@"{_imageRepoFolder}\{job.Fixed.Title}\{job.Fixed.Title}.nii"));
            Assert.IsTrue(File.Exists($@"{_imageRepoFolder}\{job.Floating.Title}\{job.Floating.Title}.nii"));

            // Clean up
            Directory.Delete($@"{_imageRepoFolder}\{job.Fixed.Title}", true);
            Directory.Delete($@"{_imageRepoFolder}\{job.Floating.Title}", true);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (!string.IsNullOrEmpty(Destination) && Directory.Exists(Destination))
                Directory.Delete(Destination, true);
        }
    }
}