using CAPI.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CAPI.Tests.Config
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void DefaultConfig()
        {
            var conf = CapiConfig.GenerateDefault(); 
        }

        [TestMethod]
        public void ConfFile()
        {
            var confPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (System.IO.File.Exists(confPath)) System.IO.File.Delete(confPath);

            var conf = CapiConfig.GetConfig();
            conf = CapiConfig.GetConfig();
        }
    }
}
