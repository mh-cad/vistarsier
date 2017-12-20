using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager.Abstraction;
using System;
using System.IO;

namespace CAPI.JobManager
{
    public class ExtractBrainSurface : IIntegratedProcess
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

        public event EventHandler<IProcessEventArgument> OnComplete;

        // Constructor
        public ExtractBrainSurface(IImageProcessor imageProcessor)
        {
            _imageProcessor = imageProcessor;
            Id = "1";
            Version = "1";
        }

        public IJob<IRecipe> Run(IJob<IRecipe> jobToBeProcessed)
        {
            jobToBeProcessed.DicomSeriesFixed =
                ExtractBrainMask(jobToBeProcessed.DicomSeriesFixed);

            jobToBeProcessed.DicomSeriesFloating =
                ExtractBrainMask(jobToBeProcessed.DicomSeriesFloating);

            OnComplete?.Invoke(this, new ProcessEventArgument(
                "Brain Mask Extraction is completed " +
                $"[Version: {Version}] [Parameters: {string.Join(" | ", Parameters)}]"));

            return jobToBeProcessed;
        }

        private IJobSeriesBundle ExtractBrainMask(IJobSeriesBundle jobSeriesBundle)
        {
            var fixedHdrFileFullPath = jobSeriesBundle.Original.HdrFileFullPath;
            var outputPath = Path.GetDirectoryName(fixedHdrFileFullPath);

            _imageProcessor.ExtractBrainMask(fixedHdrFileFullPath, outputPath,
                out var brainMaskRemoved, out var brainMask);

            jobSeriesBundle.Transformed.HdrFileFullPath = outputPath + "\\" + brainMaskRemoved;
            jobSeriesBundle.BrainMask.HdrFileFullPath = outputPath + "\\" + brainMask;

            return jobSeriesBundle;
        }
    }
}