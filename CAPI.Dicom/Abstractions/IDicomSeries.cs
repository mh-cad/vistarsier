﻿using System.Collections.Generic;

namespace CAPI.Dicom.Abstractions
{
    public interface IDicomSeries
    {
        string SeriesInstanceUid { get; set; }
        string SeriesDescription { get; set; }
        string StudyInstanceUid { get; set; }
        IEnumerable<IDicomImage> Images { get; set; }
    }
}