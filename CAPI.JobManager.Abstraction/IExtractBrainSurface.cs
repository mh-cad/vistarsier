using CAPI.Dicom.Abstraction;

namespace CAPI.JobManager.Abstraction
{
    public interface IExtractBrainSurface : IIntegratedProcess
    {
        IDicomSeries DicomSeries { get; set; }
        string InputHdrFileFullPath { get; set; }
        string BrainMaskRemovedHdrFilePath { get; set; }
        string BrainMaskHdrFilePath { get; set; }

        void Run(out string brainMaskRemoved, out string brainMask);
    }
}