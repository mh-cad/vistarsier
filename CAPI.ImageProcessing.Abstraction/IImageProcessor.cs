namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessor
    {
        void ExtractBrainMask(string inNii, string bseParams, string outBrainNii, string outMaskNii);
        void Registration(string currentNii, string priorNii, string outPriorReslicedNii);
        void BiasFieldCorrection(string inNii, string bfcParams, string outNii);
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