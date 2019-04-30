using CAPI.Agent.Abstractions;
using CAPI.Config;
using CAPI.Dicom.Abstractions;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using IImageProcessor = CAPI.Agent.Abstractions.IImageProcessor;

namespace CAPI.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AgentFactory : IAgentFactory
    {
        public IAgent CreateAgent(string[] args, IDicomFactory dicomFactory,
                                  IImageProcessingFactory imgProcFactory)
        {
            return new Agent(args, dicomFactory, imgProcFactory);
        }

        public IImageProcessor CreateAgentImageProcessor(
            IDicomServices dicomServices, IImageProcessingFactory imgProcFactory,
            IImgProcConfig imfProcConfig, IAgentRepository context)
        {
            return new ImageProcessor(dicomServices, imgProcFactory, imfProcConfig, context as AgentRepository);
        }
    }
}
