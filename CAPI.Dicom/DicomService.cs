using VisTarsier.Dicom.Abstractions;
using VisTarsier.Common;
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

namespace VisTarsier.Dicom
{
    public class DicomService : IDicomService
    {
        public readonly IDicomNode LocalNode;
        public readonly IDicomNode RemoteNode;

        public DicomService(IDicomNode localNode, IDicomNode remoteNode)
        {
            LocalNode = localNode;
            RemoteNode = remoteNode;
        }

        public void SendDicomFile(string filepath)
        {
            using (var scu = new StorageScu(LocalNode.AeTitle, RemoteNode.AeTitle, RemoteNode.IpAddress, RemoteNode.Port))
            {
                scu.ImageStoreCompleted += ScuOnImageStorageCompleted;
                scu.AddFile(filepath);
                scu.Send();
            }
        }

        public void SendDicomFiles(string[] filepaths)
        {
            using (var scu = new StorageScu(LocalNode.AeTitle, RemoteNode.AeTitle, RemoteNode.IpAddress, RemoteNode.Port))
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

       

        public IEnumerable<IDicomStudy> GetStudiesForPatientId(string patientId)
        {
            var query = new StudyQueryIod();
            query.SetCommonTags();
            query.PatientId = patientId;

            return GetStudies(query);
        }

        public IEnumerable<IDicomStudy> GetStudiesForPatient(string patientFullName, string patientBirthDate)
        {
            var query = new StudyQueryIod();
            query.SetCommonTags();
            query.PatientsName = new PersonName(patientFullName);
            query.PatientsBirthDate = DateTime.ParseExact(patientBirthDate, "yyyyMMdd", new DateTimeFormatInfo());

            return GetStudies(query);
        }

        public IDicomPatient GetPatientIdFromPatientDetails(
            string patientFullname, string patientBirthDate)
        {
            var query = new PatientQueryIod();
            query.SetCommonTags();
            query.PatientsName = new PersonName(patientFullname);
            query.PatientsBirthDate = DateTime.ParseExact(patientBirthDate, "yyyyMMdd", CultureInfo.CurrentCulture);
            var findScu = new PatientRootFindScu();
            var patient = findScu.Find(LocalNode.AeTitle, RemoteNode.AeTitle,
                RemoteNode.IpAddress, RemoteNode.Port, query).ToList();
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

        private IEnumerable<IDicomStudy> GetStudies(StudyQueryIod query)
        {
            CheckRemoteNodeAvailability();
            var findScu = new StudyRootFindScu();
            return findScu
                .Find(LocalNode.AeTitle, RemoteNode.AeTitle, RemoteNode.IpAddress, RemoteNode.Port, query)
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

        public IEnumerable<IDicomSeries> GetSeriesForStudy(string studyUid)
        {
            CheckRemoteNodeAvailability();

            var seriesQuery = new SeriesQueryIod();
            seriesQuery.SetCommonTags();
            seriesQuery.StudyInstanceUid = studyUid;
            var find = new StudyRootFindScu();

            return find.Find(LocalNode.AeTitle, RemoteNode.AeTitle, RemoteNode.IpAddress, RemoteNode.Port, seriesQuery)
                .Select(s => new DicomSeries
                {
                    SeriesInstanceUid = s.SeriesInstanceUid,
                    SeriesDescription = s.SeriesDescription,
                    StudyInstanceUid = s.StudyInstanceUid,
                });
        }

        public string GetStudyUidForAccession(string accession)
        {
            CheckRemoteNodeAvailability();

            var studyQuery = new StudyQueryIod();
            studyQuery.SetCommonTags();
            studyQuery.AccessionNumber = accession;

            var findScu = new StudyRootFindScu();
            var studies = findScu.Find(LocalNode.AeTitle, RemoteNode.AeTitle,
                RemoteNode.IpAddress, RemoteNode.Port, studyQuery);

            if (studies.Count == 0) throw new DicomException($"No study was found for accession: {accession}");
            return studies.Count == 0 ? "" : studies.FirstOrDefault()?.StudyInstanceUid;
        }

        public void CheckRemoteNodeAvailability()
        {
            var verificationScu = new VerificationScu();
            var result = verificationScu.Verify(LocalNode.AeTitle, RemoteNode.AeTitle, RemoteNode.IpAddress, RemoteNode.Port);
            // TODO check what's going on here.
            if (result != VerificationResult.Success)
                throw new Exception($"Remote Dicom node not reachable. AET: [{RemoteNode.AeTitle}] IP: [{RemoteNode.IpAddress}]");
        }

        public IDicomSeries GetSeriesForSeriesUid(string studyUid, string seriesUid)
        {
            CheckRemoteNodeAvailability();

            var seriesQueryIod = new SeriesQueryIod();
            seriesQueryIod.SetCommonTags();
            seriesQueryIod.StudyInstanceUid = studyUid;
            seriesQueryIod.SeriesInstanceUid = seriesUid;

            var findScu = new StudyRootFindScu();
            var series = findScu.Find(LocalNode.AeTitle, RemoteNode.AeTitle,
                RemoteNode.IpAddress, RemoteNode.Port, seriesQueryIod);
            var seriesDescription = series.Count == 0 ? "" : series.FirstOrDefault()?.SeriesDescription;

            var imageQueryIods = GetImageIodsForSeries(studyUid, seriesUid);

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

        private IList<ImageQueryIod> GetImageIodsForSeries(string studyUid, string seriesUid)
        {
            var imageQuery = new ImageQueryIod();

            imageQuery.SetCommonTags();
            imageQuery.StudyInstanceUid = studyUid;
            imageQuery.SeriesInstanceUid = seriesUid;

            var find = new StudyRootFindScu();
            var results = find.Find(LocalNode.AeTitle, RemoteNode.AeTitle, RemoteNode.IpAddress,
                RemoteNode.Port, imageQuery);

            if (!string.IsNullOrEmpty(find.FailureDescription))
                throw new DicomException($"Series query failed: {find.FailureDescription}{Environment.NewLine}" +
                                         $"AET: {RemoteNode.AeTitle}{Environment.NewLine}" +
                                         $"Study Uid: {studyUid}{Environment.NewLine}" +
                                         $"Series Uid: {seriesUid}");
            return results;
        }

        public void SaveSeriesToLocalDisk(IDicomSeries dicomSeries, string folderPath)
        {
            var moveScu = new StudyRootMoveScu(LocalNode.AeTitle, RemoteNode.AeTitle,
                RemoteNode.IpAddress, RemoteNode.Port, LocalNode.AeTitle);

            moveScu.AddStudyInstanceUid(dicomSeries.StudyInstanceUid);
            moveScu.AddSeriesInstanceUid(dicomSeries.SeriesInstanceUid);

            StorageScp.StorageLocation = folderPath;

            try
            {
                StorageScp.StartListening(LocalNode.AeTitle, LocalNode.Port);

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

        public IDicomStudy GetStudyForAccession(string accessionNumber)
        {
            var query = new StudyQueryIod();
            query.SetCommonTags();
            query.AccessionNumber = accessionNumber;
            
            var findScu = new StudyRootFindScu();
            var studies = findScu
                .Find(LocalNode.AeTitle, RemoteNode.AeTitle, RemoteNode.IpAddress, RemoteNode.Port, query)
                .Select(MapToDicomStudy).ToList();
            if (studies.Count > 1)
                throw new Exception($"{studies.Count} studies returned for accession {accessionNumber}");
            return studies.FirstOrDefault();
        }

        
    }
}