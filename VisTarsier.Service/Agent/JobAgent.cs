using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VisTarsier.Common;
using VisTarsier.Config;

namespace VisTarsier.Service
{
    public class JobAgent : IAgent
    {
        public void Run()
        {
            // Lock the agent because we don't want more than one to process the pending cases.
            lock (this)
            {
                // Setup config and db connection
                var log = Log.GetLogger();
                var cfg = CapiConfig.GetConfig();
                var dbBroker = cfg.AgentDbConnectionString == null ? new DbBroker() : new DbBroker(cfg.AgentDbConnectionString);
                
                // Get the cases which are pending in the database.
                var pendingAttempts = dbBroker.GetCaseByStatus("Pending").ToList();

                var jobs = new List<Job>();

                // Try to create jobs for each of the pending attempts
                foreach(var attempt in pendingAttempts)
                {
                    // If this was a manually added attempt with a custom recipe
                    if (attempt.CustomRecipe != null)
                    {
                        var recipe = JsonConvert.DeserializeObject<Recipe>(attempt.CustomRecipe);
                        var job = BuildJob(attempt, recipe, dbBroker);
                        if (job != null)
                        {
                            jobs.Add(job);
                            job.RecipeId = StoredRecipe.NO_ID;
                            dbBroker.SaveChanges();
                        }
                    }
                    // Otherwise we're going to try every stored recipe.
                    else
                    {
                        foreach(var storedRecipe in dbBroker.StoredRecipes.ToList())
                        {
                            if (storedRecipe.Id < 0) continue; // This is the custom recipe placeholder.
                            var recipe = JsonConvert.DeserializeObject<Recipe>(storedRecipe.RecipeString);
                            var job = BuildJob(attempt, recipe, dbBroker);
                            if (job != null)
                            {
                                jobs.Add(job);
                                job.RecipeId = storedRecipe.Id;
                                dbBroker.SaveChanges();
                            }
                        }
                    }

                    // Check if we've been able to create a job for the attempt and update the status.
                    if (jobs.AsEnumerable().Where(j => j.AttemptId == attempt.Id).Count() == 0)
                    {
                        @attempt.Status = "Unworkable";
                        @attempt.Comment = "Could not create a job from known recipes";
                    }
                    else
                    {
                        @attempt.Status = "Complete";
                        @attempt.Comment = "Jobs : [";
                        foreach(var j in jobs.AsEnumerable().Where(j => j.AttemptId == attempt.Id).ToList())
                        {
                            @attempt.Comment += $"{ j.Id},";
                        }
                        @attempt.Comment += "]";
                    }
                }

                dbBroker.SaveChanges();

                // Now it's time to process each job.
                foreach (var job in jobs)
                {
                    
                    JobProcessor jp = new JobProcessor(dbBroker);
                    try
                    {
                        @job.Status = "Processing";
                        dbBroker.SaveChanges();
                        jp.Process(job);
                        @job.Status = "Complete";
                        dbBroker.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Log.GetLogger().Info($"{ex.Message} Job [{job.Id}] failed during processing");
                        @job.Status = "Failed";
                    }
                }

                dbBroker.SaveChanges();
                dbBroker.Dispose();
            }
        }

        private Job BuildJob(Attempt attempt, Recipe recipe, DbBroker dbBroker)
        {
            var log = Log.GetLogger();

            try
            {
                var jb = new JobBuilder(new ValueComparer(), dbBroker);
                var job = jb.Build(recipe, attempt);
                return job;
            }
            catch (JobBuilder.StudyNotFoundException ex)
            {
                log.Info($"{ex.Message} Accession: [{attempt.CurrentAccession}]");
                return null;
            }
            catch (DirectoryNotFoundException ex)
            {
                if (ex.Message.ToLower().Contains("workable series were found"))
                {
                    log.Info($"{ex.Message} Accession: [{attempt.CurrentAccession}]");
                }
                else throw;
            }
            catch (Exception ex)
            {
                log.Info($"{ex.Message} Attempt [{attempt.Id}] failed during build.");
                @attempt.Comment = ex.Message;
            }

            return null;
        }
    }
}
