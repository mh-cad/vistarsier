using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
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
            IFileSystem fileSystem, IProcessBuilder processBuilder, IImgProcConfig config)
        {
            return new ImageConverter(fileSystem, processBuilder, config);
        }

        public IImageProcessor CreateImageProcessor(IFileSystem filesystem, IProcessBuilder processBuilder,
                                                    IImgProcConfig config, ILog log)
        {
            return new ImageProcessor(filesystem, processBuilder, config, log);
        }
    }
}