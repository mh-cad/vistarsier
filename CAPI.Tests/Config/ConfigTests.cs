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
            // Delete any prior config files. 
            var confPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (System.IO.File.Exists(confPath)) System.IO.File.Delete(confPath);

            // The first read will create a new default config file.
            _ = CapiConfig.GetConfig();

            // The second read will read the created file.
            _ = CapiConfig.GetConfig();

            // TODO ASSERT some things.
        }
    }
}
