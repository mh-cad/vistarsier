using CAPI.Dicom.Abstraction;

namespace CAPI.JobManager.Abstraction
{
    public interface IExtractBrainSurface : IIntegratedProcess
    {
        IDicomSeries DicomSeries { get; set; }
        string BrainMaskRemovedHdrFilePath { get; set; }
        string BrainMaskHdrFilePath { get; set; }
    }
}