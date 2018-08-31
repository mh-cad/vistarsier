using CAPI.Agent.Abstractions;
using CAPI.Common.Abstractions.Services;
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

        public IAgent CreateAgent(string[] args, IDicomFactory dicomFactory,
                                  IImageProcessingFactory imgProcFactory,
                                  IFileSystem fileSystem, IProcessBuilder processBuilder,
                                  ILog log)
        {
            return new Agent(args, dicomFactory, imgProcFactory, fileSystem, processBuilder, log);
        }
    }
}
