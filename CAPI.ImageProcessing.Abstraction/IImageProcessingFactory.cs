using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
using log4net;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessingFactory
    {
        INifti CreateNifti();
        IImageConverter CreateImageConverter(IFileSystem fileSystem, IProcessBuilder processBuilder, IImgProcConfig config);
        IImageProcessor CreateImageProcessor(IFileSystem filesystem, IProcessBuilder processBuilder, IImgProcConfig config, ILog log);
    }
}