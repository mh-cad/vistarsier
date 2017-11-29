namespace CAPI.Domain.Model
{
    public interface ISeries
    {
        string Description { get; set; }
        string FileFullPath { get; set; }
        string FolderPath { get; set; }
        int NumberOfImages { get; set; }
    }
}