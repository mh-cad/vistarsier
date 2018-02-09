using CAPI.Dicom.Abstraction;

namespace CAPI.JobManager.Abstraction
{
    public interface IDestination
    {
        string Id { get; set; }
        string FolderPath { get; set; }
        string AeTitle { get; set; }
        IDicomNode DicomNode { get; set; }
    }
}