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
            // Create a test array
            var array = new float[] { 130, 110, 70, 100, 59, 104, 109, 101, 118, 101 };

            // Set our target mean and standard deviation
            const int targetMean = 50;
            const int targetStd = 12;
            // Normalise
            array.Normalize(targetMean, targetStd);

            // We expect some floating point errors, but we want them to be less than 10^-5
            var meanError = System.Math.Abs(array.Mean() - targetMean);
            var stdError = System.Math.Abs(array.StandardDeviation() - targetStd);
            const double maxError = 0.00001; 

            Assert.IsTrue(meanError < maxError);
            Assert.IsTrue(stdError < maxError);
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