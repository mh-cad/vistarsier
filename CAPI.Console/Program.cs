using CAPI.Agent.Abstractions;
using CAPI.Common.Config;
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
                .AddSingleton<CapiConfig>()
                .BuildServiceProvider();

            InitialiseLog4Net();

            _log.Info("App Started...");

            var agent = services.GetService<IAgent>();
            agent.Config = CapiConfig.GetConfig(args);
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
