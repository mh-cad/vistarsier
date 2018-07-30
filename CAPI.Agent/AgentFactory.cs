using CAPI.Agent.Abstractions;
using CAPI.Common.Abstractions.Config;

namespace CAPI.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AgentFactory : IAgentFactory
    {
        public IAgentRepository CreateAgentRepository(string dbConnectionString)
        {
            return new AgentRepository(dbConnectionString);
        }

        public IAgent CreateAgent(ICapiConfig config)
        {
            return new Agent(config);
        }
    }
}
