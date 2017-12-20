using System;

namespace CAPI.JobManager
{
    public interface ILogEventArgument
    {
        string LogContent { get; set; }
        Exception Exception { get; set; }
    }
}