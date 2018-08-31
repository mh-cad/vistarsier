﻿using CAPI.Dicom.Abstractions;
using System.Collections.Generic;

namespace CAPI.Dicom
{
    public class DicomSeries : IDicomSeries
    {
        public string SeriesInstanceUid { get; set; }
        public string SeriesDescription { get; set; }
        public string StudyInstanceUid { get; set; }
        public IEnumerable<IDicomImage> Images { get; set; }

        public DicomSeries()
        {
            Images = new List<IDicomImage>();
        }
    }
}