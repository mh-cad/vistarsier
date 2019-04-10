namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessor
    {
        void ExtractBrainRegisterAndCompare(
            string currentNii, string priorNii, string referenceNii, string[] lookupTablePaths, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string[] resultNiis, string outPriorReslicedNii);

        void CompareDicomInNiftiOut(
            string currentDicomFolder, string priorDicomFolder, string referenceSeriesDicomFolder,
            string[] lookupTablePaths, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string[] resultNiis, string outPriorReslicedNii);
    }
}