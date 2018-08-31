using CAPI.Agent.Abstractions;
using CAPI.Agent.Abstractions.Models;
using CAPI.Agent.Models;
using CAPI.Common.Config;
using CAPI.Dicom.Abstractions;
using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace CAPI.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Agent : IAgent
    {
        private readonly ILog _log;
        private readonly IDicomFactory _dicomFactory;
        private readonly IImageProcessingFactory _imgProcFactory;
        private readonly IFileSystem _fileSystem;
        private readonly IProcessBuilder _processBuilder;

        public CapiConfig Config { get; set; }
        public bool IsBusy { get; set; }
        private readonly AgentRepository _context;
        private readonly string[] _args;
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
        public Agent(string[] args, IDicomFactory dicomFactory,
                     IImageProcessingFactory imgProcFactory,
                     IFileSystem fileSystem, IProcessBuilder processBuilder,
                     ILog log)
        {
            _dicomFactory = dicomFactory;
            _fileSystem = fileSystem;
            _processBuilder = processBuilder;
            _log = log;
            _imgProcFactory = imgProcFactory;
            _args = args;
            Config = new CapiConfig().GetConfig(args);
            _context = new AgentRepository();
        }

        public void Run()
        {
            Init();

            StartTimer(int.Parse(Config.RunInterval));
        }

        // Runs every interval
        private void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            if (IsBusy)
            {
                _log.Info("Agent is processing cases");
                return;
            }

            IsBusy = true;

            try
            {
                _log.Info("Checking for new cases");
                Config = new CapiConfig().GetConfig(_args);
                _timer.Interval = int.Parse(Config.RunInterval) * 1000;
                ProcessNewlyAddedCases();
            }
            catch (Exception ex)
            {
                _log.Error($"{Environment.NewLine}Error occured while adding new cases to database", ex);
                IsBusy = false;
                return;
            }

            try
            {
                var pendingCases = _context.GetCaseByStatus("Pending").ToList();
                if (pendingCases.ToArray().Any())
                {
                    _log.Info("Start processing pending cases...");
                    pendingCases.ToList().ForEach(ProcessCase);
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                var dbConnectionString = _context.Database.GetDbConnection().ConnectionString;
                _log.Error($"{Environment.NewLine}Unable to get pending cases from database. {dbConnectionString}", ex);
                IsBusy = false;
                throw;
            }
        }

        /// <summary>
        /// Do the actual Processing of the case
        /// </summary>
        /// <param name="case">Pending case to be processed</param>
        private void ProcessCase(ICase @case)
        {
            var recipe = FindRecipe(@case);
            try
            {
                _log.Info($"Accession: [{@case.Accession}] Addition method: [{@case.AdditionMethod}] Start processing case for this case.");
                SetCaseStatus(@case, "Processing");

                var cs = @case as Case ?? throw new ArgumentNullException(nameof(@case), "Case not found in database to be updated");

                Case.Process(recipe, _dicomFactory, _imgProcFactory, Config, _fileSystem, _processBuilder, _log);

                _log.Info($"Accession: [{@case.Accession}] Addition method: [{@case.AdditionMethod}] Processing completed for this case.");
                SetCaseStatus(@case, "Complete");
            }
            catch (Exception ex)
            {
                _log.Error($"{Environment.NewLine}Case failed during processing", ex);
                IsBusy = false;

                SetCaseStatus(@case, "Failed");
                SetCaseComment(@case, ex.Message);
                //throw;
            }
        }

        private void Init()
        {
            HandleFailedCasesAndJobs();
        }
        private void HandleFailedCasesAndJobs()
        {
            var failedCases = _context.GetCaseByStatus("Processing");
            failedCases.ToList().ForEach(c =>
            {
                var tmp = c;
                tmp.Status = "Pending";
                _context.Cases.Update(tmp);
            });
            var failedJobs = _context.GetJobByStatus("Processing");
            failedJobs.ToList().ForEach(j =>
            {
                var tmp = j;
                tmp.Status = "Failed";
                _context.Jobs.Update(tmp);
            });
        }
        private void StartTimer(int interval)
        {
            _timer = new Timer { Interval = interval * 1000, Enabled = true };
            _timer.Elapsed += OnTimeEvent;
            _timer.Start();
        }
        private void ProcessNewlyAddedCases()
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
        private Recipe FindRecipe(ICase @case)
        {
            Recipe recipe;
            if (@case.AdditionMethod == AdditionMethod.Hl7)
            {
                recipe = GetDefaultRecipe();
                recipe.CurrentAccession = @case.Accession;
            }
            else
            {
                // TODO1: Find recipe in manually processing folder
                recipe = GetDefaultRecipe();
                recipe.CurrentAccession = @case.Accession;
            }
            return recipe;
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
        private void SetCaseStatus(ICase @case, string status)
        {
            @case.Status = status;
            _context.Cases.Update(@case as Case ?? throw new ArgumentNullException(nameof(@case),
                                      "Case not found in database to be updated"));
            _context.SaveChanges();
        }
        private void SetCaseComment(ICase @case, string comment)
        {
            @case.Comment = comment;
            _context.Cases.Update(@case as Case ?? throw new ArgumentNullException(nameof(@case),
                                      "Case not found in database to be updated"));
            _context.SaveChanges();
        }

        #region "Handle Manually Added Cases"
        private static IEnumerable<ICase> GetManuallyAddedCases(string manualFolder)
        {
            return (
                from file in Directory.GetFiles(manualFolder)
                let accession = Path.GetFileNameWithoutExtension(file)
                where !file.EndsWith(".recipe.json", StringComparison.InvariantCultureIgnoreCase) // Exclude Recipe Files
                select new Case { Accession = accession, AdditionMethod = AdditionMethod.Manually }
            ).ToList();
        }
        private void HandleManuallyAddedCases()
        {
            var manuallyAddedCases = GetManuallyAddedCases(Config.ManualProcessPath);
            manuallyAddedCases.ToList().ForEach(mc =>
            {
                var inDb = _context.Cases.Select(c => c.Accession)
                    .Any(ac => ac.ToLower().Contains(mc.Accession.ToLower()));
                if (inDb)
                { // If already in database, set status to Pending
                    var inDbCase = _context.Cases
                        .Single(c => c.Accession.Equals(mc.Accession, StringComparison.InvariantCultureIgnoreCase));
                    inDbCase.Status = "Pending";
                    try
                    {
                        _context.Cases.Update(inDbCase);
                        _context.SaveChanges();
                        _log.Info($"Case already in database re-instantiated. Accession: [{inDbCase.Accession}]");
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"{Environment.NewLine}Failed to insert manually added case into database." +
                                   $"{Environment.NewLine}Accession: [{inDbCase.Accession}]", ex);
                        return;
                    }
                }
                else // if not in database, add to database
                {
                    var newCase = new Case { Accession = mc.Accession, Status = "Pending", AdditionMethod = AdditionMethod.Manually };
                    try
                    {
                        _context.Cases.Add(newCase);
                        _context.SaveChanges();
                        _log.Info($"Successfully inserted manually added case into database. Accession: [{newCase.Accession}]");
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"{Environment.NewLine}Failed to insert manually added case into database." +
                                   $"{Environment.NewLine}Accession: [{newCase.Accession}]", ex);
                        return;
                    }
                }
                // Delete file in Manual Folder after it was added to DB
                DeleteFileInFolderAfterAddedToDb(Config.ManualProcessPath, mc.Accession);
            });
        }
        #endregion

        #region "Handle HL7 Added Cases"
        private static IEnumerable<ICase> GetHl7AddedCases(string hl7Folder)
        {
            return (
                from file in Directory.GetFiles(hl7Folder)
                let accession = Path.GetFileNameWithoutExtension(file)
                select new Case { Accession = accession, AdditionMethod = AdditionMethod.Hl7 }
            ).ToList();
        }
        private void HandleHl7AddedCases()
        {
            var hl7AddedCases = GetHl7AddedCases(Config.Hl7ProcessPath);
            hl7AddedCases.ToList().ForEach(mc =>
            {
                var inDb = _context.Cases.Select(c => c.Accession)
                        .Any(ac => ac.ToLower().Contains(mc.Accession.ToLower()));
                if (inDb) return; // If not in database, add to db
                var newCase = new Case
                {
                    Accession = mc.Accession,
                    Status = "Pending",
                    AdditionMethod = AdditionMethod.Hl7
                };
                try
                {
                    _context.Cases.Add(newCase);
                    _context.SaveChanges();
                    _log.Info($"Successfully inserted HL7 added case into database. Accession: [{newCase.Accession}]");
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to insert HL7 added case into database. Accession: [{newCase.Accession}]", ex);
                }
                // Delete file in HL7 Folder after it was added to DB
                DeleteFileInFolderAfterAddedToDb(Config.Hl7ProcessPath, mc.Accession);
                // if already in database, disregard
            });
        }
        #endregion

        private void DeleteFileInFolderAfterAddedToDb(string folderPath, string accession)
        {
            var file = Directory.GetFiles(folderPath)
                .Single(f => Path.GetFileNameWithoutExtension(f).ToLower()
                    .Contains(accession.ToLower()));
            File.Delete(file);
            _log.Info($"File deleted from process folder [{folderPath}] for Accession: [{accession}]");
        }
    }
}
