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

        // Constructor
        public ExtractBrainSurface(IImageProcessor imageProcessor)
        {
            _imageProcessor = imageProcessor;
            Id = "1";
            Version = "1";
            Parameters = new[] { "" };
        }

        public IJobNew<IRecipe> Run(IJobNew<IRecipe> job)
        {
            OnStart?.Invoke(this, new ProcessEventArgument(
                "Extracting brain surface... " +
                $"[Version: {Version}] [Parameters: {string.Join(" | ", Parameters)}]"));

            job = ExtractBrainMasks(job);

            OnComplete?.Invoke(this, new ProcessEventArgument("Brain Surface Extraction completed."));

            return job;
        }

        private IJobNew<IRecipe> ExtractBrainMasks(IJobNew<IRecipe> job)
        {
            var outputPath = job.OutputFolderPath;

            var @fixed = job.Fixed.NiiFilePath;
            _imageProcessor.ExtractBrainMask(@fixed, outputPath, Parameters[0],
                out var fixedBrainSurfaceRemoved, out var fixedBrainMask);
            _imageProcessor.CopyNiftiImage2PatientTransform($@"{outputPath}\{fixedBrainMask}", @fixed);
            _imageProcessor.CopyNiftiImage2PatientTransform($@"{outputPath}\{fixedBrainSurfaceRemoved}", @fixed);
            job.Fixed.Brain = $@"{outputPath}\{fixedBrainSurfaceRemoved}";
            job.Fixed.BrainMask = $@"{outputPath}\{fixedBrainMask}";

            var floating = job.Floating.NiiFilePath;
            _imageProcessor.ExtractBrainMask(floating, outputPath, Parameters[0],
                out var floatingBrainSurfaceRemoved, out var floatingBrainMask);
            _imageProcessor.CopyNiftiImage2PatientTransform($@"{outputPath}\{floatingBrainMask}", floating);
            _imageProcessor.CopyNiftiImage2PatientTransform($@"{outputPath}\{floatingBrainSurfaceRemoved}", floating);
            job.Floating.Brain = $@"{outputPath}\{floatingBrainSurfaceRemoved}";
            job.Floating.BrainMask = $@"{outputPath}\{floatingBrainMask}";

            return job;
        }

        public IJob<IRecipe> Run(IJob<IRecipe> jobToBeProcessed)
        {
            OnStart?.Invoke(this, new ProcessEventArgument(
                "Extracting brain mask... " +
                $"[Version: {Version}] [Parameters: {string.Join(" | ", Parameters)}]"));

            jobToBeProcessed.DicomSeriesFixed =
                ExtractBrainMask(jobToBeProcessed.DicomSeriesFixed);

            jobToBeProcessed.DicomSeriesFloating =
                ExtractBrainMask(jobToBeProcessed.DicomSeriesFloating);

            OnComplete?.Invoke(this, new ProcessEventArgument("Brain Mask Extraction completed."));

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
            _imageProcessor.ExtractBrainMask(hdrFileFullPath, outputPath, Parameters[0],
                out var brainMaskRemoved, out var brainMask);

            _imageProcessor.CopyNiftiImage2PatientTransform($@"{outputPath}\{brainMask}", hdrFileFullPath);  // Added
            _imageProcessor.CopyNiftiImage2PatientTransform($@"{outputPath}\{brainMaskRemoved}", hdrFileFullPath); // Added

            // Add hdr file path for Brain-Mask-Removed series to each JobSeriesBundle
            jobSeriesBundle.Transformed.NiiFileFullPath = outputPath + "\\" + brainMaskRemoved.Replace(".hdr", ".nii");  // jobSeriesBundle.Transformed.HdrFileFullPath = outputPath + "\\" + brainMaskRemoved;
            jobSeriesBundle.Transformed.CompletedProcesses.Add(IntegratedProcessType.ExtractBrainSurface);

            // Add hdr file path for Brain Mask to each JobSeriesBundle
            jobSeriesBundle.BrainMask.NiiFileFullPath = outputPath + "\\" + brainMask.Replace(".hdr", ".nii");   // jobSeriesBundle.BrainMask.HdrFileFullPath = outputPath + "\\" + brainMask;
            jobSeriesBundle.BrainMask.CompletedProcesses.Add(IntegratedProcessType.ExtractBrainSurface);

            return jobSeriesBundle;
        }

        private IJobSeriesBundle ExtractBrainMaskUsingResize(IJobSeriesBundle jobSeriesBundle)
        {
            var hdrFileFullPath = jobSeriesBundle.Original.HdrFileFullPath;
            var outputPath = Path.GetDirectoryName(hdrFileFullPath);

            // Resize
            const int destinationWidth = 512;
            var resizedNii = hdrFileFullPath.Replace(".hdr", "_resized.nii");
            _imageProcessor.Resize(hdrFileFullPath, resizedNii, destinationWidth);

            // Extract Brain Mask and output Brain Mask as well as Brain-Mask-Removed series as hdr files and add back to job
            _imageProcessor.ExtractBrainMask(resizedNii, outputPath, Parameters[0],
                out var brainMaskRemoved, out var brainMask);

            // Resize Back to original size
            var brainMaskNii = $@"{outputPath}\{brainMask.Replace("_resized", "").Replace(".hdr", ".nii")}";
            //_imageProcessor.ResizeBacktToOriginalSize($@"{outputPath}\{brainMask}", brainMaskNii, hdrFileFullPath);

            var brainMaskRemovedNii = $@"{outputPath}\{brainMaskRemoved.Replace("_resized", "").Replace(".hdr", ".nii")}";
            //_imageProcessor.ResizeBacktToOriginalSize($@"{outputPath}\{brainMaskRemoved}", brainMaskRemovedNii, hdrFileFullPath);

            _imageProcessor.CopyNiftiImage2PatientTransform(brainMaskNii, hdrFileFullPath);
            _imageProcessor.CopyNiftiImage2PatientTransform(brainMaskRemovedNii, hdrFileFullPath);

            // Add hdr file path for Brain-Mask-Removed series to each JobSeriesBundle
            jobSeriesBundle.Transformed.NiiFileFullPath = brainMaskRemovedNii;
            jobSeriesBundle.Transformed.CompletedProcesses.Add(IntegratedProcessType.ExtractBrainSurface);

            // Add hdr file path for Brain Mask to each JobSeriesBundle
            jobSeriesBundle.BrainMask.NiiFileFullPath = brainMaskNii;
            jobSeriesBundle.BrainMask.CompletedProcesses.Add(IntegratedProcessType.ExtractBrainSurface);

            return jobSeriesBundle;
        }

        public event EventHandler<IProcessEventArgument> OnStart;
        public event EventHandler<IProcessEventArgument> OnComplete;

    }
}