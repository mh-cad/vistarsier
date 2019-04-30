using CAPI.Config;
using CAPI.NiftiLib;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessingFactory
    {
        INifti CreateNifti();

        IImageConverter CreateImageConverter(IImgProcConfig config);
        IImageProcessor CreateImageProcessor(IImgProcConfig config);
    }
}