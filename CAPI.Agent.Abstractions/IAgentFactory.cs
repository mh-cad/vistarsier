using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
using CAPI.Dicom.Abstraction;
using log4net;

namespace CAPI.Agent.Abstractions
{
    public interface IAgentFactory
    {
        IAgentRepository CreateAgentRepository(string dbConnectionString);

        IAgent CreateAgent(ICapiConfig config, IDicomFactory dicomFactory,
            IFileSystem fileSystem, IProcessBuilder processBuilder, ILog log);
    }
}
