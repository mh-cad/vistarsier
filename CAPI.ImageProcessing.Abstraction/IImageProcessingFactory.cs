using CAPI.ImageProcessing.Abstraction.ImageProcessor;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessingFactory
    {
        IBrainMaskExtractor CreateBrainMaskExtractor();
        IResizer CreateResizer();
    }
}