using System;
using System.IO;
using VisTarsier.Common;
using VisTarsier.Dicom;
using VisTarsier.Dicom.Abstractions;
using VisTarsier.Dicom.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisTarsier.Config;

namespace VisTarsier.Tests.Dicom
{
    [TestClass]
    public class DicomFilesTest
    {
        string _tmpFolder;

        [TestInitialize]
        public void TestInit()
        {
            _tmpFolder = Path.Combine(Helper.GetTestResourcesPath(), "tmp");
            FileSystem.DirectoryExistsIfNotCreate(_tmpFolder);
        }

        [TestMethod]
        public void BmpTests()
        {
            var bmpFolder = Path.Combine(Helper.GetTestResourcesPath(), "bmp");
            var bmpFile = Path.Combine(bmpFolder, "test.bmp");
            var dcmFolder = Path.Combine(_tmpFolder, "newdicomfolder");
            var dcmFile = Path.Combine(dcmFolder, "testdicomfile.dcm");

            FileSystem.DirectoryExistsIfNotCreate(dcmFolder);

            // Convert our bmp file to a dcm file.
            DicomFileOps.ConvertBmpToDicom(bmpFile, dcmFile);
            // Check that we can read it as a dicom file. 
            Assert.IsTrue(File.Exists(dcmFile), "Could not convert BMP to DCM.");
            try { _ = DicomFileOps.GetDicomTags(dcmFile); }
            catch { Assert.Fail("Could not read output DCM file."); }

            // Same deal, but this time it will convert all files from .bmp to a similarly named file w/ no extension.
            DicomFileOps.ConvertBmpsToDicom(bmpFolder, dcmFolder, SliceType.Axial, dcmFolder);
            // Check that it did that.
            Assert.IsTrue(File.Exists(Path.Combine(dcmFolder, "test")), "Could not convert BMP to DCM.");
            try { _ = DicomFileOps.GetDicomTags(Path.Combine(dcmFolder, "test")); }
            catch { Assert.Fail("Could not read output DCM file."); }

            //// Another similar method, but we're doing a single file and using adding existing tags.
            //DicomFileOps.ConvertBmpToDicomAndAddToExistingFolder(bmpFile, dcmFolder, "testdicomfile2.dcm");
            //// Check that it did that.
            //Assert.IsTrue(File.Exists(Path.Combine(dcmFolder, "testdicomfile2.dcm")), "Could not convert BMP to DCM.");
            //try { _ = DicomFileOps.GetDicomTags(Path.Combine(dcmFolder, "testdicomfile2.dcm")); }
            //catch { Assert.Fail("Could not read output DCM file."); }
        }

        [TestMethod]
        public void UpdateDicomHeadersTests()
        {
            var bmpFile = Path.Combine(Helper.GetTestResourcesPath(), "bmp", "test.bmp");
            var dcmFile = Path.Combine(_tmpFolder, "testdicomfile.dcm");

            DicomFileOps.ConvertBmpToDicom(bmpFile, dcmFile);
            DicomTagCollection tags = new DicomTagCollection();

            // Set PatientId
            tags.SetTagValue(tags.PatientId.GetTagValue(), new string[] { "123456" });
            // SetImage UID
            //tags.SetTagValue(tags.ImageUid.GetTagValue(), new string[] { "1.1.1" });
            try
            {
                // Only update patient data (so image data should be updated.
                //DicomFileOps.UpdateDicomHeaders(dcmFile, tags, DicomNewObjectType.NewPatient);
                var nutags = DicomFileOps.GetDicomTags(dcmFile);
                Assert.IsTrue(nutags.PatientId.Values[0].Equals("123456"), "Error reading patient ID tag.");
                // At the moment the tags will be overwritted with a generated UID. 
                // I'm not sure if there are cases where we want to modify a file in place
                // (or could this mess with the PACs system?)
                //DicomFileOps.UpdateDicomHeaders(dcmFile, tags, DicomNewObjectType.NewImage);
                //Assert.IsTrue(nutags.ImageUid.Values[0].Equals("1.1.1"), "Error reading ImageUID tag. " + nutags.ImageUid.Values[0]);
            }
            catch (Exception e)
            {
                Assert.Fail("Failed on UpdateDicomHeaders. " + e.Message);
            }
        }

        [TestMethod]
        public void GetPatientIdFromDicomFileTest()
        {
            var bmpFile = Path.Combine(Helper.GetTestResourcesPath(), "bmp", "test.bmp");
            var dcmFile = Path.Combine(_tmpFolder, "testdicomfile.dcm");

            DicomFileOps.ConvertBmpToDicom(bmpFile, dcmFile);
            DicomTagCollection tags = new DicomTagCollection();

            // Set PatientId
            tags.SetTagValue(tags.PatientId.GetTagValue(), new string[] { "123456" });
            //DicomFileOps.UpdateDicomHeaders(dcmFile, tags, DicomNewObjectType.NewPatient);

            Assert.IsTrue("123456".Equals(DicomFileOps.GetPatientIdFromDicomFile(dcmFile)));
        }

        [TestMethod]
        public void GenerateSeriesHeadersForAllFilesTest()
        {
            var bmpFile = Path.Combine(Helper.GetTestResourcesPath(), "bmp", "test.bmp");
            var dcmFile = Path.Combine(_tmpFolder, "testdicomfile.dcm");

            DicomFileOps.ConvertBmpToDicom(bmpFile, dcmFile);
            var oldSeriesUid = DicomFileOps.GetDicomTags(dcmFile).SeriesInstanceUid.Values[0];

            // Generate new series ID
            DicomFileOps.GenerateSeriesHeaderForAllFiles(new string[] { dcmFile }, new DicomTagCollection());

            Assert.IsFalse(oldSeriesUid.Equals(DicomFileOps.GetDicomTags(dcmFile).SeriesInstanceUid.Values[0]), "Error generating new Series UID");
        }

        [TestMethod]
        public void UpdateImagePositionFromReferenceSeriesTests()
        {
            var bmpFile = Path.Combine(Helper.GetTestResourcesPath(), "bmp", "test.bmp");
            FileSystem.DirectoryExistsIfNotCreate(Path.Combine(_tmpFolder, "1"));
            FileSystem.DirectoryExistsIfNotCreate(Path.Combine(_tmpFolder, "2"));
            var dcmFile = Path.Combine(_tmpFolder, "1", "testdicomfile.dcm");
            var dcmFile2 = Path.Combine(_tmpFolder, "2", "testdicomfile.dcm");

            DicomFileOps.ConvertBmpToDicom(bmpFile, dcmFile);
            DicomFileOps.ConvertBmpToDicom(bmpFile, dcmFile2);
            DicomTagCollection tags = new DicomTagCollection();

            // Setup the basic thing...
            //tags.SetTagValue(tags.SliceLocation.GetTagValue(), new string[] { "somewhere" });
            //DicomFileOps.UpdateDicomHeaders(dcmFile, tags, DicomNewObjectType.NewPatient);
            //tags.SetTagValue(tags.SliceLocation.GetTagValue(), new string[] { "nowhere" });
            //DicomFileOps.UpdateDicomHeaders(dcmFile2, tags, DicomNewObjectType.NewPatient);

            // Check that we set everything up correctly...
            Assert.IsTrue(DicomFileOps.GetDicomTags(dcmFile).SliceLocation.Values[0].Equals("somewhere"), "Failed to setup.");
            Assert.IsTrue(DicomFileOps.GetDicomTags(dcmFile2).SliceLocation.Values[0].Equals("nowhere"), "Failed to setup.");

            // Update things...
            DicomFileOps.UpdateImagePositionFromReferenceSeries(new string[] { dcmFile }, new string[] { dcmFile2 });

            // Check that things were updated.
            Assert.IsTrue(DicomFileOps.GetDicomTags(dcmFile).SliceLocation.Values[0].Equals("nowhere"), "Failed to update.");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Directory.Delete(_tmpFolder, true);
        }
    }
}
