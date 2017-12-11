﻿using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using ClearCanvas.Dicom;
using ClearCanvas.Dicom.Iod;
using ClearCanvas.Dicom.Iod.Iods;
using ClearCanvas.Dicom.Network.Scu;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StorageScp = ClearCanvas.Dicom.Samples.StorageScp;

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

        private static void ScuOnImageStorageCompleted(object sender, ImageStoreEventArgs eventArgs)
        {
        }

        public void UpdateDicomHeaders(
            string filepath, IDicomTagCollection tags, DicomNewObjectType dicomNewObjectType)
        {
            var dcmFile = new DicomFile(filepath);
            dcmFile.Load(filepath);

            switch (dicomNewObjectType)
            {
                case DicomNewObjectType.Anonymized:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Patient, true);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Study);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
                    break;
                case DicomNewObjectType.SiteDetailsRemoved:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Site, true);
                    break;
                case DicomNewObjectType.CareProviderDetailsRemoved:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.CareProvider, true);
                    break;
                case DicomNewObjectType.NewPatient:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Patient);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Study);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
                    break;
                case DicomNewObjectType.NewStudy:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Study);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
                    break;
                case DicomNewObjectType.NewSeries:
                    tags = UpdateUidsForNewSeries(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
                    break;
                case DicomNewObjectType.NewImage:
                    tags = UpdateUidsForNewImage(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
                    break;
                case DicomNewObjectType.NoChange:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dicomNewObjectType), dicomNewObjectType, null);
            }
            dcmFile.Save(filepath);
        }

        private static DicomFile UpdateTags(
            DicomFile dcmFile, IDicomTagCollection newTags, TagType tagType, bool overwriteIfNotProvided = false)
        {
            if (newTags == null) return dcmFile;
            newTags.ToList().ForEach(tag =>
            {
                if (tag.DicomTagType == tagType)
                    dcmFile = UpdateTag(dcmFile, tag, overwriteIfNotProvided);
            });
            return dcmFile;
        }
        private static DicomFile UpdateTag(
            DicomFile dcmFile, IDicomTag newTag, bool overwriteIfNotProvided = false)
        {
            if (newTag.Values == null && !overwriteIfNotProvided) return dcmFile;
            var value = newTag.Values != null ? newTag.Values[0] : "";
            return UpdateTag(dcmFile, newTag, value);
        }
        private static DicomFile UpdateTag(
            DicomFile dcmFile, IDicomTag newTag, string value)
        {
            if (newTag.GetValueType() == typeof(string[])) dcmFile.DataSet[newTag.GetTagValue()].Values = new[] { value };
            else if (newTag.GetValueType() == typeof(string)) dcmFile.DataSet[newTag.GetTagValue()].Values = value;
            return dcmFile;
        }

        private static IDicomTagCollection UpdateUidsForNewStudy(IDicomTagCollection tags)
        {
            if (tags == null) return null;
            tags.StudyUid.Values = new[] { GetNewStudyUid() };
            tags = UpdateUidsForNewSeries(tags);
            tags = UpdateUidsForNewImage(tags);
            return tags;
        }
        private static IDicomTagCollection UpdateUidsForNewSeries(IDicomTagCollection tags)
        {
            if (tags == null) return null;
            tags.SeriesUid.Values = new[] { GetNewSeriesUid() };
            tags = UpdateUidsForNewImage(tags);
            return tags;
        }
        private static IDicomTagCollection UpdateUidsForNewImage(IDicomTagCollection tags)
        {
            if (tags == null) return null;
            tags.ImageUid.Values = new[] { GetNewImageUid() };
            return tags;
        }

        private static string GetNewStudyUid()
        {
            return $"1.2.826.0.1.3680043.9.7303.1.1.{DateTime.Now:yyyyMMddHHmmssfff}.1";
        }
        private static string GetNewSeriesUid()
        {
            return $"1.2.826.0.1.3680043.9.7303.1.2.{DateTime.Now:yyyyMMddHHmmssfff}.1";
        }
        private static string GetNewImageUid()
        {
            return $"1.2.826.0.1.3680043.9.7303.1.3.{DateTime.Now:yyyyMMddHHmmssfff}.1";
        }

        public IDicomTagCollection GetDicomTags(string filePath)
        {
            var dcmFile = new DicomFile(filePath);
            dcmFile.Load(filePath);
            var tags = new DicomTagCollection();
            var updatedTags = new DicomTagCollection();
            foreach (var tag in tags.ToList())
                updatedTags.SetTagValue(tag.GetTagValue(), dcmFile.DataSet[tag.GetTagValue()].Values);
            return updatedTags;
        }

        public IEnumerable<IDicomStudy> GetStudiesForPatientId(
            string patientId, IDicomNode localNode, IDicomNode remoteNode)
        {
            var query = new StudyQueryIod();
            query.SetCommonTags();
            query.PatientId = patientId;

            return GetStudies(query, localNode, remoteNode);
        }

        public IEnumerable<IDicomStudy> GetStudiesForPatient(string patientFullName, string patientBirthDate,
            IDicomNode localNode, IDicomNode sourceNode)
        {
            var query = new StudyQueryIod();
            query.SetCommonTags();
            query.PatientsName = new PersonName(patientFullName);
            query.PatientsBirthDate = DateTime.ParseExact(patientBirthDate, "yyyyMMdd", new DateTimeFormatInfo());

            return GetStudies(query, localNode, sourceNode);
        }

        private IEnumerable<IDicomStudy> GetStudies(StudyQueryIod query, IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);
            var findScu = new StudyRootFindScu();
            return findScu
                .Find(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port, query)
                .Select(study => new DicomStudy
                {
                    AccessionNumber = study.AccessionNumber,
                    StudyDescription = study.StudyDescription,
                    StudyInstanceUid = study.StudyInstanceUid,
                    StudyDate = study.StudyDate,
                    PatientBirthDate = study.PatientsBirthDate,
                    PatientId = study.PatientId,
                    PatientsName = study.PatientsName,
                    PatientsSex = study.PatientsSex
                });
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
                    StudyInstanceUid = s.StudyInstanceUid
                });
        }

        public string GetStudyUidForAccession(string accession, IDicomNode localNode, IDicomNode remoteNode)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); // TODO3: Implement proper logging
                throw;
            }
            finally
            {
                StorageScp.StopListening(localNode.Port);
            }

        }

        public IDicomStudy GetStudyForAccession(string accessionNumber)
        {
            var study = new DicomStudy { AccessionNumber = accessionNumber };

            // ???

            return study;
        }
    }
}