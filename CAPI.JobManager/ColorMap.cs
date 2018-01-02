using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    public class ColorMap : IColorMap
    {
        private readonly IImageProcessor _imageProcessor;

        public IntegratedProcessType Type
        {
            get => IntegratedProcessType.ColorMap;
            set { }
        }
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }

        public event EventHandler<IProcessEventArgument> OnComplete;

        // Constructor
        public ColorMap(IImageProcessor imageProcessor)
        {
            _imageProcessor = imageProcessor;
            Id = "4";
            Version = "1";
        }

        public IJob<IRecipe> Run(IJob<IRecipe> jobToBeProcessed)
        {
            jobToBeProcessed = DoColorMap(jobToBeProcessed);

            OnComplete?.Invoke(this, new ProcessEventArgument(
                $"ColorMap process is completed [Version: {Version}] " +
                $"[Parameters: {string.Join(" ", Parameters)}]"));

            return jobToBeProcessed;
        }

        private IJob<IRecipe> DoColorMap(IJob<IRecipe> job)
        {
            var fixedHdrFullPath =
                job.DicomSeriesFixed.Original.HdrFileFullPath;
            var fixedDicomFolderPath =
                job.DicomSeriesFixed.Original.DicomFolderPath;
            var fixedBrainMaskHdrFullpath =
                job.DicomSeriesFixed.BrainMask.NiiFileFullPath;
            var darkInFloating2BrightInFixedNiiFullPath =
                job.StructChangesDarkInFloating2BrightInFixed.NiiFileFullPath;
            var brightInFloating2DarkInFixedNiiFullPath =
                job.StructChangesBrightInFloating2DarkInFixed.NiiFileFullPath;

            _imageProcessor.ColorMap(
                fixedHdrFullPath, fixedDicomFolderPath, fixedBrainMaskHdrFullpath,
                darkInFloating2BrightInFixedNiiFullPath,
                brightInFloating2DarkInFixedNiiFullPath,
                job.OutputFolderPath, out var positive, out var negative);

            job.PositiveOverlay.BmpFolderPath = positive;
            job.NegativeOverlay.BmpFolderPath = negative;

            return job;
        }
    }
}