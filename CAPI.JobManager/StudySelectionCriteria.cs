using CAPI.JobManager.Abstraction;

namespace CAPI.JobManager
{
    public class StudySelectionCriteria : IStudySelectionCriteria
    {
        public string AccessionNumber { get; set; }
        public string Region { get; set; }
        public string StudyDescription { get; set; }
        public StringOperand StudyDescriptionOperand { get; set; }
        public string StudyDate { get; set; }
        public DateOperand StudyDateOperand { get; set; }
        public string SeriesDescription { get; set; }
        public StringOperand SeriesDescriptionOperand { get; set; }
    }
}