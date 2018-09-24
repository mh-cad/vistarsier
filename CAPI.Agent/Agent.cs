﻿using CAPI.Agent.Abstractions;
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
        public bool IsHealthy { get; set; }
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
            IsHealthy = true;
            _dicomFactory = dicomFactory;
            _fileSystem = fileSystem;
            _processBuilder = processBuilder;
            _log = log;
            _imgProcFactory = imgProcFactory;
            _args = args;
            Config = GetCapiConfig(args);
            _context = GetAgentRepository();
        }

        private AgentRepository GetAgentRepository()
        {
            try
            {
                _log.Info("Connecting to database...");
                var repo = Config?.AgentDbConnectionString != null ?
                    new AgentRepository(Config.AgentDbConnectionString) :
                    new AgentRepository();
                repo.Database.EnsureCreated();
                _log.Info($"Database connection established{Environment.NewLine}ConnectionString " +
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
        private CapiConfig GetCapiConfig(string[] args)
        {
            try
            {
                return new CapiConfig().GetConfig(args);
            }
            catch (Exception ex)
            {
                _log.Error("Capi cofig failed to be retreived.", ex);
                IsHealthy = false;
                return null;
            }
        }

        public void Run()
        {
            if (!IsHealthy) return;

            Init();

            CheckForNewCasesAndProcessPendings();

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
            CheckForNewCasesAndProcessPendings();
        }
        /// <summary>
        /// Runs once after app starts, then every interval
        /// </summary>
        private void CheckForNewCasesAndProcessPendings()
        {
            if (IsBusy)
            {
                _log.Info("Agent is busy processing cases");
                return;
            }

            IsBusy = true;

            // Handle newly added cases
            try
            {
                _log.Info("Checking for new cases");
                Config = new CapiConfig().GetConfig(_args);
                _timer.Interval = int.Parse(Config.RunInterval) * 1000;
                HandleNewlyAddedCases();
            }
            catch (Exception ex)
            {
                _log.Error($"{Environment.NewLine}Error occured while adding new cases to database", ex);
                IsBusy = false;
                return;
            }

            // Process next pending case
            try
            {
                var pendingCases = _context.GetCaseByStatus("Pending").ToList();
                if (pendingCases.ToArray().Any())
                {
                    _log.Info("Processing next pending case...");
                    var firstCase = pendingCases.OrderBy(c => c.Id).First();
                    if (firstCase != null) ProcessCase(firstCase);
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                var dbConnectionString = _context.Database.GetDbConnection().ConnectionString;
                _log.Error($"{Environment.NewLine}Failed to get pending cases from database. {dbConnectionString}", ex);
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
                _log.Info(
                    $"Accession: [{@case.Accession}] Addition method: [{@case.AdditionMethod}] Start processing case for this case.");
                SetCaseStatus(@case, "Processing");

                Case.Process(recipe, _dicomFactory, _imgProcFactory, Config, _fileSystem, _processBuilder, _log);

                _log.Info(
                    $"Accession: [{@case.Accession}] Addition method: [{@case.AdditionMethod}] Processing completed for this case.");
                @case.Comment = string.Empty;
                SetCaseStatus(@case, "Complete");
            }
            catch (DirectoryNotFoundException ex)
            {
                if (ex.Message.ToLower().Contains("workable series were found"))
                {
                    _log.Info($"{ex.Message} Accession: [{@case.Accession}]");
                    SetCaseStatus(@case, "Failed");
                    SetCaseComment(@case, ex.Message);
                }
                else throw;
            }
            catch (Exception ex)
            {
                _log.Error($"{Environment.NewLine}Case failed during processing", ex);
                IsBusy = false;

                SetCaseStatus(@case, "Failed");
                SetCaseComment(@case, ex.Message);
            }
        }

        private void Init()
        {
            HandleFailedCasesAndJobs();

            var interval = int.Parse(Config.RunInterval);
            InitTimer(interval);
        }
        private void CleanupProcessFolder(string folderPath, string accession)
        {
            var file = Directory.GetFiles(folderPath)
                .Single(f => Path.GetFileNameWithoutExtension(f).ToLower()
                    .Contains(accession.ToLower()));
            File.Delete(file);
            _log.Info($"File deleted from process folder [{folderPath}] for Accession: [{accession}]");
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
                    inDbCase.AdditionMethod = AdditionMethod.Manually;
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
                CleanupProcessFolder(Config.ManualProcessPath, mc.Accession);
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
                CleanupProcessFolder(Config.Hl7ProcessPath, mc.Accession);
                // if already in database, disregard
            });
        }
        #endregion
    }
}
