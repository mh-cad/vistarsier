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
                                  IProcessBuilder processBuilder)
        {
            return new Agent(args, dicomFactory, imgProcFactory, processBuilder);
        }

        public IImageProcessor CreateAgentImageProcessor(
            IDicomServices dicomServices, IImageProcessingFactory imgProcFactory,
            IProcessBuilder processBuilder, IImgProcConfig imfProcConfig, IAgentRepository context)
        {
            return new ImageProcessor(dicomServices, imgProcFactory, processBuilder, imfProcConfig, context as AgentRepository);
        }
    }
}
