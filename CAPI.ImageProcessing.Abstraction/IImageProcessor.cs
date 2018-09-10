namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessor
    {
        void ExtractBrainMask(string inNii, string bseParams, string outBrainNii, string outMaskNii);
        void Registration(string currentNii, string priorNii, string outPriorReslicedNii);
        void BiasFieldCorrection(string inNii, string mask, string bfcParams, string outNii);
        void Normalize(string niftiFilePath, string maskFilePath, SliceType sliceType, string lookupTable);
        void Normalize(string niftiFilePath, string maskFilePath, SliceType sliceType, int mean, int std, int widthRange);
        void Compare(
            string currentNii, string priorNii, string lookupTable, SliceType sliceType, string resultNiiFile);
        void CompareBrainNiftiWithReslicedBrainNifti_OutNifti(
            string currentNii, string priorNii, string lookupTable, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string resultNii, string outPriorReslicedNii);

        void CompareDicomInNiftiOut(
            string currentDicomFolder, string priorDicomFolder, string lookupTable, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string resultNii, string outPriorReslicedNii);
    }
}