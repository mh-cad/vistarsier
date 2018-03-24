using CAPI.Agent_Console.Abstractions;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace CAPI.Agent_Console
{
    public class PendingCase : IPendingCase
    {
        // Fields
        private readonly string _vtConnectionString;
        private readonly string _capiConnectionString;

        // Constructor
        public PendingCase()
        {
            _vtConnectionString = Properties.Settings.Default.VtConnectionString;
            _capiConnectionString = Properties.Settings.Default.CapiConnectionString;
        }

        // Properties
        public string Id { get; set; }
        public string Accession { get; set; }
        public string Status { get; set; }
        public string AdditionMethod { get; set; }
        public Exception Exception { get; set; }

        // Public Methods
        public IEnumerable<IPendingCase> GetVtCases(int count)
        {
            return GetCases(count, _vtConnectionString);
        }
        public IEnumerable<IPendingCase> GetCapiCases(int count)
        {
            return GetCases(count, _capiConnectionString);
        }

        public IEnumerable<IPendingCase> GetRecentPendingCapiCases(bool manual, int numOfcasesToCheckInDb = 1000)
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
        public IEnumerable<IPendingCase> GetProcessingCapiCases(int numOfcasesToCheckInDb = 1000)
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
        public IEnumerable<IPendingCase> GetQueuedCapiCases(int numOfcasesToCheckInDb = 1000)
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

        public void AddToCapiDb(bool manual = false)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(_capiConnectionString))
                {
                    const string sqlCommand =
                        "INSERT INTO PendingAccessions (Accession, Status, AdditionMethod) VALUES (@accession, @status, @method)";
                    db.Query(sqlCommand,
                        new { accession = Accession, status = "Pending", method = manual ? "Manual" : "HL7" });
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

        public void UpdateAdditionMethodToManual(bool manual)
        {
            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "UPDATE PendingAccessions SET AdditionMethod=(@method) WHERE Accession=(@accession)";

                db.Query<PendingCase>(sqlCommand, new { method = manual ? "Manual" : "Hl7", accession = Accession });
            }
        }

        public IQueryable<IPendingCase> GetAllCapiCases()
        {
            IEnumerable<PendingCase> allCapiCases;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select * FROM PendingAccessions";

                allCapiCases = db.Query<PendingCase>(sqlCommand);
            }

            return allCapiCases.AsQueryable();
        }

        // Private Methods
        private static IEnumerable<IPendingCase> GetCases(int count, string connectionString)
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
    }
}