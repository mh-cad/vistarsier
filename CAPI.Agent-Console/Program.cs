using CAPI.DAL;
using CAPI.DAL.Abstraction;
using CAPI.Dicom;
using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager;
using CAPI.JobManager.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Unity;

namespace CAPI.Agent_Console
{
    internal static class Program
    {
        // Fields
        private static int _interval;
        private const int DefaultNoOfCasesToCheck = 1000;
        private static int _numberOfCasesToCheck;
        private static UnityContainer _unityContainer;

        private static void Main(string[] args)
        {
            Log.Write("Agent started");

            SetEnvironmentVariables();

            InitializeUnity();

            GetFirstParamFromArgs(args);

            SetFailedCasesStatusToPending(); // These are cases which failed in the middle of being processed - Set status to "Pending" so they get processed

            Run(); // Run for the first time
            StartTimer();

            Log.Write("Enter 'q' to quit!");
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
                    Log.WriteError($"Failed to set case status from Processing/Queued to Pending. Accession: ${processingCase.Accession}");
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
            Log.Write("Timer started");
        }
        private static void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            Run();
        }
        private static void Run()
        {
            var completedCases = ProcessAllPendingCases(out var failedCases);

            foreach (var completedCase in completedCases)
                Log.Write($"Job completed for accession {completedCase.Accession}");

            foreach (var failedCase in failedCases)
            {
                Log.Write($"Job failed for accession {failedCase.Accession}");
                Log.Exception(failedCase.Exception);
            }

            Log.Write($"Checking for new cases in {_interval} seconds...");
        }
        // Main Process
        private static IEnumerable<PendingCase> ProcessAllPendingCases(out List<PendingCase> failedCases)
        {
            failedCases = new List<PendingCase>();
            var completedCases = new List<PendingCase>();
            var broker = GetBroker();

            // Get Pending Cases and return an empty list if nothing found
            var pendingCases = Broker.GetPendingCasesFromCapiDb(_numberOfCasesToCheck).ToList();
            if (pendingCases.Count < 1) return completedCases;

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
                    pendingCase.SetStatus("Pending");
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
            _unityContainer = new UnityContainer();
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