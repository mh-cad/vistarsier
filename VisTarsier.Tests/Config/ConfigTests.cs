using VisTarsier.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VisTarsier.Tests.Config
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void DefaultConfig()
        {
            _ = CapiConfig.GenerateDefault(); 
        }

        [TestMethod]
        public void ConfFile()
        {
            // Delete any prior config files. 
            var confPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (System.IO.File.Exists(confPath)) System.IO.File.Delete(confPath);

            // The first read will create a new default config file.
            var generated = CapiConfig.GetConfig();

            // The second read will read the created file.
            var stored = CapiConfig.GetConfig();

            // Check that we're reading/writing okay
            Assert.IsTrue(generated.AgentDbConnectionString.Equals(stored.AgentDbConnectionString));
            Assert.IsTrue(generated.Binaries.antsRegistration.Equals(stored.Binaries.antsRegistration));
            Assert.IsTrue(generated.Binaries.bfc.Equals(stored.Binaries.bfc));
            Assert.IsTrue(generated.Binaries.bse.Equals(stored.Binaries.bse));
            Assert.IsTrue(generated.Binaries.dcm2niix.Equals(stored.Binaries.dcm2niix));
            Assert.IsTrue(generated.Binaries.img2dcm.Equals(stored.Binaries.img2dcm));
            Assert.IsTrue(generated.Binaries.N4BiasFieldCorrection.Equals(stored.Binaries.N4BiasFieldCorrection));
            Assert.IsTrue(generated.Binaries.reformatx.Equals(stored.Binaries.reformatx));
            Assert.IsTrue(generated.Binaries.registration.Equals(stored.Binaries.registration));
            Assert.IsTrue(generated.Binaries.N4BiasFieldCorrection.Equals(stored.Binaries.N4BiasFieldCorrection));
            Assert.IsTrue(generated.DefaultRecipePath.Equals(stored.DefaultRecipePath));
            Assert.IsTrue(generated.DicomConfig.LocalNode.AeTitle.Equals(stored.DicomConfig.LocalNode.AeTitle));
            Assert.IsTrue(generated.DicomConfig.RemoteNodes.Count == stored.DicomConfig.RemoteNodes.Count);
            Assert.IsTrue(generated.Hl7ProcessPath.Equals(stored.Hl7ProcessPath));
            Assert.IsTrue(generated.ImagePaths.ImageRepositoryPath.Equals(stored.ImagePaths.ImageRepositoryPath));
            Assert.IsTrue(generated.ImagePaths.PriorReslicedDicomSeriesDescription.Equals(stored.ImagePaths.PriorReslicedDicomSeriesDescription));
            Assert.IsTrue(generated.ImagePaths.ResultsDicomSeriesDescription.Equals(stored.ImagePaths.ResultsDicomSeriesDescription));
            Assert.IsTrue(generated.ProcessCasesAddedByHL7  == stored.ProcessCasesAddedByHL7);
            Assert.IsTrue(generated.ProcessCasesAddedManually  == stored.ProcessCasesAddedManually);
            Assert.IsTrue(generated.RunInterval.Equals(stored.RunInterval));
        }
    }
}
