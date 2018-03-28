using CAPI.Agent_Console.Abstractions;
using CAPI.Common.Config;
using CAPI.DAL.Abstraction;
using CAPI.Dicom.Abstraction;
using CAPI.JobManager;
using CAPI.JobManager.Abstraction;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CAPI.Agent_Console
{
    public class Broker
    {
        private readonly IDicomNodeRepository _dicomNodeRepo;
        private readonly IRecipeRepositoryInMemory<IRecipe> _recipeRepositoryInMemory;
        private readonly IJobBuilder _jobBuilder;
        private readonly IAgentConsoleFactory _agentConsoleFactory;
        private readonly IAgentConsoleRepository _agentConsoleRepository;
        private readonly ILog _log = LogHelper.GetLogger();
        private readonly string _manualProcessFolder = ImgProc.GetManualProcessPath();
        private readonly string _hl7ProcessFolder = ImgProc.GetHl7ProcessPath();
        public bool IsBusy;
        private readonly int _interval = Properties.Settings.Default.DbCheckInterval;

        // Constructor
        public Broker(
            IDicomNodeRepository dicomNodeRepo,
            IRecipeRepositoryInMemory<IRecipe> recipeRepositoryInMemory,
            IJobBuilder jobBuilder, IAgentConsoleFactory agentConsoleFactory)
        {

            //Log = LogHelper.GetLogger();
            _dicomNodeRepo = dicomNodeRepo;
            _recipeRepositoryInMemory = recipeRepositoryInMemory;
            _jobBuilder = jobBuilder;
            _agentConsoleFactory = agentConsoleFactory;
            _agentConsoleRepository = _agentConsoleFactory.CreateAgentConsoleRepository();
        }

        public void Run()
        {
            IsBusy = true;
            var thisMachineProcessesManualCases = ImgProc.GetProcessCasesAddedManually() == "1";
            var thisMachineProcessesHl7Cases = ImgProc.GetProcessCasesAddedByHl7() == "1";

            var completedCasesAll = new List<IVerifiedMri>();
            var failedCasesAll = new List<IVerifiedMri>();
            var failedCasesHl7 = new List<IVerifiedMri>();

            if (thisMachineProcessesManualCases)
                completedCasesAll.AddRange(
                    GetPendingCasesAndProcessThem("Manual", out failedCasesAll)
                );

            if (thisMachineProcessesHl7Cases)
                completedCasesAll.AddRange(
                    GetPendingCasesAndProcessThem("HL7", out failedCasesHl7)
                );

            failedCasesAll.AddRange(failedCasesHl7);

            LogProcessedCases(completedCasesAll, failedCasesAll);
            IsBusy = false;
        }

        private IEnumerable<IVerifiedMri> GetPendingCasesAndProcessThem
            (string additionMethod, out List<IVerifiedMri> failedCases)
        {
            if (additionMethod == "Manual") AddNewCasesFromManualProcessFolderToDb();
            else if (additionMethod == "HL7") AddNewCasesFromHl7ProcessFolderToDb();

            var pendingCases =
                _agentConsoleRepository.GetPendingCases()
                .Where(c => c.AdditionMethod == additionMethod).ToList();

            var completedCasesManual =
                ProcessPendingCases(pendingCases, out failedCases).ToList();

            DeleteManuallyAddedCompletedFiles(completedCasesManual);

            return completedCasesManual;
        }

        public void AddNewCasesFromManualProcessFolderToDb()
        {
            var newlyAddedManualFiles = Directory.GetFiles(_manualProcessFolder).ToList();

            var allManualCases = _agentConsoleRepository.GetAllManualCases().Select(c => c.Accession);

            newlyAddedManualFiles.ForEach(filePath =>
            {
                var filename = Path.GetFileName(filePath);
                var verifiedMri = _agentConsoleFactory.CreateVerifiedMri();
                verifiedMri.Accession = filename;
                verifiedMri.AdditionMethod = "Manual";
                verifiedMri.Status = "Pending";

                if (allManualCases.Contains(filename))
                    _agentConsoleRepository.SetVerifiedMriStatus(filename, "Pending"); // If accession already exists in DB, update status to Pending
                else _agentConsoleRepository.InsertVerifiedMriIntoDb(verifiedMri); // If not, add it to DB

                DeleteFileIfAlreadyInDb(filePath);
            });
        }
        public void AddNewCasesFromHl7ProcessFolderToDb()
        {
            var newlyAddedHl7Files = Directory.GetFiles(_hl7ProcessFolder).ToList();

            var allManualCases = _agentConsoleRepository.GetAllManualCases().Select(c => c.Accession);

            newlyAddedHl7Files.ForEach(filePath =>
            {
                var filename = Path.GetFileName(filePath);
                var verifiedMri = _agentConsoleFactory.CreateVerifiedMri();
                verifiedMri.Accession = filename;
                verifiedMri.AdditionMethod = "HL7";
                verifiedMri.Status = "Pending";

                if (allManualCases.Contains(filename))
                    _agentConsoleRepository.UpdateVerifiedMri(verifiedMri); // If accession already exists in DB, update status to Pending
                else _agentConsoleRepository.InsertVerifiedMriIntoDb(verifiedMri); // If not, add it to DB

                DeleteFileIfAlreadyInDb(filePath);
            });
        }

        private void DeleteFileIfAlreadyInDb(string filePath)
        {
            var filename = Path.GetFileName(filePath);
            if (_agentConsoleRepository.AccessionExistsInDb(filename)) File.Delete(filePath);
        }

        private IEnumerable<IVerifiedMri> ProcessPendingCases
            (IList<IVerifiedMri> pendingCases, out List<IVerifiedMri> failedCases)
        {
            failedCases = new List<IVerifiedMri>();
            var completedCases = new List<IVerifiedMri>();

            // Return an empty list if no pending case is found
            if (!pendingCases.Any()) return completedCases;

            // Set status of all pending cases to Queued
            foreach (var pendingCase in pendingCases)
                _agentConsoleRepository.SetVerifiedMriStatus(pendingCase.Accession, "Queued");

            foreach (var pendingCase in pendingCases)
            {
                _agentConsoleRepository.SetVerifiedMriStatus(pendingCase.Accession, "Processing");
                var success = ProcessCase(pendingCase);

                if (success)
                {
                    _agentConsoleRepository.SetVerifiedMriStatus(pendingCase.Accession, "Completed");
                    completedCases.Add(pendingCase);
                }
                else
                {
                    _agentConsoleRepository.SetVerifiedMriStatus(pendingCase.Accession, "Failed");
                    failedCases.Add(pendingCase);
                }
            }

            return completedCases;
        }

        private bool ProcessCase(IVerifiedMri pendingCase)
        {
            try
            {
                var recipe = _recipeRepositoryInMemory.GetAll().FirstOrDefault();
                if (recipe != null && string.IsNullOrEmpty(recipe.NewStudyAccession))
                    recipe.NewStudyAccession = pendingCase.Accession;

                var localDicomNode = GetLocalNode();
                var sourceNode = _dicomNodeRepo.GetAll()
                    .FirstOrDefault(n => n.AeTitle == recipe.SourceAet);

                var job = _jobBuilder.Build(recipe, localDicomNode, sourceNode);
                job.OnLogContentReady += JobLogContentReady;
                job.OnEachProcessCompleted += JobProcessCompleted;

                job.Run();
                return true;
            }
            catch (Exception ex)
            {
                pendingCase.Exception = ex;
                return false;
            }
        }

        #region "Old Methods"
        public IEnumerable<IPendingCase> GetPendingCasesFromCapiDbHl7Added(int numberOfRowsToCheckInDb)
        {
            CopyAllUnprocessedCasesFromVtDb(numberOfRowsToCheckInDb);

            var pendingCases = new PendingCase().GetRecentPendingCapiCases(false, numberOfRowsToCheckInDb)
                .OrderBy(c => c.Accession).ToList();

            return pendingCases;
        }
        public static IEnumerable<IPendingCase> GetPendingCasesFromCapiDbManuallyAdded(int numberOfCasesToCheck)
        {
            var allCapiCases = new PendingCase().GetAllCapiCases();

            var manualProcessPath = ImgProc.GetManualProcessPath();
            var manuallyAddedAccessions = Directory.GetFiles(manualProcessPath).Select(Path.GetFileName).ToList();

            // Add manually added accession if it doesn't exist in CAPI DB
            var accessionsToAdd = manuallyAddedAccessions.Where(mc => !allCapiCases.Any(ac => mc == ac.Accession));
            foreach (var accession in accessionsToAdd)
                new PendingCase { Accession = accession }.AddToCapiDb(true); // also sets the status to 'Pending' and AdditionMethod to 'Manual'

            // If manually added accession exists in CAPI DB THEN mark it as PENDING
            var casesToUpdate = allCapiCases.Where(c => manuallyAddedAccessions.Contains(c.Accession));
            foreach (var pendingCase in casesToUpdate)
            {
                pendingCase.SetStatus("Pending");
                pendingCase.UpdateAdditionMethodToManual(true);
            }

            var pendingCases = new PendingCase().GetRecentPendingCapiCases(true, numberOfCasesToCheck);

            return pendingCases;
        }

        private void CopyAllUnprocessedCasesFromVtDb(int numberOfCasesToCheck)
        {
            var unprocessedCases = GetLatestVtCasesNotProcessedByCapi(numberOfCasesToCheck)
                .OrderBy(c => c.Accession);

            foreach (var unprocessedCase in unprocessedCases)
            {
                try
                {
                    unprocessedCase.AddToCapiDb();
                    _log.Info($"Accession copied to CAPI database: {unprocessedCase.Accession}");
                }
                catch (Exception ex)
                {
                    _log.Error(
                        $"Failed to add unprocessed case with accession {unprocessedCase.Accession} to CAPI database"
                        , ex);
                    throw;
                }
            }
        }

        private static IEnumerable<IPendingCase> GetLatestVtCasesNotProcessedByCapi(int numberOfCasesToCheck)
        {
            var latestVtCases = new PendingCase().GetVtCases(numberOfCasesToCheck).ToList();
            var latestCapiAccessions = new PendingCase().GetCapiCases(numberOfCasesToCheck)
                .Select(c => c.Accession);

            var vtCasesNotProcessed =
                latestVtCases
                .Where(c => c.Status.ToLower() == "case created"
                    && !latestCapiAccessions.Contains(c.Accession));

            return vtCasesNotProcessed;
        }

        public bool ProcessCaseOld(IPendingCase pendingCase)
        {
            try
            {
                var recipe = _recipeRepositoryInMemory.GetAll().FirstOrDefault();
                if (recipe != null && string.IsNullOrEmpty(recipe.NewStudyAccession))
                    recipe.NewStudyAccession = pendingCase.Accession;

                var localDicomNode = GetLocalNode();
                var sourceNode = _dicomNodeRepo.GetAll()
                    .FirstOrDefault(n => n.AeTitle == recipe.SourceAet);

                var job = _jobBuilder.Build(recipe, localDicomNode, sourceNode);
                job.OnLogContentReady += JobLogContentReady;
                job.OnEachProcessCompleted += JobProcessCompleted;

                job.Run();
                return true;
            }
            catch (Exception ex)
            {
                pendingCase.Exception = ex;
                return false;
            }
        }
        #endregion

        private IDicomNode GetLocalNode()
        {
            return _dicomNodeRepo.GetAll()
                .FirstOrDefault(n => string.Equals(n.AeTitle,
                    Environment.GetEnvironmentVariable("DcmNodeAET_Local", EnvironmentVariableTarget.User),
                    StringComparison.CurrentCultureIgnoreCase));
        }

        // Events
        private void JobProcessCompleted(object sender, IProcessEventArgument e)
        {
            JobLogContentReady(sender, new LogEventArgument { LogContent = e.LogContent });
            //Log.Write(e.LogContent);
        }
        private void JobLogContentReady(object sender, ILogEventArgument e)
        {
            _log.Info(e.LogContent);
        }

        private void LogProcessedCases(
            IEnumerable<IVerifiedMri> completedCases, IEnumerable<IVerifiedMri> failedCases)
        {
            foreach (var completedCase in completedCases)
                _log.Info($"Job completed for accession {completedCase.Accession}");

            foreach (var failedCase in failedCases)
                _log.Error($"Job failed for accession {failedCase.Accession}", failedCase.Exception);

            _log.Info($"Checking for new cases in {_interval} seconds...");
        }
        private void DeleteManuallyAddedCompletedFiles(IEnumerable<IVerifiedMri> completedCasesManual)
        {
            var manualProcessPath = ImgProc.GetManualProcessPath();
            var manualProcessFiles = Directory.GetFiles(manualProcessPath);
            manualProcessFiles
                .Where(f => completedCasesManual.Select(c => c.Accession).Contains(Path.GetFileName(f)))
                .ToList()
                .ForEach(File.Delete);
        }
    }
}