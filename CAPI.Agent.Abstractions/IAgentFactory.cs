using CAPI.Common.Abstractions.Services;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using log4net;

namespace CAPI.Agent.Abstractions
{
    public interface IAgentFactory
    {
        IAgentRepository CreateAgentRepository();

        IAgent CreateAgent(string[] args, IDicomFactory dicomFactory,
            IImageProcessingFactory imgProcFactory,
            IFileSystem fileSystem, IProcessBuilder processBuilder, ILog log);
    }
}
