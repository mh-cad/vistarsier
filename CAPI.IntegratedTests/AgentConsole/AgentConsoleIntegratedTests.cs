using CAPI.Agent_Console;
using CAPI.Agent_Console.Abstractions;
using CAPI.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using Unity;
using Unity.Lifetime;

namespace CAPI.IntegratedTests.AgentConsole
{
    [TestClass]
    public class AgentConsoleIntegratedTests
    {
        private IVerifiedMri _verifiedMri;

        [TestInitialize]
        public void TestInit()
        {
            // Debugger.Launch();
            var container = CreateContainerCore();
            _verifiedMri = container.Resolve<IVerifiedMri>();
        }

        [TestMethod]
        public void ManualProcessTest()
        {
            var pendingCases = Broker.GetPendingCasesFromCapiDbManuallyAdded(1000);
            // TODO1: To be implemented
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

            var accessionsInDb = _verifiedMri.GetRecentVerifiedMris(topEntriesCount);

            var accessionAlreadyInDb = accessionsInDb.Select(a => a.Accession).Contains(accession);
            if (accessionAlreadyInDb) Assert.Fail($"Accession [{accession}] already exists in DB. Please remove it and try agian.");

            var verifiedMri = new VerifiedMri // TODO1: Use IVerifiedMri
            {
                Accession = accession,
                AdditionMethod = "Unit Testing",
                Status = "Testing",
                AdditionTime = DateTime.Now
            };
            verifiedMri.InsertIntoDb();

            accessionsInDb = verifiedMri.GetRecentVerifiedMris(topEntriesCount);
            var accessionExistsInDb = accessionsInDb.Select(a => a.Accession).Contains(accession);
            if (!accessionExistsInDb) Assert.Fail($"Accession [{accession}] does not exist in DB.");

            verifiedMri.DeleteInDb();
        }

        [TestCleanup]
        public void Cleanup()
        {
            var manualProcPath = ImgProc.GetManualProcessPath();
            var testAccessionFilePath = Path.Combine(manualProcPath, "TestAccession");
            if (File.Exists(testAccessionFilePath)) File.Delete(testAccessionFilePath);
        }

        private static IUnityContainer CreateContainerCore()
        {
            var container = new UnityContainer();
            container.RegisterType<IVerifiedMri, VerifiedMri>(new TransientLifetimeManager());
            return container;
        }
    }
}