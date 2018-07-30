using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessingFactory
    {
        INifti CreateNifti();
        IImageConverter CreateImageConverter(IFileSystem fileSystem, IProcessBuilder processBuilder);
        IImageProcessorNew CreateImageProcessor(IFileSystem filesystem, IProcessBuilder processBuilder, IImgProcConfig config);
    }
}