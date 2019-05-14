using CAPI.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;

namespace CAPI.Tests.Agent
{
    [TestClass]
    public class Recipe
    {

        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestMethod]
        public void GetDefaultRecipe()
        {
            // Arrange
            var recipeText = File.ReadAllText(CapiConfig.GenerateDefault().DefaultRecipePath);

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
