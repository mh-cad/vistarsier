using System;

namespace CAPI.JobManager.Abstraction
{
    public class ProcessEventArgument : EventArgs
    {
        public string LogContent { get; set; }

        public ProcessEventArgument(string logContent)
        {
            LogContent = logContent;
        }


    }
}
