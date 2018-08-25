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
using Unity;
using Unity.log4net;
using IImageProcessor = CAPI.ImageProcessing.Abstraction.IImageProcessor;
using ImgProcConfig = CAPI.Common.Config.ImgProcConfig;


namespace CAPI.Console.NetFramework.Helpers
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
            container.RegisterType<Dicom.Abstraction.IDicomConfig, Dicom.DicomConfig>();
            container.RegisterType<IImageConverter, ImageConverter>();
            container.RegisterType<IImageProcessor, ImageProcessor>();
            //container.RegisterType<IJobManagerFactory, JobManagerFactory>();
            //container.RegisterType<JobManager.Abstraction.IRecipe, Recipe>();
            //container.RegisterType<IJobNew<JobManager.Abstraction.IRecipe>, JobNew<JobManager.Abstraction.IRecipe>>();
            //container.RegisterType<IJobBuilderNew, JobBuilderNew>();
            container.RegisterType<Agent.Abstractions.Models.ISeriesSelectionCriteria, Agent.Models.SeriesSelectionCriteria>();
            //container.RegisterType<IIntegratedProcess, IntegratedProcess>();
            //container.RegisterType<IDestination, Destination>();
            container.RegisterType<Agent.Abstractions.Models.IDestination, Agent.Models.Destination>();
            //container.RegisterType<IRecipeRepositoryInMemory<JobManager.Abstraction.IRecipe>, RecipeRepositoryInMemory<Recipe>>();
            //container.RegisterType<IDicomNodeRepository, DicomNodeRepositoryInMemory>();
            container.RegisterType<Agent.Abstractions.Models.IValueComparer, Agent.Models.ValueComparer>();
            container.RegisterType<IImageProcessingFactory, ImageProcessingFactory>();
            container.RegisterType<INifti, Nifti>();
            container.RegisterType<ISubtractionLookUpTable, SubtractionLookUpTable>();
            container.RegisterType<IAgent, Agent.Agent>();
            container.RegisterType<Agent.Abstractions.IImageProcessor, Agent.ImageProcessor>();
            container.RegisterType<IAgentFactory, Agent.AgentFactory>();
            //container.RegisterType<ICapiConfig, CapiConfig>();
            //container.RegisterType<IDicomConfig, CAPI.Common.Config.DicomConfig>();
            container.RegisterType<IImgProcConfig, ImgProcConfig>();
            container.RegisterType<ITestsConfig, TestsConfig>();
            container.RegisterType<IFileSystem, FileSystem>();
            container.RegisterType<IProcessBuilder, ProcessBuilder>();

            return container;
        }
    }
}