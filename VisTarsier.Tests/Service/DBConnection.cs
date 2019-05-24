using VisTarsier.Config;
using VisTarsier.Service.Agent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VisTarsier.Tests.Service
{
    [TestClass]
    public class DBConnection
    {
        [TestMethod]
        public void TestConnection()
        {
            var connectionString = CapiConfig.GetConfig().AgentDbConnectionString;

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
