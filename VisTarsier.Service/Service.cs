using VisTarsier.Common;
using VisTarsier.Config;
using System.IO;
using System.ServiceProcess;
using System.Collections.Generic;
using System;

namespace VisTarsier.Service
{
    public partial class Service : ServiceBase
    {
        List<IAgent> _agents;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var log = Log.GetLogger();
            try
            {
                log.Info("Cleaning DB...");
                CleanupDatabase();
                log.Info("Checking config...");
                CheckConfig();
                log.Info("Loading agents...");
                LoadAgents();
                log.Info("Starting main loop...");
                StartLoop();
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message);
                log.Fatal(ex.StackTrace);
            }
        }

        private void StartLoop()
        {
            // Setup a timer to run the agents every interval.
            var interval = int.Parse(CapiConfig.GetConfig().RunInterval);
            var timer = new System.Timers.Timer { Interval = interval * 1000, Enabled = true };
            timer.Elapsed += (s, e) =>
            {
                foreach (var agent in _agents) agent.Run();
            };

            // Run the agents now to start things off.
            foreach (var agent in _agents) agent.Run();
        }

        private void LoadAgents()
        {
            _agents = new List<IAgent>
            {
                new HL7Agent(),
                new ManualCaseAgent(),
                new JobAgent()
            };
        }

        private void CheckConfig()
        {
            var conf = CapiConfig.GetConfig();
            FileSystem.DirectoryExistsIfNotCreate(conf.ManualProcessPath);
            FileSystem.DirectoryExistsIfNotCreate(conf.Hl7ProcessPath);

            var log = Log.GetLogger();
            log.Info("conf.Binaries.antsRegistration: " + conf.Binaries.antsRegistration + " exists? " + File.Exists(conf.Binaries.antsRegistration));
            log.Info("conf.Binaries.antsRegistration: " + conf.Binaries.antsApplyTransforms + " exists? " + File.Exists(conf.Binaries.antsApplyTransforms));
            log.Info("conf.Binaries.antsRegistration: " + conf.Binaries.N4BiasFieldCorrection + " exists? " + File.Exists(conf.Binaries.N4BiasFieldCorrection));
            log.Info("conf.Binaries.antsRegistration: " + conf.Binaries.img2dcm + " exists? " + File.Exists(conf.Binaries.img2dcm));
            log.Info("conf.Binaries.antsRegistration: " + conf.Binaries.dcm2niix + " exists? " + File.Exists(conf.Binaries.dcm2niix));
            log.Info("conf.Binaries.antsRegistration: " + conf.Binaries.bse + " exists? " + File.Exists(conf.Binaries.bse));


            if (!File.Exists(conf.Binaries.antsRegistration)
                || !File.Exists(conf.Binaries.antsApplyTransforms)
                || !File.Exists(conf.Binaries.N4BiasFieldCorrection)
                || !File.Exists(conf.Binaries.img2dcm)
                || !File.Exists(conf.Binaries.dcm2niix)
                || !File.Exists(conf.Binaries.bse))
            {
                
                throw new FileNotFoundException("Could not find one or more essential binaries as referenced in config.json");
            }
        }

        private void CleanupDatabase()
        {
            var cfg = CapiConfig.GetConfig();
            if (cfg == null) throw new ApplicationException("Unable to find config file.");
            var dbBroker = new DbBroker(cfg.AgentDbConnectionString);
            var failedCases = dbBroker.GetCaseByStatus("Processing");
            // Debugging code
            var log = Log.GetLogger();
            log.Info("DB Connection good? " + dbBroker.Database.CanConnect());
            log.Info("------- RECIPES --------");
            foreach (var recipe in dbBroker.StoredRecipes)
            {
                log.Info(recipe.Name + " : " + recipe.RecipeString);
            }
            log.Info("------------------------");


            foreach (var c in failedCases)
            {
                var tmp = c;
                tmp.Status = "Pending";
                dbBroker.Attempts.Update(tmp);
                dbBroker.SaveChanges();
            }
            var failedJobs = dbBroker.GetJobByStatus("Processing");
            foreach (var j in failedJobs)
            {
                var tmp = j;
                tmp.Status = "Failed";
                dbBroker.Jobs.Update(tmp);
                dbBroker.SaveChanges();
            }
        }

        protected override void OnStop()
        {
        }
    }
}
