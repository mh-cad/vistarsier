using CAPI.Agent;
using CAPI.Config;
using CAPI.Dicom.Abstractions;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CAPI.UAT.Tests
{
    public class DicomConnectivity : IUatTest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SuccessMessage { get; set; }
        public string FailureMessage { get; set; }
        public string TestGroup { get; set; }
        public CapiConfig CapiConfig { get; set; }
        public AgentRepository Context { get; set; }

        private readonly IDicomFactory _dicomFactory;
        private readonly ILog _log;

        public DicomConnectivity(IDicomFactory dicomFactory, ILog log)
        {
            Name = "Dicom Connectivity";
            Description = "Check whether Dicom association can be made with details entailed in config.json file and Query Retrieve and Storage are successfully done";
            SuccessMessage = "Dicom connection and related activities were successful";
            FailureMessage = "Unable to establish Dicom connection or failure in query, retrieve or storage functions.";
            TestGroup = "Dicom";

            _dicomFactory = dicomFactory;
            _log = log;
        }

        public bool Run()
        {
            var dicomConfig = _dicomFactory.CreateDicomConfig();
            dicomConfig.Img2DcmFilePath = CapiConfig.DicomConfig.Img2DcmFilePath;

            var dicomServices = _dicomFactory.CreateDicomServices(dicomConfig);

            var localNode = CapiConfig.DicomConfig.LocalNode;

            CheckConnectivityToAllRemoteNodes(localNode, CapiConfig.DicomConfig.RemoteNodes, dicomServices);
            Thread.Sleep(1000);

            var remoteNodes = CapiConfig.DicomConfig.RemoteNodes;

            Logger.Write($"{string.Join("       ", remoteNodes.Select(n => $"{Array.IndexOf(remoteNodes.ToArray(), n) + 1}. " + n.AeTitle))}", true, Logger.TextType.Content, false, 1, 0);
            Logger.Write("Please specify index of AET of source node to test Query Retrieve and Storage: ", false, Logger.TextType.Content, false, 0, 0);

            var sourceAetIndex = Convert.ToInt32(Console.ReadKey().KeyChar.ToString());
            if (sourceAetIndex > remoteNodes.Count) throw new ArgumentOutOfRangeException($"Invalid index entered! Value sohuld be from 1 to {remoteNodes.Count}");
            var sourceAet = remoteNodes.ToArray()[sourceAetIndex - 1].AeTitle;

            var sourceNode = CapiConfig.DicomConfig.RemoteNodes.FirstOrDefault(n =>
                             n.AeTitle.ToLower().Equals(sourceAet, StringComparison.CurrentCultureIgnoreCase));
            if (sourceNode != null)
            {
                Logger.Write($"Please enter a patient Id existing in source node AET [{sourceNode.AeTitle}] to get list of studies: ", false, Logger.TextType.Content, false, 1, 0);

                var patientId = Console.ReadLine();
                if (string.IsNullOrEmpty(patientId)) throw new Exception("Patient Id cannot be blank.");
                var studies = dicomServices.GetStudiesForPatientId(patientId, localNode, sourceNode).ToList();
                if (!studies.Any()) throw new Exception($"No studies found for patient Id [{patientId}] in source node AET [{sourceAet}]");
                foreach (var study in studies)
                    Logger.Write($"Found accession [{study.AccessionNumber}] for patient Id [{patientId}] in Dicom node AET [{sourceNode}]", true, Logger.TextType.Success);

                // Query
                if (!QueryFromDicomNode(patientId, studies, out var queriedSeries, dicomServices, localNode, sourceNode))
                    return false;

                // Retrieve
               // if (!RetrieveFromDicomNode(patientId, studies, out var retrievedSeries, dicomServices, localNode, sourceNode))
                    return false;

                // Storage

            }

            return true;
        }

        private static bool RetrieveFromDicomNode(string patientId, IDicomSeries queriedSeries, 
                                           out IDicomSeries retrievedSeries, IDicomServices dicomServices, 
                                           IDicomNode localNode, IDicomNode sourceNode)
        {
            retrievedSeries = null;
            try
            {
                var tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
                if (Directory.Exists(tempFolder)) Directory.Delete(tempFolder);
                Directory.CreateDirectory(tempFolder);

                dicomServices.SaveSeriesToLocalDisk(queriedSeries, tempFolder, localNode, sourceNode);

            }
            catch
            {
                Logger.Write($"Failed to retrieve series UID [{queriedSeries.SeriesInstanceUid}] for patient id {patientId} from dicom node with AET [{sourceNode.AeTitle}]", true, Logger.TextType.Fail, true, 1);
                return false;
            }
            return true;
        }

        private static bool QueryFromDicomNode(string patientId, IEnumerable<IDicomStudy> studies,
                                               out IDicomSeries queriedSeries, IDicomServices dicomServices,
                                               IDicomNode localNode, IDicomNode sourceNode)
        {
            queriedSeries = null;
            try
            {
                var study1 = studies.FirstOrDefault();
                var series1 = dicomServices.GetSeriesForStudy(study1?.StudyInstanceUid, localNode, sourceNode).FirstOrDefault();
                series1 = dicomServices.GetSeriesForSeriesUid(series1?.StudyInstanceUid, series1?.SeriesInstanceUid, localNode, sourceNode);
                if (series1 == null) return false;
                if (!series1.Images.Any()) return false;
                queriedSeries = series1;
            }
            catch
            {
                Logger.Write($"Failed to Query study UID [{queriedSeries?.StudyInstanceUid}] for patient Id {patientId} from dicom node with AET [{sourceNode.AeTitle}]", true, Logger.TextType.Fail, true, 1);
                return false;
            }
            return true;
        }

        private static void CheckConnectivityToAllRemoteNodes(IDicomNode localNode, IEnumerable<IDicomNode> dicomConfigRemoteNodes, IDicomServices dicomServices)
        {
            foreach (var remoteNode in dicomConfigRemoteNodes)
            {
                Logger.Write($"Connecting from local node AET [{localNode.AeTitle}] to remote node AET [{remoteNode.AeTitle}]", true, Logger.TextType.Content, false, 1);

                dicomServices.CheckRemoteNodeAvailability(localNode, remoteNode);

                Logger.Write($"Successfully connected from local node AET [{localNode.AeTitle}] to remote node AET [{remoteNode.AeTitle}]", true, Logger.TextType.Success, true);
            }
        }

        public void FailureResolution()
        {
            Logger.Write("Please provide valid dicom nodes in config file and patient id with at least one study in any node to test dicom connectivity and dicom functions.", true, Logger.TextType.Content, false, 0, 0);
        }
    }
}
