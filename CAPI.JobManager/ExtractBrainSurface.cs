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

        /// <summary>
        /// Extracts Brain Mask and output Brain Mask as well as Brain-Mask-Removed series as hdr files and add back to job
        /// </summary>
        /// <param name="jobSeriesBundle">Job to be processed</param>
        /// <returns>back the job being processed now containing hdr files 1. Brain-Mask-Removed series 2. Brain Mask</returns>
        private IJobSeriesBundle ExtractBrainMask(IJobSeriesBundle jobSeriesBundle)
        {
            var hdrFileFullPath = jobSeriesBundle.Original.HdrFileFullPath;
            var outputPath = Path.GetDirectoryName(hdrFileFullPath);

            // Extract Brain Mask and output Brain Mask as well as Brain-Mask-Removed series as hdr files and add back to job
            _imageProcessor.ExtractBrainMask(hdrFileFullPath, outputPath,
                out var brainMaskRemoved, out var brainMask);

            // Add hdr file path for Brain-Mask-Removed series to each JobSeriesBundle
            jobSeriesBundle.Transformed.HdrFileFullPath = outputPath + "\\" + brainMaskRemoved;
            jobSeriesBundle.Transformed.CompletedProcesses.Add(IntegratedProcessType.ExtractBrainSurface);

            // Add hdr file path for Brain Mask to each JobSeriesBundle
            jobSeriesBundle.BrainMask.HdrFileFullPath = outputPath + "\\" + brainMask;
            jobSeriesBundle.BrainMask.CompletedProcesses.Add(IntegratedProcessType.ExtractBrainSurface);

            return jobSeriesBundle;
        }
    }
}