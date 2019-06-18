using VisTarsier.Service;
using VisTarsier.Config;
using VisTarsier.Common;
using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace VisTarsier.Service
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Agent : IAgent
    {
        private readonly ILog _log;

        public CapiConfig Config { get; set; }
        public bool IsBusy { get; set; }
        public bool IsHealthy { get; set; }
        private DbBroker _context;
        private Timer _timer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="args">Arguements passed to agent</param>
        /// <param name="dicomFactory">Creates required dicom services</param>
        /// <param name="imgProcFactory">ImageProcessing Factory</param>
        /// <param name="fileSystem">CAPI FileSystem service</param>
        /// <param name="processBuilder">CAPI Process Builder</param>
        /// <param name="log">log4net logger</param>
        public Agent()
        {
            IsHealthy = true;
            _log = Log.GetLogger();
            Config = GetCapiConfig();
            _context = GetAgentRepository(true);
        }

        private DbBroker GetAgentRepository(bool firstUse = false)
        {
            try
            {
                if (firstUse) _log.Info("Connecting to database...");
                var repo = Config?.AgentDbConnectionString != null ?
                    new DbBroker(Config.AgentDbConnectionString) :
                    new DbBroker();
                repo.Database.EnsureCreated();
                if (firstUse) _log.Info($"Database connection established{Environment.NewLine}ConnectionString " +
                          $"[{repo.Database.GetDbConnection().ConnectionString}]");

                return repo;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to connect to database.", ex);
                IsHealthy = false;
            }
            return null;
        }

        private CapiConfig GetCapiConfig()
        {
            try
            {
                return CapiConfig.GetConfig();
            }
            catch (Exception ex)
            {
                _log.Error("Capi cofig failed to be retreived.", ex);
                IsHealthy = false;
                return null;
            }
        }

        public void Start()
        {
            _log.Info("Agent started. Healthy : " + IsHealthy);
            if (!IsHealthy) return;

            _log.Info("Handing failed cases and jobs");
            try { HandleFailedCasesAndJobs(); }
            catch(Exception e) { _log.Error(e); }

            _log.Info("Starting timer");
            var interval = int.Parse(Config.RunInterval);
            InitTimer(interval);

            _log.Info("Initial run...");
            Run();

            _timer.Start();
        }

        private void InitTimer(int interval)
        {
            _timer = new Timer { Interval = interval * 1000, Enabled = true };
            _timer.Elapsed += OnTimeEvent;
        }

        /// <summary>
        /// Runs every interval
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            _context = GetAgentRepository();
            Run();
        }
        /// <summary>
        /// Runs once after app starts, then every interval
        /// </summary>
        public void Run()
        {
            // Check if we're busy processing cases.
            if (IsBusy)
            {
                _log.Info("Agent is busy processing cases");
                return;
            }

            // Global agent lock.
            IsBusy = true;

            // Handle newly added cases (add to DB)
            try
            {
                _log.Info("Checking for new cases");
                Config = CapiConfig.GetConfig();
                _timer.Interval = int.Parse(Config.RunInterval) * 1000;
                HandleNewlyAddedCases();
            }
            catch (Exception ex)
            {
                _log.Error($"{Environment.NewLine}Error occured while adding new cases to database", ex);
                IsBusy = false;
                return;
            }

            // Process next pending case (from the DB)
            try
            {
                var pendingCases = _context.GetCaseByStatus("Pending").ToList();
                while (pendingCases.Count > 0)
                {
                    _log.Info("Processing next pending case...");
                    var firstCase = pendingCases.OrderBy(c => c.Id).First();
                    if (firstCase != null)
                    {
                        pendingCases.Remove(firstCase);
                        ProcessCase(firstCase);
                    }
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                var dbConnectionString = _context.Database.GetDbConnection().ConnectionString;
                _log.Error($"{Environment.NewLine}Failed to get pending attempts from database. {dbConnectionString}", ex);
                IsBusy = false;
                throw;
            }
        }

        /// <summary>
        /// Do the actual Processing of the case
        /// </summary>
        /// <param name="attempt">Pending case to be processed</param>
        private void ProcessCase(Attempt attempt)
        {
            var recipe = FindRecipe(attempt);
            try
            {
                _log.Info(
                    $"Accession: [{attempt.CurrentAccession}] Addition method: [{attempt.Method}] Start processing case for this case.");
                SetCaseStatus(attempt, "Processing");

                attempt.Process(recipe, _context);

                _log.Info(
                    $"Accession: [{attempt.CurrentAccession}] Addition method: [{attempt.Method}] Processing completed for this case.");
                _log.Info("-------------------------");
                attempt.Comment = string.Empty;
                SetCaseStatus(attempt, "Complete");
            }
            catch (DirectoryNotFoundException ex)
            {
                if (ex.Message.ToLower().Contains("workable series were found"))
                {
                    _log.Info($"{ex.Message} Accession: [{attempt.CurrentAccession}]");
                    SetCaseStatus(attempt, "Unworkable");
                    SetCaseComment(attempt, ex.Message);
                }
                else throw;
            }
            catch (Exception ex)
            {
                _log.Error($"{Environment.NewLine}Case failed during processing", ex);
                IsBusy = false;

                SetCaseStatus(attempt, "Failed");
                SetCaseComment(attempt, ex.Message);
            }
        }


        private void CleanupProcessFolder(string folderPath, string accession)
        {
            if (!Directory.Exists(folderPath))
            {
                _log.Error($"Folder does not exist in the following path: [{folderPath}]");
                return;
            }

            var files = Directory.GetFiles(folderPath);
            if (!files.Any()) return;
            try
            {
                var file = files
                    .OrderBy(f => f)
                    .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).ToLower()
                        .Contains(accession.ToLower()));
                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                {
                    File.Delete(file);
                    _log.Info($"File deleted from process folder [{folderPath}] for accession: [{accession}]");
                }
                else _log.Warn($"Corresponding file for accession [{accession}] in folder [{folderPath}] was NOT found to be deleted!");
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to delete file in process folder [{folderPath}] for accession [{accession}]", ex);
                throw;
            }
        }
        private void HandleFailedCasesAndJobs()
        {
            var failedCases = _context.GetCaseByStatus("Processing");
            foreach (var c in failedCases)
            {
                var tmp = c;
                tmp.Status = "Pending";
                _context.Attempts.Update(tmp);
                _context.SaveChanges();
            }
            var failedJobs = _context.GetJobByStatus("Processing");
            foreach (var j in failedJobs)
            {
                var tmp = j;
                tmp.Status = "Failed";
                _context.Jobs.Update(tmp);
                _context.SaveChanges();
            }
        }
        private void HandleNewlyAddedCases()
        {
            try
            {
                if (Config.ProcessCasesAddedManually)
                    HandleManuallyAddedCases();
            }
            catch (Exception ex)
            {
                _log.Error($"{Environment.NewLine}Failed to get manually added cases.", ex);
                throw;
            }

            try
            {
                if (Config.ProcessCasesAddedByHL7)
                    HandleHl7AddedCases();
            }
            catch (Exception ex)
            {
                _log.Error($"{Environment.NewLine}Failed to get HL7 added cases.", ex);
                throw;
            }
        }
        private Recipe FindRecipe(Attempt @case)
        {
            Recipe recipe;

            
            if (@case.Method == Attempt.AdditionMethod.Hl7)
            {
                recipe = GetDefaultRecipe();
                recipe.CurrentAccession = @case.CurrentAccession;
                // Delete file in HL7 Folder after it was added to DB
                CleanupProcessFolder(Config.Hl7ProcessPath, @case.CurrentAccession);
            }
            else
            {
                recipe = GetRecipeForManualCase(@case);
                recipe.CurrentAccession = @case.CurrentAccession;
                // Delete file in Manual Folder after it was added to DB
                CleanupProcessFolder(Config.ManualProcessPath, @case.CurrentAccession);
            }
            return recipe;
        }

        private Recipe GetRecipeForManualCase(Attempt @case)
        {
            var recipeFilePath = Directory.GetFiles(Config.ManualProcessPath)
                .OrderBy(f => f)
                .FirstOrDefault(f => Path.GetFileName(f).ToLower()
                    .StartsWith(@case.CurrentAccession, StringComparison.CurrentCultureIgnoreCase));

            if (recipeFilePath == null || string.IsNullOrEmpty(recipeFilePath) || !File.Exists(recipeFilePath) || !recipeFilePath.EndsWith(".json", StringComparison.CurrentCultureIgnoreCase))
                return GetDefaultRecipe();

            var recipeText = File.ReadAllText(recipeFilePath);
            try
            {
                return JsonConvert.DeserializeObject<Recipe>(recipeText);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to convert from json to recipe object.", ex);
                throw;
            }
        }

        private Recipe GetDefaultRecipe()
        {
            if (!File.Exists(Config.DefaultRecipePath))
                throw new FileNotFoundException($"Unable to locate default recipe file in following path: [{Config.DefaultRecipePath}]");
            var recipeText = File.ReadAllText(Config.DefaultRecipePath);
            try
            {
                return JsonConvert.DeserializeObject<Recipe>(recipeText);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to convert from json to recipe object.", ex);
                throw;
            }
        }

        private void SetCaseStatus(Attempt @case, string status)
        {
            @case.Status = status;
            _context.Attempts.Update(@case as Attempt ?? throw new ArgumentNullException(nameof(@case),
                                      "Case not found in database to be updated"));
            _context.SaveChanges();
        }
        private void SetCaseComment(Attempt @case, string comment)
        {
            @case.Comment = comment;
            _context.Attempts.Update(@case as Attempt ?? throw new ArgumentNullException(nameof(@case),
                                      "Case not found in database to be updated"));
            _context.SaveChanges();
        }

        #region "Handle Manually Added Cases"
        private static IEnumerable<Attempt> GetManuallyAddedCases(string manualFolder)
        {
            var cases = (
                from file in Directory.GetFiles(manualFolder)
                let accession =
                    file.EndsWith(".recipe.json", StringComparison.CurrentCultureIgnoreCase) ?
                        Path.GetFileName(file.ToLower()).Replace(".recipe.json", "").Split('.')[0] : // recipe file
                        Path.GetFileNameWithoutExtension(file) // non-recipe file
                select new Attempt { CurrentAccession = accession, Method = Attempt.AdditionMethod.Manually }
            ).ToList();
            return cases;

        }
        private void HandleManuallyAddedCases()
        {
            var manuallyAddedCases = GetManuallyAddedCases(Config.ManualProcessPath).ToList();
            if (!manuallyAddedCases.Any()) return;

            manuallyAddedCases.ToList().ForEach((Action<Attempt>)(mc =>
            {
                var inDb = _context.Attempts.Any((System.Linq.Expressions.Expression<Func<Attempt, bool>>)(c => 
                        (bool)(c.CurrentAccession.Equals((string)mc.CurrentAccession) 
                        && ((c.CurrentSeriesUID == null && mc.CurrentSeriesUID == null) || c.CurrentSeriesUID.Equals(mc.CurrentSeriesUID))
                        && ((c.PriorSeriesUID == null && mc.PriorSeriesUID == null) || c.PriorSeriesUID.Equals(mc.PriorSeriesUID)))));

                if (inDb)
                { // If already in database, set status to Pending
                    var inDbCase = _context.Attempts
                        .Single((System.Linq.Expressions.Expression<Func<Attempt, bool>>)(c => (bool)c.CurrentAccession.Equals((string)mc.CurrentAccession, StringComparison.InvariantCultureIgnoreCase)));
                    inDbCase.Status = "Pending";
                    inDbCase.Method = Attempt.AdditionMethod.Manually;
                    try
                    {
                        _context.Attempts.Update(inDbCase);
                        _context.SaveChanges();
                        _log.Info($"Case already in database re-instantiated. Accession: [{inDbCase.CurrentAccession}]");
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"{Environment.NewLine}Failed to insert manually added case into database." +
                                   $"{Environment.NewLine}Accession: [{inDbCase.CurrentAccession}]", ex);
                    }
                }
                else // if not in database, add to database
                {
                    var newCase = new Attempt { CurrentAccession = mc.CurrentAccession.ToUpper(), Status = "Pending", Method = Attempt.AdditionMethod.Manually };
                    try
                    {
                        _context.Attempts.Add(newCase);
                        _context.SaveChanges();
                        _log.Info($"Successfully inserted manually added case into database. Accession: [{newCase.CurrentAccession}]");
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"{Environment.NewLine}Failed to insert manually added case into database." +
                                   $"{Environment.NewLine}Accession: [{newCase.CurrentAccession}]", ex);
                    }
                }
            }));

        }
        #endregion

        #region "Handle HL7 Added Cases"
        private static IEnumerable<Attempt> GetHl7AddedCases(string hl7Folder)
        {
            return (
                from file in Directory.GetFiles(hl7Folder)
                let accession = Path.GetFileNameWithoutExtension(file)
                select new Attempt { CurrentAccession = accession, Method = Attempt.AdditionMethod.Hl7 }
            ).ToList();
        }
        private void HandleHl7AddedCases()
        {
            var hl7AddedCases = GetHl7AddedCases(Config.Hl7ProcessPath);
            hl7AddedCases.ToList().ForEach((Action<Attempt>)(mc =>
            {
                var inDb = _context.Attempts.Any((System.Linq.Expressions.Expression<Func<Attempt, bool>>)(c => 
                        (bool)(c.CurrentAccession.Equals((string)mc.CurrentAccession)
                        && ((c.CurrentSeriesUID == null && mc.CurrentSeriesUID == null) || c.CurrentSeriesUID.Equals(mc.CurrentSeriesUID))
                        && ((c.PriorSeriesUID == null && mc.PriorSeriesUID == null) || c.PriorSeriesUID.Equals(mc.PriorSeriesUID)))));

            if (inDb) return; // If not in database, add to db
                var @newCase = new Attempt
                {
                    CurrentAccession = mc.CurrentAccession,
                    Status = "Pending",
                    Method = Attempt.AdditionMethod.Hl7
                };
                try
                {
                    _log.Info("JobID for newCase : " + newCase.JobId);
                    _context.Attempts.Add(newCase);
                    _context.SaveChanges();
                    _log.Info($"Successfully inserted HL7 added case into database. Accession: [{newCase.CurrentAccession}]");
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to insert HL7 added case into database. Accession: [{newCase.CurrentAccession}]", ex);
                }
                // if already in database, disregard
            }));
        }
        #endregion
    }
}
