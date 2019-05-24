using CAPI.Agent_Console;
using CAPI.Agent_Console.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;
using Unity.Lifetime;

namespace CAPI.Tests.Database
{
    [TestClass]
    public class Database
    {
        private IAgentConsoleRepository _agentConsoleRepository;

        [TestInitialize]
        public void TestInit()
        {
            var container = CreateContainerCore();
            _agentConsoleRepository = container.Resolve<IAgentConsoleRepository>();
        }

        [TestMethod]
        public void CheckDbConnection()
        {
            if (!_agentConsoleRepository.DbIsAvailable()) Assert.Fail("No access to CAPI database.");
        }

        [TestMethod]
        public void CheckDbVerifiedMriTable()
        {
            if (!_agentConsoleRepository.DbTableVerifiedMriExists()) Assert.Fail("Table [VerifiedMri] is missing in CAPI database.");
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        private static IUnityContainer CreateContainerCore()
        {
            var container = new UnityContainer();
            container.RegisterType<IAgentConsoleRepository, AgentConsoleRepository>(new TransientLifetimeManager());
            return container;
        }
    }
}