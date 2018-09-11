using CAPI.Agent.Abstractions.Models;
using CAPI.Common.Abstractions.Config;
using CAPI.Dicom.Abstractions;
using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IImgProcConfig _imgProcConfig;

        public ImageProcessor(IDicomServices dicomServices, IImageProcessingFactory imgProcFactory,
                              IFileSystem filesystem, IProcessBuilder processBuilder,
                              IImgProcConfig imgProcConfig, ILog log)
        {
            _dicomServices = dicomServices;
            _imgProcFactory = imgProcFactory;
            _log = log;
            _imgProcConfig = imgProcConfig;
            _imgProc = imgProcFactory.CreateImageProcessor(filesystem, processBuilder, imgProcConfig, log);
        }

        public void CompareAndSendToFilesystem(
            string currentDicomFolder, string priorDicomFolder,
            string lookupTable, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string resultDicom, string outPriorReslicedDicom,
            string resultsDicomSeriesDescription, string priorReslicedDicomSeriesDescription)
        {
            var resultNiiFile = resultDicom + ".nii";
            var outPriorReslicedNiiFile = outPriorReslicedDicom + ".nii";

            _imgProc.CompareDicomInNiftiOut(
                currentDicomFolder, priorDicomFolder, lookupTable, sliceType,
                extractBrain, register, biasFieldCorrect,
                resultNiiFile, outPriorReslicedNiiFile);

            var task1 = Task.Run(() =>
            {
                _log.Info("Start Converting Results back to Dicom");
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                ConvertToDicom(resultNiiFile, resultDicom, sliceType, currentDicomFolder,
                    _imgProcConfig.ResultsDicomSeriesDescription);

                UpdateSeriesDescriptionForAllFiles(resultDicom, resultsDicomSeriesDescription);

                stopwatch.Stop();

                _log.Info("Finished Converting Results back to Dicom in " +
                          $"{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds} minutes.");
            });

            // current study headers are used as this series is going to be sent to the current study
            // prior study date will be added to the end of Series Description tag
            var task2 = Task.Run(() =>
            {
                _log.Info("Start Converting Resliced Prior Series back to Dicom");

                var priorStudyDate = GetStudyDateFromDicomFile(Directory.GetFiles(priorDicomFolder).FirstOrDefault());
                var priorStudyDescription = $"{_imgProcConfig.PriorReslicedDicomSeriesDescription} {priorStudyDate}";

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                ConvertToDicom(outPriorReslicedNiiFile, outPriorReslicedDicom, sliceType,
                    currentDicomFolder, priorStudyDescription);

                UpdateSeriesDescriptionForAllFiles(outPriorReslicedDicom, priorStudyDescription);

                stopwatch.Stop();

                _log.Info("Finished Converting Resliced Prior Series back to Dicom in " +
                          $"{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds} minutes.");
            });
            task1.Wait();
            task2.Wait();
        }

        public void CompareAndSendToFilesystem(IJob job, IRecipe recipe, SliceType sliceType)
        {
            CompareAndSendToFilesystem(
                job.CurrentSeriesDicomFolder, job.PriorSeriesDicomFolder,
                recipe.LookUpTablePath, sliceType,
                job.ExtractBrain, job.Register, job.BiasFieldCorrection,
                job.ResultSeriesDicomFolder, job.PriorReslicedSeriesDicomFolder,
                recipe.ResultsDicomSeriesDescription, recipe.PriorReslicedDicomSeriesDescription
            );
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
                                    SliceType sliceType, string dicomFolderForReadingHeaders,
                                    string overlayText)
        {
            //if (normalize && File.Exists(maskFilePath))
            //{
            //    var nim = _imgProcFactory.CreateNifti().ReadNifti(inNiftiFile);
            //    var mask = _imgProcFactory.CreateNifti().ReadNifti(maskFilePath);
            //    nim = nim.NormalizeEachSlice(nim, sliceType, 128, 32, 256, mask);
            //    File.Move(inNiftiFile, inNiftiFile.Replace(".nii", ".prenormalization.nii"));
            //    nim.WriteNifti(inNiftiFile);
            //}

            var bmpFolder = outDicomFolder + "_Images";

            ConvertToBmp(inNiftiFile, bmpFolder, sliceType, overlayText);

            _dicomServices.ConvertBmpsToDicom(bmpFolder, outDicomFolder, dicomFolderForReadingHeaders);
        }

        private void ConvertToBmp(string inNiftiFile, string bmpFolder, SliceType sliceType, string overlayText)
        {
            var nim = _imgProcFactory.CreateNifti().ReadNifti(inNiftiFile);

            nim.ExportSlicesToBmps(bmpFolder, sliceType);

            foreach (var bmpFilePath in Directory.GetFiles(bmpFolder))
                AddOverlayToImage(bmpFilePath, overlayText);
        }

        public void AddOverlayToImage(string bmpFilePath, string overlayText)
        {
            if (string.IsNullOrEmpty(overlayText) || string.IsNullOrWhiteSpace(overlayText)) return;
            Bitmap bmpWithOverlay;
            using (var fs = new FileStream(bmpFilePath, FileMode.Open))
            {
                var bitmap = (Bitmap)Image.FromStream(fs);

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    using (var text = new Font("Tahoma", 9))
                    {
                        var x = (float)(bitmap.Width - overlayText.Length * 5.4) / 2;
                        var y = bitmap.Height - text.Height - 5;
                        graphics.DrawString(overlayText, text, Brushes.White, new PointF(x, y));
                    }
                }
                bmpWithOverlay = bitmap;
            }
            if (File.Exists(bmpFilePath)) File.Delete(bmpFilePath);
            bmpWithOverlay.Save(bmpFilePath);
        }
    }
}
