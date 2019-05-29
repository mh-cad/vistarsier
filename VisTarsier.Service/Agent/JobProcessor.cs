using VisTarsier.Service.Db;
using VisTarsier.Config;
using VisTarsier.Dicom.Abstractions;
using VisTarsier.Common;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SliceType = VisTarsier.NiftiLib.SliceType;
using VisTarsier.NiftiLib.Processing;
using VisTarsier.NiftiLib;
using VisTarsier.MS;
using System.Drawing;
using VisTarsier.Service.Agent.Abstractions;
using VisTarsier.Dicom;
using System.Globalization;

namespace VisTarsier.Service.Agent
{
    /// <summary>
    /// Compares current and prior sereis and saves results into filesystem or sends off to a dicom node
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class JobProcessor : IJobProcessor
    {
        private readonly ILog _log;
        private readonly DbBroker _dbBroker;

        private const string ResultsFolderName = "Results";
        private const string ImagesFolderSuffix = "_Images";

        public JobProcessor(DbBroker dbBroker)
        {
            _log = Log.GetLogger();
            _dbBroker = dbBroker;
        }

        public IJobResult[] CompareAndSaveLocally(
            string currentDicomFolder, string priorDicomFolder, string referenceDicomFolder,
            SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string outPriorReslicedDicom,
            string resultsDicomSeriesDescription, string priorReslicedDicomSeriesDescription)
        {
            // Working directory.
            var workingDir = Directory.GetParent(outPriorReslicedDicom).FullName;
            // We're getting result files based on paths for look up tables??? //TODO deconvolve...
            //var resultNiis = BuildResultNiftiPathsFromLuts(lookupTablePaths, workingDir).ToArray();
            // Create results folder
            var allResultsFolder = Path.Combine(workingDir, ResultsFolderName);
            FileSystem.DirectoryExistsIfNotCreate(allResultsFolder);
            string[] resultNiis = { Path.Combine(allResultsFolder, "increase.nii"), Path.Combine(allResultsFolder, "decrease.nii") };


            var outPriorReslicedNii = outPriorReslicedDicom + ".nii";

            var dicomFilePath = Directory.GetFiles(currentDicomFolder)[0];
            var patientId = DicomFileOps.GetPatientIdFromDicomFile(dicomFilePath);
            var job = _dbBroker.Jobs.LastOrDefault(j => j.PatientId == patientId);


            // Generate Nifti file from Dicom and pass to ProcessNifti Method for current series.
            _log.Info($@"Start converting series dicom files to Nii");
            var task1 = Task.Run(() => { return Tools.Dcm2Nii(currentDicomFolder, "current.nii"); });
            var task2 = Task.Run(() => { return Tools.Dcm2Nii(priorDicomFolder, "prior.nii"); });
            var task3 = Task.Run(() => { return Tools.Dcm2Nii(referenceDicomFolder, "reference.nii"); });
            task1.Wait();
            task2.Wait();
            task3.Wait();

            var currentNifti = task1.Result;
            var priorNifti = task2.Result;
            var referenceNifti = task3.Result;
            _log.Info($@"Finished converting series dicom files to Nii");

            // Process Nifti files.
            var pipe = new MSPipeline(
                currentNifti, priorNifti, referenceNifti,
                extractBrain, register, biasFieldCorrect,
                resultNiis, outPriorReslicedNii);

            var metrics = pipe.Process(); // TODO: use theres. <-- What does this mean??

            var results = new List<IJobResult>();

            if (job != null)
            {
                var jobToUpdate = _dbBroker.Jobs.FirstOrDefault(j => j.Id == job.Id);
                if (jobToUpdate == null) throw new Exception($"No job was found in database with id: [{job.Id}]");

                // Check if there is a reference series from last processes for the patient, if not set the current series as reference for future
                GetReferenceSeries(referenceDicomFolder, currentDicomFolder, out var refStudyUid, out var refSeriesUid);
                jobToUpdate.WriteStudyAndSeriesIdsToReferenceSeries(refStudyUid, refSeriesUid);
                _dbBroker.Jobs.Update(jobToUpdate);
                _dbBroker.SaveChanges();
            }

            // "current" study dicom headers are used as the "prior resliced" series gets sent as part of the "current" study
            // prior study date will be added to the end of Series Description tag
            var task = Task.Run(() =>
            {
                _log.Info("Start converting resliced prior series back to Dicom");

                var priorStudyDate = GetStudyDateFromDicomFile(Directory.GetFiles(priorDicomFolder).FirstOrDefault());
                priorStudyDate = FormatDate(priorStudyDate);
                var priorStudyDescBase = string.IsNullOrEmpty(priorReslicedDicomSeriesDescription) ?
                    CapiConfig.GetConfig().ImagePaths.PriorReslicedDicomSeriesDescription :
                    priorReslicedDicomSeriesDescription;
                var priorStudyDescription = $"{priorStudyDescBase} {priorStudyDate}";

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                ConvertNiftiToDicom(outPriorReslicedNii, outPriorReslicedDicom, sliceType,
                                    currentDicomFolder, priorStudyDescription, referenceDicomFolder);

                UpdateSeriesDescriptionForAllFiles(outPriorReslicedDicom, priorStudyDescription);

                stopwatch.Stop();
                _log.Info("Finished Converting resliced prior series back to Dicom in " +
                          $"{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds:D2} minutes.");

                results.Add(new JobResult()
                {
                    DicomFolderPath = outPriorReslicedDicom,
                    NiftiFilePath = outPriorReslicedNii,
                    ImagesFolderPath = outPriorReslicedDicom + ImagesFolderSuffix,
                });
            });

            _log.Debug(resultNiis);


            foreach (var resultNii in resultNiis)
            {
                _log.Info("Start converting results back to Dicom");

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var resultsSeriesDescription = string.IsNullOrEmpty(resultsDicomSeriesDescription)
                    ? CapiConfig.GetConfig().ImagePaths.ResultsDicomSeriesDescription
                    : resultsDicomSeriesDescription;

                string dicomFolderPath;

                if (resultNii.ToLower().Contains("nicta"))
                {
                    var resultFolder = Path.GetDirectoryName(resultNii);
                    dicomFolderPath = Path.Combine(resultFolder ?? throw new InvalidOperationException($"unable to get folder [{resultNii}] resides in."),
                        "Result_Dicom");
                    var imagesFolder = Directory.GetDirectories(resultFolder)
                        .FirstOrDefault(d => d.ToLower().Contains("images"));

                    var seriesDescription = resultNii.ToLower().Contains("nictapos") ? "Nicta Increased Signal" : "Nicta Decreased Signal";
                    ConvertBmpsToDicom(imagesFolder, dicomFolderPath, currentDicomFolder, sliceType, seriesDescription, true);
                }
                else
                {
                    dicomFolderPath = resultNii.Replace(".nii", "");
                    var priorDate = GetStudyDateFromDicomFile(Directory.GetFiles(priorDicomFolder).FirstOrDefault());
                    var currentDate = GetStudyDateFromDicomFile(Directory.GetFiles(currentDicomFolder).FirstOrDefault());
                    var description = dicomFolderPath.ToLower().Contains("increase") ? "increase" : "decrease";
                    //lutFilePath = GetLookupTableForResult(resultNii, lookupTablePaths);
                    //var lutFileName = Path.GetFileNameWithoutExtension(lutFilePath);
                    ConvertNiftiToDicom(resultNii, dicomFolderPath, sliceType, currentDicomFolder,
                                        $"{resultsSeriesDescription}-{description}\n {FormatDate(priorDate)} -> {FormatDate(currentDate)}", referenceDicomFolder);
                }

                stopwatch.Stop();
                _log.Info("Finished converting results back to Dicom in " +
                          $"{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds:D2} minutes.");

                results.Add(new JobResult
                {
                    DicomFolderPath = dicomFolderPath,
                    NiftiFilePath = resultNii,
                    ImagesFolderPath = dicomFolderPath + ImagesFolderSuffix,
                });
            }

            task.Wait();
            task.Dispose();

            return results.ToArray();
        }

        public IJobResult[] GenerateMetadataSlides(IJob job)
        {
            // Create results folder
            var allResultsFolder = job.ResultSeriesDicomFolder;
            FileSystem.DirectoryExistsIfNotCreate(allResultsFolder);

            // Make our metadata...
            string metadataPath = Path.Combine(allResultsFolder, "metadata");
            Directory.CreateDirectory(metadataPath); 

            string summary = GenerateSummarySlide(
                Directory.GetFiles(job.PriorSeriesDicomFolder).FirstOrDefault(),
                Directory.GetFiles(job.CurrentSeriesDicomFolder).FirstOrDefault(),
                $"{job.Id}",
                metadataPath);

            GenerateMetadataSlide(
                Directory.GetFiles(job.PriorSeriesDicomFolder).FirstOrDefault(),
                Directory.GetFiles(job.CurrentSeriesDicomFolder).FirstOrDefault(),
                DicomFileOps.GetDicomTags(summary),
                metadataPath);

            return new IJobResult[] {
                new JobResult()
                {
                    DicomFolderPath = metadataPath
                }
            };
        }

        /// <summary>
        /// This is just a dirty method to replace numeric months with letters to avoid confusion.
        /// </summary>
        /// <param name="priorStudyDate"></param>
        /// <returns></returns>
        private string FormatDate(string priorStudyDate)
        {
            // Silly and inefficient but it's not in an inner loop ;)
            priorStudyDate = priorStudyDate.Replace("-01-", "-Jan-");
            priorStudyDate = priorStudyDate.Replace("-02-", "-Feb-");
            priorStudyDate = priorStudyDate.Replace("-03-", "-Mar-");
            priorStudyDate = priorStudyDate.Replace("-04-", "-Apr-");
            priorStudyDate = priorStudyDate.Replace("-05-", "-May-");
            priorStudyDate = priorStudyDate.Replace("-06-", "-Jun-");
            priorStudyDate = priorStudyDate.Replace("-07-", "-Jul-");
            priorStudyDate = priorStudyDate.Replace("-08-", "-Aug-");
            priorStudyDate = priorStudyDate.Replace("-09-", "-Sep-");
            priorStudyDate = priorStudyDate.Replace("-10-", "-Oct-");
            priorStudyDate = priorStudyDate.Replace("-11-", "-Nov-");
            priorStudyDate = priorStudyDate.Replace("-12-", "-Dec-");

            return priorStudyDate;
        }

        private void GetReferenceSeries(string referenceDicomFolder, string currentDicomFolder,
                                        out string refStudyUId, out string refSeriesUid)
        {
            // Check if a reference series exists from previous process(es)
            var refStudyExists =
                (!string.IsNullOrEmpty(referenceDicomFolder) && Directory.Exists(referenceDicomFolder) &&
                 Directory.GetFiles(referenceDicomFolder).Length > 0);

            var referenceForFutureProcessesDicomFolder = refStudyExists ? referenceDicomFolder : currentDicomFolder;

            var dicomFilePath = Directory.GetFiles(referenceForFutureProcessesDicomFolder).FirstOrDefault();
            var dicomFileHeaders = DicomFileOps.GetDicomTags(dicomFilePath);
            refStudyUId = dicomFileHeaders.StudyInstanceUid.Values[0];
            refSeriesUid = dicomFileHeaders.SeriesInstanceUid.Values[0];
        }

        public IJobResult[] CompareAndSaveLocally(IJob job, IRecipe recipe, SliceType sliceType)
        {
            return CompareAndSaveLocally(
                job.CurrentSeriesDicomFolder, job.PriorSeriesDicomFolder, job.ReferenceSeriesDicomFolder,
                sliceType,
                job.ExtractBrain, job.Register, job.BiasFieldCorrection,
                job.PriorReslicedSeriesDicomFolder,
                recipe.ResultsDicomSeriesDescription, recipe.PriorReslicedDicomSeriesDescription
            );
        }

        private void UpdateSeriesDescriptionForAllFiles(string dicomFolder, string seriesDescription,
                                                        string orientationDicomFolder = "")
        {
            var dicomFiles = Directory.GetFiles(dicomFolder);
            if (dicomFiles.FirstOrDefault() == null)
                throw new FileNotFoundException($"Dicom folder contains no files: [{dicomFolder}]");

            var orientationDicomFiles = string.IsNullOrEmpty(orientationDicomFolder) ? null : Directory.GetFiles(orientationDicomFolder);
            if (!string.IsNullOrEmpty(orientationDicomFolder) &&
                (!Directory.Exists(orientationDicomFolder) || Directory.GetFiles(orientationDicomFolder).Length == 0))
                throw new FileNotFoundException($"Orientation dicom folder contains no files: [{orientationDicomFolder}]");

            var dicomTags = DicomFileOps.GetDicomTags(dicomFiles.FirstOrDefault());
            dicomTags.SeriesDescription.Values = new[] { seriesDescription };

            DicomFileOps.GenerateSeriesHeaderForAllFiles(dicomFiles.ToArray(), dicomTags);
            if (!string.IsNullOrEmpty(orientationDicomFolder))
                DicomFileOps.UpdateImagePositionFromReferenceSeries(dicomFiles.ToArray(), orientationDicomFiles);
        }

        private string GetStudyDateFromDicomFile(string dicomFile)
        {
            var headers = DicomFileOps.GetDicomTags(dicomFile);
            var studyDateVal = headers.StudyDate.Values[0];
            var year = studyDateVal.Substring(0, 4);
            var month = studyDateVal.Substring(4, 2);
            var day = studyDateVal.Substring(6, 2);
            return $"{year}-{month}-{day}";
        }

        private void ConvertBmpsToDicom(string bmpFolder, string outDicomFolder, string sourceDicomFolder,
                                        SliceType sliceType, string seriesDescription, bool matchWithFileNames = false)
        {
            var dicomSliceType = GetDicomSliceType(sliceType);

            DicomFileOps.ConvertBmpsToDicom(bmpFolder, outDicomFolder, dicomSliceType,
                                              sourceDicomFolder, matchWithFileNames);

            UpdateSeriesDescriptionForAllFiles(outDicomFolder, seriesDescription);
        }
        private void ConvertNiftiToDicom(string inNiftiFile, string outDicomFolder,
                                         SliceType sliceType, string dicomFolderForReadingHeaders,
                                         string overlayText, string referenceDicomFolder = "")
        {
            var bmpFolder = outDicomFolder + ImagesFolderSuffix;
            _log.Debug("Converting to dicom -- " + inNiftiFile);
            ConvertToBmp(inNiftiFile, bmpFolder, sliceType, overlayText);

            ConvertBmpsToDicom(bmpFolder, outDicomFolder, dicomFolderForReadingHeaders, sliceType, overlayText);

            if (!string.IsNullOrEmpty(referenceDicomFolder) && Directory.Exists(referenceDicomFolder) && Directory.GetFiles(referenceDicomFolder).Length > 0)
                DicomFileOps.UpdateImagePositionFromReferenceSeries(Directory.GetFiles(outDicomFolder), Directory.GetFiles(referenceDicomFolder));

            //if (!string.IsNullOrEmpty(lookupTableFilePath) &&
            //    File.Exists(lookupTableFilePath))
            //    _dicomServices.ConvertBmpToDicomAndAddToExistingFolder(lookupTableFilePath, outDicomFolder);
        }

        private Dicom.Abstractions.SliceType GetDicomSliceType(SliceType sliceType)
        {
            switch (sliceType)
            {
                case SliceType.Sagittal:
                    return Dicom.Abstractions.SliceType.Sagittal;
                case SliceType.Coronal:
                    return Dicom.Abstractions.SliceType.Coronal;
                case SliceType.Axial:
                    return Dicom.Abstractions.SliceType.Axial;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sliceType), sliceType, null);
            }
        }

        private void ConvertToBmp(string inNiftiFile, string bmpFolder, SliceType sliceType, string overlayText)
        {
            _log.Debug("Converting to bmp -- " + inNiftiFile);
            var nim = (NiftiFloat32)new NiftiFloat32().ReadNifti(inNiftiFile);

            nim.ExportSlicesToBmps(bmpFolder, sliceType);

            foreach (var bmpFilePath in Directory.GetFiles(bmpFolder))
                AddOverlayToImage(bmpFilePath, overlayText);
        }

        public void AddOverlayToImage(string bmpFilePath, string overlayText)
        {
            if (string.IsNullOrEmpty(overlayText) || string.IsNullOrWhiteSpace(overlayText)) return;
            Bitmap bmpWithOverlay;
            var lines = overlayText.Split('\n');

            using (var fs = new FileStream(bmpFilePath, FileMode.Open))
            {
                var bitmap = (Bitmap)Image.FromStream(fs);

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    using (var text = new Font("Tahoma", 9))
                    {
                        var x = (float)(bitmap.Width - lines[0].Length * 5.4) / 2;
                        var y = bitmap.Height - text.Height - 20;
                        graphics.DrawString(lines[0], text, Brushes.Black, new PointF(x+1, y+1));
                        graphics.DrawString(lines[0], text, Brushes.White, new PointF(x, y));
                    }

                    if(lines.Length > 1)
                    {
                        using (var text = new Font("Tahoma", 7))
                        {
                            var x = (float)(bitmap.Width - lines[0].Length * 5.4) / 2;
                            var y = bitmap.Height - text.Height - 10;
                            graphics.DrawString(lines[1], text, Brushes.Black, new PointF(x + 1, y + 1));
                            graphics.DrawString(lines[1], text, Brushes.White, new PointF(x, y));
                        }
                    }
                }
                bmpWithOverlay = bitmap;
            }
            if (File.Exists(bmpFilePath)) File.Delete(bmpFilePath);
            bmpWithOverlay.Save(bmpFilePath);
        }

        public string GenerateSummarySlide(string priorDcm, string currentDcm, string jobId, string outfolder)
        {
            var patientIdField = new Point(183, 177);
            var patientNameField = new Point(205, 200);
            var patientDobField = new Point(194, 221);
            var patientSexField = new Point(194, 243);
            var priorAccessionField = new Point(260, 358);
            var priorDateField = new Point(204, 379);
            var priorDescField = new Point(175, 418);
            var currentAccessionField = new Point(260, 539);
            var currentDateField = new Point(204, 557);
            var currentDescField = new Point(175, 597);

            var outfile = Path.Combine(outfolder, "summary.dcm");

            var priorTags = DicomFileOps.GetDicomTags(priorDcm);
            var currentTags = DicomFileOps.GetDicomTags(currentDcm);
            var bmppath = Path.Combine(".", "resources", "templates", "summary.bmp");
            Bitmap slide = new Bitmap(bmppath);
            using (var g = Graphics.FromImage(slide))
            {
                var font = new Font("Courier New", 12);
                var brush = Brushes.White;
                var priorStudyDate = DateTime.ParseExact(priorTags.StudyDate?.Values?[0], "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy");
                var dob = DateTime.ParseExact(priorTags.PatientBirthDate?.Values?[0], "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy");
                var currentStudyDate = DateTime.ParseExact(priorTags.PatientBirthDate?.Values?[0], "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy");

                g.DrawString(priorTags.PatientId?.Values?[0], font, Brushes.White, patientIdField);
                g.DrawString(priorTags.PatientName?.Values?[0], font, Brushes.White, patientNameField);
                g.DrawString(dob, font, Brushes.White, patientDobField);
                g.DrawString(priorTags.PatientSex?.Values?[0], font, Brushes.White, patientSexField);
                g.DrawString(priorTags.StudyAccessionNumber?.Values?[0], font, Brushes.White, priorAccessionField);
                g.DrawString(priorStudyDate, font, Brushes.White, priorDateField);
                g.DrawString(priorTags.StudyDescription?.Values?[0], font, Brushes.White, priorDescField);
                g.DrawString(currentTags.StudyAccessionNumber?.Values?[0], font, Brushes.White, currentAccessionField);
                g.DrawString(currentStudyDate, font, Brushes.White, currentDateField);
                g.DrawString(currentTags.StudyDescription?.Values?[0], font, Brushes.White, currentDescField);

                g.DrawString($"[{jobId}]", new Font("Courier New", 14, FontStyle.Bold), Brushes.White, new Point(524,112));
            }

            try
            {
                new Bitmap(slide).Save($"{outfile}.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }
            catch (Exception e)
            {
                _log.Debug("Threw an error while trying to save bitmap because GDI+ is dumb.");
                _log.Debug($"Tried to save bmp to {outfile}.bmp");
                throw e;
            }

            DicomFileOps.ConvertBmpToDicom($"{outfile}.bmp", outfile);
            File.Delete($"{outfile}.bmp");

            // Generate a bunch of metadata for the outgoing DICOM...
            var metatags = DicomFileOps.GetDicomTags(outfile);
            metatags.InstitutionAddress.Values = currentTags.InstitutionAddress.Values;
            metatags.InstitutionalDepartmentName.Values = currentTags.InstitutionalDepartmentName.Values;
            metatags.InstitutionName.Values = currentTags.InstitutionName.Values;
            metatags.PatientId.Values = currentTags.PatientId.Values;
            metatags.PatientName.Values = currentTags.PatientName.Values;
            metatags.PatientSex.Values = currentTags.PatientSex.Values;
            metatags.PerformingPhysiciansName.Values = currentTags.PerformingPhysiciansName.Values;
            metatags.PhysiciansOfRecord.Values = currentTags.PhysiciansOfRecord.Values;
            metatags.ReferringPhysician.Values = currentTags.ReferringPhysician.Values;
            metatags.RequestingPhysician.Values = currentTags.RequestingPhysician.Values;
            metatags.SeriesInstanceUid.Values = new string[] { DicomFileOps.GenerateNewSeriesUid() };
            metatags.SeriesDescription.Values = new string[] { $"VisTarsier Metadata Job [{jobId}]" };
            metatags.StudyAccessionNumber.Values = currentTags.StudyAccessionNumber.Values;
            metatags.StudyDate.Values = currentTags.StudyDate.Values;
            metatags.StudyDescription.Values = currentTags.StudyDescription.Values;
            metatags.StudyInstanceUid.Values = currentTags.StudyInstanceUid.Values;
            metatags.InstanceNumber.Values = new string[] { "1" };

            DicomFileOps.ForceUpdateDicomHeaders(outfile, metatags);


            return outfile;
        }

        public string GenerateMetadataSlide(string priorDcm, string currentDcm, IDicomTagCollection metatags, string outFolder)
        {
            var priorTags = DicomFileOps.GetDicomTags(priorDcm);
            var currentTags = DicomFileOps.GetDicomTags(currentDcm);

            var outpath = Path.Combine(outFolder, "metadata.dcm");

            // References for bitmap template (TODO: Maybe we could put these in a file for easy changing)
            //var col1X = 77;
            var col2X = 295;
            var col3X = 530;
            var rowHeight = 22;
            var rowstartY = 213;

            Bitmap slide = new Bitmap(Path.Combine("resources", "templates", "metadata.bmp"));
            using (var g = Graphics.FromImage(slide))
            {
                var font = new Font("Courier New", 12);
                var brush = Brushes.White;

                var priorModality = priorTags.Modality?.Values?[0].ToString();
                var currentModality = currentTags.Modality?.Values?[0].ToString();
                brush = priorModality.Equals(currentModality) ? Brushes.White : Brushes.Red;
                g.DrawString(priorModality, font, brush, new Point(col2X, rowstartY + rowHeight * 0));
                g.DrawString(currentModality, font, brush, new Point(col3X, rowstartY + rowHeight * 0));

                var priorProtocol = priorTags.ProtocolName?.Values?[0].ToString();
                var currentProtocol = currentTags.ProtocolName?.Values?[0].ToString();
                brush = priorProtocol.Equals(currentProtocol) ? Brushes.White : Brushes.Red;
                if (priorProtocol.Length > 20) priorProtocol = priorProtocol.Substring(0, 17) + "...";
                if (currentProtocol.Length > 20) currentProtocol = currentProtocol.Substring(0, 17) + "...";

                g.DrawString(priorProtocol, font, brush, new Point(col2X, rowstartY + rowHeight * 1));
                g.DrawString(currentProtocol, font, brush, new Point(col3X, rowstartY + rowHeight * 1));

                var priorOptions = "";
                foreach (var val in priorTags.ScanOptions?.Values) priorOptions += val + "/";
                var currentOptions = "";
                foreach (var val in currentTags.ScanOptions?.Values) currentOptions += val + "/";
                brush = priorOptions.Equals(currentOptions) ? Brushes.White : Brushes.Orange;
                g.DrawString(priorOptions, font, brush, new Point(col2X, rowstartY + rowHeight * 2));
                g.DrawString(currentOptions, font, brush, new Point(col3X, rowstartY + rowHeight * 2));

                var priorScanner = $"{priorTags.Manufacturer?.Values?[0].ToString()} {priorTags.ManufacturersModelName?.Values?[0].ToString()}";
                var currentScanner = $"{currentTags.Manufacturer?.Values?[0].ToString()} {currentTags.ManufacturersModelName?.Values?[0].ToString()}";
                brush = priorScanner.Equals(currentScanner) ? Brushes.White : Brushes.Orange;
                g.DrawString(priorScanner, font, brush, new Point(col2X, rowstartY + rowHeight * 3));
                g.DrawString(currentScanner, font, brush, new Point(col3X, rowstartY + rowHeight * 3));

                var priorSerial = priorTags.DeviceSerialNumber?.Values?[0].ToString();
                var currentSerial = currentTags.DeviceSerialNumber?.Values?[0].ToString();
                brush = priorSerial.Equals(currentSerial) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorSerial, font, brush, new Point(col2X, rowstartY + rowHeight * 4));
                g.DrawString(currentSerial, font, brush, new Point(col3X, rowstartY + rowHeight * 4));

                var priorSoftware = "";
                foreach (var val in priorTags.SoftwareVersion?.Values) priorSoftware += val + " ";
                var currentSoftware = "";
                foreach (var val in currentTags.SoftwareVersion?.Values) currentSoftware += val + " ";
                brush = priorSoftware.Equals(currentSoftware) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorSoftware, font, brush, new Point(col2X, rowstartY + rowHeight * 5));
                g.DrawString(currentSoftware, font, brush, new Point(col3X, rowstartY + rowHeight * 5));

                var priorEcho = priorTags.EchoTime?.Values?[0].ToString();
                var currentEcho = currentTags.EchoTime?.Values?[0].ToString();
                brush = priorEcho.Equals(currentEcho) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorEcho, font, brush, new Point(col2X, rowstartY + rowHeight * 6));
                g.DrawString(currentEcho, font, brush, new Point(col3X, rowstartY + rowHeight * 6));

                var priorIt = priorTags.InversionTime?.Values?[0].ToString();
                var currentIt = currentTags.InversionTime?.Values?[0].ToString();
                brush = priorIt.Equals(currentIt) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorIt, font, brush, new Point(col2X, rowstartY + rowHeight * 7));
                g.DrawString(currentIt, font, brush, new Point(col3X, rowstartY + rowHeight * 7));

                var priorIn = priorTags.ImagedNucleus?.Values?[0].ToString();
                var currentIn = currentTags.ImagedNucleus?.Values?[0].ToString();
                brush = priorIn.Equals(currentIn) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorIn, font, brush, new Point(col2X, rowstartY + rowHeight * 8));
                g.DrawString(currentIn, font, brush, new Point(col3X, rowstartY + rowHeight * 8));

                var priorTeslas = priorTags.MagneticFieldStrength?.Values?[0].ToString() + "T";
                var currentTeslas = currentTags.MagneticFieldStrength?.Values?[0].ToString() + "T";
                brush = priorTeslas.Equals(currentTeslas) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorTeslas, font, brush, new Point(col2X, rowstartY + rowHeight * 9));
                g.DrawString(currentTeslas, font, brush, new Point(col3X, rowstartY + rowHeight * 9));

                var priorEt = priorTags.EchoTrainLength?.Values?[0].ToString();
                var currentEt = currentTags.EchoTrainLength?.Values?[0].ToString();
                brush = priorEt.Equals(currentEt) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorEt, font, brush, new Point(col2X, rowstartY + rowHeight * 10));
                g.DrawString(currentEt, font, brush, new Point(col3X, rowstartY + rowHeight * 10));

                var priorTc = priorTags.TransmitCoilName?.Values?[0].ToString();
                var currentTc = currentTags.TransmitCoilName?.Values?[0].ToString();
                brush = priorTc.Equals(currentTc) ? Brushes.White : Brushes.Yellow;
                g.DrawString(priorTc, font, brush, new Point(col2X, rowstartY + rowHeight * 11));
                g.DrawString(currentTc, font, brush, new Point(col3X, rowstartY + rowHeight * 11));
            }


            try
            {
                new Bitmap(slide).Save($"{outpath}.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }
            catch (Exception e)
            {
                _log.Debug("Threw an error while trying to save bitmap because GDI+ is dumb.");
                _log.Debug($"Tried to save bmp to {outpath}.bmp");
                throw e;
            }

            DicomFileOps.ConvertBmpToDicom($"{outpath}.bmp", outpath);
            File.Delete($"{outpath}.bmp");

            metatags = DicomFileOps.UpdateUidsForNewImage(metatags);
            metatags.InstanceNumber.Values = new string[] { "2" };
            DicomFileOps.ForceUpdateDicomHeaders(outpath, metatags);

           // DicomFileOps.UpdateDicomHeaders(outpath, metatags, DicomNewObjectType.NewImage);

            return outpath;
        }

        public string GenerateResultsSlide(Metrics results, IDicomTagCollection metatags)
        {
            return null;
        }
    }
}
