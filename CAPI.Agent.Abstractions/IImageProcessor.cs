using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;

namespace CAPI.Agent.Abstractions
{
    public interface IImageProcessor
    {
        void CompareAndSendToFilesystem(string currentDicomFolder, string priorDicomFolder, string lookupTable, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string resultDicom, string outPriorReslicedDicom);

        void CompareAndSendToDicomNode(string inCurrentDicomFolder, string inPriorDicomFolder, string inLookupTable,
            SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string outResultDicom, string outPriorReslicedDicom, IDicomNode localNode, IDicomNode destination);

        //void CompareAndSendToFilesystem1(string inCurrentDicomFolder, string inPriorDicomFolder, string inLookupTable,
        //    SliceType sliceType,
        //    bool extractBrain, bool register, bool biasFieldCorrect,
        //    string outResultDicom, string outPriorReslicedDicom, string destinationFolder);
    }
}
