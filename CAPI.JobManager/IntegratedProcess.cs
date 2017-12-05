using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    public class IntegratedProcess : IIntegratedProcess
    {
        public IntegratedProcessType Type { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<ProcessEventArgument> OnComplete;
    }
}
