using CAPI.Common.Abstractions.Config;

namespace CAPI.Agent.Abstractions
{
    public interface IAgentFactory
    {
        IAgentRepository CreateAgentRepository(string dbConnectionString);
        IAgent CreateAgent(ICapiConfig config);
    }
}
