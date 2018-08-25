using CAPI.Agent.Abstractions;
using CAPI.Common.Abstractions.Services;
using CAPI.Common.Config;
using CAPI.Console.NetFramework.Helpers;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using Unity;

namespace CAPI.Console.NetFramework
{
    internal static class Program
    {
        private static ILog _log;

        private static void Main(string[] args)
        {
            var container = Helpers.Unity.CreateContainerCore();//new ServiceCollection().RegisterTypes().BuildServiceProvider();

            InitialiseLog4Net();

            _log.Info("App Started...");

            var agentFactory = container.Resolve<IAgentFactory>();
            var dicomFactory = container.Resolve<IDicomFactory>();
            var imgProcFactory = container.Resolve<IImageProcessingFactory>();
            var fileSystem = container.Resolve<IFileSystem>();
            var processBuilder = container.Resolve<IProcessBuilder>();

            var config = new CapiConfig().GetConfig(args);

            var agent = agentFactory.CreateAgent(config, dicomFactory, imgProcFactory, fileSystem, processBuilder, _log);

            agent.Run();

            while (true) { }
        }

        private static void InitialiseLog4Net()
        {
            _log = LogHelper.GetLogger();
        }
    }
}
