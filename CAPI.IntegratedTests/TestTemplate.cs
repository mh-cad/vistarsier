using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;

namespace CAPI.IntegratedTests
{
    //[TestClass]
    public class TestTemplate
    {
        [TestInitialize]
        public void TestInit()
        {
            var container = CreateContainerCore();

            // Resolve Types
        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }
        private static IUnityContainer CreateContainerCore()
        {
            var container = new UnityContainer();

            // Register Types here

            return container;
        }

        [TestMethod]
        public void TestMethod()
        {
            // Arrange

            // Act

            // Assert

        }
    }
}
