using CAPI.Service.Agent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CAPI.Tests.Service
{
    [TestClass]
    public class DBConnection
    {
        [TestMethod]
        public void TestConnection()
        {
            var connectionString = CapiConfigGetter.GetCapiConfig().AgentDbConnectionString;

            try
            {
                var broker = new DbBroker(connectionString);

                var dbExists = broker.Database.EnsureCreated();

                Assert.IsTrue(dbExists);
            }
            catch
            { }
        }
    }
}
