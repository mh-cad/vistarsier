﻿using CAPI.Agent.Abstractions;
using CAPI.Console.Net.Helpers;
using CAPI.Dicom.Abstractions;
using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using Unity;

namespace CAPI.Console.Net
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

            var agent = agentFactory.CreateAgent(args, dicomFactory, imgProcFactory, fileSystem, processBuilder, _log);

            agent.Run();

            while (System.Console.ReadKey(true).KeyChar != 'q') { }
        }

        private static void InitialiseLog4Net()
        {
            _log = LogHelper.GetLogger();
        }
    }
}
