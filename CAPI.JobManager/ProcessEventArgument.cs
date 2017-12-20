using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    public class ProcessEventArgument : EventArgs, IProcessEventArgument
    {
        public string LogContent { get; set; }

        public ProcessEventArgument(string logContent)
        {
            LogContent = logContent;
        }
    }
}