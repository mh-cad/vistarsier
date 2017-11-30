using System.Collections.Generic;

namespace CAPI.Dicom.Abstraction
{
    public interface IDicomServices
    {
        void SendDicomFile(string filepath, string localAe, IDicomNode destinationDicomNode);
        void UpdateDicomHeaders(string filepath, IDicomTagCollection tags, DicomNewObjectType dicomNewObjectType);

        IDicomTagCollection GetDicomTags(string filePath);

        IEnumerable<string> GetStudiesForPatientId(string patientId, IDicomNode localNode, IDicomNode remoteNode);

        IEnumerable<string> GetSeriesForStudy(string studyUid, string accession, IDicomNode localNode,
            IDicomNode remoteNode);

        string GetSeriesUidForAccession(string accession, IDicomNode localNode, IDicomNode remoteNode);

        IEnumerable<string> GetImagesForSeries(string studyUid, string seriesUid, IDicomNode localNode,
            IDicomNode remoteNode);
        
        IDicomStudy GetStudyForAccession(string accesstionNumber);
    }
}