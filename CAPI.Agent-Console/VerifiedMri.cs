using CAPI.Agent_Console.Abstractions;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace CAPI.Agent_Console
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class VerifiedMri : IVerifiedMri
    {
        private readonly string _capiConnectionString;
        public string Id { get; set; }
        public string Accession { get; set; }
        public string Status { get; set; }
        public string AdditionMethod { get; set; }
        public DateTime AdditionTime { get; set; }

        public VerifiedMri()
        {
            _capiConnectionString = Properties.Settings.Default.CapiConnectionString;
        }

        public IEnumerable<IVerifiedMri> GetRecentVerifiedMris(int numbersToCheck)
        {
            IEnumerable<IVerifiedMri> recentCapiCases;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select TOP {=numbersToCheck} * FROM [VerifiedMris] ORDER BY Id DESC";

                recentCapiCases = db.Query<VerifiedMri>(sqlCommand, new { numbersToCheck });
            }

            return recentCapiCases;
        }

        public void InsertIntoDb()
        {
            if (AccessionExistsInDb())
                throw new Exception($"Unable to insert accession [{Accession}] into DB as it already exists there.");

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "INSERT INTO [VerifiedMris] (Accession, Status, AdditionMethod, AdditionTime) " +
                    "VALUES (@accession, @status, @method, @time)";
                db.Query(sqlCommand,
                    new { accession = Accession, status = Status, method = AdditionMethod, time = AdditionTime });
            }
        }

        public void DeleteInDb()
        {
            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "DELETE FROM [VerifiedMris] WHERE [Accession]=@accession";
                db.Query(sqlCommand, new { accession = Accession });
            }
        }

        public bool AccessionExistsInDb()
        {
            IEnumerable<IVerifiedMri> verifiedMris;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select * FROM [VerifiedMris] WHERE [Accession]=@accession";

                verifiedMris = db.Query<VerifiedMri>(sqlCommand, new { accession = Accession });
            }

            return verifiedMris.Any();
        }

        public bool DbIsAvailable()
        {
            try
            {
                using (IDbConnection db = new SqlConnection(_capiConnectionString))
                {
                    db.Open();
                    db.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DbTableVerifiedMriExists()
        {
            try
            {
                using (IDbConnection db = new SqlConnection(_capiConnectionString))
                {
                    const string sqlCommand = "SELECT * FROM INFORMATION_SCHEMA.TABLES ";// +
                                                                                         //"WHERE TABLE_NAME = N'VerifiedMris1'";
                    var tables = db.Query(sqlCommand);
                    if (tables.All(t => t.TABLE_NAME != "VerifiedMris")) return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}