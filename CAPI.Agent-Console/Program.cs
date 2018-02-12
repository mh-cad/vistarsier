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
        private static int _interval;
        private const int DefaultNoOfCasesToCheck = 1000;
        private static int _numberOfCasesToCheck;
        private static IDicomNodeRepository _dicomNodeRepo;
        private static UnityContainer _unityContainer;
        private static IRecipeRepositoryInMemory<IRecipe> _recipeRepositoryInMemory;
        private static IJobBuilder _jobBuilder;
        private static IDicomNode _localDicomNode;

        private static void Main(string[] args)
        {
            Log.Write("Agent started");

            SetEnvironmentVariables();

            InitializeUnity();

            GetFirstParamFromArgs(args);

            ProcessAllPendingCases();

            Log.Write("Enter 'q' to quit!");
            while (Console.Read() != 'q') { }
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

        private static void ProcessAllPendingCases()
        {
            var currentAccession = string.Empty;

            try
            {
                var resultLog = Broker.CopyPendingCasesFromVtDbToCapiDb(_numberOfCasesToCheck);
                Log.Write(resultLog);
                Log.Write("Pending cases copied from old database to CAPI database");

                StartTimer();
                Log.Write("Timer started");

                _localDicomNode = GetLocalNode();

                var pendingCases = Broker.GetPendingCasesFromCapiDb();

                foreach (var pendingCase in pendingCases)
                {
                    currentAccession = pendingCase.Accession;
                    ProcessCase(pendingCase);
                    Log.Write($"Job completed for accession {pendingCase.Accession}");
                }
            }
            catch (Exception ex)
            {
                Log.Write($"Job failed for accession {currentAccession}");
                Log.Error(ex);
                Log.Write("trying again...");
                ProcessAllPendingCases();
            }
        }

        private static IDicomNode GetLocalNode()
        {
            return _dicomNodeRepo.GetAll()
                .FirstOrDefault(n => string.Equals(n.AeTitle,
                    Environment.GetEnvironmentVariable("DcmNodeAET_Local", EnvironmentVariableTarget.User),
                    StringComparison.CurrentCultureIgnoreCase));
        }

        // Unity
        private static void InitializeUnity()
        {
            _unityContainer = new UnityContainer();
            RegisterClasses();
            _dicomNodeRepo = _unityContainer.Resolve<IDicomNodeRepository>();
            _jobBuilder = _unityContainer.Resolve<IJobBuilder>();
            _recipeRepositoryInMemory = _unityContainer.Resolve<IRecipeRepositoryInMemory<IRecipe>>();
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

        private static void ProcessCase(PendingAccessions pendingCase)
        {
            var recipe = _recipeRepositoryInMemory.GetAll().FirstOrDefault();
            if (recipe != null) recipe.NewStudyAccession = pendingCase.Accession;

            var sourceNode = _dicomNodeRepo.GetAll()
                .FirstOrDefault(n => n.AeTitle == recipe.SourceAet);

            var job = _jobBuilder.Build(recipe, _localDicomNode, sourceNode);
            job.OnLogContentReady += JobLogContentReady;
            job.OnEachProcessCompleted += JobProcessCompleted;

            job.Run();

            Broker.SetJobStatusToComplete(job.DicomSeriesFixed.Original.ParentDicomStudy.AccessionNumber);
        }

        // Events // TODO3: Make JobProcessCompleted event to call JobLogContentReady
        private static void JobProcessCompleted(object sender, IProcessEventArgument e)
        {
            Log.Write(e.LogContent);
        }
        private static void JobLogContentReady(object sender, ILogEventArgument e)
        {
            Log.Write(e.LogContent);
        }

        private static void StartTimer()
        {
            var timer = new Timer();
            timer.Elapsed += OnTimeEvent;
            _interval = Properties.Settings.Default.DbCheckInterval * 1000;
            timer.Interval = _interval;
            timer.Enabled = true;
            timer.Start();
        }
        private static void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            ProcessAllPendingCases();
        }

        private static void GetFirstParamFromArgs(IReadOnlyList<string> args)
        {
            if (args != null && args.Count > 0 && int.TryParse(args[1], out var arg1))
                _numberOfCasesToCheck = arg1;
            else _numberOfCasesToCheck = DefaultNoOfCasesToCheck;
        }
    }
}