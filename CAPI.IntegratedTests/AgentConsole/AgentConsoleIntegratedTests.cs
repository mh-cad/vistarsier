using CAPI.Agent_Console;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CAPI.IntegratedTests.AgentConsole
{
    [TestClass]
    public class AgentConsoleIntegratedTests
    {
        [TestMethod]
        public void ManualProcessTest()
        {
            var pendingCases = Broker.GetPendingCasesFromCapiDbManuallyAdded(1000);

        }
    }
}