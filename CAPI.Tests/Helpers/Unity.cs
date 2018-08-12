using CAPI.Agent.Abstractions;
using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
using CAPI.Common.Config;
using CAPI.Common.Services;
using CAPI.Dicom;
using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager;
using CAPI.JobManager.Abstraction;
using Unity;
using Unity.log4net;
using IDicomConfig = CAPI.Common.Abstractions.Config.IDicomConfig;
using ImgProcConfig = CAPI.Common.Config.ImgProcConfig;


namespace CAPI.Tests.Helpers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Unity
    {
        public static IUnityContainer CreateContainerCore()
        {
            var container = (UnityContainer)new UnityContainer()
                .AddNewExtension<Log4NetExtension>();

            container.RegisterType<IDicomNode, DicomNode>();
            container.RegisterType<IDicomFactory, DicomFactory>();
            container.RegisterType<IDicomServices, DicomServices>();
            container.RegisterType<IImageConverter, ImageConverter>();
            container.RegisterType<IImageProcessorNew, ImageProcessorNew>();
            container.RegisterType<IJobManagerFactory, JobManagerFactory>();
            container.RegisterType<JobManager.Abstraction.IRecipe, Recipe>();
            container.RegisterType<IJobNew<JobManager.Abstraction.IRecipe>, JobNew<JobManager.Abstraction.IRecipe>>();
            container.RegisterType<IJobBuilderNew, JobBuilderNew>();
            container.RegisterType<CAPI.Agent.Abstractions.Models.ISeriesSelectionCriteria, CAPI.Agent.Models.SeriesSelectionCriteria>();
            container.RegisterType<IIntegratedProcess, IntegratedProcess>();
            //container.RegisterType<IDestination, Destination>();
            container.RegisterType<CAPI.Agent.Abstractions.Models.IDestination, CAPI.Agent.Models.Destination>();
            //container.RegisterType<IRecipeRepositoryInMemory<JobManager.Abstraction.IRecipe>, RecipeRepositoryInMemory<Recipe>>();
            //container.RegisterType<IDicomNodeRepository, DicomNodeRepositoryInMemory>();
            container.RegisterType<CAPI.Agent.Abstractions.Models.IValueComparer, CAPI.Agent.Models.ValueComparer>();
            container.RegisterType<IImageProcessingFactory, ImageProcessingFactory>();
            container.RegisterType<INifti, Nifti>();
            container.RegisterType<ISubtractionLookUpTable, SubtractionLookUpTable>();
            container.RegisterType<IAgent, CAPI.Agent.Agent>();
            container.RegisterType<CAPI.Agent.Abstractions.IImageProcessor, CAPI.Agent.ImageProcessor>();
            container.RegisterType<ICapiConfig, CapiConfig>();
            container.RegisterType<IDicomConfig, CAPI.Common.Config.DicomConfig>();
            container.RegisterType<IImgProcConfig, ImgProcConfig>();
            container.RegisterType<ITestsConfig, TestsConfig>();
            container.RegisterType<IFileSystem, CAPI.Common.Services.FileSystem>();
            container.RegisterType<IProcessBuilder, ProcessBuilder>();

            return container;
        }
    }
}