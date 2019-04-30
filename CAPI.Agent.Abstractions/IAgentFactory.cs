using CAPI.Config;
using CAPI.Dicom.Abstractions;
using CAPI.ImageProcessing.Abstraction;

namespace CAPI.Agent.Abstractions
{
    public interface IAgentFactory
    {
        /// <summary>
        /// Creates a new IAgent.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="dicomFactory"></param>
        /// <param name="imgProcFactory"></param>
        /// <param name="fileSystem"></param>
        /// <param name="processBuilder"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        IAgent CreateAgent(string[] args, IDicomFactory dicomFactory,
            IImageProcessingFactory imgProcFactory);

        /// <summary>
        /// Creates a new Agent.Abstractions.IImageProcessor
        /// </summary>
        /// <param name="dicomServices"></param>
        /// <param name="imgProcFactory"></param>
        /// <param name="fileSystem"></param>
        /// <param name="processBuilder"></param>
        /// <param name="imfProcConfig"></param>
        /// <param name="log"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        IImageProcessor CreateAgentImageProcessor(
            IDicomServices dicomServices, IImageProcessingFactory imgProcFactory,IImgProcConfig imfProcConfig, IAgentRepository context);
    }
}
