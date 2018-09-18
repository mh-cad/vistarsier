using CAPI.Agent.Abstractions.Models;
using CAPI.Common.Abstractions.Config;
using CAPI.Dicom.Abstractions;
using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using System.Collections.Generic;
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

        public string[] CompareAndSaveLocally(
            string currentDicomFolder, string priorDicomFolder,
            string[] lookupTablePaths, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string outPriorReslicedDicom,
            string resultsDicomSeriesDescription, string priorReslicedDicomSeriesDescription)
        {
            var workingDir = Directory.GetParent(outPriorReslicedDicom).FullName;
            var resultNiis = BuildResultNiftiPathsFromLuts(lookupTablePaths, workingDir).ToArray();
            var outPriorReslicedNiiFile = outPriorReslicedDicom + ".nii";

            _imgProc.CompareDicomInNiftiOut(
                currentDicomFolder, priorDicomFolder, lookupTablePaths, sliceType,
                extractBrain, register, biasFieldCorrect,
                resultNiis, outPriorReslicedNiiFile);

            // "current" study dicom headers are used as the "prior resliced" series gets sent as part of the "current" study
            // prior study date will be added to the end of Series Description tag
            var task = Task.Run(() =>
            {
                _log.Info("Start converting resliced prior series back to Dicom");

                var priorStudyDate = GetStudyDateFromDicomFile(Directory.GetFiles(priorDicomFolder).FirstOrDefault());
                var priorStudyDescBase = string.IsNullOrEmpty(priorReslicedDicomSeriesDescription) ?
                    _imgProcConfig.PriorReslicedDicomSeriesDescription :
                    priorReslicedDicomSeriesDescription;
                var priorStudyDescription = $"{priorStudyDescBase} {priorStudyDate}";

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                ConvertToDicom(outPriorReslicedNiiFile, outPriorReslicedDicom, sliceType, currentDicomFolder, priorStudyDescription);

                UpdateSeriesDescriptionForAllFiles(outPriorReslicedDicom, priorStudyDescription);

                stopwatch.Stop();
                _log.Info("Finished Converting resliced prior series back to Dicom in " +
                          $"{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds:D2} minutes.");
            });

            foreach (var resultNii in resultNiis)
            {
                _log.Info("Start converting results back to Dicom");

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var resultsSeriesDescription = string.IsNullOrEmpty(resultsDicomSeriesDescription)
                    ? _imgProcConfig.ResultsDicomSeriesDescription
                    : resultsDicomSeriesDescription;

                var dicomFolderPath = resultNii.Replace(".nii", "");
                ConvertToDicom(resultNii, dicomFolderPath, sliceType, currentDicomFolder, resultsSeriesDescription);

                UpdateSeriesDescriptionForAllFiles(dicomFolderPath, resultsSeriesDescription);

                stopwatch.Stop();
                _log.Info("Finished converting results back to Dicom in " +
                          $"{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds:D2} minutes.");
            }

            task.Wait();
            task.Dispose();

            var resultDicomFolderPaths = resultNiis.Select(r => r.Replace(".nii", "")).ToArray();
            return resultDicomFolderPaths;
        }

        private static IEnumerable<string> BuildResultNiftiPathsFromLuts(IReadOnlyList<string> lookupTablePaths, string workingDir)
        {
            var allResultsFolder = Path.Combine(workingDir, "Results");
            Directory.CreateDirectory(allResultsFolder);
            var resultPaths = new string[lookupTablePaths.Count];
            for (var i = 0; i < lookupTablePaths.Count; i++)
            {
                if (!File.Exists(lookupTablePaths[i]))
                    throw new FileNotFoundException($"Could not find the lookup table in following path [{lookupTablePaths[i]}]", lookupTablePaths[i]);
                var lookupTableName = Path.GetFileNameWithoutExtension(lookupTablePaths[i]);
                var resultFolder = Path.Combine(allResultsFolder, lookupTableName ?? "_");
                Directory.CreateDirectory(resultFolder);
                resultPaths[i] = Path.Combine(resultFolder, "result.nii");
            }

            return resultPaths;
        }

        public void CompareAndSaveLocally(IJob job, IRecipe recipe, SliceType sliceType)
        {
            CompareAndSaveLocally(
                job.CurrentSeriesDicomFolder, job.PriorSeriesDicomFolder,
                recipe.LookUpTablePaths, sliceType,
                job.ExtractBrain, job.Register, job.BiasFieldCorrection,
                job.PriorReslicedSeriesDicomFolder,
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
