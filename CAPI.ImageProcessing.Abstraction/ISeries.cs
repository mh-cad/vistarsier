using CAPI.Dicom.Abstraction;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface ISeries
    {
        IDicomStudy ParentDicomStudy { get; set; }
        string BmpFolderPath { get; set; }
        string DicomFolderPath { get; set; }
        string HdrFileFullPath { get; set; }
        string NiiFileFullPath { get; set; }
    }
}