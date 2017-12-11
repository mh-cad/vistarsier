using CAPI.Dicom.Abstraction;
using System;
using System.Collections.Generic;

namespace CAPI.Dicom
{
    public class DicomStudy : IDicomStudy
    {
        public string AccessionNumber { get; set; }
        public string Region { get; set; }
        public string StudyDescription { get; set; }
        public string StudyInstanceUid { get; set; }
        public DateTime? StudyDate { get; set; }

        public string PatientId { get; set; }
        public string PatientsName { get; set; }
        public DateTime PatientBirthDate { get; set; }
        public string PatientsSex { get; set; }

        public IList<IDicomSeries> Series { get; set; }

        public DicomStudy()
        {
            Series = new List<IDicomSeries>();
        }
    }
}