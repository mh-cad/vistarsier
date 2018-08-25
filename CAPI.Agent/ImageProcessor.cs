using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using System.IO;
using System.Linq;

namespace CAPI.Agent
{
    /// <summary>
    /// Compares current and prior sereis and saves results into filesystem or sends off to a dicom node
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImageProcessor : Abstractions.IImageProcessor
    {
        private readonly IDicomServices _dicomServices;
        private readonly IImageProcessor _imgProc;
        private readonly IImageProcessingFactory _imgProcFactory;
        private readonly ILog _log;

        public ImageProcessor(IDicomServices dicomServices, IImageProcessingFactory imgProcFactory,
                              IFileSystem filesystem, IProcessBuilder processBuilder,
                              IImgProcConfig imgProcConfig, ILog log)
        {
            _dicomServices = dicomServices;
            _imgProcFactory = imgProcFactory;
            _log = log;
            _imgProc = imgProcFactory.CreateImageProcessor(filesystem, processBuilder, imgProcConfig, log);
        }

        public void CompareAndSendToFilesystem(
            string currentDicomFolder, string priorDicomFolder,
            string lookupTable, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string resultDicom, string outPriorReslicedDicom)
        {
            var resultNiiFile = resultDicom + ".nii";
            var outPriorReslicedNiiFile = outPriorReslicedDicom + ".nii";

            _imgProc.CompareDicomInNiftiOut(
                currentDicomFolder, priorDicomFolder, lookupTable, sliceType,
                extractBrain, register, biasFieldCorrect,
                resultNiiFile, outPriorReslicedNiiFile);

            _log.Info("Start Converting Results back to Dicom");
            ConvertToDicom(resultNiiFile, resultDicom, sliceType, currentDicomFolder);
            _log.Info("Finished Converting Results back to Dicom");

            UpdateSeriesDescriptionForAllFiles(resultDicom, "CAPI Modified Signal");

            // current study headers are used as this series is going to be sent to the current study
            // prior study date will be added to the end of Series Description tag
            _log.Info("Start Converting Resliced Prior Series back to Dicom");
            ConvertToDicom(outPriorReslicedNiiFile, outPriorReslicedDicom, sliceType, currentDicomFolder);
            _log.Info("Finished Converting Resliced Prior Series back to Dicom");

            var studydate = GetStudyDateFromDicomFile(Directory.GetFiles(priorDicomFolder).FirstOrDefault());
            UpdateSeriesDescriptionForAllFiles(outPriorReslicedDicom, $"CAPI Old Study (comparison) re-slilced ({studydate})");
        }

        private void UpdateSeriesDescriptionForAllFiles(string dicomFolder, string seriesDescription)
        {
            var dicomFiles = Directory.GetFiles(dicomFolder);
            if (dicomFiles.FirstOrDefault() == null)
                throw new FileNotFoundException($"Dicom folder contains no files: [{dicomFolder}]");

            var dicomTags = _dicomServices.GetDicomTags(dicomFiles.FirstOrDefault());
            dicomTags.SeriesDescription.Values = new[] { seriesDescription };
            _dicomServices.UpdateSeriesHeadersForAllFiles(dicomFiles.ToArray(), dicomTags);
        }

        private string GetStudyDateFromDicomFile(string dicomFile)
        {
            var headers = _dicomServices.GetDicomTags(dicomFile);
            var studyDateVal = headers.StudyDate.Values[0];
            var year = studyDateVal.Substring(0, 4);
            var month = studyDateVal.Substring(4, 2);
            var day = studyDateVal.Substring(6, 2);
            return $"{year}-{month}-{day}";
        }

        private void ConvertToDicom(string inNiftiFile, string outDicomFolder,
                                    SliceType sliceType, string dicomFolderForReadingHeaders)
        {
            var nim = _imgProcFactory.CreateNifti().ReadNifti(inNiftiFile);
            var bmpFolder = outDicomFolder + "_Images";
            nim.ExportSlicesToBmps(bmpFolder, sliceType);

            _dicomServices.ConvertBmpsToDicom(bmpFolder, outDicomFolder, dicomFolderForReadingHeaders);
        }

        public void CompareAndSendToDicomNode(string inCurrentDicomFolder, string inPriorDicomFolder,
                                              string inLookupTable, SliceType sliceType,
                                              bool extractBrain, bool register, bool biasFieldCorrect,
                                              string outResultDicom, string outPriorReslicedDicom,
                                              IDicomNode localNode, IDicomNode destination)
        {
            CompareAndSendToFilesystem(inCurrentDicomFolder, inPriorDicomFolder, inLookupTable, sliceType,
                    extractBrain, register, biasFieldCorrect,
                    outResultDicom, outPriorReslicedDicom);

            foreach (var dcmFile in Directory.GetFiles(outResultDicom))
                _dicomServices.SendDicomFile(dcmFile, localNode.AeTitle, destination);

            foreach (var dcmFile in Directory.GetFiles(outPriorReslicedDicom))
                _dicomServices.SendDicomFile(dcmFile, localNode.AeTitle, destination);
        }
    }
}
