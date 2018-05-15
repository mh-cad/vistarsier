using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class IntegratedProcess : IIntegratedProcess
    {
        public IntegratedProcessType Type { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }

        public IJob<IRecipe> Run(IJob<IRecipe> jobToBeProcessed)
        {
            throw new NotImplementedException();
        }

        public IJobNew<IRecipe> Run(IJobNew<IRecipe> jobToBeProcessed)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<IProcessEventArgument> OnStart;
        public event EventHandler<IProcessEventArgument> OnComplete;
    }
}
