using CAPI.ImageProcessing.Abstraction;
using CAPI.ImageProcessing.Abstraction.ImageProcessor;
using CAPI.ImageProcessing.ImageProcessor;

namespace CAPI.ImageProcessing
{
    public class ImageProcessingFactory : IImageProcessingFactory
    {
        public ImageProcessingFactory()
        {
        }

        public IBrainMaskExtractor CreateBrainMaskExtractor()
        {
            return new BrainMaskExtractor();
        }

        public IResizer CreateResizer()
        {
            return new Resizer();
        }
    }
}
