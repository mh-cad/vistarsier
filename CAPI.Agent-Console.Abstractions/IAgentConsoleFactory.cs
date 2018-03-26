namespace CAPI.Agent_Console.Abstractions
{
    public interface IAgentConsoleFactory
    {
        IVerifiedMri CreateVerifiedMri();
        IAgentConsoleRepository CreateAgentConsoleRepository();
    }
}