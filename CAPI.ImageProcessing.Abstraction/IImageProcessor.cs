using CAPI.NiftiLib;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessor
    {
        void MSLesionCompare(
            string currentNii, string priorNii, string referenceNii,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string[] resultNiis, string outPriorReslicedNii);

        string DicomToNifti(string dicomFolder);
    }
}