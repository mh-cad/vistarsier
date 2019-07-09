using System;
using System.Collections.Generic;
using VisTarsier.Config;

namespace VisTarsier.Dicom.Abstractions
{
    public interface IDicomStudy
    {
        string AccessionNumber { get; set; }
        string Region { get; set; }
        string StudyDescription { get; set; }
        string StudyInstanceUid { get; set; }
        DateTime? StudyDate { get; set; }

        string PatientId { get; set; }
        string PatientsName { get; set; }
        DateTime PatientBirthDate { get; set; }
        string PatientsSex { get; set; }

        IList<IDicomSeries> Series { get; set; }
    }
}
