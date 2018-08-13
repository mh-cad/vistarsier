﻿using CAPI.Agent.Abstractions.Models;
using CAPI.Agent.Models;
using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CAPI.Agent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class JobBuilder //: IJobBuilder
    {
        private const string Current = "Current";
        private const string Prior = "Prior";
        private const string Dicom = "Dicom";
        private const string Results = "Results";
        private const string PriorResliced = "PriorResliced";

        private readonly IDicomServices _dicomServices;
        private readonly IImageProcessingFactory _imgProcFactory;
        private readonly IValueComparer _valueComparer;
        private readonly IFileSystem _fileSystem;
        private readonly IProcessBuilder _processBuilder;
        private readonly ICapiConfig _capiConfig;
        private readonly ILog _log;

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
        public JobBuilder(IDicomServices dicomServices,
                          IImageProcessingFactory imgProcFactory, IValueComparer valueComparer,
                          IFileSystem fileSystem, IProcessBuilder processBuilder,
                          ICapiConfig capiConfig, ILog log)
        {
            _dicomServices = dicomServices;
            _imgProcFactory = imgProcFactory;
            _valueComparer = valueComparer;
            _fileSystem = fileSystem;
            _processBuilder = processBuilder;
            _capiConfig = capiConfig;
            _log = log;
        }

        /// <summary>
        /// Builds a Job using details in Recipe
        /// </summary>
        /// <param name="recipe">Contains details of studies to process, source and destinations</param>
        /// <returns></returns>
        public IJob Build(Recipe recipe)
        {
            var localNode = _capiConfig.DicomConfig.LocalNode;
            var sourceNode = _capiConfig.DicomConfig.RemoteNodes
                .Single(n => n.AeTitle.Equals(recipe.SourceAet, StringComparison.InvariantCultureIgnoreCase));

            _dicomServices.CheckRemoteNodeAvailability(localNode, sourceNode);

            var patientId = GetPatientIdFromRecipe(recipe, localNode, sourceNode);

            var allStudiesForPatient = GetDicomStudiesForPatient(patientId,
                recipe.PatientFullName, recipe.PatientBirthDate, localNode, sourceNode)
                    .OrderByDescending(s => s.StudyDate).ToList();

            if (allStudiesForPatient.Count < 1)
                throw new Exception(
                    $"No studies for patient [{recipe.PatientFullName}] could be found in node AET: [{sourceNode.AeTitle}]");

            // Get Current Study (Fixed)
            var currentDicomStudy = GetCurrentDicomStudy(recipe, localNode, sourceNode, allStudiesForPatient);
            if (currentDicomStudy == null ||
                currentDicomStudy.Series.Count == 0)
                throw new Exception("No workable series were found for accession");

            var job = new Job(recipe,
                              _dicomServices, _imgProcFactory,
                              _fileSystem, _processBuilder, _capiConfig, _log);
            var imageRepositoryPath = _capiConfig.ImgProcConfig.ImageRepositoryPath;
            var jobFolderName = $"{recipe.PatientFullName}-{DateTime.Now:yyyy-MM-dd_HHmmssfff}";
            job.CurrentSeriesDicomFolder = SaveDicomFilesToFilesystem(
                currentDicomStudy, imageRepositoryPath, jobFolderName, Current, localNode, sourceNode);

            var studyFixedIndex = allStudiesForPatient.IndexOf(currentDicomStudy);

            // Get Prior Study (Floating)
            var priorDicomStudy =
                GetPriorDicomStudy(recipe, studyFixedIndex, localNode, sourceNode, allStudiesForPatient);

            if (priorDicomStudy == null)
                throw new Exception("No prior workable series were found");

            job.PriorSeriesDicomFolder = SaveDicomFilesToFilesystem(
                currentDicomStudy, imageRepositoryPath, recipe.PatientFullName, Prior, localNode, sourceNode);

            job.ResultSeriesDicomFolder = Path.Combine(imageRepositoryPath, jobFolderName, Results);
            job.PriorSeriesDicomFolder = Path.Combine(imageRepositoryPath, jobFolderName, PriorResliced);

            return job;
        }

        private string SaveDicomFilesToFilesystem(
                                                  IDicomStudy dicomStudy, string imageRepositoryPath,
                                                  string jobFolderName, string studyName,
                                                  IDicomNode locaNode, IDicomNode sourceNode)
        {
            var series = dicomStudy.Series.FirstOrDefault();

            var folderPath = Path.Combine(imageRepositoryPath, jobFolderName, studyName, Dicom);

            _dicomServices.SaveSeriesToLocalDisk(series, folderPath, locaNode, sourceNode);

            return folderPath;
        }

        private IDicomStudy GetCurrentDicomStudy(
                                                 Recipe recipe, IDicomNode localNode, IDicomNode sourceNode,
                                                 IEnumerable<IDicomStudy> allStudiesForPatient)
        {
            var currentDicomStudy =
                string.IsNullOrEmpty(recipe.CurrentAccession)
                ? FindStudyMatchingCriteria(allStudiesForPatient, recipe.CurrentSeriesCriteria, -1, localNode, sourceNode)
                : FindStudyMatchingAccession(allStudiesForPatient, recipe.CurrentAccession);

            currentDicomStudy = AddMatchingSeriesToStudy(
                currentDicomStudy, recipe.CurrentSeriesCriteria, localNode, sourceNode);

            return currentDicomStudy;
        }

        private IDicomStudy GetPriorDicomStudy(
                                               Recipe recipe, int studyFixedIndex,
                                               IDicomNode localNode, IDicomNode sourceNode,
                                               IEnumerable<IDicomStudy> allStudiesForPatient)
        {
            var floatingSeriesBundle =
                string.IsNullOrEmpty(recipe.PriorAccession)
                    ? FindStudyMatchingCriteria(allStudiesForPatient, recipe.PriorSeriesCriteria,
                        studyFixedIndex, localNode, sourceNode)
                    : FindStudyMatchingAccession(allStudiesForPatient, recipe.PriorAccession);

            if (floatingSeriesBundle != null)
                floatingSeriesBundle = AddMatchingSeriesToStudy(
                    floatingSeriesBundle, recipe.PriorSeriesCriteria, localNode, sourceNode);

            return floatingSeriesBundle;
        }

        /// <summary>
        /// In case patient Id is not available get patient Id using accession number
        /// </summary>
        /// <param name="recipe"></param>
        /// <param name="localNode"></param>
        /// <param name="sourceNode"></param>
        /// <returns></returns>
        private string GetPatientIdFromRecipe(IRecipe recipe, IDicomNode localNode, IDicomNode sourceNode)
        {
            if (!string.IsNullOrEmpty(recipe.PatientId)) return recipe.PatientId;
            if (!string.IsNullOrEmpty(recipe.PatientFullName)
                && !string.IsNullOrEmpty(recipe.PatientBirthDate))
                return _dicomServices.GetPatientIdFromPatientDetails(recipe.PatientFullName, recipe.PatientBirthDate,
                    localNode, sourceNode).PatientId;

            if (string.IsNullOrEmpty(recipe.CurrentAccession))
                throw new NoNullAllowedException("Either patient details or study accession number should be defined!");

            try
            {
                var study = _dicomServices.GetStudyForAccession(recipe.CurrentAccession, localNode, sourceNode);
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
                                                                   string patientId, string patientFullName, string patientBirthDate,
                                                                   IDicomNode localNode, IDicomNode sourceNode)
        {
            var patientIdIsProvided = !string.IsNullOrEmpty(patientId) && !string.IsNullOrWhiteSpace(patientId);

            return patientIdIsProvided ?
                _dicomServices.GetStudiesForPatientId(patientId, localNode, sourceNode) :
                _dicomServices.GetStudiesForPatient(patientFullName, patientBirthDate, localNode, sourceNode);
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
                                                      int referenceStudyIndex, IDicomNode localNode, IDicomNode sourceNode)
        {
            var allStudies = studies as IList<IDicomStudy> ?? studies.ToList();
            var seriesSelectionCriteria = criteria as IList<ISeriesSelectionCriteria> ?? criteria.ToList();

            var studiesMatchingDateCriteria =
                    GetStudiesMatchingDateCriteria(allStudies, seriesSelectionCriteria, referenceStudyIndex);

            var studiesMatchingStudyDetails =
                    GetStudiesMatchingStudyDetails(studiesMatchingDateCriteria, seriesSelectionCriteria);

            var studiesContainingMatchingSeries =
                GetStudiesContainingMatchingSeries(studiesMatchingStudyDetails, seriesSelectionCriteria,
                localNode, sourceNode).ToList();

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
            var allStudies = studies as IDicomStudy[] ?? studies.ToArray();

            return allStudies.Where(study =>
            {
                return criteria.All(criterion =>
                {
                    // If MostRecentPriorStudy is true
                    if (criterion.MostRecentPriorStudy && criterion.OldestPriorStudy)
                        throw new Exception("MostRecentPriorStudy and OldestPriorStudy should not be concurrently set to True");

                    if (criterion.MostRecentPriorStudy)
                        return study.Equals(allStudies.FirstOrDefault());

                    // If OldestPriorStudy is true
                    return criterion.OldestPriorStudy && study.Equals(allStudies.LastOrDefault());
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
            IEnumerable<IDicomStudy> studies, IList<ISeriesSelectionCriteria> criteria,
            IDicomNode localNode, IDicomNode sourceNode)
        {
            return studies
                .Where(study =>
                {
                    return criteria.All(criterion =>
                    {
                        var seriesList = _dicomServices.GetSeriesForStudy(study.StudyInstanceUid,
                            localNode, sourceNode);
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
            IDicomStudy study, IEnumerable<ISeriesSelectionCriteria> criteria,
            IDicomNode localNode, IDicomNode sourceNode)
        {
            var allSeries = _dicomServices.GetSeriesForStudy(study.StudyInstanceUid, localNode, sourceNode);
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