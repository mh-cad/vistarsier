using CAPI.Config;
using CAPI.NiftiLib;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessingFactory
    {
        INifti CreateNifti();

        IImageProcessor CreateImageProcessor(IImgProcConfig config);
    }
}