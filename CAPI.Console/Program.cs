using CAPI.Agent;
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
using log4net;
using Microsoft.Extensions.DependencyInjection;
using System;
using ImgProcConfig = CAPI.Common.Config.ImgProcConfig;

namespace CAPI.Console
{
    internal static class Program
    {
        private static ILog _log;

        private static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection().RegisterTypes().BuildServiceProvider();

            InitialiseLog4Net();

            _log.Info("App Started...");

            var agentFactory = serviceProvider.GetService<IAgentFactory>();
            var dicomFactory = serviceProvider.GetService<IDicomFactory>();
            var imgProcFactory = serviceProvider.GetService<IImageProcessingFactory>();
            var fileSystem = serviceProvider.GetService<IFileSystem>();
            var processBuilder = serviceProvider.GetService<IProcessBuilder>();

            var config = new CapiConfig().GetConfig(args);

            var agent = agentFactory.CreateAgent(config, dicomFactory, imgProcFactory, fileSystem, processBuilder, _log);

            agent.Run();

            _log.Info($"{Environment.NewLine}Press any key to exit.");

            System.Console.ReadKey();
        }

        private static void InitialiseLog4Net()
        {
            _log = LogHelper.GetLogger();
        }

        private static IServiceCollection RegisterTypes(this IServiceCollection services)
        {
            services
                .AddSingleton<IAgent, Agent.Agent>()
                .AddSingleton<IAgentFactory, AgentFactory>()
                //.AddSingleton<IAgentRepository, AgentRepository>()
                //.AddSingleton<ICapiConfig, CapiConfig>()
                //.AddSingleton<Common.Abstractions.Config.IDicomConfig, Common.Config.DicomConfig>()
                .AddSingleton<IImgProcConfig, ImgProcConfig>()
                .AddSingleton<ITestsConfig, TestsConfig>()
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton<IDicomFactory, DicomFactory>()
                .AddSingleton<IDicomNode, DicomNode>()
                .AddSingleton<IImageProcessingFactory, ImageProcessingFactory>()
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton<IProcessBuilder, ProcessBuilder>();
            return services;
        }
    }
}
