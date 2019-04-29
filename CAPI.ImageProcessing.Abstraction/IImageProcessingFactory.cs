using CAPI.Common.Abstractions.Config;
using CAPI.General.Abstractions.Services;
using CAPI.NiftiLib;
using log4net;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessingFactory
    {
        INifti CreateNifti();

        IImageConverter CreateImageConverter(IProcessBuilder processBuilder, IImgProcConfig config);
        IImageProcessor CreateImageProcessor(IProcessBuilder processBuilder, IImgProcConfig config);
    }
}