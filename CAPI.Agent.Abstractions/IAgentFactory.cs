using CAPI.Common.Abstractions.Config;
using CAPI.Dicom.Abstractions;
using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using log4net;

namespace CAPI.Agent.Abstractions
{
    public interface IAgentFactory
    {
        IAgent CreateAgent(string[] args, IDicomFactory dicomFactory,
            IImageProcessingFactory imgProcFactory,
            IFileSystem fileSystem, IProcessBuilder processBuilder, ILog log);

        IImageProcessor CreateAgentImageProcessor(
            IDicomServices dicomServices, IImageProcessingFactory imgProcFactory,
            IFileSystem fileSystem, IProcessBuilder processBuilder, IImgProcConfig imfProcConfig, ILog log);
    }
}
