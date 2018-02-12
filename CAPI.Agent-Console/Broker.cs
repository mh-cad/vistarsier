using System.Collections.Generic;
using System.Linq;

namespace CAPI.Agent_Console
{
    public static class Broker
    {
        private static bool _isBusy;

        public static string CopyPendingCasesFromVtDbToCapiDb(int numberOfCasesToCheck)
        {
            const string brokerIsBusy = "Checking for new cases to process...";

            if (_isBusy) return brokerIsBusy;
            _isBusy = true;

            CopyAllUnprocessedCasesFromVtDb(numberOfCasesToCheck);

            _isBusy = false;
            return brokerIsBusy;
        }

        private static void CopyAllUnprocessedCasesFromVtDb(int numberOfCasesToCheck)
        {
            var unprocessedCases = GetLatestUnprocessedVtCases(numberOfCasesToCheck)
                .OrderBy(c => c.Accession);

            foreach (var unprocessedCase in unprocessedCases)
                unprocessedCase.AddToCapiDb();
        }

        private static IEnumerable<PendingAccessions> GetLatestUnprocessedVtCases(int numberOfCasesToCheck)
        {
            var latestVtCases = new PendingAccessions().GetVtCases(numberOfCasesToCheck);
            var latestCapiCases = new PendingAccessions().GetCapiCases(numberOfCasesToCheck);
            var latestCapiAccessions = latestCapiCases.Select(c => c.Accession);

            return latestVtCases
                .Where(c => c.Status.ToLower() == "case created")
                .Where(c => !latestCapiAccessions.Contains(c.Accession));
        }

        public static IEnumerable<PendingAccessions> GetPendingCasesFromCapiDb()
        {
            var pendingCases = new PendingAccessions().GetPendingCapiCases()
                .OrderBy(c => c.Accession).ToList();

            foreach (var pendingCase in pendingCases)
                Log.Write($"Accession copied to CAPI database: {pendingCase.Accession}");

            return pendingCases;
        }

        public static void SetJobStatusToComplete(string accessionNumber)
        {
            var pendingCase = new PendingAccessions { Accession = accessionNumber };
            pendingCase.SetStatus("Completed");
        }
    }
}