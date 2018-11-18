using CAPI.Agent.Abstractions;
using CAPI.Common.Abstractions.Config;
using CAPI.Dicom.Abstractions;
using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using IImageProcessor = CAPI.Agent.Abstractions.IImageProcessor;

namespace CAPI.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AgentFactory : IAgentFactory
    {
        public IAgent CreateAgent(string[] args, IDicomFactory dicomFactory,
                                  IImageProcessingFactory imgProcFactory,
                                  IFileSystem fileSystem, IProcessBuilder processBuilder,
                                  ILog log)
        {
            return new Agent(args, dicomFactory, imgProcFactory, fileSystem, processBuilder, log);
        }

        public IImageProcessor CreateAgentImageProcessor(
            IDicomServices dicomServices, IImageProcessingFactory imgProcFactory,
            IFileSystem fileSystem, IProcessBuilder processBuilder, IImgProcConfig imfProcConfig, ILog log, IAgentRepository context)
        {
            return new ImageProcessor(dicomServices, imgProcFactory, fileSystem, processBuilder, imfProcConfig, log, context as AgentRepository);
        }
    }
}
