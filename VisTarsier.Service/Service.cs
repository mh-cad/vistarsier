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

            if (!File.Exists(conf.Binaries.antsRegistration)
                || !File.Exists(conf.Binaries.antsApplyTransforms)
                || !File.Exists(conf.Binaries.N4BiasFieldCorrection)
                || !File.Exists(conf.Binaries.img2dcm)
                || !File.Exists(conf.Binaries.dcm2niix)
                || !File.Exists(conf.Binaries.bse))
            {
                var badFiles = new List<string>();
                if (!File.Exists(conf.Binaries.antsRegistration)) badFiles.Add(conf.Binaries.antsRegistration);
                if (!File.Exists(conf.Binaries.antsApplyTransforms)) badFiles.Add(conf.Binaries.antsApplyTransforms);
                if (!File.Exists(conf.Binaries.N4BiasFieldCorrection)) badFiles.Add(conf.Binaries.N4BiasFieldCorrection);
                if (!File.Exists(conf.Binaries.img2dcm)) badFiles.Add(conf.Binaries.img2dcm);
                if (!File.Exists(conf.Binaries.dcm2niix)) badFiles.Add(conf.Binaries.dcm2niix);
                if (!File.Exists(conf.Binaries.dcm2niix)) badFiles.Add(conf.Binaries.bse);
                
                throw new FileNotFoundException(String.Join("Could not find one or more essential binaries as referenced in config.json. ", badFiles.ToArray()));
            }
        }

        private void CleanupDatabase()
        {
            var log = Log.GetLogger();
            var cfg = CapiConfig.GetConfig();
            if (cfg == null) throw new ApplicationException("Unable to find config file.");
            var dbBroker = new DbBroker(cfg.AgentDbConnectionString);
            
            if(dbBroker.Database.EnsureCreated())
            {
                log.Info("Database not found, so it has been created.");
            }
            else
            {
                log.Info("Database found.");
            }

            if(!dbBroker.Database.CanConnect())
            {
                log.Error("Could not connect to database using string: " + cfg.AgentDbConnectionString);
            }
            else
            {
                log.Info("DB Connection established");
            }

            try
            {
                log.Info("DB Connection good? " + dbBroker.Database.CanConnect());
                var failedCases = dbBroker.GetCaseByStatus("Processing");

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
            catch (Exception e)
            {
                log.Error("Error while accesing database. If this is due to an incompatible schema, maybe try wiping the database and starting again?");
                log.Error(e);
            }
            
        }

        protected override void OnStop()
        {
        }
    }
}
 