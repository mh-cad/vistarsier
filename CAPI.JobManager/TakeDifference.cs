using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    public class TakeDifference : ITakeDifference
    {
        private readonly IImageProcessor _imageProcessor;

        public IntegratedProcessType Type
        {
            get => IntegratedProcessType.TakeDifference;
            set { }
        }
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }

        public event EventHandler<IProcessEventArgument> OnStart;
        public event EventHandler<IProcessEventArgument> OnComplete;

        // Constructor
        public TakeDifference(IImageProcessor imageProcessor)
        {
            _imageProcessor = imageProcessor;
            Id = "3";
            Version = "1";
        }

        public IJob<IRecipe> Run(IJob<IRecipe> jobToBeProcessed)
        {
            OnStart?.Invoke(this, new ProcessEventArgument(
                $"Taking Difference between two studies [Version: {Version}] " +
                $"[Parameters: {string.Join(" ", Parameters)}]"));

            jobToBeProcessed = DoTakeDifference(jobToBeProcessed);

            OnComplete?.Invoke(this, new ProcessEventArgument("Take Difference process is completed!"));

            return jobToBeProcessed;
        }

        private IJob<IRecipe> DoTakeDifference(IJob<IRecipe> job)
        {
            var fixedHdrFullPath = job.DicomSeriesFixed.Original.HdrFileFullPath;
            var floatingReslicedNiiFullPath = job.DicomSeriesFloating.Transformed.NiiFileFullPath;
            var brainMaskNiiFullPath = job.DicomSeriesFixed.BrainMask.NiiFileFullPath;

            _imageProcessor.TakeDifference(
                fixedHdrFullPath, floatingReslicedNiiFullPath,
                brainMaskNiiFullPath, job.OutputFolderPath,
                out var darkInFloatingToBrightInFexed, out var brightInFloatingToDarkInFexed,
                out var brainMask);

            job.StructChangesDarkInFloating2BrightInFixed.NiiFileFullPath =
                darkInFloatingToBrightInFexed;

            job.StructChangesBrightInFloating2DarkInFixed.NiiFileFullPath =
                brightInFloatingToDarkInFexed;

            job.StructChangesBrainMask.NiiFileFullPath = brainMask;

            return job;
        }
    }
}