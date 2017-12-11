using System.Collections.Generic;

namespace CAPI.Dicom.Abstraction
{
    public interface IDicomServices
    {
        void SendDicomFile(string filepath, string localAe, IDicomNode destinationDicomNode);
        void UpdateDicomHeaders(string filepath, IDicomTagCollection tags, DicomNewObjectType dicomNewObjectType);

        IDicomTagCollection GetDicomTags(string filePath);

        IEnumerable<IDicomStudy> GetStudiesForPatientId(string patientId, IDicomNode localNode, IDicomNode remoteNode);

        IEnumerable<IDicomSeries> GetSeriesForStudy(string studyUid, IDicomNode localNode,
            IDicomNode remoteNode);

        string GetStudyUidForAccession(string accession, IDicomNode localNode, IDicomNode remoteNode);

        IDicomSeries GetSeriesForSeriesUid(
            string studyUid, string seriesUid, IDicomNode localNode, IDicomNode remoteNode);

        IDicomStudy GetStudyForAccession(string accesstionNumber);

        void SaveSeriesToLocalDisk(IDicomSeries dicomSeries, string folderPath, IDicomNode localNode, IDicomNode remoteNode);

        void CheckRemoteNodeAvailability(IDicomNode localNode, IDicomNode remoteNode);
        IEnumerable<IDicomStudy> GetStudiesForPatient(
            string patientFullName, string patientBirthDate, IDicomNode localNode, IDicomNode sourceNode);
    }
}