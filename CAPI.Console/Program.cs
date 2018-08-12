using CAPI.Agent;
using CAPI.Agent.Abstractions;
using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
using CAPI.Common.Config;
using CAPI.Common.Services;
using CAPI.Dicom;
using CAPI.Dicom.Abstraction;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using IDicomConfig = CAPI.Common.Abstractions.Config.IDicomConfig;

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
            var config = serviceProvider.GetService<ICapiConfig>().GetConfig(args);
            var dicomFactory = serviceProvider.GetService<IDicomFactory>();
            var fileSystem = serviceProvider.GetService<IFileSystem>();
            var processBuilder = serviceProvider.GetService<IProcessBuilder>();

            var agent = agentFactory.CreateAgent(config, dicomFactory, fileSystem, processBuilder, _log);
            agent.Run();

            _log.Info("\r\nPress any key to exit.");

            System.Console.ReadKey();
        }

        private static void InitialiseLog4Net()
        {
            _log = LogHelper.GetLogger();
        }

        private static IServiceCollection RegisterTypes(this IServiceCollection services)
        {
            return services
                .AddSingleton<IAgent, Agent.Agent>()
                .AddSingleton<IAgentFactory, AgentFactory>()
                .AddSingleton<ICapiConfig, CapiConfig>()
                .AddSingleton<IImgProcConfig, ImgProcConfig>()
                .AddSingleton<IDicomConfig, Common.Config.DicomConfig>()
                .AddSingleton<ITestsConfig, TestsConfig>()
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton<IDicomFactory, DicomFactory>()
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton<IProcessBuilder, ProcessBuilder>();
        }
    }
}
