using CAPI.Agent.Abstractions;
using CAPI.Common.Abstractions.Services;
using CAPI.Common.Config;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using log4net;

namespace CAPI.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AgentFactory : IAgentFactory
    {
        public IAgentRepository CreateAgentRepository()
        {
            return new AgentRepository();
        }

        public IAgent CreateAgent(CapiConfig config, IDicomFactory dicomFactory,
                                  IImageProcessingFactory imgProcFactory,
                                  IFileSystem fileSystem, IProcessBuilder processBuilder,
                                  ILog log)
        {
            return new Agent(config, dicomFactory, imgProcFactory, fileSystem, processBuilder, log);
        }
    }
}
