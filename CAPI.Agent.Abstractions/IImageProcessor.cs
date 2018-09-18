using CAPI.ImageProcessing.Abstraction;

namespace CAPI.Agent.Abstractions
{
    public interface IImageProcessor
    {
        string[] CompareAndSaveLocally(
            string currentDicomFolder, string priorDicomFolder,
            string[] lookupTablePaths, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string outPriorReslicedDicom,
            string resultsDicomSeriesDescription, string priorReslicedDicomSeriesDescription);

        void AddOverlayToImage(string bmpFilePath, string overlayText);

        //void CompareAndSendToDicomNode(string inCurrentDicomFolder, string inPriorDicomFolder, string inLookupTable,
        //    SliceType sliceType,
        //    bool extractBrain, bool register, bool biasFieldCorrect,
        //    string outResultDicom, string outPriorReslicedDicom, IDicomNode localNode, IDicomNode destination);

        //void CompareAndSendToFilesystem1(string inCurrentDicomFolder, string inPriorDicomFolder, string inLookupTable,
        //    SliceType sliceType,
        //    bool extractBrain, bool register, bool biasFieldCorrect,
        //    string outResultDicom, string outPriorReslicedDicom, string destinationFolder);
    }
}
