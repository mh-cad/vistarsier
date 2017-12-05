using CAPI.Dicom.Abstraction;
using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    public class ExtractBrainSurface : IExtractBrainSurface
    {
        public IntegratedProcessType Type
        {
            get => IntegratedProcessType.ExtractBrainSurface;
            set { }
        }
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }
        public IDicomSeries DicomSeries { get; set; }
        public event EventHandler<ProcessEventArgument> OnComplete;

        // Constructor
        public ExtractBrainSurface(string[] parameters)
        {
            Id = "1";
            Version = "1";
            Parameters = parameters;
        }

        public void Run()
        {
            var handler = OnComplete;
            handler?.Invoke(this, new ProcessEventArgument(
                $"Brain Mask Extraction is completed [Version: {Version}] [Parameters: {string.Join(" ", Parameters)}]"));
        }
    }
}