using CAPI.Agent.Abstractions;
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
                .BuildServiceProvider();

            InitialiseLog4Net();

            _log.Info("App Started...");

            services.GetService<IAgent>().Run();

            _log.Info("\r\nPress any key to exit.");

            System.Console.ReadKey();
        }

        private static void InitialiseLog4Net()
        {
            _log = LogHelper.GetLogger();
        }
    }
}
