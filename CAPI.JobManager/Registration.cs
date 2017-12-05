using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    public class Registration : IRegistration
    {
        public IntegratedProcessType Type
        {
            get => IntegratedProcessType.Registeration;
            set { }
        }
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }
        public event EventHandler<ProcessEventArgument> OnComplete;

        // Constructor
        public Registration(string[] parameters)
        {
            Id = "2";
            Version = "1";
            Parameters = parameters;
        }

        public void Run()
        {
            var handler = OnComplete;
            handler?.Invoke(this, new ProcessEventArgument(
                $"Registration is completed [Version: {Version}] [Parameters: {string.Join(" ", Parameters)}]"));
        }
    }
}