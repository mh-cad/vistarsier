using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using ClearCanvas.Dicom;
using ClearCanvas.Dicom.Iod.Iods;
using ClearCanvas.Dicom.Network.Scu;
using System;
using System.Collections.Generic;
using System.Linq;
using StorageScp = ClearCanvas.Dicom.Samples.StorageScp;

namespace CAPI.Dicom
{
    public class DicomServices : IDicomServices
    {
        private readonly IDicomFactory _dicomFactory;

        public DicomServices(IDicomFactory dicomFactory)
        {
            _dicomFactory = dicomFactory;
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

        public void UpdateDicomHeaders(string filepath, IDicomTagCollection tags, DicomNewObjectType dicomNewObjectType)
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

        private static DicomFile UpdateTags(DicomFile dcmFile, IDicomTagCollection newTags, TagType tagType, bool overwriteIfNotProvided = false)
        {
            if (newTags == null) return dcmFile;
            newTags.ToList().ForEach(tag =>
            {
                if (tag.DicomTagType == tagType)
                    dcmFile = UpdateTag(dcmFile, tag, overwriteIfNotProvided);
            });
            return dcmFile;
        }
        private static DicomFile UpdateTag(DicomFile dcmFile, IDicomTag newTag, bool overwriteIfNotProvided = false)
        {
            if (newTag.Values == null && !overwriteIfNotProvided) return dcmFile;
            var value = newTag.Values != null ? newTag.Values[0] : "";
            return UpdateTag(dcmFile, newTag, value);
        }
        private static DicomFile UpdateTag(DicomFile dcmFile, IDicomTag newTag, string value)
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

        public IEnumerable<string> GetStudiesForPatientId(string patientId, IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);
            var query = new StudyQueryIod();
            query.SetCommonTags();
            query.PatientId = patientId;
            var findScu = new StudyRootFindScu();
            var studies = findScu.Find(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port, query);
            return studies.Count == 0 ? null :
                studies.Select(study => study.AccessionNumber + "|" + study.StudyInstanceUid);//.Where(a => !string.IsNullOrEmpty(a));
        }

        public IEnumerable<string> GetSeriesForStudy(string studyUid, string accession, IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);
            if (string.IsNullOrEmpty(studyUid))
                studyUid = GetSeriesUidForAccession(accession, localNode, remoteNode);
            if (string.IsNullOrEmpty(studyUid)) throw new Exception($"Failed to find StudyInstanceUid for accession: {accession}");

            var seriesQuery = new SeriesQueryIod();
            seriesQuery.SetCommonTags();
            seriesQuery.StudyInstanceUid = studyUid;
            var find = new StudyRootFindScu();
            var series = find.Find(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port, seriesQuery)
                .Select(s => $"{s.SeriesDescription}|{s.SeriesInstanceUid}");
            return series;
        }

        public string GetSeriesUidForAccession(string accession, IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);
            var studyQuery = new StudyQueryIod();
            studyQuery.SetCommonTags();
            studyQuery.AccessionNumber = accession;
            var findScu = new StudyRootFindScu();
            var studies = findScu.Find(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port,
                studyQuery);
            return studies.Count == 0 ? "" : studies.FirstOrDefault()?.StudyInstanceUid;
        }

        private static void CheckRemoteNodeAvailability(IDicomNode localNode, IDicomNode remoteNode)
        {
            var verificationScu = new VerificationScu();
            var result = verificationScu.Verify(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port);
            if (result != VerificationResult.Success)
                throw new Exception($"Remote Dicom node not reachable. AET: [{remoteNode.AeTitle}] IP: [{remoteNode.IpAddress}]");
        }

        public IDicomSeries GetSeriesDataForSeriesUid(
            string studyUid, string seriesUid, IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);

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

            if (results == null || results.Count == 0) return null;

            return new DicomSeries
            {
                Images = results
                .Select(r => new DicomImage { ImageUid = r.SopInstanceUid }).ToList()
            };
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
            var study = _dicomFactory.CreateStudy();
            study.AccessionNumber = accessionNumber;

            // ???

            return study;
        }
    }
}