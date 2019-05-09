using CAPI.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;

namespace CAPI.Tests.Agent
{
    [TestClass]
    public class Recipe
    {
        private string _testResourcesPath;
        private string _defaultRecipePath;

        [TestInitialize]
        public void TestInitialize()
        {
            _testResourcesPath = Helper.GetTestResourcesPath();
            _defaultRecipePath = $@"{_testResourcesPath}\DefaultRecipe.recipe.json";
        }

        [TestMethod]
        public void GetDefaultRecipe()
        {
            // Arrange
            var recipeText = File.ReadAllText(_defaultRecipePath);

            // Act
            var recipe = JsonConvert.DeserializeObject<Recipe>(recipeText);

            //// Assert
            Assert.IsNotNull(recipe);
        }



        [TestCleanup]
        public void TestCleanup()
        {

        }
    }
}
