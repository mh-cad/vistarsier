using System.Collections.Generic;

namespace CAPI.Dicom.Abstractions
{
    public interface IDicomServices
    {
        void SendDicomFile(string filepath, string localAe, IDicomNode destinationDicomNode);
        void UpdateDicomHeaders(string filepath, IDicomTagCollection tags, DicomNewObjectType dicomNewObjectType);
        void UpdateSeriesHeadersForAllFiles(string[] filesPath, IDicomTagCollection tags);
        //void CopyDicomHeadersToNewFiles(string dicomFolderWithHeaders, string dicomFolderWithPixelData,
        //    string ouputFolder);

        IDicomTagCollection GetDicomTags(string filePath);

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

        string GenerateNewStudyUid();
        string GenerateNewSeriesUid();
        string GenerateNewImageUid();

        void ConvertBmpsToDicom(string bmpFolder, string dicomFolder, string dicomHeadersFolder = "");
        void ConvertBmpToDicom(string bmpFilepath, string dicomFilePath, string dicomHeadersFilePath = "");
        void ConvertBmpToDicomAndAddToExistingFolder(string bmpFilePath, string dicomFolderPath, string newFileName = "");
    }
}