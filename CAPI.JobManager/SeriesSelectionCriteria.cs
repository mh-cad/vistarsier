﻿using CAPI.JobManager.Abstraction;

namespace CAPI.JobManager
{
    public class SeriesSelectionCriteria : ISeriesSelectionCriteria
    {
        public bool MostRecentPriorStudy { get; set; }
        public bool OldestPriorStudy { get; set; }
        public string CutOffPeriodValueInMonths { get; set; }
        public string Region { get; set; }
        public string StudyDescription { get; set; }
        public StringOperand StudyDescriptionOperand { get; set; }
        public string StudyDate { get; set; }
        public DateOperand StudyDateOperand { get; set; }
        public string SeriesDescription { get; set; }
        public StringOperand SeriesDescriptionOperand { get; set; }
        public string SeriesDescriptionDelimiter { get; set; }
    }
}