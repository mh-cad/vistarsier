using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager.Abstraction;
using System;
using IExtractBrainSurface = CAPI.JobManager.Abstraction.IExtractBrainSurface;

namespace CAPI.JobManager
{
    public class ExtractBrainSurface : IExtractBrainSurface
    {
        private readonly IImageProcessor _imageProcessor;

        public IntegratedProcessType Type
        {
            get => IntegratedProcessType.ExtractBrainSurface;
            set { }
        }
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }
        public IDicomSeries DicomSeries { get; set; }
        public string InputHdrFileFullPath { get; set; }
        public string OutputPath { get; set; }
        public string BrainMaskRemovedHdrFilePath { get; set; }
        public string BrainMaskHdrFilePath { get; set; }

        public event EventHandler<ProcessEventArgument> OnComplete;

        // Constructor
        public ExtractBrainSurface(IImageProcessor imageProcessor)
        {
            _imageProcessor = imageProcessor;
            Id = "1";
            Version = "1";
        }

        public ExtractBrainSurface(string inputHdrFileFullPath, string outputPath, string parameters,
            IImageProcessor imageProcessing) : this(imageProcessing)
        {
            Parameters[0] = parameters;
            InputHdrFileFullPath = inputHdrFileFullPath;
            OutputPath = outputPath;
        }

        public void Run(out string brainMaskExtracted, out string brainMask)
        {
            _imageProcessor.ExtractBrainMask(InputHdrFileFullPath, OutputPath, out brainMaskExtracted, out brainMask);

            var handler = OnComplete;
            handler?.Invoke(this, new ProcessEventArgument(
                $"Brain Mask Extraction is completed [Version: {Version}] [Parameters: {string.Join(" ", Parameters)}]"));
        }
    }
}