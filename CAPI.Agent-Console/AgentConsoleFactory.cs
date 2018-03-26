using CAPI.Agent_Console.Abstractions;

namespace CAPI.Agent_Console
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AgentConsoleFactory : IAgentConsoleFactory
    {
        public IVerifiedMri CreateVerifiedMri()
        {
            return new VerifiedMri();
        }

        public IAgentConsoleRepository CreateAgentConsoleRepository()
        {
            return new AgentConsoleRepository();
        }
    }
}