using CAPI.Agent.Abstractions;
using CAPI.Common.Abstractions.Config;
using System;
using System.Linq;
using System.Timers;

namespace CAPI.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Agent : IAgent
    {
        private readonly AgentRepository _context;

        public ICapiConfig Config { get; set; }
        public bool IsBusy { get; set; }

        public Agent(ICapiConfig config)
        {
            Config = config;
            _context = new AgentRepository(config.AgentDbConnectionString);
        }

        public void Run()
        {
            Init();

            StartTimer(int.Parse(Config.RunInterval));
        }

        private void Init()
        {
            SetFailedCasesStatusToPending();
        }
        private void SetFailedCasesStatusToPending()
        {
            var failedCases = _context.GetCaseByStatus("Processing");
            failedCases.ToList().ForEach(c => { c.UpdateStatus("Pending"); });
        }

        private void StartTimer(int interval)
        {
            var timer = new Timer { Interval = interval * 1000, Enabled = true };
            timer.Elapsed += OnTimeEvent;
            timer.Start();
        }
        private void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            if (IsBusy) return;

            try
            {
                var newAccessions = GetNewAccessions();
                UpdateDb(newAccessions);
                var pendingCases = _context.GetCaseByStatus("Pending");
                pendingCases.ToList().ForEach(c => c.Process());
            }
            catch
            {
                // TODO1: Log to be implemented
                IsBusy = false;
                throw;
            }
        }
        private static string[] GetNewAccessions()
        {
            throw new NotImplementedException();
        }
        private static void UpdateDb(string[] accession)
        {
            throw new NotImplementedException();
        }
    }
}
