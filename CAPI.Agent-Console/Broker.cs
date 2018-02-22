﻿using CAPI.Agent_Console.Abstractions;
using CAPI.Common;
using CAPI.DAL.Abstraction;
using CAPI.Dicom.Abstraction;
using CAPI.JobManager;
using CAPI.JobManager.Abstraction;
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

        // Constructor
        public Broker(
            IDicomNodeRepository dicomNodeRepo,
            IRecipeRepositoryInMemory<IRecipe> recipeRepositoryInMemory,
            IJobBuilder jobBuilder)
        {
            _dicomNodeRepo = dicomNodeRepo;
            _recipeRepositoryInMemory = recipeRepositoryInMemory;
            _jobBuilder = jobBuilder;
        }

        // Broker Entry Point
        public static IEnumerable<IPendingCase> GetPendingCasesFromCapiDbHl7Added(int numberOfRowsToCheckInDb)
        {
            CopyAllUnprocessedCasesFromVtDb(numberOfRowsToCheckInDb);

            var pendingCases = new PendingCase().GetRecentPendingCapiCases(false, numberOfRowsToCheckInDb)
                .OrderBy(c => c.Accession).ToList();

            return pendingCases;
        }
        public static IEnumerable<IPendingCase> GetPendingCasesFromCapiDbManuallyAdded(int numberOfCasesToCheck)
        {
            var allCapiCases = new PendingCase().GetAllCapiCases();

            var manualProcessPath = Config.GetManualProcessPath();
            var manuallyAddedAccessions = Directory.GetFiles(manualProcessPath).Select(Path.GetFileName).ToList();

            // Add manually added accession if it doesn't exist in CAPI DB
            var accessionToAdd = manuallyAddedAccessions.Where(mc => !allCapiCases.Any(ac => mc == ac.Accession));
            foreach (var accession in accessionToAdd)
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

        private static void CopyAllUnprocessedCasesFromVtDb(int numberOfCasesToCheck)
        {
            var unprocessedCases = GetLatestVtCasesNotProcessedByCapi(numberOfCasesToCheck)
                .OrderBy(c => c.Accession);

            foreach (var unprocessedCase in unprocessedCases)
            {
                try
                {
                    unprocessedCase.AddToCapiDb();
                    Log.Write($"Accession copied to CAPI database: {unprocessedCase.Accession}");
                }
                catch
                {
                    Log.WriteError($"Failed to add unprocessed case with accession {unprocessedCase.Accession} to CAPI database");
                    throw;
                }
            }
        }

        private static IEnumerable<IPendingCase> GetLatestVtCasesNotProcessedByCapi(int numberOfCasesToCheck)
        {
            var latestVtCases = new PendingCase().GetVtCases(numberOfCasesToCheck);
            var latestCapiCases = new PendingCase().GetCapiCases(numberOfCasesToCheck);
            var latestCapiAccessions = latestCapiCases.Select(c => c.Accession);

            return latestVtCases
                .Where(c => c.Status.ToLower() == "case created")
                .Where(c => !latestCapiAccessions.Contains(c.Accession));
        }

        public bool ProcessCase(IPendingCase pendingCase)
        {
            try
            {
                var recipe = _recipeRepositoryInMemory.GetAll().FirstOrDefault();
                if (recipe != null) recipe.NewStudyAccession = pendingCase.Accession;

                var localDicomNode = GetLocalNode();
                var sourceNode = _dicomNodeRepo.GetAll()
                    .FirstOrDefault(n => n.AeTitle == recipe.SourceAet);

                var job = _jobBuilder.Build(recipe, localDicomNode, sourceNode);
                job.OnLogContentReady += JobLogContentReady;
                job.OnEachProcessCompleted += JobProcessCompleted;

                job.Run();
                return true;
            }
            catch (Exception e)
            {
                pendingCase.Exception = e;
                return false;
            }
        }
        private IDicomNode GetLocalNode()
        {
            return _dicomNodeRepo.GetAll()
                .FirstOrDefault(n => string.Equals(n.AeTitle,
                    Environment.GetEnvironmentVariable("DcmNodeAET_Local", EnvironmentVariableTarget.User),
                    StringComparison.CurrentCultureIgnoreCase));
        }

        // Events
        private static void JobProcessCompleted(object sender, IProcessEventArgument e)
        {
            JobLogContentReady(sender, new LogEventArgument { LogContent = e.LogContent });
            //Log.Write(e.LogContent);
        }
        private static void JobLogContentReady(object sender, ILogEventArgument e)
        {
            Log.Write(e.LogContent);
        }
    }
}