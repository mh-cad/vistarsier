using System;
using VisTarsier.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VisTarsier.Tests.Agent
{
    [TestClass]
    public class AgentTests
    {
        [TestMethod]
        public void CreateAgent()
        {
            VisTarsier.Service.Agent agent = new VisTarsier.Service.Agent();
            agent.Run();
        }
    }
}
