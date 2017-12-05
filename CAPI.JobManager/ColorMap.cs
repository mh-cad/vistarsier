using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    public class ColorMap : IColorMap
    {
        public IntegratedProcessType Type
        {
            get => IntegratedProcessType.ColorMap;
            set { }
        }
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }
        public event EventHandler<ProcessEventArgument> OnComplete;

        // Constructor
        public ColorMap(string[] parameters)
        {
            Id = "4";
            Version = "1";
            Parameters = parameters;
        }

        public void Run()
        {
            var handler = OnComplete;
            handler?.Invoke(this, new ProcessEventArgument(
                $"ColorMap process is completed [Version: {Version}] [Parameters: {string.Join(" ", Parameters)}]"));
        }
    }
}