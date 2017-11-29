namespace CAPI.JobManager.Abstraction
{
    public interface IStudySelectionCriteria
    {
        string AccessionNumber { get; set; }
        string Region { get; set; }
        string StudyDescription { get; set; }
        StringOperand StudyDescriptionOperand { get; set; }
        string StudyDate { get; set; }
        DateOperand StudyDateOperand { get; set; }
        string SeriesDescription { get; set; }
        StringOperand SeriesDescriptionOperand { get; set; }
    }
}