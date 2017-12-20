using System;

namespace CAPI.JobManager
{
    public class LogEventArgument : EventArgs, ILogEventArgument
    {
        public string LogContent { get; set; }
        public Exception Exception { get; set; }
    }
}