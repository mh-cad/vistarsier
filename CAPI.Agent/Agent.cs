using CAPI.Agent.Abstractions;
using CAPI.Agent.Abstractions.Models;
using CAPI.Agent.Models;
using CAPI.Common.Abstractions.Services;
using CAPI.Common.Config;
using CAPI.Dicom.Abstraction;
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">All configuration parameters</param>
        /// <param name="dicomFactory">Creates required dicom services</param>
        /// <param name="imgProcFactory">ImageProcessing Factory</param>
        /// <param name="fileSystem">CAPI FileSystem service</param>
        /// <param name="processBuilder">CAPI Process Builder</param>
        /// <param name="log">log4net logger</param>
        public Agent(CapiConfig config, IDicomFactory dicomFactory,
                     IImageProcessingFactory imgProcFactory,
                     IFileSystem fileSystem, IProcessBuilder processBuilder,
                     ILog log)
        {
            _dicomFactory = dicomFactory;
            _fileSystem = fileSystem;
            _processBuilder = processBuilder;
            _log = log;
            _imgProcFactory = imgProcFactory;
            Config = config;
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
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                ProcessNewlyAddedCases();
            }
            catch
            {
                IsBusy = false;
                return;
            }

            try
            {
                var pendingCases = _context.GetCaseByStatus("Pending");
                pendingCases.ToList().ForEach(ProcessCase);
            }
            catch (Exception ex)
            {
                var dbConnectionString = _context.Database.GetDbConnection().ConnectionString;
                _log.Error($"Unable to get pending cases from database. {dbConnectionString}", ex);
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
                cs.Process(recipe, _dicomFactory, _imgProcFactory, Config, _fileSystem, _processBuilder, _log);

                _log.Info($"Accession: [{@case.Accession}] Addition method: [{@case.AdditionMethod}] Processing completed for this case.");
                SetCaseStatus(@case, "Complete");
            }
            catch (Exception ex)
            {
                _log.Error("Case failed during processing", ex);
                IsBusy = false;

                var failedCases = _context.GetCaseByStatus("Processing");
                failedCases.ToList().ForEach(c => { SetCaseStatus(c, "Failed"); });

                throw;
            }
        }

        private void Init()
        {
            SetFailedCasesStatusToPending();
        }
        private void SetFailedCasesStatusToPending()
        {
            var failedCases = _context.GetCaseByStatus("Processing");
            failedCases.ToList().ForEach(c =>
            {
                var tmp = c;
                tmp.Status = "Pending";
                _context.Cases.Update(tmp);
            });
        }
        private void StartTimer(int interval)
        {
            var timer = new Timer { Interval = interval * 1000, Enabled = true };
            timer.Elapsed += OnTimeEvent;
            timer.Start();
        }
        private void ProcessNewlyAddedCases()
        {
            try
            {
                HandleManuallyAddedCases();
            }
            catch (Exception ex)
            {
                _log.Error("Failed to get manually added cases.", ex);
                throw;
            }

            try
            {
                HandleHl7AddedCases();
            }
            catch (Exception ex)
            {
                _log.Error("Failed to get HL7 added cases.", ex);
                throw;
            }
        }
        private Recipe FindRecipe(ICase @case)
        {
            if (@case.AdditionMethod == AdditionMethod.Hl7)
            {
                return GetDefaultRecipe();
            }
            else
            {
                // Find recipe in manually processing folder
                //throw new NotImplementedException();
                return GetDefaultRecipe();
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
            catch
            {
                // Log "Unable to convert default recipe file to json"
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
                //.Contains(mc.Accession, StringComparer.InvariantCultureIgnoreCase);
                if (inDb)
                { // If already in database, set status to Pending
                    var inDbCase = _context.Cases.Single(c => c.Accession.Equals(mc.Accession, StringComparison.InvariantCultureIgnoreCase));
                    inDbCase.Status = "Pending";
                    _context.Cases.Update(inDbCase);
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
                        _log.Error($"Failed to insert manually added case into database. Accession: [{newCase.Accession}]", ex);
                    }

                }
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
            var hl7AddedCases = GetHl7AddedCases(Config.ManualProcessPath);
            hl7AddedCases.ToList().ForEach(mc =>
            {
                var inDb = _context.Cases.Select(c => c.Accession)
                        .Any(ac => ac.ToLower().Contains(mc.Accession.ToLower()));
                //.Contains(mc.Accession, StringComparer.InvariantCultureIgnoreCase);
                if (inDb) return; // If not in database, add to db
                var newCase = new Case { Accession = mc.Accession, AdditionMethod = AdditionMethod.Hl7 };
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
                // if already in database, disregard?
            });
        }
        #endregion
    }
}
