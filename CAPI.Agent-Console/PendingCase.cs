using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CAPI.Agent_Console
{
    public class PendingCase
    {
        private readonly string _vtConnectionString;
        private readonly string _capiConnectionString;

        public PendingCase()
        {
            _vtConnectionString = Properties.Settings.Default.VtConnectionString;
            _capiConnectionString = Properties.Settings.Default.CapiConnectionString;
        }

        public string Id { get; set; }
        public string Accession { get; set; }
        public string Status { get; set; }
        public bool ProcessCompleted { get; set; }
        public bool ProcessFailed { get; set; }
        public Exception Exception { get; set; }

        public IEnumerable<PendingCase> GetVtCases(int count)
        {
            return GetCases(count, _vtConnectionString);
        }

        public IEnumerable<PendingCase> GetCapiCases(int count)
        {
            return GetCases(count, _capiConnectionString);
        }
        private static IEnumerable<PendingCase> GetCases(int count, string connectionString)
        {
            IEnumerable<PendingCase> latestVtCases;

            using (IDbConnection db = new SqlConnection(connectionString))
            {
                const string sqlCommand =
                    "Select TOP {=count} * FROM PendingAccessions ORDER BY Id DESC";

                latestVtCases = db.Query<PendingCase>(sqlCommand, new { count });
            }

            return latestVtCases;
        }
        public IEnumerable<PendingCase> GetPendingCapiCases(int numOfcasesToCheckInDb = 1000)
        {
            IEnumerable<PendingCase> capiPendingCases;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select TOP (@count) * FROM PendingAccessions WHERE Status=(@status)";

                capiPendingCases = db.Query<PendingCase>(sqlCommand, new { count = numOfcasesToCheckInDb, status = "Pending" });
            }

            return capiPendingCases;
        }
        public IEnumerable<PendingCase> GetProcessingCapiCases(int numOfcasesToCheckInDb = 1000)
        {
            IEnumerable<PendingCase> processingCapiCases;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select TOP (@count) * FROM PendingAccessions WHERE Status=(@status)";

                processingCapiCases = db.Query<PendingCase>(sqlCommand, new { count = numOfcasesToCheckInDb, status = "Processing" });
            }

            return processingCapiCases;
        }
        public IEnumerable<PendingCase> GetQueuedCapiCases(int numOfcasesToCheckInDb = 1000)
        {
            IEnumerable<PendingCase> queuedCapiCases;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select TOP (@count) * FROM PendingAccessions WHERE Status=(@status)";

                queuedCapiCases = db.Query<PendingCase>(sqlCommand, new { count = numOfcasesToCheckInDb, status = "Queued" });
            }

            return queuedCapiCases;
        }

        public void AddToCapiDb()
        {
            try
            {
                using (IDbConnection db = new SqlConnection(_capiConnectionString))
                {
                    const string sqlCommand =
                        "INSERT INTO PendingAccessions (Accession, Status) VALUES (@accession, @status)";
                    db.Query(sqlCommand, new { accession = Accession, status = "Pending" });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error Message: {e.Message}");
                Console.WriteLine($"Error Source: {e.Source}");
                Console.WriteLine("Error Stack Trace:");
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }

        public void SetStatus(string statusText)
        {
            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "UPDATE PendingAccessions SET Status=(@status) WHERE Accession=(@accession)";

                db.Query<PendingCase>(sqlCommand, new { status = statusText, accession = Accession });
            }
        }



    }
}