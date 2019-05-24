using CAPI.Agent_Console;
using CAPI.Agent_Console.Abstractions;
using CAPI.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Unity;
using Unity.Lifetime;

namespace CAPI.Tests.AgentConsole
{
    [TestClass]
    public class Repository
    {
        private IAgentConsoleRepository _agentConsoleRepository;
        private IAgentConsoleFactory _agentConsoleFactory;

        [TestInitialize]
        public void TestInit()
        {
            var container = CreateContainerCore();
            _agentConsoleRepository = container.Resolve<IAgentConsoleRepository>();
            _agentConsoleFactory = container.Resolve<IAgentConsoleFactory>();
        }

        [TestMethod]
        public void AddNewCaseToDb()
        {
            var manualProcPath = ImgProc.GetManualProcessPath();

            var testAccessionFilePath = Path.Combine(manualProcPath, "TestAccession");

            if (Directory.GetFiles(manualProcPath).Length > 0)
                Assert.Fail($"Manual Processing directory [{manualProcPath}] has to be empty!");

            File.Create(testAccessionFilePath).Close();

            var allAccessions = Directory.GetFiles(manualProcPath).Select(Path.GetFileName).ToArray();
            if (allAccessions.Length != 1) Assert.Fail($"Test file was not created in [{manualProcPath}] directory.");
            var accession = allAccessions.FirstOrDefault();

            const int topEntriesCount = 1000;

            var accessionsInDb = _agentConsoleRepository.GetRecentVerifiedMris(topEntriesCount);

            var accessionAlreadyInDb = accessionsInDb.Select(a => a.Accession).Contains(accession);
            if (accessionAlreadyInDb) Assert.Fail($"Accession [{accession}] already exists in DB. Please remove it and try agian.");

            var verifiedMri = _agentConsoleFactory.CreateVerifiedMri();
            verifiedMri.Accession = accession;
            verifiedMri.AdditionMethod = "Unit Testing";
            verifiedMri.Status = "Testing";
            verifiedMri.AdditionTime = DateTime.Now;
            _agentConsoleRepository.InsertVerifiedMriIntoDb(verifiedMri);

            accessionsInDb = _agentConsoleRepository.GetRecentVerifiedMris(topEntriesCount);
            var accessionExistsInDb = accessionsInDb.Select(a => a.Accession).Contains(accession);
            if (!accessionExistsInDb) Assert.Fail($"Accession [{accession}] does not exist in DB.");
        }

        [TestMethod]
        public void GetPendingCases()
        {
            // Arrange
            // Make sure there is no pending cases in DB
            Assert.IsTrue(!_agentConsoleRepository.GetPendingCases().Any());

            // Add a test Pending Case to DB
            var testVerifiedMri = _agentConsoleFactory.CreateVerifiedMri();
            testVerifiedMri.Accession = "TestAccession";
            testVerifiedMri.Status = "Pending";
            _agentConsoleRepository.InsertVerifiedMriIntoDb(testVerifiedMri);

            // Act
            // Get pending cases from DB
            var pendingCases = _agentConsoleRepository.GetPendingCases().ToArray();

            // Assert
            // Check if added test case exists
            Assert.IsTrue(pendingCases.Length == 1, "A pending case was added in the middle of testing! Try running the test again.");
            var addedCaseFoundInDb = pendingCases[0].Accession == testVerifiedMri.Accession;

            Assert.IsTrue(addedCaseFoundInDb);
        }

        [TestMethod]
        public void GetProcessingCases()
        {
            // Arrange
            // Make sure there is no processing cases in DB
            Assert.IsTrue(!_agentConsoleRepository.GetProcessingCases().Any());

            // Add a test Processing Case to DB
            var testVerifiedMri = _agentConsoleFactory.CreateVerifiedMri();
            testVerifiedMri.Accession = "TestAccession";
            testVerifiedMri.Status = "Pending";
            _agentConsoleRepository.InsertVerifiedMriIntoDb(testVerifiedMri);

            // Act
            // Get processing cases from DB
            var processingCases = _agentConsoleRepository.GetPendingCases().ToArray();

            // Assert
            // Check if added test case exists
            Assert.IsTrue(processingCases.Length == 1, "A processing case was added in the middle of testing! Try running the test again.");
            var addedCaseFoundInDb = processingCases[0].Accession == testVerifiedMri.Accession;

            Assert.IsTrue(addedCaseFoundInDb);
        }

        [TestMethod]
        public void GetQueuedCases()
        {
            // Arrange
            // Make sure there is no queued cases in DB
            Assert.IsTrue(!_agentConsoleRepository.GetQueuedCases().Any());

            // Add a test Queued Case to DB
            var testVerifiedMri = _agentConsoleFactory.CreateVerifiedMri();
            testVerifiedMri.Accession = "TestAccession";
            testVerifiedMri.Status = "Queued";
            _agentConsoleRepository.InsertVerifiedMriIntoDb(testVerifiedMri);

            // Act
            // Get queued cases from DB
            var queuedCases = _agentConsoleRepository.GetQueuedCases().ToArray();

            // Assert
            // Check if added test case exists
            Assert.IsTrue(queuedCases.Length == 1, "A queued case was added in the middle of testing! Try running the test again.");
            var addedCaseFoundInDb = queuedCases[0].Accession == testVerifiedMri.Accession;

            Assert.IsTrue(addedCaseFoundInDb);
        }

        [TestMethod]
        public void SetStatusOfCase()
        {
            // Arrange
            // Add a test Pending Case to DB
            var testVerifiedMri = _agentConsoleFactory.CreateVerifiedMri();
            testVerifiedMri.Accession = "TestAccession";
            testVerifiedMri.Status = "Pending";
            _agentConsoleRepository.InsertVerifiedMriIntoDb(testVerifiedMri);

            // Act
            _agentConsoleRepository.SetVerifiedMriStatus(testVerifiedMri.Accession, "Testing");

            // Assert
            var testVerifiedMriFromDb = _agentConsoleRepository.GetVerifiedMriByAccession("TestAccession");
            var testStatusIsCorrect = testVerifiedMriFromDb.Status == "Testing";
            Assert.IsTrue(testStatusIsCorrect);
        }

        [TestMethod]
        public void UpdateCase()
        {
            // Arrange
            // Add a test Pending Case to DB
            var testVerifiedMri = _agentConsoleFactory.CreateVerifiedMri();
            testVerifiedMri.Accession = "TestAccession";
            testVerifiedMri.Status = "Pending";
            testVerifiedMri.AdditionMethod = "Manual";
            _agentConsoleRepository.InsertVerifiedMriIntoDb(testVerifiedMri);

            // Act
            Thread.Sleep(1000);
            var testLastModified = DateTime.Now;
            testVerifiedMri.LastModified = testLastModified;
            testVerifiedMri.Status = "Testing";
            testVerifiedMri.AdditionMethod = "HL7";

            _agentConsoleRepository.UpdateVerifiedMri(testVerifiedMri);

            // Assert
            var testVerifiedMriFromDb = _agentConsoleRepository.GetVerifiedMriByAccession("TestAccession");

            Assert.IsTrue(testVerifiedMriFromDb.Status == "Testing");
            Assert.IsTrue(testVerifiedMriFromDb.AdditionMethod == "HL7");
            var diff = testVerifiedMriFromDb.LastModified.Subtract(testLastModified).TotalMilliseconds;
            Assert.IsTrue(Math.Abs(diff) < 10);
        }

        [TestCleanup]
        public void Cleanup()
        {
            var manualProcPath = ImgProc.GetManualProcessPath();
            var testAccessionFilePath = Path.Combine(manualProcPath, "TestAccession");
            if (File.Exists(testAccessionFilePath)) File.Delete(testAccessionFilePath);

            // Delete any case in DB with accession 'TestAccession'
            var testAccessionCase = _agentConsoleFactory.CreateVerifiedMri();
            testAccessionCase.Accession = "TestAccession";
            if (_agentConsoleRepository.AccessionExistsInDb(testAccessionCase.Accession))
                _agentConsoleRepository.DeleteInDbByAccession(testAccessionCase.Accession);
        }

        private static IUnityContainer CreateContainerCore()
        {
            var container = new UnityContainer();
            container.RegisterType<IAgentConsoleRepository, AgentConsoleRepository>(new TransientLifetimeManager());
            container.RegisterType<IAgentConsoleFactory, AgentConsoleFactory>(new TransientLifetimeManager());
            return container;
        }
    }
}