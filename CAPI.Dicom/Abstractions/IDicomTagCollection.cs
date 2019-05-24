using System.Collections.Generic;

namespace CAPI.Dicom.Abstractions
{
    public interface IDicomTagCollection : IEnumerable<IDicomTag>
    {
        IDicomTag PatientName { get; }
        IDicomTag PatientId { get; }
        IDicomTag PatientBirthDate { get; }
        IDicomTag PatientSex { get; }
        IDicomTag StudyAccessionNumber { get; }
        IDicomTag StudyDescription { get; }
        IDicomTag StudyInstanceUid { get; }
        IDicomTag StudyDate { get; }
        IDicomTag SeriesDescription { get; }
        IDicomTag SeriesInstanceUid { get; }
        IDicomTag ImageUid { get; }
        IDicomTag InstanceNumber { get; }
        IDicomTag ImagePositionPatient { get; }
        IDicomTag ImageOrientation { get; }
        IDicomTag FrameOfReferenceUid { get; }
        IDicomTag SliceLocation { get; }
        IDicomTag RequestingService { get; }
        IDicomTag InstitutionName { get; }
        IDicomTag InstitutionAddress { get; }
        IDicomTag InstitutionalDepartmentName { get; }
        IDicomTag ReferringPhysician { get; }
        IDicomTag RequestingPhysician { get; }
        IDicomTag PhysiciansOfRecord { get; }
        IDicomTag PerformingPhysiciansName { get; }
        // About the scanner
        IDicomTag Modality { get; }
        IDicomTag Manufacturer { get; }
        IDicomTag ManufacturersModelName { get; }
        IDicomTag DeviceSerialNumber { get; }
        IDicomTag SoftwareVersion { get; }
        // Scanner settings
        IDicomTag EchoTime { get; }
        IDicomTag InversionTime { get; }
        IDicomTag ImagedNucleus { get; }
        IDicomTag MagneticFieldStrength { get; }
        IDicomTag EchoTrainLength { get; }
        IDicomTag TransmitCoilName { get; }
        IDicomTag ProtocolName { get; }
        IDicomTag ScanOptions { get; }

        void SetTagValue(uint tagValue, object value);
    }
}