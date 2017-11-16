using CAPI.Dicom.Model;
using ClearCanvas.Dicom;
using ClearCanvas.Dicom.Network.Scu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.Dicom
{
    public static class DicomServices
    {
        public static void SendDicomFile(string filepath, string localAe, DicomNode destinationDicomNode)
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

        public static void UpdateDicomHeaders(string filepath, DicomTagCollection tags, DicomNewObjectType dicomNewObjectType)
        {
            var dcmFile = new DicomFile(filepath);
            dcmFile.Load(filepath);

            switch (dicomNewObjectType)
            {
                case DicomNewObjectType.Anonymized:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Patient, true);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Study);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Series);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Image);
                    break;
                case DicomNewObjectType.SiteDetailsRemoved:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Site, true);
                    break;
                case DicomNewObjectType.CareProviderDetailsRemoved:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.CareProvider, true);
                    break;
                case DicomNewObjectType.NewPatient:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Patient);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Study);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Series);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Image);
                    break;
                case DicomNewObjectType.NewStudy:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Study);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Series);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Image);
                    break;
                case DicomNewObjectType.NewSeries:
                    tags = UpdateUidsForNewSeries(tags);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Series);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Image);
                    break;
                case DicomNewObjectType.NewImage:
                    tags = UpdateUidsForNewImage(tags);
                    dcmFile = UpdateTags(dcmFile, tags, DicomTag.TagType.Image);
                    break;
                case DicomNewObjectType.NoChange:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dicomNewObjectType), dicomNewObjectType, null);
            }
            dcmFile.Save(filepath);
        }
        
        private static DicomFile UpdateTags(DicomFile dcmFile, DicomTagCollection newTags, DicomTag.TagType tagType, bool overwriteIfNotProvided = false)
        {
            if (newTags == null) return dcmFile;
            newTags.ToList().ForEach(tag => {
                if (tag.DicomTagType == tagType)
                    dcmFile = UpdateTag(dcmFile, tag, overwriteIfNotProvided);
            });
            return dcmFile;
        }
        private static DicomFile UpdateTag(DicomFile dcmFile, DicomTag newTag, bool overwriteIfNotProvided = false)
        {
            if (newTag.Values == null && !overwriteIfNotProvided) return dcmFile;
            var value = newTag.Values != null ? newTag.Values[0] : "";
            return UpdateTag(dcmFile, newTag, value);
        }
        private static DicomFile UpdateTag(DicomFile dcmFile, DicomTag newTag, string value)
        {
            if (newTag.GetValueType() == typeof(string[])) dcmFile.DataSet[newTag.GetTagValue()].Values = new [] { value };
            else if (newTag.GetValueType() == typeof(string)) dcmFile.DataSet[newTag.GetTagValue()].Values = value;
            return dcmFile;
        }

        private static DicomTagCollection UpdateUidsForNewStudy(DicomTagCollection tags)
        {
            if (tags == null) return null;
            tags.StudyUid.Values = new[] { GetNewStudyUid() };
            tags = UpdateUidsForNewSeries(tags);
            tags = UpdateUidsForNewImage(tags);
            return tags;
        }
        private static DicomTagCollection UpdateUidsForNewSeries(DicomTagCollection tags)
        {
            if (tags == null) return null;
            tags.SeriesUid.Values = new[] { GetNewSeriesUid() };
            tags = UpdateUidsForNewImage(tags);
            return tags;
        }
        private static DicomTagCollection UpdateUidsForNewImage(DicomTagCollection tags)
        {
            if (tags == null) return null;
            tags.ImageUid.Values = new [] { GetNewImageUid() };
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
    }

    public enum DicomNewObjectType
    {
        NoChange,
        NewStudy,
        NewSeries,
        NewImage,
        NewPatient,
        Anonymized,
        SiteDetailsRemoved,
        CareProviderDetailsRemoved
    }

    public class DicomTagCollection : IEnumerable<DicomTag> {
        public DicomTag PatientName { get; }
        public DicomTag PatientId { get; }
        public DicomTag PatientBirthDate { get; }
        public DicomTag PatientSex { get; }
        public DicomTag StudyAccessionNumber { get; }
        public DicomTag StudyDescription { get; }
        public DicomTag StudyUid { get; }
        public DicomTag SeriesDescription { get; }
        public DicomTag SeriesUid { get; }
        public DicomTag ImageUid { get; }
        public DicomTag RequestingService { get; }
        public DicomTag InstitutionName { get; }
        public DicomTag InstitutionAddress { get; }
        public DicomTag InstitutionalDepartmentName { get; }
        public DicomTag ReferringPhysician { get; }
        public DicomTag RequestingPhysician { get; }
        public DicomTag PhysiciansOfRecord { get; }
        public DicomTag PerformingPhysiciansName { get; }

        public DicomTagCollection()
        {
            PatientName = new DicomTag("Patient's Name", 1048592, DicomTag.TagType.Patient, typeof(string[]));
            PatientId = new DicomTag ("Patient's Id", 1048608, DicomTag.TagType.Patient, typeof(string[]));
            PatientBirthDate = new DicomTag ("Patient's Birth Date", 1048624, DicomTag.TagType.Patient, typeof(string[]));
            PatientSex = new DicomTag ("Patient's Sex", 1048640, DicomTag.TagType.Patient, typeof(string[]));
            StudyAccessionNumber = new DicomTag ("Study Accession Number", 524368, DicomTag.TagType.Study, typeof(string[]));
            StudyDescription = new DicomTag ("Requested Procedure Description", 3280992, DicomTag.TagType.Study, typeof(string[]));
            StudyUid = new DicomTag ("Study Instance UID", 2097165, DicomTag.TagType.Study, typeof(string[]));
            SeriesUid = new DicomTag ("Series Instance UID", 2097166, DicomTag.TagType.Series, typeof(string[]));
            SeriesDescription = new DicomTag ("Series Description", 528446, DicomTag.TagType.Series, typeof(string[]));
            ImageUid = new DicomTag ("Sop Instance Id", 524312, DicomTag.TagType.Image, typeof(string[]));
            RequestingService = new DicomTag ("Requesting Service", 3280947, DicomTag.TagType.Site, typeof(string[])); // e.g. RMH
            InstitutionName = new DicomTag("Institution Name", 524416, DicomTag.TagType.Site, typeof(string[]));
            InstitutionAddress = new DicomTag("Institution Address", 524417, DicomTag.TagType.Site, typeof(string));
            InstitutionalDepartmentName = new DicomTag("Institutional Department Name", 528448, DicomTag.TagType.Site, typeof(string[]));
            ReferringPhysician = new DicomTag ("Referring Physician's Name", 524432, DicomTag.TagType.CareProvider, typeof(string[]));
            RequestingPhysician = new DicomTag ("Requesting Physician", 3280946, DicomTag.TagType.CareProvider, typeof(string[]));
            PhysiciansOfRecord = new DicomTag ("Physician(s) of Record", 528456, DicomTag.TagType.CareProvider, typeof(string[]));
            PerformingPhysiciansName = new DicomTag("Performing Physician's Name", 528464, DicomTag.TagType.CareProvider, typeof(string[]));
        }

        public IEnumerator<DicomTag> GetEnumerator()
        {
            var dicomTags = GetType().GetProperties()
                .Select(propertyInfo => (DicomTag) propertyInfo.GetValue(this)).ToList();
            return dicomTags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class DicomTag
    {
        private string Name { get; }
        private uint TagValue { get; }
        public string[] Values { get; set; }
        private Type ValueType { get; }
        public TagType DicomTagType { get; }
        
        public DicomTag(string name, uint tagValue, TagType dicomTagType, Type valueType)
        {
            Name = name;
            TagValue = tagValue;
            DicomTagType = dicomTagType;
            ValueType = valueType;
        }

        public string GetName()
        {
            return Name;
        }
        public uint GetTagValue()
        {
            return TagValue;
        }
        public Type GetValueType()
        {
            return ValueType;
        }

        public enum TagType
        {
            Site,
            CareProvider,
            Patient,
            Study,
            Series,
            Image,
            All
        }
    }
}
