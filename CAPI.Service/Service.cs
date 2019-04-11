using CAPI.Agent.Abstractions;
using CAPI.Agent;
using CAPI.Common.Abstractions.Config;
using CAPI.Common.Config;
using CAPI.Dicom;
using CAPI.Dicom.Abstractions;
using CAPI.Dicom.Model;
using CAPI.General.Abstractions.Services;
using CAPI.General.Services;
using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.log4net;

namespace CAPI.Service
{
    public partial class Service : ServiceBase
    {
        IAgent _agent;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var container = CreateContainerCore();

            var agentFactory = container.Resolve<IAgentFactory>();
            var dicomFactory = container.Resolve<IDicomFactory>();
            var imgProcFactory = container.Resolve<IImageProcessingFactory>();
            var fileSystem = container.Resolve<IFileSystem>();
            var processBuilder = container.Resolve<IProcessBuilder>();

            var log = GetLogger();
            _agent = agentFactory.CreateAgent(args, dicomFactory, imgProcFactory, fileSystem, processBuilder, log);
            System.Console.ForegroundColor = ConsoleColor.Gray;
            log.Info("App Started...");

            _agent.Run();
        }

        protected override void OnStop()
        {
        }

        public static ILog GetLogger([CallerFilePath] string filename = "")
        {
            var fileSplit = filename.Split('\\');

            if (fileSplit.Length > 1)
                filename = $@"{fileSplit[fileSplit.Length - 2]}\{fileSplit[fileSplit.Length - 1]}";

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            return LogManager.GetLogger(logRepository.Name, filename);
        }

        public static IUnityContainer CreateContainerCore()
        {
            var container = (UnityContainer)new UnityContainer()
                .AddNewExtension<Log4NetExtension>();

            container.RegisterType<IDicomNode, DicomNode>();
            container.RegisterType<IDicomFactory, DicomFactory>();
            container.RegisterType<IDicomServices, DicomServices>();
            container.RegisterType<Dicom.Abstractions.IDicomConfig, Dicom.DicomConfig>();
            container.RegisterType<IImageConverter, ImageConverter>();
            container.RegisterType<ImageProcessing.Abstraction.IImageProcessor, ImageProcessing.ImageProcessor>();
            container.RegisterType<Agent.Abstractions.Models.ISeriesSelectionCriteria, Agent.Models.SeriesSelectionCriteria>();
            container.RegisterType<Agent.Abstractions.Models.IDestination, Agent.Models.Destination>();
            container.RegisterType<Agent.Abstractions.Models.IValueComparer, Agent.Models.ValueComparer>();
            container.RegisterType<IImageProcessingFactory, ImageProcessingFactory>();
            container.RegisterType<INifti, Nifti>();
            container.RegisterType<ISubtractionLookUpTable, SubtractionLookUpTable>();
            container.RegisterType<IAgent, Agent.Agent>();
            container.RegisterType<Agent.Abstractions.IImageProcessor, Agent.ImageProcessor>();
            container.RegisterType<IAgentFactory, Agent.AgentFactory>();
            container.RegisterType<IImgProcConfig, ImgProcConfig>();
            container.RegisterType<ITestsConfig, TestsConfig>();
            container.RegisterType<IFileSystem, FileSystem>();
            container.RegisterType<IProcessBuilder, ProcessBuilder>();

            return container;
        }
    }
}
