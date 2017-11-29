using CAPI.Dicom.Abstraction;

namespace CAPI.JobManager.Abstraction
{
    public interface IExtractBrainSurface : IIntegratedProcess
    {
        ISeries Series { get; set; }
    }
}