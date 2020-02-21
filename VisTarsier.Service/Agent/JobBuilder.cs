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
using Newtonsoft.Json;
using VisTarsier.Config;

namespace VisTarsier.Service
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class JobBuilder
    {
        public class StudyNotFoundException : Exception
        {
            public StudyNotFoundException(string message) : base(message)
            {
            }
        }

        private const string Current = "Current";
        private const string Prior = "Prior";
        private const string Reference = "Reference";
        private const string Dicom = "Dicom";
        private const string Results = "Results";
        private const string PriorResliced = "PriorResliced";

        private readonly IValueComparer _valueComparer;
        private readonly ILog _log;
        private readonly DbBroker _context;

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
        }

        /// <summary>
        /// Builds a Job using details in Recipe
        /// </summary>
        /// <param name="recipe">Contains details of studies to process, source and destinations</param>
        /// <returns></returns>
        public Job Build(Recipe recipe, Attempt attempt)
        {
            if (string.IsNullOrEmpty(recipe.CurrentAccession)) recipe.CurrentAccession = attempt.CurrentAccession;
            // TODO: We probably don't need to replace the prior accession since the attempt shouldn't have anything at this stage.
            if (string.IsNullOrEmpty(recipe.PriorAccession)) recipe.PriorAccession = attempt.PriorAccession;
            
            // Setup out dicom service.
            var dicomSource = CreateDicomSource(recipe.SourceAet);

            // Get patient id
            var patientId = GetPatientId(recipe, dicomSource);

            // A list of all studies for the patient.
            // TODO: I don't love the fact that we can't just use the patient ID which we should have above.
            // But this code is defensive so I don't want to change it and introduce more bugs.
            var allStudiesForPatient =
                GetStudiesForPatient(patientId, recipe.PatientFullName, recipe.PatientBirthDate, dicomSource);
                        
            // Update attempt in DB
            @attempt.PatientId = recipe.PatientId;
            _context.Attempts.Update(@attempt);
            _context.SaveChanges();

            // If there are no studies, have a cry.
            if (allStudiesForPatient.Count < 1)
                throw new StudyNotFoundException(
                    $"No studies for patient [{recipe.PatientFullName}] could be found in node AET: [{dicomSource.RemoteNode.AeTitle}]");

            // Update patient ID in DB for a second time?
            @attempt.PatientId = allStudiesForPatient.FirstOrDefault().PatientId;
            @attempt.PatientFullName = allStudiesForPatient.FirstOrDefault().PatientsName;
            @attempt.PatientBirthDate = allStudiesForPatient.FirstOrDefault().PatientBirthDate.ToString("yyyyMMdd");
            _context.Attempts.Update(@attempt);
            _context.SaveChanges();

            // Update the recipe with all the details for the patient?
            recipe = UpdateRecipeWithPatientDetails(recipe, allStudiesForPatient);

            // Find Current Study
            _log.Info("Finding current series using recipe provided...");
            var currentDicomStudy = GetCurrentDicomStudy(recipe, allStudiesForPatient, dicomSource);

            // Update case in DB with patient name and DOB
            @attempt.PatientFullName = recipe.PatientFullName;
            @attempt.PatientBirthDate = recipe.PatientBirthDate;
            _context.Attempts.Update(@attempt);
            _context.SaveChanges();

            // If we don't have a matching study for current have a cry.
            if (currentDicomStudy == null ||
                currentDicomStudy.Series.Count == 0)
                throw new StudyNotFoundException("No workable series were found for accession");

            // Update attempt with accession number.
            @attempt.CurrentAccession = currentDicomStudy.AccessionNumber;
            @attempt.CurrentSeriesUID = currentDicomStudy.Series.FirstOrDefault().SeriesInstanceUid;
            _context.Attempts.Update(@attempt);
            _context.SaveChanges();

            // Create a new job based on the recipe.
            var job = new Job(recipe, attempt);
            job.Status = "Pending";
            job.AttemptId = attempt.Id;

            // Setup temp directory for images.
            var capiConfig = CapiConfig.GetConfig();
            try
            {
                job.ProcessingFolder = Path.GetFullPath(Path.Combine(capiConfig.ImagePaths.ImageRepositoryPath, job.Attempt.CurrentAccession + "-" + job.Id));
            }
            catch (Exception ex)
            {
                Log.GetLogger().Error($"{capiConfig.ImagePaths.ImageRepositoryPath} may not be a valid path.");
                Log.GetLogger().Error(ex.Message);
            }
            try
            { 
               job.ResultSeriesDicomFolder = Path.GetFullPath(Path.Combine(job.ProcessingFolder, Results));
            }
            catch (Exception ex)
            {
                Log.GetLogger().Error($"{Path.Combine(job.ProcessingFolder, Results)} may not be a valid path.");
                Log.GetLogger().Error(ex.Message);
            }
            try
            {
                job.PriorReslicedSeriesDicomFolder = Path.GetFullPath(Path.Combine(job.ProcessingFolder, PriorResliced));
            }
            catch (Exception ex)
            {
                Log.GetLogger().Error($"{Path.Combine(job.ProcessingFolder, PriorResliced)} may not be a valid path.");
                Log.GetLogger().Error(ex.Message);
            }
            try
            { 
                job.ReferenceSeriesDicomFolder = Path.GetFullPath(GetReferenceSeriesForRegistration(job, allStudiesForPatient, dicomSource));
            }
            catch (Exception ex)
            {
                Log.GetLogger().Info($"{GetReferenceSeriesForRegistration(job, allStudiesForPatient, dicomSource)} may not be a valid path.");
            }

            // Find Prior Study (Floating)
            _log.Info("Finding prior series using recipe provided...");
            var studyFixedIndex = allStudiesForPatient.IndexOf(currentDicomStudy);
            var priorDicomStudy =
                GetPriorDicomStudy(recipe, studyFixedIndex, allStudiesForPatient, dicomSource);
            // If we couldn't find the prior study, have a cry.
            if (priorDicomStudy == null)
                throw new StudyNotFoundException("No prior workable series were found");
            // Otherwise update the attempt
            job.Attempt.PriorAccession = priorDicomStudy.AccessionNumber;
            job.Attempt.PatientId = currentDicomStudy.PatientId;

            // Update attempt with prior accession details.
            @attempt.PriorAccession = priorDicomStudy?.AccessionNumber;
            @attempt.PriorSeriesUID = priorDicomStudy?.Series?.FirstOrDefault()?.SeriesInstanceUid;
            @attempt.SourceAet = recipe.SourceAet;
            @attempt.DestinationAet = JsonConvert.SerializeObject(recipe.OutputSettings.DicomDestinations);
            @job.RecipeString = JsonConvert.SerializeObject(job.Recipe, new CapiConfigJsonConverter());
            _context.Attempts.Update(@attempt);
            _context.Jobs.Update(@job);
            _context.SaveChanges();

            // If both current and prior are found, save them to disk for processing
            _log.Info("Saving current series to disk...");
            try
            {
                job.CurrentSeriesDicomFolder =
                    SaveDicomFilesToFilesystem(currentDicomStudy, job.ProcessingFolder, Current, dicomSource);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to save current series dicom files to disk.", ex);
                throw ex;
            }
            
            if (Directory.EnumerateFiles(job.CurrentSeriesDicomFolder).Count() < 1)
            {
                _log.Error($"Could not find any dicom files for current series in directory {job.CurrentSeriesDicomFolder}. Check that Accession exists and you can perform a CMOVE to this machine.");
                throw new Exception($"Could not find any dicom files for current series in directory {job.CurrentSeriesDicomFolder}. Check that Accession exists and you can perform a CMOVE to this machine.");
            }

            _log.Info($"Saved current series to [{job.CurrentSeriesDicomFolder}]");

            _log.Info("Saving prior series to disk...");
            try
            {
                job.PriorSeriesDicomFolder = SaveDicomFilesToFilesystem(
                        priorDicomStudy, job.ProcessingFolder, Prior, dicomSource);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to save prior series dicom files to disk.", ex);
                throw;
            }

            if (Directory.EnumerateFiles(job.PriorSeriesDicomFolder).Count() < 1)
            {
                _log.Error($"Could not find any dicom files for prior series in directory {job.PriorSeriesDicomFolder}. Check that Accession exists and you can perform a CMOVE to this machine.");
                throw new Exception($"Could not find any dicom files for prior series in directory {job.PriorSeriesDicomFolder}. Check that Accession exists and you can perform a CMOVE to this machine.");
            }

            _log.Info($"Saved prior series to [{job.PriorReslicedSeriesDicomFolder}]");

            // All done.
            return job;
        }

        /// <summary>
        /// Creates a DicomService object given a source AET (which should be in the config file)
        /// </summary>
        /// <param name="sourceAet"></param>
        /// <returns></returns>
        private DicomService CreateDicomSource(string sourceAet)
        {
            var capiConfig = CapiConfig.GetConfig();
            var localNode = capiConfig.DicomConfig.LocalNode;
            var remoteNodes = capiConfig.DicomConfig.RemoteNodes;
            IDicomNode sourceNode;

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
                var dicomSource = new DicomService(localNode, sourceNode);
                dicomSource.CheckRemoteNodeAvailability();
                return dicomSource;
            }
            catch (Exception ex)
            {
                _log.Error($"Local Node AET [{localNode.AeTitle}] cannot reach" +
                           $"Source Node AET [{sourceAet}]", ex);
                throw;
            }
        }

        /// <summary>
        /// In case patient Id is not available get patient Id using accession number
        /// </summary>
        /// <param name="recipe"></param>
        /// <param name="localNode"></param>
        /// <param name="sourceNode"></param>
        /// <returns></returns>
        private string GetPatientId(Recipe recipe, IDicomService dicomSource)
        {
            if (!string.IsNullOrEmpty(recipe.PatientId)) return recipe.PatientId;
            if (!string.IsNullOrEmpty(recipe.PatientFullName)
                && !string.IsNullOrEmpty(recipe.PatientBirthDate))
                return dicomSource.GetPatientIdFromPatientDetails(recipe.PatientFullName, recipe.PatientBirthDate).PatientId;

            if (string.IsNullOrEmpty(recipe.CurrentAccession))
                throw new NoNullAllowedException("Either patient details or study accession number should be defined!");

            try
            {
                var study = dicomSource.GetStudyForAccession(recipe.CurrentAccession);
                return study.PatientId;
            }
            catch
            {
                _log.Error($"Failed to find accession {recipe.CurrentAccession} in source {recipe.SourceAet}");

                throw new StudyNotFoundException($"Failed to find accession {recipe.CurrentAccession} in source {recipe.SourceAet}");
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
        private List<IDicomStudy> GetStudiesForPatient(
            string patientId, string patientFullName, string patientBirthDate, IDicomService dicomSource)
        {
            var patientIdIsProvided = !string.IsNullOrEmpty(patientId) && !string.IsNullOrWhiteSpace(patientId);

            var result = patientIdIsProvided ?
                dicomSource.GetStudiesForPatientId(patientId) :
                dicomSource.GetStudiesForPatient(patientFullName, patientBirthDate);

            return result.OrderByDescending(s => s.StudyDate).ToList();
        }

        private static Recipe UpdateRecipeWithPatientDetails(Recipe recipe, IReadOnlyCollection<IDicomStudy> studies)
        {
            recipe.PatientFullName = studies.FirstOrDefault()?.PatientsName;
            recipe.PatientBirthDate = studies.FirstOrDefault()?.PatientBirthDate.ToString("yyyyMMdd");
            return recipe;
        }


        private string GetReferenceSeriesForRegistration(Job job, IEnumerable<IDicomStudy> allStudiesForPatient, DicomService dicomSource)
        {
            var studiesForPatient = allStudiesForPatient.ToList();

            if (string.IsNullOrEmpty(job.Attempt.ReferenceSeries))
                job.Attempt.ReferenceSeries = FindReferenceSeriesInPreviousJobs(job.Attempt.PatientId);

            if (string.IsNullOrEmpty(job.Attempt.ReferenceSeries)) return string.Empty;

            var studyId = job.GetStudyIdFromReferenceSeries();
            var seriesId = job.GetSeriesIdFromReferenceSeries();
            var study = studiesForPatient.FirstOrDefault(s => s.StudyInstanceUid == studyId);
            if (study == null)
            {
                _log.Error($"Failed to find reference study to register series against StudyInstanceUid: [{studyId}]");
                return string.Empty;
            }
            var allSeries = dicomSource.GetSeriesForStudy(study.StudyInstanceUid);
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
                referenceFolderPath = SaveDicomFilesToFilesystem(study, job.ProcessingFolder, Reference, dicomSource);
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
            var jobWithRefSeries = _context.Jobs.AsEnumerable().LastOrDefault(j => j.Attempt != null && j.Attempt.PatientId == patientId &&
                                                                    !string.IsNullOrEmpty(j.Attempt.ReferenceSeries));

            return jobWithRefSeries != null && jobWithRefSeries.Attempt != null ? jobWithRefSeries.Attempt.ReferenceSeries : string.Empty;
        }


        private string SaveDicomFilesToFilesystem(
            IDicomStudy dicomStudy, string jobProcessingFolder, string studyName, DicomService dicomSource)
        {
            var series = dicomStudy.Series.FirstOrDefault();

            var folderPath = Path.GetFullPath(Path.Combine(jobProcessingFolder, studyName, Dicom));

            dicomSource.SaveSeriesToLocalDisk(series, folderPath);

            CopyDicomFilesToRootFolder(folderPath);

            return Path.GetFullPath(folderPath);
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
                File.Move(files[i], Path.GetFullPath(Path.Combine(folderPath, i.ToString("D3"))));

            Directory.Delete(studyFolderPath, true);
        }

        private IDicomStudy GetCurrentDicomStudy(Recipe recipe, List<IDicomStudy> allStudiesForPatient, DicomService dicomSource)
        {
            _log.Info($"ssc.c: {recipe.CurrentSeriesCriteria.Count()}");
            var currentDicomStudy =
                string.IsNullOrEmpty(recipe.CurrentAccession)
                ? FindStudyMatchingCriteria(allStudiesForPatient, recipe.CurrentSeriesCriteria, -1, dicomSource)
                : FindStudyMatchingAccession(allStudiesForPatient, recipe.CurrentSeriesCriteria, recipe.CurrentAccession);

            currentDicomStudy = AddMatchingSeriesToStudy(currentDicomStudy, recipe.CurrentSeriesCriteria, dicomSource);

            return currentDicomStudy;
        }

        private IDicomStudy GetPriorDicomStudy(
            Recipe recipe, int studyFixedIndex, IEnumerable<IDicomStudy> allStudiesForPatient, DicomService dicomSource)
        {
            var floatingSeriesBundle =
                string.IsNullOrEmpty(recipe.PriorAccession)
                    ? FindStudyMatchingCriteria(allStudiesForPatient, recipe.PriorSeriesCriteria, studyFixedIndex, dicomSource)
                    : FindStudyMatchingAccession(allStudiesForPatient, recipe.PriorSeriesCriteria, recipe.PriorAccession);

            if (floatingSeriesBundle != null)
                floatingSeriesBundle = AddMatchingSeriesToStudy(floatingSeriesBundle, recipe.PriorSeriesCriteria, dicomSource);

            return floatingSeriesBundle;
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
            IEnumerable<IDicomStudy> allStudies,
            IEnumerable<SeriesSelectionCriteria> seriesSelectionCriteria,
            int referenceStudyIndex,
            DicomService dicomSource)
        {
            _log.Info($"ssc.c: {seriesSelectionCriteria.Count()}");
            var studiesMatchingDateCriteria = GetStudiesMatchingDateCriteria(allStudies, seriesSelectionCriteria, referenceStudyIndex);
            _log.Info($"ssc.c: {seriesSelectionCriteria.Count()}");
            var studiesMatchingStudyDetails = GetStudiesMatchingStudyDetails(studiesMatchingDateCriteria, seriesSelectionCriteria);
            _log.Info($"ssc.c: {seriesSelectionCriteria.Count()}");
            var studiesContainingMatchingSeries = GetStudiesContainingMatchingSeries(studiesMatchingStudyDetails, seriesSelectionCriteria, dicomSource);

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
            IEnumerable<IDicomStudy> studies, IEnumerable<SeriesSelectionCriteria> criteria)
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
        private List<IDicomStudy> GetStudiesContainingMatchingSeries(
            IEnumerable<IDicomStudy> studies, IEnumerable<SeriesSelectionCriteria> criteria, IDicomService dicomSource)
        {
            // A list of studies where...
            return studies
                .Where(study =>
                {
                    // The study matches true to all criteria...
                    return criteria.All(criterion =>
                    {
                        var seriesList = dicomSource.GetSeriesForStudy(study.StudyInstanceUid);
                        var matchingSeries = seriesList.Where(series =>
                            _valueComparer.CompareStrings(
                                series.SeriesDescription, criterion.SeriesDescription,
                                criterion.SeriesDescriptionOperand, criterion.SeriesDescriptionDelimiter)
                            ).ToList();
                        if (!matchingSeries.Any()) return false;
                        // Not sure why this is a problem. We may need to determine "best match" at some point though.
                        //if (matchingSeries.Count > 1)
                        //{
                        //    throw new Exception("More than one matching series were found");
                        //}
                        return true;
                    });
                }).ToList();
        }

        /// <summary>
        /// Find all studies that match the criteria in terms of study details e.g. Study Description
        /// </summary>
        /// <param name="studies"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private IEnumerable<IDicomStudy> GetStudiesMatchingStudyDetails(
            IEnumerable<IDicomStudy> studies, IEnumerable<SeriesSelectionCriteria> criteria)
        {
            _log.Info($"Matching study details.");
            _log.Info($"First criteria desc: { criteria.FirstOrDefault().StudyDescription}");
            
            // Check if the study description field is used anywhere...
            var studyDescCriteria = criteria.Where(c => !string.IsNullOrEmpty(c.StudyDescription)).ToArray();
            // If not then we're good to return the whole set.
            _log.Info($"We found {studyDescCriteria?.Count()} criteria: { studyDescCriteria?.FirstOrDefault()?.StudyDescription}");

            if (!studyDescCriteria.Any()) return studies;

            // Othewise we want to select the studies which match (any of?) the criteria.
            var matchedStudies = studies.Where(s =>
            {
                return studyDescCriteria.All(criterion =>
                    _valueComparer.CompareStrings(
                        s.StudyDescription, criterion.StudyDescription, criterion.StudyDescriptionOperand
                    ));
            }).ToList();

            _log.Info($"Matched {matchedStudies.Count} studies.");

            if (!matchedStudies.Any()) throw new StudyNotFoundException("No study found for study criteria");

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
            IEnumerable<SeriesSelectionCriteria> seriesSelectionCriteria,
            int referenceStudyIndex)
        {
            var studies = allStudies as IDicomStudy[] ?? allStudies.ToArray();
            var criteria = seriesSelectionCriteria as SeriesSelectionCriteria[] ?? seriesSelectionCriteria.ToArray();

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
        /// <param name="accessionNumber">Accession number to check against each study accession number [Attempt insensitive]</param>
        /// <returns></returns>
        private IDicomStudy FindStudyMatchingAccession(
            IEnumerable<IDicomStudy> allStudies,
            IEnumerable<SeriesSelectionCriteria> seriesSelectionCriteria,
            string accessionNumber)
        {
            var studyMatchingAccession = allStudies.Where(s =>
                _valueComparer.CompareStrings(s.AccessionNumber, accessionNumber, StringOperand.Equals));

            // Match to accession
            var matchingAccession = studyMatchingAccession as IList<IDicomStudy> ?? studyMatchingAccession.ToList();
            // We also want to match to the criteria.
            var studiesMatchingStudyDetails = GetStudiesMatchingStudyDetails(matchingAccession, seriesSelectionCriteria);
            var matchedStudies = GetStudiesMatchingOtherCriteria(studiesMatchingStudyDetails, seriesSelectionCriteria).ToList();

            // Handle "error" cases.
            if (!matchingAccession.Any()) throw new StudyNotFoundException($"No study found for accession: {accessionNumber}");
            if (!matchedStudies.Any()) throw new StudyNotFoundException($"No study found for accession: {accessionNumber} which matches the recipe.");
            if (matchedStudies.Count > 1) throw new StudyNotFoundException(
                $"Only one study should match accession number. {matchedStudies.Count} " +
                $"studies found for accession: {accessionNumber}");

            // Return the matched study.
            return matchedStudies.FirstOrDefault();
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
            IDicomStudy study, IEnumerable<SeriesSelectionCriteria> criteria, DicomService dicomSource)
        {
            var allSeries = dicomSource.GetSeriesForStudy(study.StudyInstanceUid);
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