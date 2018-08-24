using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
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

        public IImageConverter CreateImageConverter(
            IFileSystem fileSystem, IProcessBuilder processBuilder, IImgProcConfig config)
        {
            return new ImageConverter(fileSystem, processBuilder, config);
        }

        public IImageProcessorNew CreateImageProcessor(IFileSystem filesystem, IProcessBuilder processBuilder, IImgProcConfig config)
        {
            return new ImageProcessorNew(filesystem, processBuilder, config);
        }
    }
}