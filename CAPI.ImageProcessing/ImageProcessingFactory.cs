using CAPI.ImageProcessing.Abstraction;

namespace CAPI.ImageProcessing
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImageProcessingFactory : IImageProcessingFactory
    {
        public INifti CreateNifti()
        {
            return new Nifti();
        }
    }
}