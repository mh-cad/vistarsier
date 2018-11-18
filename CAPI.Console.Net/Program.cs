﻿using CAPI.Agent.Abstractions;
using CAPI.Console.Net.Helpers;
using CAPI.Dicom.Abstractions;
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
            var fileSystem = container.Resolve<IFileSystem>();
            var processBuilder = container.Resolve<IProcessBuilder>();

            if (args.Length > 0 && args[0].ToLower() == "uat")
            {
                var uatTestRunner = new TestRunner(dicomFactory, imgProcFactory, fileSystem, processBuilder);
                uatTestRunner.Run(); return;
            }

            InitialiseLog4Net();

            System.Console.ForegroundColor = ConsoleColor.Gray;
            _log.Info("App Started...");

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
