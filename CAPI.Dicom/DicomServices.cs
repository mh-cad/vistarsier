using CAPI.Dicom.Abstractions;
using CAPI.Common;
using ClearCanvas.Dicom;
using ClearCanvas.Dicom.Iod;
using ClearCanvas.Dicom.Iod.Iods;
using ClearCanvas.Dicom.Network.Scu;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace CAPI.Dicom
{
    public class DicomServices : IDicomServices
    {

        public DicomServices()
        {
        }

        public void SendDicomFile(string filepath, string localAe, IDicomNode destinationDicomNode)
        {
            using (var scu = new StorageScu(localAe, destinationDicomNode.AeTitle, destinationDicomNode.IpAddress, destinationDicomNode.Port))
            {
                scu.ImageStoreCompleted += ScuOnImageStorageCompleted;
                scu.AddFile(filepath);
                scu.Send();
            }
        }

        public void SendDicomFiles(string[] filepaths, string localAe, IDicomNode destinationDicomNode)
        {
            using (var scu = new StorageScu(localAe, destinationDicomNode.AeTitle, destinationDicomNode.IpAddress, destinationDicomNode.Port))
            {
                scu.ImageStoreCompleted += ScuOnImageStorageCompleted;
                foreach (var filepath in filepaths)
                { 
                    scu.AddFile(filepath);
                }
                scu.Send();
            }
        }

        private static void ScuOnImageStorageCompleted(object sender, ImageStoreEventArgs eventArgs)
        {
            // We could do something here, but at the moment we're not. ClearCanvas already spams the log.
            //Log.GetLogger().Debug(eventArgs.StorageInstance.SendStatus);
        }

       

        public IEnumerable<IDicomStudy> GetStudiesForPatientId(
            string patientId, IDicomNode localNode, IDicomNode remoteNode)
        {
            var query = new StudyQueryIod();
            query.SetCommonTags();
            query.PatientId = patientId;

            return GetStudies(query, localNode, remoteNode);
        }

        public IEnumerable<IDicomStudy> GetStudiesForPatient(
            string patientFullName, string patientBirthDate,
            IDicomNode localNode, IDicomNode sourceNode)
        {
            var query = new StudyQueryIod();
            query.SetCommonTags();
            query.PatientsName = new PersonName(patientFullName);
            query.PatientsBirthDate = DateTime.ParseExact(patientBirthDate, "yyyyMMdd", new DateTimeFormatInfo());

            return GetStudies(query, localNode, sourceNode);
        }

        public IDicomPatient GetPatientIdFromPatientDetails(
            string patientFullname, string patientBirthDate,
            IDicomNode localNode, IDicomNode sourceNode)
        {
            var query = new PatientQueryIod();
            query.SetCommonTags();
            query.PatientsName = new PersonName(patientFullname);
            query.PatientsBirthDate = DateTime.ParseExact(patientBirthDate, "yyyyMMdd", CultureInfo.CurrentCulture);
            var findScu = new PatientRootFindScu();
            var patient = findScu.Find(localNode.AeTitle, sourceNode.AeTitle,
                sourceNode.IpAddress, sourceNode.Port, query).ToList();
            if (!patient.Any()) throw new Exception($"No patient found with name [{patientFullname}] " +
                                                    $"and birth date [{patientBirthDate}]");
            if (patient.Count > 1) throw new Exception($"{patient.Count} patients were found for name [{patientFullname}] " +
                                                    $"and birth date [{patientBirthDate}]");
            return patient.Select(MapToDicomPatient).FirstOrDefault();
        }

        private static IDicomPatient MapToDicomPatient(PatientQueryIod patientQueryIod)
        {
            return new DicomPatient
            {
                PatientId = patientQueryIod.PatientId,
                PatientFullName = patientQueryIod.PatientsName,
                PatientBirthDate = patientQueryIod.PatientsBirthDate.Date.ToString("yyyyMMdd")
            };
        }

        private IEnumerable<IDicomStudy> GetStudies(StudyQueryIod query,
            IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);
            var findScu = new StudyRootFindScu();
            return findScu
                .Find(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port, query)
                .Select(MapToDicomStudy);
        }

        private static IDicomStudy MapToDicomStudy(StudyQueryIod study)
        {
            return new DicomStudy
            {
                AccessionNumber = study.AccessionNumber,
                StudyDescription = study.StudyDescription,
                StudyInstanceUid = study.StudyInstanceUid,
                StudyDate = study.StudyDate,
                PatientBirthDate = study.PatientsBirthDate,
                PatientId = study.PatientId,
                PatientsName = study.PatientsName,
                PatientsSex = study.PatientsSex,
            };
        }

        public IEnumerable<IDicomSeries> GetSeriesForStudy(
            string studyUid, IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);

            var seriesQuery = new SeriesQueryIod();
            seriesQuery.SetCommonTags();
            seriesQuery.StudyInstanceUid = studyUid;
            var find = new StudyRootFindScu();

            return find.Find(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port, seriesQuery)
                .Select(s => new DicomSeries
                {
                    SeriesInstanceUid = s.SeriesInstanceUid,
                    SeriesDescription = s.SeriesDescription,
                    StudyInstanceUid = s.StudyInstanceUid,
                });
        }

        public string GetStudyUidForAccession(string accession,
            IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);

            var studyQuery = new StudyQueryIod();
            studyQuery.SetCommonTags();
            studyQuery.AccessionNumber = accession;

            var findScu = new StudyRootFindScu();
            var studies = findScu.Find(localNode.AeTitle, remoteNode.AeTitle,
                remoteNode.IpAddress, remoteNode.Port, studyQuery);

            if (studies.Count == 0) throw new DicomException($"No study was found for accession: {accession}");
            return studies.Count == 0 ? "" : studies.FirstOrDefault()?.StudyInstanceUid;
        }

        public void CheckRemoteNodeAvailability(IDicomNode localNode, IDicomNode remoteNode)
        {
            var verificationScu = new VerificationScu();
            var result = verificationScu.Verify(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port);
            // TODO check what's going on here.
            if (result != VerificationResult.Success)
                throw new Exception($"Remote Dicom node not reachable. AET: [{remoteNode.AeTitle}] IP: [{remoteNode.IpAddress}]");
        }

        public IDicomSeries GetSeriesForSeriesUid(
            string studyUid, string seriesUid, IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);

            var seriesQueryIod = new SeriesQueryIod();
            seriesQueryIod.SetCommonTags();
            seriesQueryIod.StudyInstanceUid = studyUid;
            seriesQueryIod.SeriesInstanceUid = seriesUid;

            var findScu = new StudyRootFindScu();
            var series = findScu.Find(localNode.AeTitle, remoteNode.AeTitle,
                remoteNode.IpAddress, remoteNode.Port, seriesQueryIod);
            var seriesDescription = series.Count == 0 ? "" : series.FirstOrDefault()?.SeriesDescription;

            var imageQueryIods = GetImageIodsForSeries(studyUid, seriesUid, localNode, remoteNode) as IList<ImageQueryIod>;

            if (imageQueryIods?.ToList().Count == 0) return null;

            return new DicomSeries
            {
                StudyInstanceUid = studyUid,
                SeriesInstanceUid = seriesUid,
                SeriesDescription = seriesDescription,
                Images = imageQueryIods?
                    .Select(r => new DicomImage { ImageUid = r.SopInstanceUid })
                    .ToList()
            };
        }

        private static IEnumerable<ImageQueryIod> GetImageIodsForSeries(
            string studyUid, string seriesUid, IDicomNode localNode, IDicomNode remoteNode)
        {
            var imageQuery = new ImageQueryIod();

            imageQuery.SetCommonTags();
            imageQuery.StudyInstanceUid = studyUid;
            imageQuery.SeriesInstanceUid = seriesUid;

            var find = new StudyRootFindScu();
            var results = find.Find(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress,
                remoteNode.Port, imageQuery);

            if (!string.IsNullOrEmpty(find.FailureDescription))
                throw new DicomException($"Series query failed: {find.FailureDescription}{Environment.NewLine}" +
                                         $"AET: {remoteNode.AeTitle}{Environment.NewLine}" +
                                         $"Study Uid: {studyUid}{Environment.NewLine}" +
                                         $"Series Uid: {seriesUid}");
            return results;
        }

        public void SaveSeriesToLocalDisk(
            IDicomSeries dicomSeries, string folderPath, IDicomNode localNode, IDicomNode remoteNode)
        {
            var moveScu = new StudyRootMoveScu(localNode.AeTitle, remoteNode.AeTitle,
                remoteNode.IpAddress, remoteNode.Port, localNode.AeTitle);

            moveScu.AddStudyInstanceUid(dicomSeries.StudyInstanceUid);
            moveScu.AddSeriesInstanceUid(dicomSeries.SeriesInstanceUid);

            StorageScp.StorageLocation = folderPath;

            try
            {
                StorageScp.StartListening(localNode.AeTitle, localNode.Port);

                moveScu.Move();

                if (!string.IsNullOrEmpty(moveScu.FailureDescription))
                    throw new DicomException(moveScu.FailureDescription);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); // TODO3: Implement proper logging
                throw;
            }
            finally
            {
                StorageScp.StopListening();
            }

        }

        public IDicomStudy GetStudyForAccession(string accessionNumber,
            IDicomNode localNode, IDicomNode remoteNode)
        {
            var query = new StudyQueryIod();
            query.SetCommonTags();
            query.AccessionNumber = accessionNumber;
            var findScu = new StudyRootFindScu();
            var studies = findScu
                .Find(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port, query)
                .Select(MapToDicomStudy).ToList();
            if (studies.Count > 1)
                throw new Exception($"{studies.Count} studies returned for accession {accessionNumber}");
            return studies.FirstOrDefault();
        }

        
    }
}