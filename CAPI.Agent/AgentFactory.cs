using CAPI.Agent.Abstractions;
using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
using CAPI.Dicom.Abstraction;
using log4net;

namespace CAPI.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AgentFactory : IAgentFactory
    {
        public IAgentRepository CreateAgentRepository(string dbConnectionString)
        {
            return new AgentRepository(dbConnectionString);
        }

        public IAgent CreateAgent(ICapiConfig config, IDicomFactory dicomFactory,
                                  IFileSystem fileSystem, IProcessBuilder processBuilder, ILog log)
        {
            return new Agent(config, dicomFactory, fileSystem, processBuilder, log);
        }
    }
}
