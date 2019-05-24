using VisTarsier.Common;
using VisTarsier.Config;
using VisTarsier.Service.Agent.Abstractions;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceProcess;

namespace VisTarsier.Service
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
            CheckConfig();
            var log = GetLogger();
            _agent = new Agent.Agent();
            System.Console.ForegroundColor = ConsoleColor.Gray;
            log.Info("App Started...");

            _agent.Run();
        }

        private void CheckConfig()
        {
            var conf = CapiConfig.GetConfig();
            FileSystem.DirectoryExistsIfNotCreate(conf.ManualProcessPath);
            FileSystem.DirectoryExistsIfNotCreate(conf.Hl7ProcessPath);

            if (!File.Exists(conf.Binaries.antsRegistration)
                || !File.Exists(conf.Binaries.antsApplyTransforms)
                || !File.Exists(conf.Binaries.N4BiasFieldCorrection)
                || !File.Exists(conf.Binaries.img2dcm)
                || !File.Exists(conf.Binaries.dcm2niix)
                || !File.Exists(conf.Binaries.bse))
            {
                throw new FileNotFoundException("Could not find one or more essential binaries as referenced in config.json");
            }
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
