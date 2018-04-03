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
    public class AgentConsoleRepository : IAgentConsoleRepository
    {
        private readonly string _capiConnectionString;

        public AgentConsoleRepository()
        {
            _capiConnectionString = Properties.Settings.Default.CapiConnectionString;
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
                    const string sqlCommand = "SELECT * FROM INFORMATION_SCHEMA.TABLES ";
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

        public IEnumerable<IVerifiedMri> GetPendingCases()
        {
            IEnumerable<IVerifiedMri> pendingCases;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select * FROM [VerifiedMris] WHERE [Status]='Pending'";

                pendingCases = db.Query<VerifiedMri>(sqlCommand);
            }

            return pendingCases;
        }

        public IEnumerable<IVerifiedMri> GetProcessingCases()
        {
            IEnumerable<IVerifiedMri> pendingCases;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select * FROM [VerifiedMris] WHERE [Status]='Processing'";

                pendingCases = db.Query<VerifiedMri>(sqlCommand);
            }

            return pendingCases;
        }

        public IEnumerable<IVerifiedMri> GetQueuedCases()
        {
            IEnumerable<IVerifiedMri> pendingCases;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select * FROM [VerifiedMris] WHERE [Status]='Queued'";

                pendingCases = db.Query<VerifiedMri>(sqlCommand);
            }

            return pendingCases;
        }

        public IEnumerable<IVerifiedMri> GetAllManualCases()
        {
            IEnumerable<IVerifiedMri> allManualCases;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select * FROM [VerifiedMris] WHERE [AdditionMethod]='Manual'";

                allManualCases = db.Query<VerifiedMri>(sqlCommand);
            }

            return allManualCases;
        }

        public IEnumerable<IVerifiedMri> GetAllHl7Cases()
        {
            IEnumerable<IVerifiedMri> allHl7Cases;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select * FROM [VerifiedMris] WHERE [AdditionMethod]='HL7'";

                allHl7Cases = db.Query<VerifiedMri>(sqlCommand);
            }

            return allHl7Cases;
        }

        public IVerifiedMri GetVerifiedMriByAccession(string accession)
        {
            IVerifiedMri verifiedMri;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select * FROM [VerifiedMris] WHERE [Accession]=@accession";

                verifiedMri = db.QuerySingle<VerifiedMri>(sqlCommand, new { accession });
            }

            return verifiedMri;
        }

        public IVerifiedMri GetVerifiedMriById(string id)
        {
            IVerifiedMri verifiedMri;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select * FROM [VerifiedMris] WHERE [Id]=@id";

                verifiedMri = db.QuerySingle<VerifiedMri>(sqlCommand, new { id });
            }

            return verifiedMri;
        }

        public void UpdateVerifiedMri(IVerifiedMri verifiedMri)
        {
            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "UPDATE [VerifiedMris] " +
                    "SET Status=@status, AdditionMethod=@method, LastModified=@time, Note=@note " +
                    "WHERE [Accession]=@accession";

                db.Execute(sqlCommand,
                    new
                    {
                        accession = verifiedMri.Accession,
                        status = verifiedMri.Status,
                        method = verifiedMri.AdditionMethod,
                        time = DateTime.Now,
                        note = verifiedMri.Note
                    });
            }
        }

        public void SetVerifiedMriStatus(string accession, string statusText)
        {
            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "UPDATE [VerifiedMris] SET Status=(@status) WHERE Accession=(@accession)";

                db.Query<PendingCase>(sqlCommand, new { status = statusText, accession });
            }
        }

        public void InsertVerifiedMriIntoDb(IVerifiedMri verifiedMri)
        {
            if (AccessionExistsInDb(verifiedMri.Accession))
                throw new Exception($"Unable to insert accession [{verifiedMri.Accession}] into DB as it already exists there.");

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "INSERT INTO [VerifiedMris] (Accession, Status, AdditionMethod, AdditionTime) " +
                    "VALUES (@accession, @status, @method, @time)";
                db.Execute(sqlCommand,
                    new
                    {
                        accession = verifiedMri.Accession,
                        status = verifiedMri.Status,
                        method = verifiedMri.AdditionMethod,
                        time = verifiedMri.AdditionTime
                    });
            }
        }

        public void DeleteInDbByAccession(string accession)
        {
            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "DELETE FROM [VerifiedMris] WHERE [Accession]=@accession";
                db.Execute(sqlCommand, new { accession });
            }
        }

        public void DeleteInDbById(string id)
        {
            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "DELETE FROM [VerifiedMris] WHERE [Id]=@id";
                db.Execute(sqlCommand, new { id });
            }
        }

        public bool AccessionExistsInDb(string accession)
        {
            IEnumerable<IVerifiedMri> verifiedMris;

            using (IDbConnection db = new SqlConnection(_capiConnectionString))
            {
                const string sqlCommand =
                    "Select * FROM [VerifiedMris] WHERE [Accession]=@accession";

                verifiedMris = db.Query<VerifiedMri>(sqlCommand, new { accession });
            }

            return verifiedMris.Any();
        }
    }
}
