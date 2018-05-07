using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;

namespace CAPI.JobManager.Abstraction
{
    public interface IJobSeriesBundleNew
    {
        string Title { get; set; }
        string DicomFolderPath { get; set; }
        IDicomStudy ParentDicomStudy { get; set; }
        string NiiFilePath { get; set; }
        string Brain { get; set; }
        string BrainMask { get; set; }
        string Resliced { get; set; }
        IFrameOfReference FrameOfReference { get; set; }
    }
}