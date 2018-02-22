using CAPI.Agent_Console.Abstractions;
using CAPI.Common;
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
        private static int _interval;
        private const int DefaultNoOfCasesToCheck = 1000;
        private static int _numberOfCasesToCheck;
        private static UnityContainer _unityContainer;
        private static bool _isBusy;

        private static void Main(string[] args)
        {
            Log.Info("Agent Started");
            Console.WriteLine("Enter 'q' to quit!");

            SetEnvironmentVariables();

            InitializeUnity();

            GetFirstParamFromArgs(args);

            SetFailedCasesStatusToPending(); // These are interrupted cases - Set status to "Pending" so they get processed

            Run(); // Run for the first time

            StartTimer();

            while (Console.Read() != 'q') { }
        }

        private static void SetFailedCasesStatusToPending()
        {
            var incompleteCases = new PendingCase().GetProcessingCapiCases(_numberOfCasesToCheck).ToList();
            incompleteCases.AddRange(new PendingCase().GetQueuedCapiCases(_numberOfCasesToCheck));

            foreach (var processingCase in incompleteCases)
            {
                try
                {
                    processingCase.SetStatus("Pending");
                }
                catch
                {
                    Agent_Console.Log.WriteError($"Failed to set case status from Processing/Queued to Pending. Accession: ${processingCase.Accession}");
                    throw;
                }
            }
        }

        private static void StartTimer()
        {
            _interval = Properties.Settings.Default.DbCheckInterval;

            var timer = new Timer { Interval = _interval * 1000, Enabled = true };
            timer.Elapsed += OnTimeEvent;
            timer.Start();
            Agent_Console.Log.Write("Timer started");
        }
        private static void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            if (_isBusy) return;

            try
            {
                Run();
            }
            catch
            {
                _isBusy = false;
                throw;
            }
        }
        private static void Run()
        {
            _isBusy = true;
            var thisMachineProcessesManualCases = Config.GetProcessManuallyAddedCases() == "1";

            var pendingCasesHl7Added = Broker.GetPendingCasesFromCapiDbHl7Added(_numberOfCasesToCheck).ToList();
            var completedCasesAll = ProcessAllPendingCases(pendingCasesHl7Added, out var failedCasesAll).ToList();

            if (thisMachineProcessesManualCases)
            {
                var pendingCasesManuallyAdded =
                    Broker.GetPendingCasesFromCapiDbManuallyAdded(_numberOfCasesToCheck).ToList();
                var completedCasesManual =
                    ProcessAllPendingCases(pendingCasesManuallyAdded, out var failedCasesManual).ToList();

                completedCasesAll.AddRange(completedCasesManual);
                failedCasesAll.AddRange(failedCasesManual);

                DeleteManuallyAddedCompletedFiles(completedCasesManual);
            }

            LogProcessedCases(completedCasesAll, failedCasesAll);
            _isBusy = false;
        }

        private static void DeleteManuallyAddedCompletedFiles(IEnumerable<IPendingCase> completedCasesManual)
        {
            var manualProcessPath = Config.GetManualProcessPath();
            var manualProcessFiles = Directory.GetFiles(manualProcessPath);
            manualProcessFiles
                .Where(f => completedCasesManual.Select(c => c.Accession).Contains(Path.GetFileName(f)))
                .ToList()
                .ForEach(File.Delete);
        }

        private static void LogProcessedCases(
            IEnumerable<IPendingCase> completedCases, IEnumerable<IPendingCase> failedCases)
        {
            foreach (var completedCase in completedCases)
                Log.Info($"Job completed for accession {completedCase.Accession}");

            foreach (var failedCase in failedCases)
                Log.Error($"Job failed for accession {failedCase.Accession}", failedCase.Exception);

            Log.Info($"Checking for new cases in {_interval} seconds...");
        }

        // Main Process
        private static IEnumerable<IPendingCase> ProcessAllPendingCases
            (IList<IPendingCase> pendingCases, out List<IPendingCase> failedCases)
        {
            failedCases = new List<IPendingCase>();
            var completedCases = new List<IPendingCase>();
            var broker = GetBroker();

            // Return an empty list if no pending case is found
            if (!pendingCases.Any()) return completedCases;

            // Set status of all pending cases to Queued
            foreach (var pendingCase in pendingCases) pendingCase.SetStatus("Queued");

            foreach (var pendingCase in pendingCases)
            {
                pendingCase.SetStatus("Processing");
                var success = broker.ProcessCase(pendingCase);

                if (success)
                {
                    pendingCase.SetStatus("Completed");
                    completedCases.Add(pendingCase);
                }
                else
                {
                    pendingCase.SetStatus("Failed");
                    failedCases.Add(pendingCase);
                }
            }

            return completedCases;
        }
        private static Broker GetBroker()
        {
            var dicomNodeRepo = _unityContainer.Resolve<IDicomNodeRepository>();
            var recipeRepositoryInMemory = _unityContainer.Resolve<IRecipeRepositoryInMemory<IRecipe>>();
            var jobBuilder = _unityContainer.Resolve<IJobBuilder>();
            return new Broker(dicomNodeRepo, recipeRepositoryInMemory, jobBuilder);
        }

        // Set up
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
            _unityContainer.RegisterType<IDicomFactory, DicomFactory>();
            _unityContainer.RegisterType<IImageConverter, ImageConverter>();
            _unityContainer.RegisterType<IImageProcessor, ImageProcessor>();
            _unityContainer.RegisterType<IJobManagerFactory, JobManagerFactory>();
            _unityContainer.RegisterType<IRecipe, Recipe>();
            _unityContainer.RegisterType<IJob<IRecipe>, Job<IRecipe>>();
            _unityContainer.RegisterType<IJobBuilder, JobBuilder>();
            _unityContainer.RegisterType<ISeriesSelectionCriteria, SeriesSelectionCriteria>();
            _unityContainer.RegisterType<IIntegratedProcess, IntegratedProcess>();
            _unityContainer.RegisterType<IDestination, Destination>();
            _unityContainer.RegisterType<IRecipeRepositoryInMemory<IRecipe>, RecipeRepositoryInMemory<Recipe>>();
            _unityContainer.RegisterType<IDicomNodeRepository, DicomNodeRepositoryInMemory>();
            _unityContainer.RegisterType<IValueComparer, ValueComparer>();
        }
    }
}