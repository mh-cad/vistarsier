using System.IO;
using System.Linq;
using VisTarsier.Common;
using VisTarsier.Config;

namespace VisTarsier.Service
{
    public class HL7Agent : IAgent
    {
        public void Run()
        {
            // Because this will be on a timer, we don't want to add multiple attempts.
            lock (this)
            {
                var log = Log.GetLogger();
                var cfg = CapiConfig.GetConfig();

                // For each file in the HL7 incomming folder, create an attempt.
                var attempts =
                    (from file in Directory.GetFiles(cfg.Hl7ProcessPath)
                     let accession = Path.GetFileNameWithoutExtension(file).ToUpper()
                     select new Attempt { CurrentAccession = accession, Method = Attempt.AdditionMethod.Hl7, Status = "Pending" }).ToList();

                // Connect to the database.
                var dbBroker = cfg.AgentDbConnectionString == null ? new DbBroker() : new DbBroker(cfg.AgentDbConnectionString);

                // Update the database
                foreach (var attempt in attempts)
                {
                    // Check if the attempt has been added to the database already.
                    var matchingAttempts =
                        dbBroker.Attempts.AsQueryable().Where(
                            (a) => a.CurrentAccession == attempt.CurrentAccession && a.Method == Attempt.AdditionMethod.Hl7);
                    // If not add it.
                    if (matchingAttempts.Count() == 0)
                    {
                        log.Info($"Adding attempt for {attempt.CurrentAccession}");
                        dbBroker.Attempts.Add(attempt);
                    }
                }
                dbBroker.SaveChanges();

                // Finally clean up the incomming cases that we've already added to the DB
                foreach (var file in Directory.GetFiles(cfg.Hl7ProcessPath))
                {
                    var accession = Path.GetFileNameWithoutExtension(file);
                    if (attempts.Where(a => a.CurrentAccession.ToUpper().Equals(accession.ToUpper())).Count() > 0)
                    {
                        log.Info($"Cleaning {file}");
                        File.Delete(file);
                    }
                }
                dbBroker.Dispose();
            }
        }
    }
}
