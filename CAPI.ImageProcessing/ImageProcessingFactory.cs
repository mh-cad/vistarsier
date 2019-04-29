using CAPI.Common.Abstractions.Config;
using CAPI.General.Abstractions.Services;
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

        public IImageConverter CreateImageConverter(
            IProcessBuilder processBuilder, IImgProcConfig config)
        {
            return new ImageConverter(processBuilder, config);
        }

        public IImageProcessor CreateImageProcessor(IProcessBuilder processBuilder,
                                                    IImgProcConfig config)
        {
            return new ImageProcessor(processBuilder, config);
        }
    }
}