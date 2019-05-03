using CAPI.Agent;
using CAPI.Agent.Abstractions;
using CAPI.Config;
using CAPI.Dicom;
using CAPI.Dicom.Abstractions;
using CAPI.Dicom.Model;
using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using CAPI.NiftiLib;
using Unity;
using Unity.log4net;
using IImageProcessor = CAPI.ImageProcessing.Abstraction.IImageProcessor;
using ImageProcessor = CAPI.ImageProcessing.ImageProcessor;
using ImgProcConfig = CAPI.Config.ImgProcConfig;

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
            container.RegisterType<CAPI.Dicom.Abstractions.IDicomConfig, CAPI.Dicom.DicomConfig>();
            container.RegisterType<IImageProcessor, ImageProcessor>();
            container.RegisterType<CAPI.Agent.Abstractions.Models.ISeriesSelectionCriteria, CAPI.Agent.Models.SeriesSelectionCriteria>();
            container.RegisterType<CAPI.Agent.Abstractions.Models.IDestination, CAPI.Agent.Models.Destination>();
            container.RegisterType<CAPI.Agent.Abstractions.Models.IValueComparer, CAPI.Agent.Models.ValueComparer>();
            container.RegisterType<IImageProcessingFactory, ImageProcessingFactory>();
            container.RegisterType<INifti, Nifti>();
            container.RegisterType<IAgent, CAPI.Agent.Agent>();
            container.RegisterType<CAPI.Agent.Abstractions.IImageProcessor, CAPI.Agent.ImageProcessor>();
            container.RegisterType<IAgentFactory, AgentFactory>();
            container.RegisterType<IImgProcConfig, ImgProcConfig>();
            container.RegisterType<ITestsConfig, TestsConfig>();

            return container;
        }
    }
}