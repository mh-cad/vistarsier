using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager.Abstraction;

namespace CAPI.JobManager
{
    public class JobSeriesBundle : IJobSeriesBundle
    {
        public IJobSeries Original { get; set; }
        public IJobSeries BrainMask { get; set; }
        public IJobSeries Transformed { get; set; }
        public IFrameOfReference FrameOfReference { get; set; }

        public JobSeriesBundle()
        {
            Original = new JobSeries();
            BrainMask = new JobSeries();
            Transformed = new JobSeries();
        }
    }
}