using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CAPI.Dicom.Abstraction;

namespace CAPI.Dicom.Model
{
    public class DicomTagCollection : IDicomTagCollection
    {
        public IDicomTag PatientName { get; }
        public IDicomTag PatientId { get; }
        public IDicomTag PatientBirthDate { get; }
        public IDicomTag PatientSex { get; }
        public IDicomTag StudyAccessionNumber { get; }
        public IDicomTag StudyDescription { get; }
        public IDicomTag StudyInstanceUid { get; }
        public IDicomTag SeriesDescription { get; }
        public IDicomTag SeriesInstanceUid { get; }
        public IDicomTag ImageUid { get; }
        public IDicomTag RequestingService { get; }
        public IDicomTag InstitutionName { get; }
        public IDicomTag InstitutionAddress { get; }
        public IDicomTag InstitutionalDepartmentName { get; }
        public IDicomTag ReferringPhysician { get; }
        public IDicomTag RequestingPhysician { get; }
        public IDicomTag PhysiciansOfRecord { get; }
        public IDicomTag PerformingPhysiciansName { get; }

        public DicomTagCollection()
        {
            PatientName = new DicomTag("Patient's Description", 1048592, TagType.Patient, typeof(string[]));
            PatientId = new DicomTag ("Patient's Id", 1048608, TagType.Patient, typeof(string[]));
            PatientBirthDate = new DicomTag ("Patient's Birth Date", 1048624, TagType.Patient, typeof(string[]));
            PatientSex = new DicomTag ("Patient's Sex", 1048640, TagType.Patient, typeof(string[]));
            StudyAccessionNumber = new DicomTag ("Study Accession Number", 524368, TagType.Study, typeof(string[]));
            StudyDescription = new DicomTag ("Requested Procedure Description", 3280992, TagType.Study, typeof(string[]));
            StudyInstanceUid = new DicomTag ("Study Instance UID", 2097165, TagType.Study, typeof(string[]));
            SeriesInstanceUid = new DicomTag ("Series Instance UID", 2097166, TagType.Series, typeof(string[]));
            SeriesDescription = new DicomTag ("Series Description", 528446, TagType.Series, typeof(string[]));
            ImageUid = new DicomTag ("Sop Instance Id", 524312, TagType.Image, typeof(string[]));
            RequestingService = new DicomTag ("Requesting Service", 3280947, TagType.Site, typeof(string[])); // e.g. RMH
            InstitutionName = new DicomTag("Institution Description", 524416, TagType.Site, typeof(string[]));
            InstitutionAddress = new DicomTag("Institution Address", 524417, TagType.Site, typeof(string));
            InstitutionalDepartmentName = new DicomTag("Institutional Department Description", 528448, TagType.Site, typeof(string[]));
            ReferringPhysician = new DicomTag ("Referring Physician's Description", 524432, TagType.CareProvider, typeof(string[]));
            RequestingPhysician = new DicomTag ("Requesting Physician", 3280946, TagType.CareProvider, typeof(string[]));
            PhysiciansOfRecord = new DicomTag ("Physician(s) of Record", 528456, TagType.CareProvider, typeof(string[]));
            PerformingPhysiciansName = new DicomTag("Performing Physician's Description", 528464, TagType.CareProvider, typeof(string[]));
        }

        public IEnumerator<IDicomTag> GetEnumerator()
        {
            var dicomTags = GetType().GetProperties()
                .Select(propertyInfo => (IDicomTag) propertyInfo.GetValue(this)).ToList();
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