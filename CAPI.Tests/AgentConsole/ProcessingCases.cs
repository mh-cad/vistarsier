using CAPI.Agent_Console;
using CAPI.Agent_Console.Abstractions;
using CAPI.Common.Config;
using CAPI.DAL;
using CAPI.DAL.Abstraction;
using CAPI.Dicom;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager;
using CAPI.JobManager.Abstraction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using Unity;
using Unity.Lifetime;

namespace CAPI.Tests.AgentConsole
{
    [TestClass]
    public class ProcessingCases
    {
        private IAgentConsoleRepository _agentConsoleRepository;
        private IAgentConsoleFactory _agentConsoleFactory;
        private readonly string _manualCasesFolder = ImgProc.GetManualProcessPath();
        private readonly string _hl7CasesFolder = ImgProc.GetHl7ProcessPath();
        private Broker _broker;
        private const string SampleAccession = "2018R0029246-1";

        [TestInitialize]
        public void TestInit()
        {
            var container = CreateContainerCore();
            _agentConsoleFactory = container.Resolve<IAgentConsoleFactory>();
            _agentConsoleRepository = _agentConsoleFactory.CreateAgentConsoleRepository();
            _broker = GetBroker(container);
        }

        [TestMethod]
        public void GetManualCasesFromFolder()
        {
            // Arrage
            // Make sure there's no pending manual TestAccession in db before starting the test
            var pendingCases = _agentConsoleRepository.GetPendingCases();
            var manualPendingCases = pendingCases.Where(c => c.Accession == "TestAccession").ToList();
            Assert.IsTrue(manualPendingCases.Count == 0, "There is already a Pending TestAccession in DB. Please try running the test again.");

            // Add TestAccession file to manual process folder
            var testManualAccessionFilePath = Path.Combine(_manualCasesFolder, "TestAccession");
            File.Create(testManualAccessionFilePath).Close();

            // Act
            _broker.AddNewCasesFromManualProcessFolderToDb();

            // Assert
            pendingCases = _agentConsoleRepository.GetPendingCases();
            manualPendingCases = pendingCases
                .Where(c => c.Accession == "TestAccession" && c.AdditionMethod == "Manual").ToList();
            Assert.IsTrue(manualPendingCases.Count == 1, "There should only be one TestAccession added manually in database!");
        }

        [TestMethod]
        public void GetHl7CasesFromFolder()
        {
            // Arrage
            // Make sure there's no pending manual TestAccession in db before starting the test
            var pendingCases = _agentConsoleRepository.GetPendingCases();
            var testCases = pendingCases.Where(c => c.Accession == "TestAccession").ToList();
            Assert.IsTrue(testCases.Count == 0, "There is already a Pending TestAccession in DB. Please try running the test again.");

            // Add TestAccession file to manual process folder
            var testManualAccessionFilePath = Path.Combine(_hl7CasesFolder, "TestAccession");
            File.Create(testManualAccessionFilePath).Close();

            // Act
            _broker.AddNewCasesFromHl7ProcessFolderToDb();

            // Assert
            pendingCases = _agentConsoleRepository.GetPendingCases();
            var hl7PendingCases = pendingCases
                .Where(c => c.Accession == "TestAccession" && c.AdditionMethod == "HL7").ToList();
            Assert.IsTrue(hl7PendingCases.Count == 1, "There should only be one TestAccession added by HL7 in database!");
        }

        [TestMethod]
        public void ProcessSampleCaseAddedByHl7()
        {
            // Arrange
            // Make sure there is no files in HL7Process folder
            Assert.IsTrue(Directory.GetFiles(_hl7CasesFolder).Length == 0,
                $"There should be no files in [{_hl7CasesFolder}] to start this test.");

            var filepath = Path.Combine(_hl7CasesFolder, SampleAccession);
            File.Create(filepath).Close();

            // Act
            _broker.Run();

            // Assert

        }

        [TestMethod]
        public void ProcessSampleCaseAddedManually()
        {
            // Arrange
            // Make sure there is no files in ManualProcess folder
            Assert.IsTrue(Directory.GetFiles(_manualCasesFolder).Length == 0,
                $"There should be no files in [{_manualCasesFolder}] to start this test.");

            var filepath = Path.Combine(_hl7CasesFolder, SampleAccession);
            File.Create(filepath).Close();

            // Act

            // Assert

        }

        [TestMethod]
        public void ProcessSampleCaseWithNoWorkableSeries()
        {
            // Arrange

            // Act

            // Assert

        }

        [TestMethod]
        public void ProcessSampleCaseWithNoPriors()
        {
            // Arrange

            // Act

            // Assert

        }

        [TestCleanup]
        public void Cleanup()
        {
            // Delete test file added to Manual and HL7 Process folder
            var testAccessionFilePath = Path.Combine(_manualCasesFolder, "TestAccession");
            if (File.Exists(testAccessionFilePath)) File.Delete(testAccessionFilePath);

            testAccessionFilePath = Path.Combine(_hl7CasesFolder, "TestAccession");
            if (File.Exists(testAccessionFilePath)) File.Delete(testAccessionFilePath);

            var sampleAccessionHl7Filepath = Path.Combine(_hl7CasesFolder, SampleAccession);
            if (File.Exists(sampleAccessionHl7Filepath)) File.Delete(sampleAccessionHl7Filepath);

            var sampleAccessionManualFilepath = Path.Combine(_manualCasesFolder, SampleAccession);
            if (File.Exists(sampleAccessionManualFilepath)) File.Delete(sampleAccessionManualFilepath);

            // Delete test cases in DB
            var accessionsToDelete = new[] { "TestAccession", SampleAccession };
            foreach (var accession in accessionsToDelete)
                if (_agentConsoleRepository.AccessionExistsInDb(accession))
                    _agentConsoleRepository.DeleteInDbByAccession(accession);
        }

        private static IUnityContainer CreateContainerCore()
        {
            var container = new UnityContainer();

            container.RegisterType<IAgentConsoleFactory, AgentConsoleFactory>(new TransientLifetimeManager());
            container.RegisterType<IDicomNodeRepository, DicomNodeRepositoryInMemory>(new TransientLifetimeManager());
            container.RegisterType<IDicomFactory, DicomFactory>();
            container.RegisterType<IDicomServices, DicomServices>(new TransientLifetimeManager());
            container.RegisterType<IRecipeRepositoryInMemory<IRecipe>, RecipeRepositoryInMemory<Recipe>>(new TransientLifetimeManager());
            container.RegisterType<IJobBuilder, JobBuilder>(new TransientLifetimeManager());
            container.RegisterType<IJobManagerFactory, JobManagerFactory>(new TransientLifetimeManager());
            container.RegisterType<IImageProcessor, ImageProcessor>(new TransientLifetimeManager());
            container.RegisterType<IImageConverter, ImageConverter>(new TransientLifetimeManager());
            container.RegisterType<IValueComparer, ValueComparer>(new TransientLifetimeManager());

            return container;
        }
        private static Broker GetBroker(IUnityContainer container)
        {
            var dicomNodeRepo = container.Resolve<IDicomNodeRepository>();
            var recipeRepositoryInMemory = container.Resolve<IRecipeRepositoryInMemory<IRecipe>>();
            var jobBuilder = container.Resolve<IJobBuilder>();
            var agentConsoleFactory = container.Resolve<IAgentConsoleFactory>();
            return new Broker(dicomNodeRepo, recipeRepositoryInMemory, jobBuilder, agentConsoleFactory);
        }
    }
}