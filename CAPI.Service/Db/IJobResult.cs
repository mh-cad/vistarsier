namespace CAPI.Service.Db
{
    public interface IJobResult
    {
        string NiftiFilePath { get; set; }
        string DicomFolderPath { get; set; }
        string ImagesFolderPath { get; set; }
        string LutFilePath { get; set; }
    }
}
