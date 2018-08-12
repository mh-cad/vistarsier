using CAPI.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using Unity;

namespace CAPI.Tests.Agent
{
    [TestClass]
    public class Recipe
    {
        private IUnityContainer _unity;
        private string _testResourcesPath;
        private string _defaultRecipePath;

        [TestInitialize]
        public void TestInitialize()
        {
            _unity = Helpers.Unity.CreateContainerCore();
            _testResourcesPath = Helper.GetTestResourcesPath();
            _defaultRecipePath = $@"{_testResourcesPath}\DefaultRecipe.recipe.json";
        }

        [TestMethod]
        public void GetDefaultRecipe()
        {
            // Arrange
            var recipeText = File.ReadAllText(_defaultRecipePath);

            // Act
            var recipe = JsonConvert.DeserializeObject<CAPI.Agent.Models.Recipe>(recipeText);

            //// Assert
            Assert.IsNotNull(recipe);
        }

        [TestCleanup]
        public void TestCleanup()
        {

        }
    }
}
