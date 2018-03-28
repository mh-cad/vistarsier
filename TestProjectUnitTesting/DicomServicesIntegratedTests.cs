using CAPI.Dicom;
using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Unity;
using Unity.Lifetime;

namespace TestProjectUnitTesting
{
    [TestClass]
    public class DicomServicesIntegratedTests
    {
        private IDicomServices _dicomServices;
        private IDicomFactory _dicomFactory;
        private IDicomNode _localNode;
        private IDicomNode _remoteNode;
        private string _testObjectsPath;
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
            var binPath = Directory.GetParent(Environment.CurrentDirectory).FullName;
            var projectPath = Directory.GetParent(binPath).FullName;
            var projectsParentPath = Directory.GetParent(projectPath).FullName;
            return Path.Combine(projectsParentPath, "TestObjects");
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

        [TestCleanup]
        public void TestCleanUp()
        {
            // Delete Updated Tag File
            var updatedTagFile = Path.Combine(_testObjectsPath, TestDicomUpdatedTagsRelativePath);
            if (File.Exists(updatedTagFile)) File.Delete(updatedTagFile);
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

        private static IUnityContainer CreateContainerCore()
        {
            var container = new UnityContainer();//(UnityContainer)new UnityContainer()
                                                 //.AddNewExtension<Log4NetExtension>();
            container.RegisterType<IDicomServices, DicomServices>(new TransientLifetimeManager());
            container.RegisterType<IDicomFactory, DicomFactory>(new TransientLifetimeManager());
            container.RegisterType<IDicomNode, DicomNode>(new TransientLifetimeManager());
            return container;
        }
    }
}