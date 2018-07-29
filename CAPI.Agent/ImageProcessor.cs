using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using System;
using System.IO;
using System.Linq;
using IImgProc = CAPI.ImageProcessing.Abstraction.IImageProcessorNew;

namespace CAPI.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImageProcessor : CAPI.Agent.Abstractions.IImageProcessor
    {
        private readonly IDicomServices _dicomServices;
        private readonly IImgProc _imgProc;
        private readonly IImageProcessingFactory _imgProcFactory;

        public ImageProcessor(IDicomServices dicomServices, IImgProc imgProc, IImageProcessingFactory imgProcFactory)
        {
            _dicomServices = dicomServices;
            _imgProc = imgProc;
            _imgProcFactory = imgProcFactory;
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

            ConvertToDicom(resultNiiFile, resultDicom, sliceType, currentDicomFolder);

            UpdateSeriesDescriptionForAllFiles(resultDicom, "CAPI Modified Signal");

            // current study headers are used as this series is going to be sent to the current study
            // prior study date will be added to the end of Series Description tag
            ConvertToDicom(outPriorReslicedNiiFile, outPriorReslicedDicom, sliceType, currentDicomFolder);

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

        private void ConvertToDicom(string inNiftiFile, string outDicomFolder, SliceType sliceType, string dicomFolderForReadingHeaders)
        {
            var nim = _imgProcFactory.CreateNifti().ReadNifti(inNiftiFile);
            var bmpFolder = outDicomFolder + "_Images";
            nim.ExportSlicesToBmps(bmpFolder, sliceType);

            _dicomServices.ConvertBmpsToDicom(bmpFolder, outDicomFolder, dicomFolderForReadingHeaders);
        }

        public void CompareAndSendToDicomNode(string inCurrentDicomFolder, string inPriorDicomFolder, string inLookupTable, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string outResultDicom, string outPriorReslicedDicom, IDicomNode localNode, IDicomNode destination)
        {
            CompareAndSendToFilesystem(inCurrentDicomFolder, inPriorDicomFolder, inLookupTable, sliceType,
                    extractBrain, register, biasFieldCorrect,
                    outResultDicom, outPriorReslicedDicom);

            foreach (var dcmFile in Directory.GetFiles(outResultDicom))
                _dicomServices.SendDicomFile(dcmFile, localNode.AeTitle, destination);

            foreach (var dcmFile in Directory.GetFiles(outPriorReslicedDicom))
                _dicomServices.SendDicomFile(dcmFile, localNode.AeTitle, destination);
        }

        private string UpdateStudyDescriptionDicomTag(string dcmFile, string studyDescription)
        {
            var headers = _dicomServices.GetDicomTags(dcmFile);
            var studyDate = DateTime.Parse(headers.StudyDate.Values[0]).ToString("yyyy-MM-dd");
            headers.SeriesDescription.Values = new[] { $"{studyDescription} ({studyDate})" };
            _dicomServices.UpdateDicomHeaders(dcmFile, headers, DicomNewObjectType.NewSeries);
            return dcmFile;
        }

        //[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        //public void CompareAndSendToFilesystem1(string inCurrentDicomFolder, string inPriorDicomFolder, string inLookupTable, SliceType sliceType,
        //    bool extractBrain, bool register, bool biasFieldCorrect,
        //    string outResultDicom, string outPriorReslicedDicom, string destinationFolder)
        //{
        //    CompareAndSendToFilesystem(inCurrentDicomFolder, inPriorDicomFolder, inLookupTable, sliceType,
        //            extractBrain, register, biasFieldCorrect,
        //            outResultDicom, outPriorReslicedDicom);

        //    var destinationResultsDicomFolder = Path.Combine(destinationFolder, Path.GetFileName(outResultDicom));
        //    Directory.Move(outResultDicom, destinationResultsDicomFolder);

        //    var destinationPriorsReslicedDicomFolder = Path.Combine(destinationFolder, Path.GetFileName(outPriorReslicedDicom));
        //    Directory.Move(outResultDicom, destinationPriorsReslicedDicomFolder);
        //}
    }
}
