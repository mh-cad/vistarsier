namespace CAPI.BLL.Model
{
    public interface ISeries
    {
        string Name { get; set; }
        string FileFullPath { get; set; }
        string FolderPath { get; set; }
        int NumberOfImages { get; set; }
    }
}