using CAPI.Agent.Abstractions;
using CAPI.Common.Config;

namespace CAPI.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Agent : IAgent
    {
        public int Interval { get; set; }
        public string DatabaseConnectionString { get; set; }
        public int NotifyCooloffPeriod { get; set; }

        public CapiConfig Config { get; set; }

        public Agent()
        {

        }

        public void Run()
        {

        }

        
    }
}
