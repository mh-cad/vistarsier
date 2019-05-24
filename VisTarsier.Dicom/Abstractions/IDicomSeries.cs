using System.Collections.Generic;

namespace VisTarsier.Dicom.Abstractions
{
    public interface IDicomSeries
    {
        string SeriesInstanceUid { get; set; }
        string SeriesDescription { get; set; }
        string StudyInstanceUid { get; set; }
        IEnumerable<IDicomImage> Images { get; set; }
    }
}