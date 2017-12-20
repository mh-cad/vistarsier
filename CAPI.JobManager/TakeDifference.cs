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


        public event EventHandler<IProcessEventArgument> OnComplete;

        // Constructor
        public TakeDifference()
        {
            Id = "3";
            Version = "1";
        }

        public TakeDifference(string[] parameters) : this()
        {
            Parameters = parameters;
        }

        public IJob<IRecipe> Run(IJob<IRecipe> jobToBeProcessed)
        {
            var handler = OnComplete;
            handler?.Invoke(this, new ProcessEventArgument(
                $"Take Difference process is completed [Version: {Version}] [Parameters: {string.Join(" ", Parameters)}]"));

            throw new NotImplementedException();
        }
    }
}