using CAPI.Common.Abstractions.Config;

namespace CAPI.Common.Config
{
    public class TestsConfig : ITestsConfig
    {
        public string TestResourcesPath { get; set; }
    }
}
