using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    public class Registration : IRegistration
    {
        private readonly IImageProcessor _imageProcessor;

        public IntegratedProcessType Type
        {
            get => IntegratedProcessType.Registeration;
            set { }
        }
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }

        public IJobNew<IRecipe> Run(IJobNew<IRecipe> jobToBeProcessed)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<IProcessEventArgument> OnStart;
        public event EventHandler<IProcessEventArgument> OnComplete;

        // Constructor
        public Registration(IImageProcessor imageProcessor)
        {
            _imageProcessor = imageProcessor;
            Id = "2";
            Version = "1";
        }

        public IJob<IRecipe> Run(IJob<IRecipe> jobToBeProcessed)
        {
            OnStart?.Invoke(this, new ProcessEventArgument(
                "Registering studies... " +
                $"[Version: {Version}] [Parameters: {string.Join(" | ", Parameters)}]"));

            jobToBeProcessed = DoRegistration(jobToBeProcessed);

            OnComplete?.Invoke(this, new ProcessEventArgument("Registration is complete!"));

            return jobToBeProcessed;
        }

        private IJob<IRecipe> DoRegistration(IJob<IRecipe> job)
        {
            var fixedHdr =
                // If brain mask has been removed or not
                job.DicomSeriesFixed.Transformed.CompletedProcesses
                .Contains(IntegratedProcessType.ExtractBrainSurface)
                    ? job.DicomSeriesFixed.Transformed.NiiFileFullPath // ? job.DicomSeriesFixed.Transformed.HdrFileFullPath
                    : job.DicomSeriesFixed.Original.HdrFileFullPath;

            var floatingHdr =
                // If brain mask has been removed or not
                job.DicomSeriesFloating.Transformed.CompletedProcesses
                .Contains(IntegratedProcessType.ExtractBrainSurface)
                    ? job.DicomSeriesFloating.Transformed.NiiFileFullPath // ? job.DicomSeriesFloating.Transformed.HdrFileFullPath
                    : job.DicomSeriesFloating.Original.HdrFileFullPath;

            _imageProcessor.Registration(job.OutputFolderPath, fixedHdr, floatingHdr,
                out var floatingReslicedFullPath, out var frameOfReference);

            job.DicomSeriesFixed.FrameOfReference = frameOfReference;
            job.DicomSeriesFloating.Transformed.NiiFileFullPath = floatingReslicedFullPath;

            return job;
        }
    }
}