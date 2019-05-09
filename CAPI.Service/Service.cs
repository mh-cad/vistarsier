using CAPI.Agent.Abstractions;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceProcess;

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
            var log = GetLogger();
            _agent = new Agent.Agent(args);
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
    }
}
