using CAPI.Config;
using CAPI.NiftiLib;
using CAPI.ImageProcessing.Abstraction;
using log4net;

namespace CAPI.ImageProcessing
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImageProcessingFactory : IImageProcessingFactory
    {
        public INifti CreateNifti()
        {
            return new Nifti();
        }

        public IImageConverter CreateImageConverter(IImgProcConfig config)
        {
            return new ImageConverter(config);
        }

        public IImageProcessor CreateImageProcessor(IImgProcConfig config)
        {
            return new ImageProcessor(config);
        }
    }
}