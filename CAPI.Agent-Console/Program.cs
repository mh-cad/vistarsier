using CAPI.Agent_Console.Abstractions;
using CAPI.Common.Config;
using CAPI.DAL;
using CAPI.DAL.Abstraction;
using CAPI.Dicom;
using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager;
using CAPI.JobManager.Abstraction;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Unity;
using Unity.log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace CAPI.Agent_Console
{
    internal static class Program
    {
        // Fields
        private static readonly ILog Log = LogHelper.GetLogger();
        private static readonly int Interval = Properties.Settings.Default.DbCheckInterval;
        private const int DefaultNoOfCasesToCheck = 1000;
        private static int _numberOfCasesToCheck;
        private static UnityContainer _unityContainer;
        private static Broker _broker;

        // Class Entry Point
        private static void Main(string[] args)
        {
            Log.Info("Agent Started");
            Console.WriteLine("Enter 'q' to quit!");

            SetEnvironmentVariables();

            InitializeUnity();

            GetFirstParamFromArgs(args);

            _broker = GetBroker();

            SetFailedCasesStatusToPending(); // These are interrupted cases - Set status to "Pending" so they get processed

            _broker.Run(); // Run for the first time

            StartTimer();

            while (Console.Read() != 'q') { }
        }

        private static void SetFailedCasesStatusToPending()
        {
            var incompleteCases = _broker.GetProcessingCasesFromCapiDb().ToList();
            incompleteCases.AddRange(_broker.GetQueuedCasesFromCapiDb());

            foreach (var processingCase in incompleteCases)
            {
                try
                {
                    _broker.SetCaseStatus(processingCase.Accession, "Pending");
                }
                catch (Exception ex)
                {
                    Log.Error(
                        "Failed to set case status from Processing/Queued to Pending. " +
                        $"Accession: ${processingCase.Accession}", ex);
                    throw;
                }
            }
        }

        // Timer and Run Processes
        private static void StartTimer()
        {
            var timer = new Timer { Interval = Interval * 1000, Enabled = true };
            timer.Elapsed += OnTimeEvent;
            timer.Start();
            Log.Info("Timer started");
        }
        private static void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            if (_broker.IsBusy) return;

            try
            {
                _broker.Run();
            }
            catch
            {
                _broker.IsBusy = false;
                throw;
            }
        }

        private static void GetFirstParamFromArgs(IReadOnlyList<string> args)
        {
            if (args != null && args.Count > 0 && int.TryParse(args[1], out var arg1))
                _numberOfCasesToCheck = arg1;
            else _numberOfCasesToCheck = DefaultNoOfCasesToCheck;
        }
        private static void SetEnvironmentVariables()
        {
            var settings = Properties.Settings.Default;

            var dcmNodeAetLocal = Environment.GetEnvironmentVariable("DcmNodeAET_Local");
            if (dcmNodeAetLocal == null)
                Environment.SetEnvironmentVariable("DcmNodeAET_Local", settings.DcmNodeAET_Local, EnvironmentVariableTarget.User);

            var dcmNodeIpLocal = Environment.GetEnvironmentVariable("DcmNodeIP_Local");
            if (dcmNodeIpLocal == null)
                Environment.SetEnvironmentVariable("DcmNodeIP_Local", settings.DcmNodeIP_Local, EnvironmentVariableTarget.User);

            var dcmNodePortLocal = Environment.GetEnvironmentVariable("DcmNodePort_Local");
            if (dcmNodePortLocal == null)
                Environment.SetEnvironmentVariable("DcmNodePort_Local", settings.DcmNodePort_Local, EnvironmentVariableTarget.User);

            var dcmNodeAetRemote = Environment.GetEnvironmentVariable("DcmNodeAET_Remote");
            if (dcmNodeAetRemote == null)
                Environment.SetEnvironmentVariable("DcmNodeAET_Remote", settings.DcmNodeAET_Remote, EnvironmentVariableTarget.User);

            var dcmNodeIpRemote = Environment.GetEnvironmentVariable("DcmNodeIP_Remote");
            if (dcmNodeIpRemote == null)
                Environment.SetEnvironmentVariable("DcmNodeIP_Remote", settings.DcmNodeIP_Remote, EnvironmentVariableTarget.User);

            var dcmNodePortRemote = Environment.GetEnvironmentVariable("DcmNodePort_Remote");
            if (dcmNodePortRemote == null)
                Environment.SetEnvironmentVariable("DcmNodePort_Remote", settings.DcmNodePort_Remote, EnvironmentVariableTarget.User);
        }
        private static Broker GetBroker()
        {
            var dicomNodeRepo = _unityContainer.Resolve<IDicomNodeRepository>();
            var recipeRepositoryInMemory = _unityContainer.Resolve<IRecipeRepositoryInMemory<IRecipe>>();
            var jobBuilder = _unityContainer.Resolve<IJobBuilder>();
            var agentConsoleFactory = _unityContainer.Resolve<IAgentConsoleFactory>();
            return new Broker(dicomNodeRepo, recipeRepositoryInMemory, jobBuilder, agentConsoleFactory);
        }

        // Unity
        private static void InitializeUnity()
        {
            _unityContainer = (UnityContainer)new UnityContainer()
                .AddNewExtension<Log4NetExtension>();
            RegisterClasses();
        }
        private static void RegisterClasses()
        {
            _unityContainer.RegisterType<IDicomNode, DicomNode>();
            _unityContainer.RegisterType<IDicomFactory, DicomFactory>();
            _unityContainer.RegisterType<IDicomServices, DicomServices>();
            _unityContainer.RegisterType<IImageConverter, ImageConverter>();
            _unityContainer.RegisterType<IImageProcessor, ImageProcessor>();
            _unityContainer.RegisterType<IJobManagerFactory, JobManagerFactory>();
            _unityContainer.RegisterType<IRecipe, Recipe>();
            _unityContainer.RegisterType<IJob<IRecipe>, Job<IRecipe>>();
            _unityContainer.RegisterType<IJobNew<IRecipe>, JobNew<IRecipe>>();
            _unityContainer.RegisterType<IJobBuilder, JobBuilder>();
            _unityContainer.RegisterType<ISeriesSelectionCriteria, SeriesSelectionCriteria>();
            _unityContainer.RegisterType<IIntegratedProcess, IntegratedProcess>();
            _unityContainer.RegisterType<IDestination, Destination>();
            _unityContainer.RegisterType<IRecipeRepositoryInMemory<IRecipe>, RecipeRepositoryInMemory<Recipe>>();
            _unityContainer.RegisterType<IDicomNodeRepository, DicomNodeRepositoryInMemory>();
            _unityContainer.RegisterType<IValueComparer, ValueComparer>();
            _unityContainer.RegisterType<IAgentConsoleFactory, AgentConsoleFactory>();
        }
    }
}