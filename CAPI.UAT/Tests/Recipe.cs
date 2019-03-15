using CAPI.Agent;
using CAPI.Common.Config;

namespace CAPI.UAT.Tests
{
    public class Recipe : IUatTest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SuccessMessage { get; set; }
        public string FailureMessage { get; set; }
        public string TestGroup { get; set; }
        public CapiConfig CapiConfig { get; set; }
        public AgentRepository Context { get; set; }

        public Recipe()
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
