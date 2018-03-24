using CAPI.Agent_Console;
using CAPI.Agent_Console.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Unity;
using Unity.Lifetime;

namespace CAPI.IntegratedTests.Db
{
    [TestClass]
    public class DatabaseIntegratedTests
    {
        private IVerifiedMri _verifiedMri;

        [TestInitialize]
        public void TestInit()
        {
            Debugger.Launch();
            var container = CreateContainerCore();
            _verifiedMri = container.Resolve<IVerifiedMri>();
        }

        [TestMethod]
        public void CheckDbConnection()
        {
            if (!_verifiedMri.DbIsAvailable()) Assert.Fail("No access to CAPI database.");
        }

        [TestMethod]
        public void CheckDbVerifiedMriTable()
        {
            if (!_verifiedMri.DbTableVerifiedMriExists()) Assert.Fail("Table [VerifiedMri] is missing in CAPI database.");
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        private static IUnityContainer CreateContainerCore()
        {
            var container = new UnityContainer();
            container.RegisterType<IVerifiedMri, VerifiedMri>(new TransientLifetimeManager());
            return container;
        }
    }
}