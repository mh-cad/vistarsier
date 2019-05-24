using VisTarsier.Common;
using System.Collections.Generic;

namespace VisTarsier.Dicom.Abstractions
{
    public interface IDicomService
    {
        /// <summary>
        /// Send a single dicom image file to the destination node.
        /// </summary>
        /// <param name="filepath">Local path of file.</param>
        void SendDicomFile(string filepath);
        /// <summary>
        /// Send a set of dicom images to the destination node.
        /// </summary>
        /// <param name="filepaths">Local path of file.</param>
        void SendDicomFiles(string[] filepaths);

        IEnumerable<IDicomStudy> GetStudiesForPatientId(string patientId);

        IEnumerable<IDicomSeries> GetSeriesForStudy(string studyUid);

        string GetStudyUidForAccession(string accession);

        IDicomSeries GetSeriesForSeriesUid(string studyUid, string seriesUid);

        IDicomStudy GetStudyForAccession(string accessionNumber);

        void SaveSeriesToLocalDisk(IDicomSeries dicomSeries, string folderPath);

        void CheckRemoteNodeAvailability();
        IEnumerable<IDicomStudy> GetStudiesForPatient(string patientFullName, string patientBirthDate);

        IDicomPatient GetPatientIdFromPatientDetails(string patientFullName, string patientBirthDate);
    }
}