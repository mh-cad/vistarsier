using CAPI.JobManager.Abstraction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CAPI.Tests.Job
{
    [TestClass]
    public class ExtractBrainMask
    {
        private const string FixedNii = @"D:\Capi-Tests\TestsResources\Fixed\Fixed.nii";
        private const string FloatingNii = @"D:\Capi-Tests\TestsResources\Floating\Floating.nii";

        [TestInitialize]
        public void TestInit()
        {

        }

        [TestMethod]
        public void Run()
        {
            // Arrange
            var job = Helpers.JobBuilder.GetTestJob();
            job.Fixed.NiiFilePath = FixedNii;
            job.Floating.NiiFilePath = FloatingNii;

            // Act
            var integratedProcess = job.IntegratedProcesses
                .FirstOrDefault(p => p.Type == IntegratedProcessType.ExtractBrainSurface);
            job.RunExtractBrainSurfaceProcess(integratedProcess);

            // Assert


            // Clean up

        }

        [TestCleanup]
        public void TestCleanup()
        {

        }
    }
}