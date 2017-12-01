using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    public class ColorMap : IColorMap
    {
        private string[] _parameters;

        public IntegratedProcessType Type
        {
            get => IntegratedProcessType.ColorMap;
            set => value = IntegratedProcessType.ColorMap;
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
            Init(parameters);
        }

        public void Init()
        {
            throw new System.NotImplementedException();
        }

        public void Init(params string[] parameters)
        {
            _parameters = parameters;
        }

        public void Run()
        {
            var handler = OnComplete;
            handler?.Invoke(this, new ProcessEventArgument("ColorMap process is completed"));
        }
    }
}