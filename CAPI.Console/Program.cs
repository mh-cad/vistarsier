using CAPI.Agent;
using CAPI.Agent.Abstractions;
using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
using CAPI.Common.Config;
using CAPI.Common.Services;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace CAPI.Console
{
    internal static class Program
    {
        private static ILog _log;

        private static void Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddSingleton<IAgent, Agent.Agent>()
                .AddSingleton<IAgentFactory, AgentFactory>()
                .AddSingleton<ICapiConfig, CapiConfig>()
                .AddSingleton<IFileSystem, FileSystem>()
                .BuildServiceProvider();

            InitialiseLog4Net();

            _log.Info("App Started...");

            var agentFactory = services.GetService<IAgentFactory>();
            var config = services.GetService<ICapiConfig>().GetConfig(args);
            var agent = agentFactory.CreateAgent(config);
            agent.Run();

            _log.Info("\r\nPress any key to exit.");

            System.Console.ReadKey();
        }

        private static void InitialiseLog4Net()
        {
            _log = LogHelper.GetLogger();
        }
    }
}
