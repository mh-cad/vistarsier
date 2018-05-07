using CAPI.DAL;
using CAPI.DAL.Abstraction;
using CAPI.Dicom;
using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager;
using CAPI.JobManager.Abstraction;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity;
using Unity.log4net;

namespace CAPI.Tests.Job
{
    [TestClass]
    public class Job
    {
        private const string DicomFolderPath = @"D:\Capi-Tests\DicomForTests";
        private const string TestAccession = "2018R0021135-1";
        private const string OutputPath = @"D:\temp\Capi-Tests-Output";
        private const string FixedTest = "FixedTest";
        private const string FloatingTest = "FloatingTest";

        private IJobNew<IRecipe> _job;
        private IDicomNodeRepository _dicomNodeRepo;
        private IJobBuilderNew _jobBuilder;
        private IRecipeRepositoryInMemory<IRecipe> _recipeRepositoryInMemory;

        private static readonly ILog Log = LogHelper.GetLogger();
        private IJobManagerFactory _jobManagerFactory;
        private string _imageRepoFolder;

        [TestInitialize]
        public void TestInit()
        {
            var container = CreateContainerCore();
            _imageRepoFolder = Common.Config.ImgProc.GetImageRepositoryPath();

            _dicomNodeRepo = container.Resolve<IDicomNodeRepository>();
            _jobBuilder = container.Resolve<IJobBuilderNew>();
            _recipeRepositoryInMemory = container.Resolve<IRecipeRepositoryInMemory<IRecipe>>();
            _jobManagerFactory = container.Resolve<IJobManagerFactory>();

            ClearFoldersAndFiles();

            _job = BuildJob();
        }

        private void ClearFoldersAndFiles()
        {
            var fixedTestFolder = $@"{_imageRepoFolder}\{FixedTest}";
            var floatingTestFolder = $@"{_imageRepoFolder}\{FloatingTest}";
            if (Directory.Exists(fixedTestFolder)) Directory.Delete(fixedTestFolder, true);
            if (Directory.Exists(floatingTestFolder)) Directory.Delete(floatingTestFolder, true);

            if (Directory.Exists(OutputPath)) Directory.Delete(OutputPath, true);
            Directory.CreateDirectory(OutputPath);
        }

        private IJobNew<IRecipe> BuildJob()
        {
            var recipe = _recipeRepositoryInMemory.GetAll().FirstOrDefault();
            if (recipe == null) Assert.Fail("Failed to retreive recipe from recipe repository!");
            if (string.IsNullOrEmpty(recipe.NewStudyAccession))
                recipe.NewStudyAccession = TestAccession; // Define Accession Number here

            var localDicomNode = GetLocalNode();
            var sourceNode = _dicomNodeRepo.GetAll()
                .FirstOrDefault(n => n.AeTitle == recipe.SourceAet);

            // Replace Recipe Destinations with Test OutputPath
            var destination = _jobManagerFactory.CreateDestination("1", OutputPath, "");
            recipe.Destinations.Clear();
            recipe.Destinations.Add(destination);

            Log.Info("test");

            return _jobBuilder.Build(recipe, localDicomNode, sourceNode);
        }

        private IDicomNode GetLocalNode()
        {
            return _dicomNodeRepo.GetAll()
                .FirstOrDefault(n => string.Equals(n.AeTitle,
                    Environment.GetEnvironmentVariable("DcmNodeAET_Local", EnvironmentVariableTarget.User),
                    StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void ConvertDicomToNii()
        {
            // Arrange
            if (!Directory.Exists(DicomFolderPath) || Directory.GetFiles(DicomFolderPath).Length == 0)
                Assert.Fail($"No files found in {DicomFolderPath}");

            _job.Fixed = new JobSeriesBundleNew
            {
                Title = FixedTest,
                DicomFolderPath = DicomFolderPath
            };
            _job.Floating = new JobSeriesBundleNew
            {
                Title = FloatingTest,
                DicomFolderPath = DicomFolderPath
            };

            // Act
            _job.ConvertDicomToNii();

            // Assert
            Assert.IsTrue(File.Exists($@"{_imageRepoFolder}\{_job.Fixed.Title}\{_job.Fixed.Title}.nii"));
            Assert.IsTrue(File.Exists($@"{_imageRepoFolder}\{_job.Floating.Title}\{_job.Floating.Title}.nii"));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (string.IsNullOrEmpty(OutputPath) && Directory.Exists(OutputPath))
                Directory.Delete(OutputPath, true);
        }

        private static IUnityContainer CreateContainerCore()
        {
            var container = (UnityContainer)new UnityContainer()
                .AddNewExtension<Log4NetExtension>();

            container.RegisterType<IDicomNode, DicomNode>();
            container.RegisterType<IDicomFactory, DicomFactory>();
            container.RegisterType<IDicomServices, DicomServices>();
            container.RegisterType<IImageConverter, ImageConverter>();
            container.RegisterType<IImageProcessor, ImageProcessor>();
            container.RegisterType<IJobManagerFactory, JobManagerFactory>();
            container.RegisterType<IRecipe, Recipe>();
            container.RegisterType<IJobNew<IRecipe>, JobNew<IRecipe>>();
            container.RegisterType<IJobNew<IRecipe>, JobNew<IRecipe>>();
            container.RegisterType<IJobBuilderNew, JobBuilderNew>();
            container.RegisterType<ISeriesSelectionCriteria, SeriesSelectionCriteria>();
            container.RegisterType<IIntegratedProcess, IntegratedProcess>();
            container.RegisterType<IDestination, Destination>();
            container.RegisterType<IList<IDestination>, List<IDestination>>();
            container.RegisterType<IRecipeRepositoryInMemory<IRecipe>, RecipeRepositoryInMemory<Recipe>>();
            container.RegisterType<IDicomNodeRepository, DicomNodeRepositoryInMemory>();
            container.RegisterType<IValueComparer, ValueComparer>();

            return container;
        }
    }
}