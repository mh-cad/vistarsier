using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CAPI.Agent_Console
{
    public class PendingAccessions
    {
        private readonly string _vtConnectionString;
        private readonly string _capiConnectionString;

        public PendingAccessions()
        {
            _vtConnectionString = Properties.Settings.Default.VtConnectionString;
            _capiConnectionString = Properties.Settings.Default.CapiConnectionString;
        }

        public string Id { get; set; }
        public string Accession { get; set; }
        public string Status { get; set; }

        public IEnumerable<PendingAccessions> GetVtCases(int count)
        {
            return GetCases(count, _vtConnectionString);
        }

        public IEnumerable<PendingAccessions> GetCapiCases(int count)
        {
            return GetCases(count, _capiConnectionString);
        }
        private static IEnumerable<PendingAccessions> GetCases(int count, string connectionString)
        {
            IEnumerable<PendingAccessions> latestVtCases;

            using (IDbConnection db = new SqlConnection(connectionString))
            {
                const string sqlCommand =
                    "Select TOP {=count} * FROM PendingAccessions ORDER BY Id DESC";

                latestVtCases = db.Query<PendingAccessions>(sqlCommand, new { count });
            }

            return latestVtCases;
        }
        public IEnumerable<PendingAccessions> GetPendingCapiCases()
        {
            IEnumerable<PendingAccessions> capiPendingCases;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select TOP (@count) * FROM PendingAccessions WHERE Status=(@status)";

                capiPendingCases = db.Query<PendingAccessions>(sqlCommand, new { count = 1000, status = "Pending" });
            }

            return capiPendingCases;
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
                Console.WriteLine($"Exception Message: {e.Message}");
                Console.WriteLine($"Exception Source: {e.Source}");
                Console.WriteLine("Exception Stack Trace:");
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

                db.Query<PendingAccessions>(sqlCommand, new { status = statusText, accession = Accession });
            }
        }
    }
}