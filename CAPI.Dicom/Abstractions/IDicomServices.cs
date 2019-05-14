using CAPI.Common;
using System.Collections.Generic;

namespace CAPI.Dicom.Abstractions
{
    public interface IDicomServices
    {
        /// <summary>
        /// Send a single dicom image file to the destination node.
        /// </summary>
        /// <param name="filepath">Local path of file.</param>
        /// <param name="localAe">AE Name for local node.</param>
        /// <param name="destinationDicomNode">Destination node.</param>
        void SendDicomFile(string filepath, string localAe, IDicomNode destinationDicomNode);
        /// <summary>
        /// Send a set of dicom images to the destination node.
        /// </summary>
        /// <param name="filepaths">Local path of file.</param>
        /// <param name="localAe">AE Name for local node.</param>
        /// <param name="destinationDicomNode">Destination node.</param>
        void SendDicomFiles(string[] filepaths, string localAe, IDicomNode destinationDicomNode);

        IEnumerable<IDicomStudy> GetStudiesForPatientId(string patientId, IDicomNode localNode, IDicomNode remoteNode);

        IEnumerable<IDicomSeries> GetSeriesForStudy(string studyUid, IDicomNode localNode,
            IDicomNode remoteNode);

        string GetStudyUidForAccession(string accession, IDicomNode localNode, IDicomNode remoteNode);

        IDicomSeries GetSeriesForSeriesUid(
            string studyUid, string seriesUid, IDicomNode localNode, IDicomNode remoteNode);

        IDicomStudy GetStudyForAccession(string accessionNumber, IDicomNode localNode, IDicomNode remoteNode);

        void SaveSeriesToLocalDisk(IDicomSeries dicomSeries, string folderPath, IDicomNode localNode, IDicomNode remoteNode);

        void CheckRemoteNodeAvailability(IDicomNode localNode, IDicomNode remoteNode);
        IEnumerable<IDicomStudy> GetStudiesForPatient(
            string patientFullName, string patientBirthDate, IDicomNode localNode, IDicomNode sourceNode);

        IDicomPatient GetPatientIdFromPatientDetails(string patientFullName, string patientBirthDate,
            IDicomNode localNode, IDicomNode sourceNode);
    }
}