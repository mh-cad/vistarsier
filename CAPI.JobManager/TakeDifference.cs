using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    public class TakeDifference : ITakeDifference
    {
        public IntegratedProcessType Type
        {
            get => IntegratedProcessType.TakeDifference;
            set { }
        }
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }
        public event EventHandler<ProcessEventArgument> OnComplete;

        // Constructor
        public TakeDifference(string[] parameters)
        {
            Id = "3";
            Version = "1";
            Parameters = parameters;
        }

        public void Run()
        {
            var handler = OnComplete;
            handler?.Invoke(this, new ProcessEventArgument(
                $"Take Difference process is completed [Version: {Version}] [Parameters: {string.Join(" ", Parameters)}]"));
        }
    }
}