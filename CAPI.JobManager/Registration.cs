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


        public event EventHandler<IProcessEventArgument> OnComplete;

        // Constructor
        public Registration()
        {
            Id = "2";
            Version = "1";
        }

        public Registration(string[] parameters) : this()
        {
            Parameters = parameters;
        }

        public IJob<IRecipe> Run(IJob<IRecipe> jobToBeProcessed)
        {
            var handler = OnComplete;
            handler?.Invoke(this, new ProcessEventArgument(
                $"Registration is completed [Version: {Version}] [Parameters: {string.Join(" ", Parameters)}]"));

            throw new NotImplementedException();
        }
    }
}