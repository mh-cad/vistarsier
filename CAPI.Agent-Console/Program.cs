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
        private static bool _isBusy;
        //
        // Class Entry Point
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
        /// <summary>
        /// Main Process that runs once at start then at every interval
        /// </summary>
        private static void Run()
        {
            _isBusy = true;
            var thisMachineProcessesManualCases = ImgProc.GetProcessCasesAddedManually() == "1";
            var thisMachineProcessesHl7Cases = ImgProc.GetProcessCasesAddedByHl7() == "1";

            var completedCasesAll = new List<IPendingCase>();
            var failedCasesAll = new List<IPendingCase>();
            var failedCasesHl7 = new List<IPendingCase>();

            if (thisMachineProcessesManualCases)
                completedCasesAll.AddRange(
                    ProcessCasesAddedManually(out failedCasesAll)
                );

            if (thisMachineProcessesHl7Cases)
                completedCasesAll.AddRange(
                    ProcessCasesAddedByHl7(out failedCasesHl7)
                );

            failedCasesAll.AddRange(failedCasesHl7);

            LogProcessedCases(completedCasesAll, failedCasesAll);
            _isBusy = false;
        }

        private static IEnumerable<IPendingCase> ProcessCasesAddedManually
            (out List<IPendingCase> failedCases)
        {
            var pendingCasesManuallyAdded =
                Broker.GetPendingCasesFromCapiDbManuallyAdded(_numberOfCasesToCheck).ToList();

            var completedCasesManual =
                ProcessAllPendingCases(pendingCasesManuallyAdded, out failedCases).ToList();

            DeleteManuallyAddedCompletedFiles(completedCasesManual);

            return completedCasesManual;
        }

        private static IEnumerable<IPendingCase> ProcessCasesAddedByHl7
            (out List<IPendingCase> failedCases)
        {
            var pendingCasesHl7Added =
                Broker.GetPendingCasesFromCapiDbHl7Added(_numberOfCasesToCheck).ToList();

            var completedCasesHl7 =
                ProcessAllPendingCases(pendingCasesHl7Added, out failedCases).ToList();

            return completedCasesHl7;
        }

        private static void DeleteManuallyAddedCompletedFiles(IEnumerable<IPendingCase> completedCasesManual)
        {
            var manualProcessPath = ImgProc.GetManualProcessPath();
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

            Log.Info($"Checking for new cases in {Interval} seconds...");
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