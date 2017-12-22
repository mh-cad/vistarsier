using CAPI.ImageProcessing.Abstraction;

namespace CAPI.JobManager.Abstraction
{
    public interface IJobSeriesBundle
    {
        IJobSeries Original { get; set; }
        IJobSeries BrainMask { get; set; }
        IJobSeries Transformed { get; set; }
        IFrameOfReference FrameOfReference { get; set; }
    }
}