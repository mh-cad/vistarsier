using CAPI.JobManager.Abstraction;

namespace CAPI.JobManager
{
    public class JobSeriesBundle : IJobSeriesBundle
    {
        public IJobSeries Original { get; set; }
        public IJobSeries BrainMask { get; set; }
        public IJobSeries Transformed { get; set; }

        public JobSeriesBundle()
        {
            Original = new JobSeries();
            BrainMask = new JobSeries();
            Transformed = new JobSeries();
        }
    }
}