using CAPI.Dicom.Abstraction;
using System.Collections.Generic;

namespace CAPI.Dicom
{
    public class DicomSeries : IDicomSeries
    {
        public string SeriesInstanceUid { get; set; }
        public string StudyInstanceUid { get; set; }
        public IEnumerable<IDicomImage> Images { get; set; }
    }
}