using CAPI.Service.Agent;
using CAPI.Config;

namespace CAPI.UAT.Tests
{
    public class TestTemplate : IUatTest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SuccessMessage { get; set; }
        public string FailureMessage { get; set; }
        public string TestGroup { get; set; }
        public CapiConfig CapiConfig { get; set; }
        public AgentRepository Context { get; set; }

        public TestTemplate()
        {
            Name = "";
            Description = "";
            SuccessMessage = "";
            FailureMessage = "";
            TestGroup = "";
        }

        public bool Run()
        {
            return false;
        }

        public void FailureResolution()
        {

        }
    }
}
