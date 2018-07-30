using CAPI.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CAPI.Tests.Common.Extensions
{
    [TestClass]
    public class Statistics
    {
        [TestInitialize]
        public void TestInit()
        {
        }

        [TestMethod]
        public void Normalize()
        {
            // Arrange
            var array = new float[] { 130, 110, 70, 100, 59, 104, 109, 101, 118, 101 };

            // Act
            array.Normalize(100 / 2, 100 / 8);

            // Assert
            Assert.AreEqual(array.Mean(), 50.000001335144042);
            Assert.AreEqual(array.StandardDeviation(), 12.000000628233449);
        }

        [TestMethod]
        public void Trim()
        {
            // Arrange
            var array = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // Act
            array.Trim(4, 7);

            // Assert
            Assert.AreEqual(array.Min(), 4);
            Assert.AreEqual(array.Max(), 7);
        }
    }
}