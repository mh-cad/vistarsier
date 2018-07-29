using CAPI.Agent.Abstractions;

namespace CAPI.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Agent : IAgent
    {
        public int Interval { get; set; }
        public string DatabaseConnectionString { get; set; }
        public int NotifyCooloffPeriod { get; set; }

        public Agent()
        {

        }

        public void Run()
        {

        }
    }
}
