
using CAPI.Common;
using CAPI.Config;
using CAPI.Dicom;
using CAPI.Dicom.Abstractions;
using CAPI.Dicom.Model;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CAPI.Tests.Dicom
{
    [TestClass]
    public class DicomServicesTest
    {
        private IDicomServices _dicomServices;
        private IDicomNode _localNode;
        private IDicomNode _remoteNode;

        private string _testObjectsPath;
        private static string _testResources;
        private string _tempOutputFolder;
        private string _orientationReferenceDicomFolder;
        private string _testingDicomFolderForOrientation;

        private const string ColorMapNegFolderRelPath = @"MF-PC\ColorMapNegDicom";
        private const string OutDicomRelPath = @"OutDicom";
        private const string TestDicomRelativePath = "Dicom\\DicomFile1";
        private const string TestDicomUpdatedTagsRelativePath = "Dicom\\DicomFile1_UpdatedTags";
        private const string OrientationReferenceDicomFolder = @"D:\Capi-Tests\TestsResources\Orientation-Series\P1-S1";
        private const string TestingDicomFolderForOrientation = @"D:\Capi-Tests\TestsResources\Orientation-Series\P1-S2";

        [TestInitialize]
        public void TestInit()
        {
            _testObjectsPath = GetTestObjectsPath();
            _dicomServices = new DicomServices();
            _testResources = Helper.GetTestResourcesPath();

            //_dicomConfig.ExecutablesPath = capiConfig.DicomConfig.DicomServicesExecutablesPath;

            _tempOutputFolder = Path.Combine(Helper.GetTestResourcesPath(), "tempOutput");
            if (Directory.Exists(_tempOutputFolder)) Directory.Delete(_tempOutputFolder, true);
            Directory.CreateDirectory(_tempOutputFolder);

            _orientationReferenceDicomFolder =
                Path.Combine(_tempOutputFolder, Path.GetFileName(OrientationReferenceDicomFolder));
            CAPI.Common.FileSystem.CopyDirectory(OrientationReferenceDicomFolder, _orientationReferenceDicomFolder);

            _testingDicomFolderForOrientation = Path.Combine(_tempOutputFolder, Path.GetFileName(TestingDicomFolderForOrientation));
            CAPI.Common.FileSystem.CopyDirectory(TestingDicomFolderForOrientation, _testingDicomFolderForOrientation);

            _localNode = GetLocalDicomNode();
            _remoteNode = GetRemoteDicomNode();
        }
        private static string GetTestObjectsPath()
        {
           // _testResources = Helper.GetTestResourcesPath();

            var binPath = Directory.GetParent(Environment.CurrentDirectory).FullName;
            var projectPath = Directory.GetParent(binPath).FullName;
            var projectsParentPath = Directory.GetParent(projectPath).FullName;
            return Path.Combine(projectsParentPath, "TestObjects");
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            // Delete Updated Tag File
            var updatedTagFile = Path.Combine(_testObjectsPath, TestDicomUpdatedTagsRelativePath);
            if (File.Exists(updatedTagFile)) File.Delete(updatedTagFile);

            // Delete temp output folder
            if (Directory.Exists(_tempOutputFolder)) Directory.Delete(_tempOutputFolder, true);
        }

        [TestMethod]
        public void DicomNodesEnvironmentVariables()
        {
            // Act
            var localNodeAet = Environment.GetEnvironmentVariable("DcmNodeAET_Local", EnvironmentVariableTarget.User);
            var localNodeIp = Environment.GetEnvironmentVariable("DcmNodeIP_Local", EnvironmentVariableTarget.User);
            var localNodePort = Environment.GetEnvironmentVariable("DcmNodePort_Local", EnvironmentVariableTarget.User);

            var remoteNodeAet = Environment.GetEnvironmentVariable("DcmNodeAET_Remote", EnvironmentVariableTarget.User);
            var remoteNodeIp = Environment.GetEnvironmentVariable("DcmNodeIP_Remote", EnvironmentVariableTarget.User);
            var remoteNodePort = Environment.GetEnvironmentVariable("DcmNodePort_Remote", EnvironmentVariableTarget.User);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(localNodeAet));
            Assert.IsFalse(string.IsNullOrEmpty(localNodeIp));
            Assert.IsTrue(int.TryParse(localNodePort, out _));
            if (localNodePort == null) Assert.Fail();

            Assert.IsFalse(string.IsNullOrEmpty(remoteNodeAet));
            Assert.IsFalse(string.IsNullOrEmpty(remoteNodeIp));
            Assert.IsTrue(int.TryParse(remoteNodePort, out _));
            if (remoteNodePort == null) Assert.Fail();
        }
        private IDicomNode GetLocalDicomNode()
        {
            var localNodeAet = Environment.GetEnvironmentVariable("DcmNodeAET_Local", EnvironmentVariableTarget.User);
            var localNodeIp = Environment.GetEnvironmentVariable("DcmNodeIP_Local", EnvironmentVariableTarget.User);
            var localNodePort = Environment.GetEnvironmentVariable("DcmNodePort_Local", EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(localNodeAet)) return null;
            if (string.IsNullOrEmpty(localNodeIp)) return null;

            return string.IsNullOrEmpty(localNodePort) ? null :
                new DicomNode("", localNodeAet, localNodeIp, int.Parse(localNodePort));
        }
        private IDicomNode GetRemoteDicomNode()
        {
            var remoteNodeAet = Environment.GetEnvironmentVariable("DcmNodeAET_Remote", EnvironmentVariableTarget.User);
            var remoteNodeIp = Environment.GetEnvironmentVariable("DcmNodeIP_Remote", EnvironmentVariableTarget.User);
            var remoteNodePort = Environment.GetEnvironmentVariable("DcmNodePort_Remote", EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(remoteNodeAet)) return null;
            if (string.IsNullOrEmpty(remoteNodeIp)) return null;

            return string.IsNullOrEmpty(remoteNodePort) ? null :
                new DicomNode("", remoteNodeAet, remoteNodeIp, int.Parse(remoteNodePort));
        }

        [TestMethod]
        public void RemoteDicomNodeConnection()
        {
            try
            {
                _dicomServices.CheckRemoteNodeAvailability(_localNode, _remoteNode);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Dicom ping failed to {_remoteNode.LogicalName}: " + ex.Message);
            }
        }

        [TestMethod]
        public void DicomNodeEnvironmentVariablesAreDefined()
        {
            var localNodeAet = Environment.GetEnvironmentVariable("DcmNodeAET_Local", EnvironmentVariableTarget.User);
            Assert.IsNotNull(localNodeAet);
            var localNodeIp = Environment.GetEnvironmentVariable("DcmNodeIP_Local", EnvironmentVariableTarget.User);
            Assert.IsNotNull(localNodeIp);
            var localNodePort = Environment.GetEnvironmentVariable("DcmNodePort_Local", EnvironmentVariableTarget.User);
            Assert.IsNotNull(localNodePort);

            var remoteNodeAet = Environment.GetEnvironmentVariable("DcmNodeAET_Remote", EnvironmentVariableTarget.User);
            Assert.IsNotNull(remoteNodeAet);
            var remoteNodeIp = Environment.GetEnvironmentVariable("DcmNodeIP_Remote", EnvironmentVariableTarget.User);
            Assert.IsNotNull(remoteNodeIp);
            var remoteNodePort = Environment.GetEnvironmentVariable("DcmNodePort_Remote", EnvironmentVariableTarget.User);
            Assert.IsNotNull(remoteNodePort);
        }

        [TestMethod]
        public void GetDicomTags()
        {
            var dicomFilePath = Path.Combine(_testObjectsPath, TestDicomRelativePath);

            var dicomTagsFile = DicomFileOps.GetDicomTags(dicomFilePath);

            Assert.IsNotNull(dicomTagsFile);
            Assert.IsTrue(dicomTagsFile.StudyInstanceUid.Values.Length > 0);
            Assert.IsFalse(string.IsNullOrEmpty(dicomTagsFile.SeriesDescription.Values[0]));
        }

        [TestMethod]
        public void UpdateDicomTagsOnFile()
        {
            var testDicomFile = Path.Combine(_testObjectsPath, TestDicomRelativePath);
            var filePathToUpdate = Path.Combine(_testObjectsPath, TestDicomUpdatedTagsRelativePath);
            File.Copy(testDicomFile, filePathToUpdate, true);

            var dicomTags = DicomFileOps.GetDicomTags(testDicomFile);

            DicomFileOps.UpdateDicomHeaders(filePathToUpdate, new DicomTagCollection(), dicomNewObjectType: DicomNewObjectType.NewStudy);

            var updatedDicomTags = DicomFileOps.GetDicomTags(filePathToUpdate);

            Assert.IsFalse(dicomTags.StudyInstanceUid.Values[0] == updatedDicomTags.StudyInstanceUid.Values[0],
                $"Dicom tags have not been updated. StudyInstanceUid has not changed.{Environment.NewLine}" +
                $"Original Study Instance Uid: { dicomTags.StudyInstanceUid.Values[0] }{Environment.NewLine}" +
                $"Updated Study Instance Uid:  { updatedDicomTags.StudyInstanceUid.Values[0] }");
        }

        [TestMethod]
        public void UpdateDicomTagsOnSeries()
        {
            var colorMapPosFullPath = Path.Combine(_testResources, ColorMapNegFolderRelPath);
            var dicomFiles = Directory.GetFiles(colorMapPosFullPath);
            var dicomTags = new DicomTagCollection();
            dicomTags.SeriesDescription.Values = new[] { "CAPI Decreased Signal" };

            var dicomServices = new DicomServices();
            DicomFileOps.UpdateSeriesHeadersForAllFiles(dicomFiles, dicomTags);

            throw new NotImplementedException("Assert to be implemented");
        }

        [TestMethod]
        public void ConvertBmpToDicom()
        {
            var bmpFolderPath = Path.Combine(_testResources, @"RgbBmps-2018R0101850-1");
            var dicomFolderPath = Path.Combine(_testResources, OutDicomRelPath);
            var headersFolder = Path.Combine(_testResources, @"Fixed2\Dicom");

            var dicomServices = new DicomServices();
            DicomFileOps.ConvertBmpsToDicom(bmpFolderPath, dicomFolderPath, SliceType.Sagittal, headersFolder);

            var dicomTags = new DicomTagCollection();
            dicomTags.SeriesDescription.Values = new[] { "CAPI Modified Signal" };
            var dicomFiles = Directory.GetFiles(dicomFolderPath);
            DicomFileOps.UpdateSeriesHeadersForAllFiles(dicomFiles, dicomTags);

            throw new NotImplementedException("Assert to be implemented");
        }


        [TestMethod]
        public void UpdateImagePositionFromReferenceSeries()
        {
            var refDicomFiles = Directory.GetFiles(_orientationReferenceDicomFolder);
            var targetDicomFiles = Directory.GetFiles(_testingDicomFolderForOrientation);

            DicomFileOps.UpdateImagePositionFromReferenceSeries(targetDicomFiles, refDicomFiles);
        }

        [TestMethod]
        public void SaveSeriesToLocalDiskTest()
        {
            FileSystem.DirectoryExistsIfNotCreate("temp");
        }
    }
}