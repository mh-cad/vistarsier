using VisTarsier.Service.Db;
using VisTarsier.Config;
using VisTarsier.Dicom.Abstractions;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VisTarsier.Common;
using VisTarsier.Dicom;

namespace VisTarsier.Service.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class JobBuilder //: IJobBuilder
    {
        private const string Current = "Current";
        private const string Prior = "Prior";
        private const string Reference = "Reference";
        private const string Dicom = "Dicom";
        private const string Results = "Results";
        private const string PriorResliced = "PriorResliced";

        private readonly IValueComparer _valueComparer;
        private readonly ILog _log;
        private readonly CapiConfig _capiConfig;
        private readonly DbBroker _context;
        private IDicomService _dicomSource;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dicomServices"></param>
        /// <param name="imgProcFactory">ImageProcessing Factory</param>
        /// <param name="valueComparer"></param>
        /// <param name="fileSystem">Provides extra filesystem capabilities</param>
        /// <param name="processBuilder">Builds exe or java processes and executes them</param>
        /// <param name="capiConfig">CAPI configuration</param>
        /// <param name="log">Log4Net logger</param>
        /// <param name="context">Agent Repository (DbContext) to communicate data with database</param>
        public JobBuilder(IValueComparer valueComparer, DbBroker context)
        {
            _valueComparer = valueComparer;
            _log = Log.GetLogger();
            _context = context;

            _capiConfig = CapiConfig.GetConfig();

        }

        /// <summary>
        /// Builds a Job using details in Recipe
        /// </summary>
        /// <param name="recipe">Contains details of studies to process, source and destinations</param>
        /// <returns></returns>
        public IJob Build(Recipe recipe)
        {
            // Setup out dicom service.
            GetLocalAndRemoteNodes(recipe.SourceAet, out var localNode, out var sourceNode);

            var patientId = GetPatientIdFromRecipe(recipe);

            var allStudiesForPatient = GetDicomStudiesForPatient(patientId,
                recipe.PatientFullName, recipe.PatientBirthDate)
                    .OrderByDescending(s => s.StudyDate).ToList();

            if (allStudiesForPatient.Count < 1)
                throw new Exception(
                    $"No studies for patient [{recipe.PatientFullName}] could be found in node AET: [{sourceNode.AeTitle}]");

            recipe = UpdateRecipeWithPatientDetails(recipe, allStudiesForPatient);

            // Find Current Study (Fixed)
            _log.Info("Finding current series using recipe provided...");
            var currentDicomStudy = GetCurrentDicomStudy(recipe, allStudiesForPatient);
            if (currentDicomStudy == null ||
                currentDicomStudy.Series.Count == 0)
                throw new DirectoryNotFoundException("No workable series were found for accession");

            var job = new Job(recipe);
            var imageRepositoryPath = _capiConfig.ImagePaths.ImageRepositoryPath;
            var patientName = recipe.PatientFullName.Split('^')[0];
            var accession = job.CurrentAccession;
            var accessionInJobName = string.Empty;
            if (Regex.IsMatch(accession, @"^\d{4}R\d{7}-\d$")) accessionInJobName = $"-{accession.Substring(2, 10)}-";
            var patientNameSubstring = patientName.Length > 4 ? patientName.Substring(0, 5) : patientName.Substring(0, patientName.Length);
            var jobFolderName = $"{patientNameSubstring}{accessionInJobName}{DateTime.Now:yyMMdd_HHmmssfff}";
            job.ProcessingFolder = Path.Combine(_capiConfig.ImagePaths.ImageRepositoryPath, jobFolderName);

            var studyFixedIndex = allStudiesForPatient.IndexOf(currentDicomStudy);

            // Find Prior Study (Floating)
            _log.Info("Finding prior series using recipe provided...");
            var priorDicomStudy =
                GetPriorDicomStudy(recipe, studyFixedIndex, allStudiesForPatient);

            if (priorDicomStudy == null)
                throw new DirectoryNotFoundException("No prior workable series were found");

            job.ResultSeriesDicomFolder = Path.Combine(imageRepositoryPath, jobFolderName, Results);
            job.PriorAccession = priorDicomStudy.AccessionNumber;
            job.PatientId = currentDicomStudy.PatientId;

            // If both current and prior are found, save them to disk for processing
            _log.Info("Saving current series to disk...");
            try
            {
                job.CurrentSeriesDicomFolder = SaveDicomFilesToFilesystem(
                                currentDicomStudy, job.ProcessingFolder, Current);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to save current series dicom files to disk.", ex);
                throw;
            }
            _log.Info($"Saved current series to [{job.CurrentSeriesDicomFolder}]");

            _log.Info("Saving prior series to disk...");
            try
            {
                job.PriorSeriesDicomFolder = SaveDicomFilesToFilesystem(
                    priorDicomStudy, job.ProcessingFolder, Prior);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to save prior series dicom files to disk.", ex);
                throw;
            }
            _log.Info($"Saved prior series to [{job.PriorReslicedSeriesDicomFolder}]");

            job.PriorReslicedSeriesDicomFolder = Path.Combine(imageRepositoryPath, jobFolderName, PriorResliced);

            // Get Registration Data for patient if exists in database
            job.ReferenceSeriesDicomFolder =
                GetReferenceSeriesForRegistration(job, allStudiesForPatient);

            return job;
        }

        private string GetReferenceSeriesForRegistration(IJob job, IEnumerable<IDicomStudy> allStudiesForPatient)
        {
            var studiesForPatient = allStudiesForPatient.ToList();

            if (string.IsNullOrEmpty(job.ReferenceSeries))
                job.ReferenceSeries = FindReferenceSeriesInPreviousJobs(job.PatientId);

            if (string.IsNullOrEmpty(job.ReferenceSeries)) return string.Empty;

            var studyId = job.GetStudyIdFromReferenceSeries();
            var seriesId = job.GetSeriesIdFromReferenceSeries();
            var study = studiesForPatient.FirstOrDefault(s => s.StudyInstanceUid == studyId);
            if (study == null)
            {
                _log.Error($"Failed to find reference study to register series against StudyInstanceUid: [{studyId}]");
                return string.Empty;
            }
            var allSeries = _dicomSource.GetSeriesForStudy(study.StudyInstanceUid);
            var matchingSeries = allSeries.FirstOrDefault(s => s.SeriesInstanceUid == seriesId);
            if (matchingSeries == null)
            {
                _log.Error($"Failed to find reference study with matching series to register series against StudyInstanceUid: [{studyId}] SeriesInstanceUid: [{seriesId}]");
                return string.Empty;
            }
            study.Series.Add(matchingSeries);
            _log.Info("Saving reference series to disk...");
            string referenceFolderPath;
            try
            {
                referenceFolderPath = SaveDicomFilesToFilesystem(study, job.ProcessingFolder, Reference);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to save reference series dicom files to disk.", ex);
                throw;
            }
            return referenceFolderPath;
        }

        private string FindReferenceSeriesInPreviousJobs(string patientId)
        {
            var jobWithRefSeries = _context.Jobs.LastOrDefault(j => j.PatientId == patientId &&
                                                                    !string.IsNullOrEmpty(j.ReferenceSeries));

            return jobWithRefSeries != null ? jobWithRefSeries.ReferenceSeries :
                                              string.Empty;
        }

        private static Recipe UpdateRecipeWithPatientDetails(Recipe recipe, IReadOnlyCollection<IDicomStudy> studies)
        {
            recipe.PatientFullName = studies.FirstOrDefault()?.PatientsName;
            recipe.PatientBirthDate = studies.FirstOrDefault()?.PatientBirthDate.ToString("yyyyMMdd");
            return recipe;
        }

        private void GetLocalAndRemoteNodes(string sourceAet, out IDicomNode localNode, out IDicomNode sourceNode)
        {
            localNode = _capiConfig.DicomConfig.LocalNode;

            var remoteNodes = _capiConfig.DicomConfig.RemoteNodes;

            if (remoteNodes.Count == 0)
            {
                _log.Error("No remote nodes found in config file.");
                throw new Exception("No remote nodes found in config file.");
            }

            try
            {
                sourceNode = remoteNodes
                    .Single(n => n.AeTitle.Equals(sourceAet, StringComparison.InvariantCultureIgnoreCase));
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"Source node AET [{sourceAet}] in Recipe was not found in config file list of remote nodes";
                _log.Error(errorMessage, ex);
                throw new Exception(errorMessage);
            }

            try
            {
                _dicomSource = new DicomService(localNode, sourceNode);
                _dicomSource.CheckRemoteNodeAvailability();
            }
            catch (Exception ex)
            {
                _log.Error($"Local Node AET [{localNode.AeTitle}] cannot reach" +
                           $"Source Node AET [{sourceAet}]", ex);
                throw;
            }
        }

        private string SaveDicomFilesToFilesystem(
                                                  IDicomStudy dicomStudy, string jobProcessingFolder,
                                                  string studyName)
        {
            var series = dicomStudy.Series.FirstOrDefault();

            var folderPath = Path.Combine(jobProcessingFolder, studyName, Dicom);

            _dicomSource.SaveSeriesToLocalDisk(series, folderPath);

            CopyDicomFilesToRootFolder(folderPath);

            return folderPath;
        }

        private static void CopyDicomFilesToRootFolder(string folderPath)
        {
            var studyFolderPath = Directory.GetDirectories(folderPath).FirstOrDefault();
            if (studyFolderPath == null)
                throw new DirectoryNotFoundException($"Study folder not found in [{folderPath}]");

            var seriesFolderPath = Directory.GetDirectories(studyFolderPath).FirstOrDefault();
            if (seriesFolderPath == null)
                throw new DirectoryNotFoundException($"Series folder not found in [{studyFolderPath}]");

            var files = Directory.GetFiles(seriesFolderPath);

            for (var i = 0; i < files.Length; i++)
                File.Move(files[i], Path.Combine(folderPath, i.ToString("D3")));

            Directory.Delete(studyFolderPath, true);
        }

        private IDicomStudy GetCurrentDicomStudy(Recipe recipe, IEnumerable<IDicomStudy> allStudiesForPatient)
        {
            var currentDicomStudy =
                string.IsNullOrEmpty(recipe.CurrentAccession)
                ? FindStudyMatchingCriteria(allStudiesForPatient, recipe.CurrentSeriesCriteria, -1)
                : FindStudyMatchingAccession(allStudiesForPatient, recipe.CurrentAccession);

            currentDicomStudy = AddMatchingSeriesToStudy(
                currentDicomStudy, recipe.CurrentSeriesCriteria);

            return currentDicomStudy;
        }

        private IDicomStudy GetPriorDicomStudy(
                                               Recipe recipe, int studyFixedIndex,
                                               IEnumerable<IDicomStudy> allStudiesForPatient)
        {
            var floatingSeriesBundle =
                string.IsNullOrEmpty(recipe.PriorAccession)
                    ? FindStudyMatchingCriteria(allStudiesForPatient, recipe.PriorSeriesCriteria, studyFixedIndex)
                    : FindStudyMatchingAccession(allStudiesForPatient, recipe.PriorAccession);

            if (floatingSeriesBundle != null)
                floatingSeriesBundle = AddMatchingSeriesToStudy(floatingSeriesBundle, recipe.PriorSeriesCriteria);

            return floatingSeriesBundle;
        }

        /// <summary>
        /// In case patient Id is not available get patient Id using accession number
        /// </summary>
        /// <param name="recipe"></param>
        /// <param name="localNode"></param>
        /// <param name="sourceNode"></param>
        /// <returns></returns>
        private string GetPatientIdFromRecipe(IRecipe recipe)
        {
            if (!string.IsNullOrEmpty(recipe.PatientId)) return recipe.PatientId;
            if (!string.IsNullOrEmpty(recipe.PatientFullName)
                && !string.IsNullOrEmpty(recipe.PatientBirthDate))
                return _dicomSource.GetPatientIdFromPatientDetails(recipe.PatientFullName, recipe.PatientBirthDate).PatientId;

            if (string.IsNullOrEmpty(recipe.CurrentAccession))
                throw new NoNullAllowedException("Either patient details or study accession number should be defined!");

            try
            {
                var study = _dicomSource.GetStudyForAccession(recipe.CurrentAccession);
                return study.PatientId;
            }
            catch
            {
                _log.Error($"Failed to find accession {recipe.CurrentAccession} in source {recipe.SourceAet}");

                throw;
            }
        }

        /// <summary>
        /// Find all studies for patient first trying patient Id and if not provided using patient full name and birth date.
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="patientFullName"></param>
        /// <param name="patientBirthDate"></param>
        /// <param name="localNode">This machine dicome node details</param>
        /// <param name="sourceNode">Dicom archive where studies reside</param>
        /// <returns></returns>
        private IEnumerable<IDicomStudy> GetDicomStudiesForPatient(
            string patientId, string patientFullName, string patientBirthDate)
        {
            var patientIdIsProvided = !string.IsNullOrEmpty(patientId) && !string.IsNullOrWhiteSpace(patientId);

            return patientIdIsProvided ?
                _dicomSource.GetStudiesForPatientId(patientId) :
                _dicomSource.GetStudiesForPatient(patientFullName, patientBirthDate);
        }

        /// <summary>
        /// Checks each study against all 'Series Selection Criteria' and returns only one that matches it and throws exception if more than one study is returned
        /// </summary>
        /// <param name="studies">List of dicom Study details</param>
        /// <param name="criteria">Series Selection Criteria</param>
        /// <param name="referenceStudyIndex">In process of finding matching studies, this parameter is used to filter in studies done only before this index. 
        /// e.g. There is a list of studies and a only studies prior to study indexed 3 is desired. This parameter can be set to 3. 
        /// In order to filter all studies this parameter to be set to -1</param>
        /// <param name="localNode"></param>
        /// <param name="sourceNode"></param>
        /// <returns></returns>
        private IDicomStudy FindStudyMatchingCriteria(
                                                      IEnumerable<IDicomStudy> studies, IEnumerable<ISeriesSelectionCriteria> criteria,
                                                      int referenceStudyIndex)
        {
            var allStudies = studies as IList<IDicomStudy> ?? studies.ToList();
            var seriesSelectionCriteria = criteria as IList<ISeriesSelectionCriteria> ?? criteria.ToList();

            var studiesMatchingDateCriteria =
                    GetStudiesMatchingDateCriteria(allStudies, seriesSelectionCriteria, referenceStudyIndex);

            var studiesMatchingStudyDetails =
                    GetStudiesMatchingStudyDetails(studiesMatchingDateCriteria, seriesSelectionCriteria);

            var studiesContainingMatchingSeries =
                GetStudiesContainingMatchingSeries(studiesMatchingStudyDetails, seriesSelectionCriteria).ToList();

            var matchedStudies = GetStudiesMatchingOtherCriteria(studiesContainingMatchingSeries, seriesSelectionCriteria).ToList();

            return matchedStudies.FirstOrDefault();
        }

        /// <summary>
        /// Check all the rest of the criteria to find matching studies
        /// </summary>
        /// <param name="studies"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private static IEnumerable<IDicomStudy> GetStudiesMatchingOtherCriteria(
            IEnumerable<IDicomStudy> studies, IList<ISeriesSelectionCriteria> criteria)
        {
            var allEligibleStudies = studies as IDicomStudy[] ?? studies.ToArray();

            return allEligibleStudies.Where(study =>
            {
                return criteria.All(criterion =>
                {
                    // If MostRecentPriorStudy is true
                    if (criterion.MostRecentPriorStudy && criterion.OldestPriorStudy)
                        throw new Exception("MostRecentPriorStudy and OldestPriorStudy should not be concurrently set to True");

                    if (criterion.MostRecentPriorStudy)
                        return study.Equals(allEligibleStudies.FirstOrDefault());

                    if (criterion.OldestPriorStudy)
                        return study.Equals(allEligibleStudies.LastOrDefault());

                    var priorStudyIndex = criterion.PriorStudyIndex;

                    return study.Equals(allEligibleStudies[priorStudyIndex]);
                });
            });
        }

        /// <summary>
        /// Check series for studies and return ones which contain series matching criteria passed
        /// </summary>
        /// <param name="studies"></param>
        /// <param name="criteria"></param>
        /// <param name="localNode"></param>
        /// <param name="sourceNode"></param>
        /// <returns></returns>
        private IEnumerable<IDicomStudy> GetStudiesContainingMatchingSeries(
            IEnumerable<IDicomStudy> studies, IList<ISeriesSelectionCriteria> criteria)
        {
            return studies
                .Where(study =>
                {
                    return criteria.All(criterion =>
                    {
                        var seriesList = _dicomSource.GetSeriesForStudy(study.StudyInstanceUid);
                        var matchingSeries = seriesList.Where(series =>
                            _valueComparer.CompareStrings(
                                series.SeriesDescription, criterion.SeriesDescription,
                                criterion.SeriesDescriptionOperand, criterion.SeriesDescriptionDelimiter)
                            ).ToList();
                        if (!matchingSeries.Any()) return false;
                        if (matchingSeries.Count > 1) throw new Exception("More than one matching series were found");
                        return true;
                    });
                });
        }

        /// <summary>
        /// Find all studies that match the criteria in terms of study details e.g. Study Description
        /// </summary>
        /// <param name="studies"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private IEnumerable<IDicomStudy> GetStudiesMatchingStudyDetails(
            IEnumerable<IDicomStudy> studies, IEnumerable<ISeriesSelectionCriteria> criteria)
        {
            var studyDescCriteria = criteria.Where(c => !string.IsNullOrEmpty(c.StudyDescription)).ToArray();
            if (!studyDescCriteria.Any()) return studies;

            var matchedStudies = studies.Where(s =>
            {
                return studyDescCriteria.All(criterion =>
                    _valueComparer.CompareStrings(
                        s.StudyDescription, criterion.StudyDescription, criterion.StudyDescriptionOperand
                    ));
            }).ToList();

            if (!matchedStudies.Any()) throw new Exception("No study found for study criteria");

            return matchedStudies;
        }

        /// <summary>
        /// Gets studies that match date criteria. Only one study date criterion should be specified // TODO3: Enable specification of multiple date criteria
        /// </summary>
        /// <param name="allStudies"></param>
        /// <param name="seriesSelectionCriteria"></param>
        /// <param name="referenceStudyIndex"></param>
        /// <returns></returns>
        private static IEnumerable<IDicomStudy> GetStudiesMatchingDateCriteria(
            IEnumerable<IDicomStudy> allStudies,
            IEnumerable<ISeriesSelectionCriteria> seriesSelectionCriteria,
            int referenceStudyIndex)
        {
            var studies = allStudies as IDicomStudy[] ?? allStudies.ToArray();
            var criteria = seriesSelectionCriteria as ISeriesSelectionCriteria[] ?? seriesSelectionCriteria.ToArray();

            var matchedStudies = new List<IDicomStudy>();

            matchedStudies.AddRange(studies
                .Where(s =>
                {
                    // Filter out current study and more recent
                    if (Array.IndexOf(studies, s) <= referenceStudyIndex) return false;

                    // Filter out studies older than cut off period
                    var cutOffCriterion =
                        criteria.FirstOrDefault(c => !string.IsNullOrEmpty(c.CutOffPeriodValueInMonths));
                    if (cutOffCriterion != null)
                    {
                        var referenceDate = referenceStudyIndex == -1 ? DateTime.Now
                            : studies[referenceStudyIndex].StudyDate.GetValueOrDefault();

                        var cutOffDate = referenceDate
                            .AddMonths(-int.Parse(cutOffCriterion.CutOffPeriodValueInMonths));
                        if (DateTime.Compare(cutOffDate, s.StudyDate.GetValueOrDefault()) > 0)
                            return false;
                    }

                    // Filter out studies not matching study date
                    var studyDateCriterion =
                        criteria.FirstOrDefault(c => !string.IsNullOrEmpty(c.StudyDate));
                    if (studyDateCriterion == null) return true;
                    var criteriaStudyDate = DateTime.ParseExact(studyDateCriterion.StudyDate, "yyyyMMdd", new DateTimeFormatInfo());
                    return DateTime.Compare(s.StudyDate.GetValueOrDefault().Date, criteriaStudyDate.Date) == 0;
                }));

            return matchedStudies;
        }

        /// <summary>
        /// Finds only one study matching the accession number in list of studies passed to it. Throws exception in case no study is matched or more than one are matched.
        /// </summary>
        /// <param name="allStudies">All Studies passed to find one that matches accession number</param>
        /// <param name="accessionNumber">Accession number to check against each study accession number [Case insensitive]</param>
        /// <returns></returns>
        private IDicomStudy FindStudyMatchingAccession(IEnumerable<IDicomStudy> allStudies,
            string accessionNumber)
        {
            var studyMatchingAccession = allStudies.Where(s =>
                _valueComparer.CompareStrings(s.AccessionNumber, accessionNumber, StringOperand.Equals));

            var matchingAccession = studyMatchingAccession as IList<IDicomStudy> ?? studyMatchingAccession.ToList();

            if (!matchingAccession.Any()) throw new Exception($"No study found for accession: {accessionNumber}");
            if (matchingAccession.Count > 1) throw new Exception(
                $"Only one study should match accession number. {matchingAccession.Count} " +
                $"studies found for accession: {accessionNumber}");

            return matchingAccession.FirstOrDefault();
        }

        /// <summary>
        /// Find study series (only one) that match the selection critera and add details to the study
        /// </summary>
        /// <param name="study"></param>
        /// <param name="criteria"></param>
        /// <param name="localNode"></param>
        /// <param name="sourceNode"></param>
        /// <returns></returns>
        private IDicomStudy AddMatchingSeriesToStudy(
            IDicomStudy study, IEnumerable<ISeriesSelectionCriteria> criteria)
        {
            var allSeries = _dicomSource.GetSeriesForStudy(study.StudyInstanceUid);
            var criterion = criteria.FirstOrDefault(c => !string.IsNullOrEmpty(c.SeriesDescription));

            if (criterion == null)
                study.Series.ToList().AddRange(allSeries);
            else
            {
                var matchedSeries = allSeries.FirstOrDefault(series =>
                    _valueComparer.CompareStrings(
                        series.SeriesDescription, criterion.SeriesDescription,
                        criterion.SeriesDescriptionOperand, criterion.SeriesDescriptionDelimiter));

                if (matchedSeries != null) study.Series.Add(matchedSeries);
            }

            return study;
        }
    }
}