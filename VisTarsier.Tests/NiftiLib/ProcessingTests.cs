using VisTarsier.NiftiLib;
using VisTarsier.NiftiLib.Processing;
using MathNet.Numerics.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using VisTarsier.Module.MS;

namespace VisTarsier.Tests.NiftiLib
{
    [TestClass]
    public class ProcessingTests
    {
        readonly string _minimalNiftiPath = $@"{Helper.GetTestResourcesPath()}/nifti/minimal.nii";
        readonly string _lrNiftiPath = $@"{Helper.GetTestResourcesPath()}/nifti/avg152T1_LR_nifti.nii";
        readonly string _lrMaskNiftiPath = $@"{Helper.GetTestResourcesPath()}/nifti/avg152T1_LR_nifti_mask.nii";

        [TestMethod]
        public void BiasCorrectionTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Assert.Inconclusive("Currently uses Windows Binaries");
            TestForChanges(BiasCorrection.AntsN4, BiasCorrection.AntsN4, _minimalNiftiPath);
        }

        [TestMethod]
        public void BrainExtractionTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Assert.Inconclusive("Currently uses Windows Binaries");
            TestForChanges(BrainExtraction.BrainSuiteBSE, BrainExtraction.BrainSuiteBSE, _lrNiftiPath);
        }

        [TestMethod]
        public void CompareTest()
        {
            // Create som Nifti objects to compare.
            var niftiA = new NiftiFloat32().ReadNifti(_minimalNiftiPath);
            var niftiB = new NiftiFloat32().ReadNifti(_minimalNiftiPath);

            // Mean and standard deviations are used to see if the change is significant, which we want it to be.
            (var mean, var stdDev) = niftiA.Voxels.Where(val => val > 10).MeanStandardDeviation();
            var change = (float)stdDev;

            // Make sure the starting values aren't background.
            niftiA.Voxels[0] = (float)mean;
            niftiA.Voxels[1] = (float)mean;

            // Increase by 1 std deviation.
            niftiB.Voxels[0] = niftiA.Voxels[0] + change;
            // Decrease by 1 std deviation.
            niftiB.Voxels[1] = niftiA.Voxels[1] - change;

            // Do a heaps compare
            var decrease = MSCompare.CompareMSLesionDecrease(niftiB, niftiA);
            var increase = MSCompare.CompareMSLesionIncrease(niftiB, niftiA);
            var changed = Compare.GatedSubract(niftiB, niftiA);

            // Check that things are working as expected (within floating point error range)
            Assert.AreEqual(decrease.Voxels[0], 0, 0.00001, "Problem with CompareMSLesionDecrease");
            Assert.AreEqual(decrease.Voxels[1], -change, 0.00001, "Problem with CompareMSLesionDecrease"); 
            Assert.AreEqual(increase.Voxels[0], change, 0.00001, "Problem with CompareMSLesionIncrease");
            Assert.AreEqual(increase.Voxels[1], 0, 0.00001, "Problem with CompareMSLesionIncrease");
            Assert.AreEqual(changed.Voxels[1], -change, 0.00001, "Problem with CompareMSLesion");
            Assert.AreEqual(changed.Voxels[0], change, 0.00001, "Problem with CompareMSLesion");

            // Check that nothing else has changed.
            var diff = 0f;
            for (int i = 2; i < niftiA.Voxels.Length; ++i)
            {
                diff += Math.Abs(decrease.Voxels[i]);
                diff += Math.Abs(increase.Voxels[i]);
                diff += Math.Abs(changed.Voxels[i]);
            }

            Assert.IsTrue(diff == 0, "We've got extra differences which we shouldn't.");
        }

        [TestMethod]
        public void NormalizationTest()
        {
            // Setup a couple of niftis. (Reading from files just makes sure we have valid headers, etc.)
            var niftiA = new NiftiFloat32().ReadNifti(_minimalNiftiPath);
            var niftiB = new NiftiFloat32().ReadNifti(_minimalNiftiPath);

            // Lets use a random element in testing! Maybe it'll pass if we get lucky!
            var rng = new Random(1000);
            for (int i = 0; i < niftiB.Voxels.Length; ++i)
            {
                niftiB.Voxels[i] = rng.Next();
            }

            // Normalize without trimming any background.
            niftiB = Normalization.ZNormalize(niftiB, niftiA, float.NegativeInfinity);

            // See how we did.
            (var mean, var stdDev) = niftiA.Voxels.MeanStandardDeviation();
            Assert.AreEqual(niftiB.Voxels.Mean(), mean, 0.000001);
            Assert.AreEqual(niftiB.Voxels.StandardDeviation(), stdDev, 0.000001);

            niftiB = Normalization.RangeNormalize(niftiA, 0, 1);
            Assert.IsTrue(niftiB.Voxels.Max() <= 1);
            Assert.IsTrue(niftiB.Voxels.Min() >= 0);
            Assert.AreEqual(niftiB.Voxels.Mean(), 0.5, 0.1); // We would expect the mean to be roughly 0.5 given a random input.

        }

        [TestMethod]
        public void RegistrationTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Assert.Inconclusive("Currently uses Windows Binaries");

            var NiftiA = new NiftiFloat32().ReadNifti(_lrMaskNiftiPath);
            var NiftiB = new NiftiFloat32().ReadNifti(_lrNiftiPath);

            // Only doing a couple of asserts here. If something horrible goes wrong, we're expecting an exception.
            // You can also check the console output to see what the tools reckon.
            _ = Registration.ANTSRegistration(NiftiA, NiftiB, (d, e) => Console.WriteLine(e.Data));
            var outFile = Registration.ANTSApplyTransforms(_lrMaskNiftiPath, _lrNiftiPath, (d, e) => Console.WriteLine(e.Data));
            Assert.IsTrue(File.Exists(outFile), "No out file for ANTSApplyTransforms");
            _ = new NiftiFloat32().ReadNifti(outFile);

            _ = Registration.CMTKRegistration(NiftiA, NiftiB, (d, e) => Console.WriteLine(e.Data));
            outFile = Registration.CMTKResliceUsingPrevious(_lrMaskNiftiPath, _lrNiftiPath, (d, e) => Console.WriteLine(e.Data));
            Assert.IsTrue(File.Exists(outFile), "No out file for CMTKResliceUsingPrevious rego");
            NiftiA.ReadNifti(outFile);
        }


        [TestMethod]
        public void ToolsTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Assert.Inconclusive("Currently uses Windows Binaries");
            // TODO :: Need DICOM test data thats open source and not private. Could make one.
        }

        private void TestForChanges(Func<string, DataReceivedEventHandler, string> func, Func<INifti<float>, DataReceivedEventHandler, INifti<float>> funcNii, string niftiPath)
        {
            // If anything's gone horribly wrong we'll probably get an exception here.
            var nii = func(niftiPath, (d, e) => Console.WriteLine(e.Data));
            var nifti = new NiftiFloat32().ReadNifti(nii);
            nifti.RecalcHeaderMinMax();

            // Quick check to see if the file exists and is sane.
            Assert.IsFalse(nifti.Header.cal_min == nifti.Header.cal_max);
            // Remove the temp file
            File.Delete(nii);
            // Read the input nifti file and run the nifti->nifti version.
            nifti.ReadNifti(niftiPath);
            var outnifti = funcNii(nifti, (d, e) => Console.WriteLine(e.Data));
            // Check that there has been some change made to the voxels.
            var diff = 0f;
            for (int i = 0; i < outnifti.Voxels.Length; ++i)
            {
                diff += Math.Abs(nifti.Voxels[i] - outnifti.Voxels[i]);
            }

            // Some change has happened.
            Assert.IsFalse(diff == 0);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            foreach (var f in Directory.GetFiles("./", "temp*.nii"))
            {
                File.Delete(f);
            }
        }
    }
}
