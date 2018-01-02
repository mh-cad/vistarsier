using CAPI.ImageProcessing.Abstraction;
using System.Collections.Generic;

namespace CAPI.JobManager.Abstraction
{
    public interface IJobSeries : ISeries
    {

        ICollection<IntegratedProcessType> CompletedProcesses { get; set; }

    }
}