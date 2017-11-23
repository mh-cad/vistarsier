using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.Dicom.Model
{
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
            PatientName = new DicomTag("Patient's Description", 1048592, DicomTag.TagType.Patient, typeof(string[]));
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
            InstitutionName = new DicomTag("Institution Description", 524416, DicomTag.TagType.Site, typeof(string[]));
            InstitutionAddress = new DicomTag("Institution Address", 524417, DicomTag.TagType.Site, typeof(string));
            InstitutionalDepartmentName = new DicomTag("Institutional Department Description", 528448, DicomTag.TagType.Site, typeof(string[]));
            ReferringPhysician = new DicomTag ("Referring Physician's Description", 524432, DicomTag.TagType.CareProvider, typeof(string[]));
            RequestingPhysician = new DicomTag ("Requesting Physician", 3280946, DicomTag.TagType.CareProvider, typeof(string[]));
            PhysiciansOfRecord = new DicomTag ("Physician(s) of Record", 528456, DicomTag.TagType.CareProvider, typeof(string[]));
            PerformingPhysiciansName = new DicomTag("Performing Physician's Description", 528464, DicomTag.TagType.CareProvider, typeof(string[]));
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

        public void SetTagValue(uint tagValue, object value)
        {
            foreach (var propertyInfo in GetType().GetProperties())
                if (((DicomTag) propertyInfo.GetValue(this)).GetTagValue() == tagValue)
                    if (((DicomTag)propertyInfo.GetValue(this)).GetValueType() == typeof(string[]))
                        ((DicomTag) propertyInfo.GetValue(this)).Values = (string[])value;
                    else if (((DicomTag)propertyInfo.GetValue(this)).GetValueType() == typeof(string))
                        ((DicomTag)propertyInfo.GetValue(this)).Values = new [] { value.ToString() };
        }
    }
}