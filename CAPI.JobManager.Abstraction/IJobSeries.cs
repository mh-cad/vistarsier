using CAPI.ImageProcessing.Abstraction;
using System.Collections.Generic;

namespace CAPI.JobManager.Abstraction
{
    public interface IJobSeries : ISeries
    {

        IEnumerable<IntegratedProcessType> CompletedProcesses { get; set; }
    }
}