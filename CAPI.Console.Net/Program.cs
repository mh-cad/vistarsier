using CAPI.Agent.Abstractions;
using CAPI.Console.Net.Helpers;
using CAPI.Dicom.Abstractions;
using CAPI.General;
using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using CAPI.UAT;
using log4net;
using System;
using Unity;

namespace CAPI.Console.Net
{
    internal static class Program
    {
        private static ILog _log;

        private static void Main(string[] args)
        {
            var container = Helpers.Unity.CreateContainerCore();

            var agentFactory = container.Resolve<IAgentFactory>();
            var dicomFactory = container.Resolve<IDicomFactory>();
            var imgProcFactory = container.Resolve<IImageProcessingFactory>();
            var processBuilder = container.Resolve<IProcessBuilder>();

            InitialiseLog4Net();

            if (args.Length > 0 && args[0].ToLower() == "uat")
            {
                var uatTestRunner = new TestRunner(dicomFactory, imgProcFactory, processBuilder, _log);
                uatTestRunner.Run(); return;
            }

            System.Console.ForegroundColor = ConsoleColor.Gray;
            _log.Info("App Started...");

            var agent = agentFactory.CreateAgent(args, dicomFactory, imgProcFactory, processBuilder);

            agent.Run();

            while (System.Console.ReadKey(true).KeyChar != 'q') { }
        }

        private static void InitialiseLog4Net()
        {
            _log = Log.GetLogger();
        }
    }
}
