using CAPI.Common.Abstractions.Services;
using CAPI.Common.Config;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using log4net;

namespace CAPI.Agent.Abstractions
{
    public interface IAgentFactory
    {
        IAgentRepository CreateAgentRepository();

        IAgent CreateAgent(CapiConfig config, IDicomFactory dicomFactory,
            IImageProcessingFactory imgProcFactory,
            IFileSystem fileSystem, IProcessBuilder processBuilder, ILog log);
    }
}
