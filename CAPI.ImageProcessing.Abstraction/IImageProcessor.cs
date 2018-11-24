namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessor
    {
        void ExtractBrainMask(string inNii, string bseParams, string outBrainNii, string outMaskNii);
        string Registration(string refNii, string priorNii, string outPriorReslicedNii, string seriesType);
        void BiasFieldCorrection(string inNii, string mask, string bfcParams, string outNii);
        void Normalize(string niftiFilePath, string maskFilePath, SliceType sliceType, string lookupTable);
        void Normalize(string niftiFilePath, string maskFilePath, SliceType sliceType, int mean, int std, int widthRange);
        void Compare(
            string currentNii, string priorNii, string lookupTable, SliceType sliceType, string resultNiiFile);

        void ExtractBrainRegisterAndCompare(
            string currentNii, string priorNii, string referenceNii, string[] lookupTablePaths, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string[] resultNiis, string outPriorReslicedNii);

        void CompareDicomInNiftiOut(
            string currentDicomFolder, string priorDicomFolder, string referenceSeriesDicomFolder,
            string[] lookupTablePaths, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string[] resultNiis, string outPriorReslicedNii);

        // TODO1: Remove when done experimenting
        #region Experimental
        void CompareUsingNictaCode(string fixedFile, string floatingFile, string fixedMaskFile,
                                   string nictaPosResultFilePath, string nictaNegResultFilePath,
                                   string colormapConfigFilePath, bool ignoreErrors);
        #endregion
    }
}