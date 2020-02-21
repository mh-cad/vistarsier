using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VisTarsier.Common;
using VisTarsier.Config;

namespace VisTarsier.Service
{
    public class ManualCaseAgent : IAgent
    {
        public void Run()
        {
            lock(this)
            {
                var log = Log.GetLogger();
                var cfg = CapiConfig.GetConfig();
                var attempts = new List<Attempt>();

                // Read newly added manual files.
                foreach (var file in Directory.GetFiles(cfg.ManualProcessPath))
                {
                    // If the file includes a custom recipe, we'll give reading it a try.
                    string recipe = null;
                    var filename = file; // we're going to do this because the original is const.
                    log.Info($"Found file: {filename}");
                    if (filename.ToLower().EndsWith(".json"))
                    {
                        try
                        {
                            log.Info($"{filename} is a json file. Attempting to extract recipe.");
                            recipe = File.ReadAllText(filename);
                            // Check that we can at the very lease deserialize the recipe without error.
                            _ = JsonConvert.DeserializeObject<Recipe>(recipe);
                        }
                        catch
                        {
                            Log.GetLogger().Error($"Could not read json recipe for {filename}");
                            recipe = null;
                        }

                        filename = filename.ToLower().Replace(".json", "");
                    }

                    // Add an attempt.
                    var accession = Path.GetFileNameWithoutExtension(filename).ToUpper();

                    attempts.Add(
                        new Attempt
                        {
                            CurrentAccession = accession,
                            Method = Attempt.AdditionMethod.Manually,
                            Status = "Pending",
                            CustomRecipe = recipe
                        });
                }

                // Connect to the database.
                var dbBroker = cfg.AgentDbConnectionString == null ? new DbBroker() : new DbBroker(cfg.AgentDbConnectionString);

                // Add attempts to database
                foreach (var attempt in attempts)
                {
                    // Check if the attempt has been added to the database already.
                    var matchingAttempts =
                        dbBroker.Attempts.AsEnumerable().Where(
                            (a) => a.CurrentAccession == attempt.CurrentAccession
                                    && a.Method == Attempt.AdditionMethod.Manually
                                    && "Pending" == a.Status
                                    && ((a.CustomRecipe == null && attempt.CustomRecipe == null)
                                            || a.CustomRecipe.Equals(attempt.CustomRecipe))) ;
                    // If not add it.
                    if (matchingAttempts.Count() == 0)
                    {
                        log.Info($"Adding attempt to database: {attempt.CurrentAccession}");
                        dbBroker.Attempts.Add(attempt);
                    }
                    else
                    {
                        log.Info($"Attempt [{attempt.CurrentAccession}] exists and is pending.");
                    }
                }
                dbBroker.SaveChanges();

                // Finally clean up the incomming cases that we've already added to the DB
                foreach (var file in Directory.GetFiles(cfg.ManualProcessPath))
                {
                    var accession = Path.GetFileNameWithoutExtension(file.ToLower().Replace(".json", ""));
                    if (attempts.AsEnumerable().Where(a => a.CurrentAccession.ToUpper().Equals(accession.ToUpper())).Count() > 0)
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
