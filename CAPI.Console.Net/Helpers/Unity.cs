using CAPI.Agent.Abstractions;
using CAPI.Config;
using CAPI.Dicom;
using CAPI.Dicom.Abstractions;
using CAPI.Dicom.Model;
using CAPI.Common;
using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using CAPI.NiftiLib;
using Unity;
using Unity.log4net;
using IImageProcessor = CAPI.ImageProcessing.Abstraction.IImageProcessor;
using ImgProcConfig = CAPI.Config.ImgProcConfig;


namespace CAPI.Console.Net.Helpers
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
            container.RegisterType<Dicom.Abstractions.IDicomConfig, Dicom.DicomConfig>();
            container.RegisterType<IImageConverter, ImageConverter>();
            container.RegisterType<IImageProcessor, ImageProcessor>();
            container.RegisterType<Agent.Abstractions.Models.ISeriesSelectionCriteria, Agent.Models.SeriesSelectionCriteria>();
            container.RegisterType<Agent.Abstractions.Models.IDestination, Agent.Models.Destination>();
            container.RegisterType<Agent.Abstractions.Models.IValueComparer, Agent.Models.ValueComparer>();
            container.RegisterType<IImageProcessingFactory, ImageProcessingFactory>();
            container.RegisterType<INifti, Nifti>();
            //container.RegisterType<ISubtractionLookUpTable, SubtractionLookUpTable>();
            container.RegisterType<IAgent, Agent.Agent>();
            container.RegisterType<Agent.Abstractions.IImageProcessor, Agent.ImageProcessor>();
            container.RegisterType<IAgentFactory, Agent.AgentFactory>();
            container.RegisterType<IImgProcConfig, ImgProcConfig>();
            container.RegisterType<ITestsConfig, TestsConfig>();
            container.RegisterType<IProcessBuilder, ProcessBuilder>();

            return container;
        }
    }
}