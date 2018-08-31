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
        IDicomTag RequestingService { get; }
        IDicomTag InstitutionName { get; }
        IDicomTag InstitutionAddress { get; }
        IDicomTag InstitutionalDepartmentName { get; }
        IDicomTag ReferringPhysician { get; }
        IDicomTag RequestingPhysician { get; }
        IDicomTag PhysiciansOfRecord { get; }
        IDicomTag PerformingPhysiciansName { get; }

        void SetTagValue(uint tagValue, object value);
    }
}