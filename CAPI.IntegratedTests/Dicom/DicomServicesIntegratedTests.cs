using CAPI.Dicom;
using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using Unity;
using Unity.Lifetime;

namespace CAPI.IntegratedTests.Dicom
{
    [TestClass]
    public class DicomServicesIntegratedTests
    {
        private IDicomServices _dicomServices;
        private IDicomFactory _dicomFactory;
        private IDicomNode _localNode;
        private IDicomNode _remoteNode;
        private string _testObjectsPath;

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
            var binPath = Directory.GetParent(Environment.CurrentDirectory).FullName;
            var projectPath = Directory.GetParent(binPath).FullName;
            var projectsParentPath = Directory.GetParent(projectPath).FullName;
            return Path.Combine(projectsParentPath, "TestObjects");
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
            Assert.IsTrue(int.TryParse(localNodePort, out var testInt));
            if (localNodePort == null) Assert.Fail();

            Assert.IsFalse(string.IsNullOrEmpty(remoteNodeAet));
            Assert.IsFalse(string.IsNullOrEmpty(remoteNodeIp));
            Assert.IsTrue(int.TryParse(remoteNodePort, out testInt));
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
                Assert.Fail("Dicom ping failed to SYNAPSE: " + ex.Message);
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
            var dicomFilePath = Path.Combine(_testObjectsPath, "Dicom\\DicomFile1");

            var dicomTagsFile = _dicomServices.GetDicomTags(dicomFilePath);

            Assert.IsNotNull(dicomTagsFile);
            Assert.IsTrue(dicomTagsFile.StudyInstanceUid.Values.Length > 0);
            Assert.IsFalse(string.IsNullOrEmpty(dicomTagsFile.SeriesDescription.Values[0]));
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
            var container = new UnityContainer();
            container.RegisterType<IDicomServices, DicomServices>(new TransientLifetimeManager());
            container.RegisterType<IDicomFactory, DicomFactory>(new TransientLifetimeManager());
            container.RegisterType<IDicomNode, DicomNode>(new TransientLifetimeManager());
            return container;
        }
    }
}