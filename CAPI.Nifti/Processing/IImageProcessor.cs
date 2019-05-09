namespace CAPI.NiftiLib.Processing
{
    public interface IImageProcessor
    {
        Metrics MSLesionCompare(
            string currentNii, string priorNii, string referenceNii,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string[] resultNiis, string outPriorReslicedNii);
    }
}