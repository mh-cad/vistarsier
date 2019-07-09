using VisTarsier.Common;

namespace VisTarsier.Config
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SeriesSelectionCriteria
    {
        public byte PriorStudyIndex { get; set; }
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
