using System;
using System.IO;
using VisTarsier.Config;
using VisTarsier.Dicom;
using VisTarsier.Dicom.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace VisTarsier.Tests.Dicom
{
    [TestClass]
    public class DicomServiceTest
    {
        //static bool _canConnect = true;

        //[ClassInitialize]
        //public static void Init(TestContext tc)
        //{
        //    if(_config == null) _config = CapiConfig.GetConfig().DicomConfig;
        //    try { getSource().CheckRemoteNodeAvailability(); } catch { _canConnect = false; }
        //    try { getDest().CheckRemoteNodeAvailability(); } catch { _canConnect = false; }
        //}

        [TestMethod]
        public void CheckConfig()
        {
            // First we need to check that we can get a connection...
            var config = CapiConfig.GetConfig().DicomConfig;

            if (config.LocalNode == null)
            {
                //_canConnect = false;
                Assert.Inconclusive("Config has no local node.");
            }
            else if (config.RemoteNodes == null || config.RemoteNodes.Count > 0)
            {
                //_canConnect = false;
                Assert.Inconclusive("Config has no remote nodes.");
            }

            var recipe = GetDefaultRecipe();
            if (recipe == null || recipe.OutputSettings.DicomDestinations == null || recipe.OutputSettings.DicomDestinations.Count < 1)
            {
                //_canConnect = false;
                Assert.Inconclusive("Recipe contains no dicom destinations.");
            }
        }

        [TestMethod]
        public void CheckConnection()
        {
            try { GetSource().CheckRemoteNodeAvailability(); }
            catch { Assert.Inconclusive("Could not connect to remote service for Source AET."); }
            try { GetDest().CheckRemoteNodeAvailability(); }
            catch { Assert.Inconclusive("Could not connect to remote service for Destination AET."); }
        }

        private static Recipe GetDefaultRecipe()
        {
            var path = CapiConfig.GetConfig().DefaultRecipePath;
            if (!File.Exists(path))
                throw new FileNotFoundException($"Unable to locate default recipe file in following path: [{path}]");
            var recipeText = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Recipe>(recipeText);
        }

        private static IDicomService GetSource()
        {
            var config = CapiConfig.GetConfig().DicomConfig;
            var source = GetDefaultRecipe().SourceAet;
            var remoteNode = config.RemoteNodes.Find((node) => node.AeTitle.ToUpper().Equals(source.ToUpper()));
            if (remoteNode == null) remoteNode = config.RemoteNodes[0];

            return new DicomService(config.LocalNode, remoteNode);
        }

        private static IDicomService GetDest()
        {
            var config = CapiConfig.GetConfig().DicomConfig;
            var source = GetDefaultRecipe().OutputSettings.DicomDestinations[0];
            var remoteNode = config.RemoteNodes.Find((node) => node.AeTitle.ToUpper().Equals(source.ToUpper()));
            if (remoteNode == null) remoteNode = config.RemoteNodes[0];

            return new DicomService(config.LocalNode, remoteNode);
        }
    }
}
