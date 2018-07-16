using CAPI.Dicom;
using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using Unity;
using Unity.Lifetime;
using Unity.log4net;

namespace CAPI.Tests.Dicom
{
    [TestClass]
    public class DicomServices
    {
        private IDicomServices _dicomServices;
        private IDicomFactory _dicomFactory;
        private IDicomNode _localNode;
        private IDicomNode _remoteNode;
        private string _testObjectsPath;
        private static string _testResources;
        private const string ColorMapPosFolderRelPath = @"MF-PC\ColorMapPosDicom";
        private const string ColorMapNegFolderRelPath = @"MF-PC\ColorMapNegDicom";
        private const string OutDicomRelPath = @"OutDicom";
        private const string TestDicomRelativePath = "Dicom\\DicomFile1";
        private const string TestDicomUpdatedTagsRelativePath = "Dicom\\DicomFile1_UpdatedTags";

        [TestInitialize]
        public void TestInit()
        {
            _testObjectsPath = GetTestObjectsPath();
            var container = CreateContainerCore();
            _dicomFactory = container.Resolve<IDicomFactory>();
            _dicomServices = container.Resolve<IDicomServices>();
            _localNode = GetLocalDicomNode();
            _remoteNode = GetRemoteDicomNode();
        }
        private static string GetTestObjectsPath()
        {
            _testResources = CAPI.Common.Config.Helper.GetTestResourcesPath();

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
                _dicomFactory.CreateDicomNode("", localNodeAet, localNodeIp, int.Parse(localNodePort));
        }
        private IDicomNode GetRemoteDicomNode()
        {
            var remoteNodeAet = Environment.GetEnvironmentVariable("DcmNodeAET_Remote", EnvironmentVariableTarget.User);
            var remoteNodeIp = Environment.GetEnvironmentVariable("DcmNodeIP_Remote", EnvironmentVariableTarget.User);
            var remoteNodePort = Environment.GetEnvironmentVariable("DcmNodePort_Remote", EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(remoteNodeAet)) return null;
            if (string.IsNullOrEmpty(remoteNodeIp)) return null;

            return string.IsNullOrEmpty(remoteNodePort) ? null :
                _dicomFactory.CreateDicomNode("", remoteNodeAet, remoteNodeIp, int.Parse(remoteNodePort));
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

            var dicomTagsFile = _dicomServices.GetDicomTags(dicomFilePath);

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

            var dicomTags = _dicomServices.GetDicomTags(testDicomFile);

            _dicomServices.UpdateDicomHeaders(filePathToUpdate, new DicomTagCollection(), DicomNewObjectType.NewStudy);

            var updatedDicomTags = _dicomServices.GetDicomTags(filePathToUpdate);

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
            var dicomTags = _dicomFactory.CreateDicomTagCollection();
            dicomTags.SeriesDescription.Values = new[] { "CAPI Decreased Signal" };

            var dicomServices = _dicomFactory.CreateDicomServices();
            dicomServices.UpdateSeriesHeadersForAllFiles(dicomFiles, dicomTags);

            Assert.Fail();
        }

        [TestMethod]
        public void ConvertBmpToDicom()
        {
            var bmpFolderPath = Path.Combine(_testResources, @"RgbBmps-2018R0101850-1");
            var dicomFolderPath = Path.Combine(_testResources, OutDicomRelPath);
            var headersFolder = Path.Combine(_testResources, @"Fixed2\Dicom");

            var dicomServices = _dicomFactory.CreateDicomServices();
            dicomServices.ConvertBmpsToDicom(bmpFolderPath, dicomFolderPath, headersFolder);

            var dicomTags = _dicomFactory.CreateDicomTagCollection();
            dicomTags.SeriesDescription.Values = new[] { "CAPI Modified Signal" };
            var dicomFiles = Directory.GetFiles(dicomFolderPath);
            dicomServices.UpdateSeriesHeadersForAllFiles(dicomFiles, dicomTags);

            throw new NotImplementedException();
        }

        [TestMethod]
        public void GetDicomStudiesForPatientId()
        {
            if (_remoteNode.AeTitle != "RMHSYNSCP") return;
            const string testPatientId = "1200633";
            var studies = _dicomServices.GetStudiesForPatientId(testPatientId, _localNode, _remoteNode);
            var dicomStudies = studies as IDicomStudy[] ?? studies.ToArray();
            Assert.IsTrue(dicomStudies.Length > 9, $"Less than 9 studies found for patient id: {testPatientId}{Environment.NewLine}" +
                                               $"Count:{dicomStudies.Length }");
        }

        private static IUnityContainer CreateContainerCore()
        {
            var container = (UnityContainer)new UnityContainer()
                .AddNewExtension<Log4NetExtension>();

            container.RegisterType<IDicomServices, CAPI.Dicom.DicomServices>(new TransientLifetimeManager());
            container.RegisterType<IDicomFactory, DicomFactory>(new TransientLifetimeManager());
            container.RegisterType<IDicomNode, DicomNode>(new TransientLifetimeManager());

            return container;
        }
    }
}