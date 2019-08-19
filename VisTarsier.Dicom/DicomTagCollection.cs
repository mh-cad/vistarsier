using VisTarsier.Dicom.Abstractions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VisTarsier.Dicom.Model
{
    public class DicomTagCollection : IEnumerable<IDicomTag>
    {
        public IDicomTag PatientName { get; }
        public IDicomTag PatientId { get; }
        public IDicomTag PatientBirthDate { get; }
        public IDicomTag PatientSex { get; }
        public IDicomTag PatientAge { get; }
        public IDicomTag StudyAccessionNumber { get; }
        public IDicomTag StudyDescription { get; }
        public IDicomTag StudyInstanceUid { get; }
        public IDicomTag StudyDate { get; }
        public IDicomTag SeriesDescription { get; }
        public IDicomTag SeriesDate { get; }
        public IDicomTag SeriesTime { get; }
        public IDicomTag AcquisitionDate { get; }
        public IDicomTag AcquisitionTime { get; }
        public IDicomTag SeriesInstanceUid { get; }
        public IDicomTag ImageUid { get; }
        public IDicomTag InstanceNumber { get; }
        public IDicomTag ImagePositionPatient { get; }
        public IDicomTag ImageOrientation { get; }
        public IDicomTag FrameOfReferenceUid { get; }
        public IDicomTag SliceLocation { get; }
        public IDicomTag RequestingService { get; }
        public IDicomTag InstitutionName { get; }
        public IDicomTag InstitutionAddress { get; }
        public IDicomTag InstitutionalDepartmentName { get; }
        public IDicomTag ReferringPhysician { get; }
        public IDicomTag RequestingPhysician { get; }
        public IDicomTag PhysiciansOfRecord { get; }
        public IDicomTag PerformingPhysiciansName { get; }
        public IDicomTag Modality { get; }
        public IDicomTag Manufacturer { get; }
        public IDicomTag ManufacturersModelName { get; }
        public IDicomTag DeviceSerialNumber { get; }
        public IDicomTag SoftwareVersion { get; }
        public IDicomTag EchoTime { get; }
        public IDicomTag InversionTime { get; }
        public IDicomTag ImagedNucleus { get; }
        public IDicomTag MagneticFieldStrength { get; }
        public IDicomTag EchoTrainLength { get; }
        public IDicomTag TransmitCoilName { get; }
        public IDicomTag ProtocolName { get; }
        public IDicomTag ScanOptions { get; }

        public DicomTagCollection()
        {
            // Patient info
            PatientName = new DicomTag("Patient's Description", 1048592, TagType.Patient, typeof(string[]));
            PatientId = new DicomTag("Patient's Id", 1048608, TagType.Patient, typeof(string[]));
            PatientBirthDate = new DicomTag("Patient's Birth Date", 1048624, TagType.Patient, typeof(string[]));
            PatientSex = new DicomTag("Patient's Sex", 1048640, TagType.Patient, typeof(string[]));
            PatientAge = new DicomTag("Patient's Age", 0x00101010, TagType.Patient, typeof(string[]));
            // Study info
            StudyAccessionNumber = new DicomTag("Study Accession Number", 524368, TagType.Study, typeof(string[]));
            StudyDescription = new DicomTag("Requested Procedure Description", 3280992, TagType.Study, typeof(string[]));
            StudyDate = new DicomTag("Study Date", 524320, TagType.Study, typeof(string[]));
            StudyInstanceUid = new DicomTag("Study Instance UID", 2097165, TagType.Study, typeof(string[]));
            // Series info
            SeriesInstanceUid = new DicomTag("Series Instance UID", 2097166, TagType.Series, typeof(string[]));
            SeriesDescription = new DicomTag("Series Description", 528446, TagType.Series, typeof(string[]));
            SeriesDate = new DicomTag("Series Date", 0x00080021, TagType.Series, typeof(string[]));
            SeriesTime = new DicomTag("Series Time", 0x00080031, TagType.Series, typeof(string[]));
            AcquisitionDate = new DicomTag("Acquisition Date", 0x00080022, TagType.Series, typeof(string[]));
            AcquisitionTime = new DicomTag("Acquisition Time", 0x00080032, TagType.Series, typeof(string[]));
            Modality = new DicomTag("Modality", 0x00080060, TagType.Series, typeof(string[]));
            Manufacturer = new DicomTag("Manufacturer", 0x00080070, TagType.Series, typeof(string[]));
            ManufacturersModelName = new DicomTag("Manufacturer's model name", 0x00081090, TagType.Series, typeof(string[]));
            EchoTime = new DicomTag("Echo Time", 0x00180081, TagType.Series, typeof(string[]));
            EchoTrainLength = new DicomTag("Echo Train Length", 0x00180091, TagType.Series, typeof(string[]));
            InversionTime = new DicomTag("Inversion Time", 0x00180082, TagType.Series, typeof(string[]));
            ImagedNucleus = new DicomTag("Imaged Nucleus", 0x00180085, TagType.Series, typeof(string[]));
            MagneticFieldStrength = new DicomTag("Magnetic Field Strength", 0x00180087, TagType.Series, typeof(string[]));
            EchoTrainLength = new DicomTag("Echo Train Length", 0x00180091, TagType.Series, typeof(string[]));
            DeviceSerialNumber = new DicomTag("Device Serial Number", 0x00181000, TagType.Series, typeof(string[]));
            SoftwareVersion = new DicomTag("Software Version(s)", 0x00180020, TagType.Series, typeof(string[]));
            ProtocolName = new DicomTag("Protocol Name", 0x00181030, TagType.Series, typeof(string[]));
            TransmitCoilName = new DicomTag("Transmit Coil Name", 0x00181251, TagType.Series, typeof(string[]));
            ScanOptions = new DicomTag("Scan Options", 0x00180022, TagType.Series, typeof(string[]));
            // Image info
            ImageUid = new DicomTag("Sop Instance Id", 524312, TagType.Image, typeof(string[]));
            InstanceNumber = new DicomTag("Instance Number", 2097171, TagType.Image, typeof(string[]));
            ImagePositionPatient = new DicomTag("Image Position (Patient)", 2097202, TagType.Image, typeof(string[]));
            ImageOrientation = new DicomTag("Image Orientation", 0x200037, TagType.Image, typeof(string[]));
            FrameOfReferenceUid = new DicomTag("Frame Of Reference Uid", 2097234, TagType.Image, typeof(string[]));
            SliceLocation = new DicomTag("Slice Location", 2101313, TagType.Image, typeof(string[]));
            // Site info
            RequestingService = new DicomTag("Requesting Service", 3280947, TagType.Site, typeof(string[])); // e.g. RMH
            InstitutionName = new DicomTag("Institution Description", 524416, TagType.Site, typeof(string[]));
            InstitutionAddress = new DicomTag("Institution Address", 524417, TagType.Site, typeof(string));
            InstitutionalDepartmentName = new DicomTag("Institutional Department Description", 528448, TagType.Site, typeof(string[]));
            // Care provider info
            ReferringPhysician = new DicomTag("Referring Physician's Description", 524432, TagType.CareProvider, typeof(string[]));
            RequestingPhysician = new DicomTag("Requesting Physician", 3280946, TagType.CareProvider, typeof(string[]));
            PhysiciansOfRecord = new DicomTag("Physician(s) of Record", 528456, TagType.CareProvider, typeof(string[]));
            PerformingPhysiciansName = new DicomTag("Performing Physician's Description", 528464, TagType.CareProvider, typeof(string[]));
        }

        public IEnumerator<IDicomTag> GetEnumerator()
        {
            var dicomTags = GetType().GetProperties()
                .Select(propertyInfo => (IDicomTag)propertyInfo.GetValue(this)).ToList();
            return dicomTags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void SetTagValue(uint tagValue, object value)
        {
            if (value == null) return;
            foreach (var propertyInfo in GetType().GetProperties())
                if (((DicomTag)propertyInfo.GetValue(this)).GetTagValue() == tagValue)
                    if (((DicomTag)propertyInfo.GetValue(this)).GetValueType() == typeof(string[]))
                        ((DicomTag)propertyInfo.GetValue(this)).Values = (string[])value;
                    else if (((DicomTag)propertyInfo.GetValue(this)).GetValueType() == typeof(string))
                        ((DicomTag)propertyInfo.GetValue(this)).Values = new[] { value.ToString() };
        }

        /// <summary>
        /// Copies the values of any tags in the given collection to this collection, overwriting as it goes.
        /// </summary>
        /// <param name="collection"></param>
        public void Merge(DicomTagCollection collection, TagType type = TagType.All)
        {
            foreach (var tag in collection)
            {
                if (type == TagType.All || tag.DicomTagType == type) SetTagValue(tag.GetTagValue(), tag.Values);
            }
        }
    }
}