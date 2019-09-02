using VisTarsier.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

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
            Assert.IsTrue(generated.ImagePaths.ResultsDicomSeriesDescription.Equals(stored.ImagePaths.ResultsDicomSeriesDescription));
            Assert.IsTrue(generated.ProcessCasesAddedByHL7  == stored.ProcessCasesAddedByHL7);
            Assert.IsTrue(generated.ProcessCasesAddedManually  == stored.ProcessCasesAddedManually);
            Assert.IsTrue(generated.RunInterval.Equals(stored.RunInterval));
        }

        [TestMethod]
        public void RecipeRW()
        {
            Recipe recipe = new Recipe()
            {
                SourceAet = "SOURCE_AET",
                PatientId = "",
                PatientFullName = "",
                PatientBirthDate = "",
                CurrentSeriesDicomFolder = "D:/temp/current/",
                CurrentAccession = "ABC123",
                CurrentSeriesCriteria = new List<SeriesSelectionCriteria>(),
                PriorSeriesDicomFolder = "D:/temp/prior/",
                PriorAccession = "ABC122",
                PriorSeriesCriteria = new List<SeriesSelectionCriteria>(),
                ExtractBrain = true,
                RegisterTo = Recipe.RegisterToOption.PRIOR,
                BiasFieldCorrection = false,
                
                CompareSettings = new CompareSettings()
                {
                    BackgroundThreshold = 10,
                    MinRelevantStd = -1,
                    MaxRelevantStd = 5,
                    MinChange = 0.8f,
                    MaxChange = 5,
                    CompareDecrease = false,
                    CompareIncrease = true,
                    GenerateHistogram = true
                },
                OutputSettings = new OutputSettings()
                {
                    ResultsDicomSeriesDescription = "VT Results",
                    ReslicedDicomSeriesDescription = "Resliced",
                    FilesystemDestinations = new List<string>(),
                    OnlyCopyResults = false,
                    DicomDestinations = new List<string>(),
                }
            };

            recipe.CurrentSeriesCriteria.Add(new SeriesSelectionCriteria());
            recipe.OutputSettings.DicomDestinations.Add("The CLOUD!");

            var recipestring = JsonConvert.SerializeObject(recipe, Formatting.Indented);

            var recipe2 = JsonConvert.DeserializeObject<Recipe>(recipestring);

            Assert.IsTrue(recipe2.SourceAet.Equals("SOURCE_AET"));
            Assert.IsTrue(recipe2.PatientId.Equals(""));
            Assert.IsTrue(recipe2.PatientFullName.Equals(""));
            Assert.IsTrue(recipe2.PatientBirthDate.Equals(""));
            Assert.IsTrue(recipe2.CurrentSeriesDicomFolder.Equals("D:/temp/current/"));
            Assert.IsTrue(recipe2.CurrentAccession.Equals("ABC123"));
            Assert.IsTrue(recipe2.CurrentSeriesCriteria.Count == 1);
            Assert.IsTrue(recipe2.PriorSeriesDicomFolder.Equals("D:/temp/prior/"));
            Assert.IsTrue(recipe2.PriorAccession.Equals("ABC122"));
            Assert.IsTrue(recipe2.PriorSeriesCriteria.Count == 0);
            Assert.IsTrue(recipe2.ExtractBrain);
            Assert.IsTrue(recipe2.RegisterTo == Recipe.RegisterToOption.PRIOR);
            Assert.IsFalse(recipe2.BiasFieldCorrection);
            Assert.IsTrue(recipe2.CompareSettings.BackgroundThreshold == 10);
            Assert.IsTrue(recipe2.CompareSettings.MinRelevantStd == -1);
            Assert.IsTrue(recipe2.CompareSettings.MaxRelevantStd == 5);
            Assert.IsTrue(recipe2.CompareSettings.MinChange == 0.8f);
            Assert.IsTrue(recipe2.CompareSettings.MaxChange == 5);
            Assert.IsFalse(recipe2.CompareSettings.CompareDecrease);
            Assert.IsTrue(recipe2.CompareSettings.CompareIncrease);
            Assert.IsTrue(recipe2.CompareSettings.GenerateHistogram);
            Assert.IsTrue(recipe2.OutputSettings.ResultsDicomSeriesDescription.Equals("VT Results"));
            Assert.IsTrue(recipe2.OutputSettings.ReslicedDicomSeriesDescription.Equals("Resliced"));
            Assert.IsTrue(recipe2.OutputSettings.FilesystemDestinations.Count == 0);
            Assert.IsFalse(recipe2.OutputSettings.OnlyCopyResults);
            Assert.IsTrue(recipe2.OutputSettings.DicomDestinations[0].Equals("The CLOUD!"));

            File.WriteAllText("D:/recipe.json", recipestring);
            //Assert.IsTrue(recipe.CurrentAccession.Equals("ABC123"));
        }
    }
}
