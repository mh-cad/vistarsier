using CAPI.Dicom.Abstraction;
using CAPI.JobManager.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.JobManager
{
    public class JobBuilder : IJobBuilder
    {
        private readonly IDicomServices _dicomServices;
        private readonly IJobManagerFactory _jobManagerFactory;
        private readonly IValueComparer _valueComparer;

        public JobBuilder(IDicomServices dicomServices, IJobManagerFactory jobManagerFactory, IValueComparer valueComparer)
        {
            _dicomServices = dicomServices;
            _jobManagerFactory = jobManagerFactory;
            _valueComparer = valueComparer;
        }

        public IJob<IRecipe> Build(IRecipe recipe, IDicomNode localNode, IDicomNode sourceNode)
        {
            _dicomServices.CheckRemoteNodeAvailability(localNode, sourceNode);

            var allStudiesForPatient = GetDicomStudiesForPatient(recipe.PatientId,
                recipe.PatientFullName, recipe.PatientBirthDate, localNode, sourceNode)
                    .OrderByDescending(s => s.StudyDate).ToList();

            if (allStudiesForPatient.Count < 1)
                throw new ArgumentOutOfRangeException($"No studies could be found in AET [{sourceNode.AeTitle}]");

            var studyFixed = FindStudyMatchingCriteria(
                allStudiesForPatient, recipe.NewStudyCriteria, -1);
            AddMatchingSeriesToStudy(studyFixed, recipe.NewStudyCriteria, localNode, sourceNode);
            var studyFixedIndex = allStudiesForPatient.IndexOf(studyFixed);

            var studyFloating = FindStudyMatchingCriteria(
                allStudiesForPatient, recipe.PriorStudyCriteria, studyFixedIndex);
            studyFloating = AddMatchingSeriesToStudy(
                studyFloating, recipe.PriorStudyCriteria, localNode, sourceNode);

            var job = _jobManagerFactory.CreateJob(
                studyFixed,
                studyFloating,
                recipe.IntegratedProcesses,
                recipe.Destinations
            );
            return job;
        }

        private IEnumerable<IDicomStudy> GetDicomStudiesForPatient(
            string patientId, string patientFullName, string patientBirthDate,
            IDicomNode localNode, IDicomNode sourceNode)
        {
            var patientIdIsProvided = !string.IsNullOrEmpty(patientId) && !string.IsNullOrWhiteSpace(patientId);

            return patientIdIsProvided ?
                _dicomServices.GetStudiesForPatientId(patientId, localNode, sourceNode) :
                _dicomServices.GetStudiesForPatient(patientFullName, patientBirthDate, localNode, sourceNode);
        }

        private IDicomStudy FindStudyMatchingCriteria(
            IEnumerable<IDicomStudy> studies,
            IEnumerable<ISeriesSelectionCriteria> criteria,
            int referenceStudy)
        {
            var allStudies = studies as IList<IDicomStudy> ?? studies.ToList();
            var matchedStudies = allStudies
                .Where(study => criteria
                    .All(criterion =>
                    {
                        var accessionIsMatched = CheckAccessionNumber(criterion.AccessionNumber, study.AccessionNumber);
                        if (accessionIsMatched) return true;

                        // Accession Number is NOT specified => Check if Study Description matches criterion:
                        var studyDescriptionIsSpecified = !string.IsNullOrEmpty(criterion.StudyDescription);
                        var studyDescriptionMatches = _valueComparer.CompareStrings(
                            study.StudyDescription, criterion.StudyDescription, criterion.StudyDescriptionOperand);
                        if (studyDescriptionIsSpecified && !studyDescriptionMatches)
                            return false;

                        // Accession Number is NOT specified + Study Description matches criterion => Check if Prior Study is requested:
                        var thisIsPriorStudy = referenceStudy + 1 == allStudies.IndexOf(study);
                        if (criterion.PriorStudy && thisIsPriorStudy) return true;

                        // Accession Number is NOT specified + Study Description matches criterion AND Prior Study is NOT requested
                        // => Check if Study Date matches criterion:
                        //var studyDateIsSpecified = !string.IsNullOrEmpty(criterion.StudyDate);
                        //var studyDateMatches = _valueComparer.CompareDates(
                        //    criterion.StudyDate, study.StudyDate.GetValueOrDefault(), criterion.StudyDateOperand);
                        //if (studyDateIsSpecified && studyDateMatches) return false;

                        return false;
                    }))
                .ToList();

            if (matchedStudies == null || matchedStudies.Count != 1)
                throw new ArgumentOutOfRangeException(
                    "Study selection criteria should match one and only one study" +
                    $" -> Matched Studies: {matchedStudies.Count}");

            return matchedStudies.FirstOrDefault();
        }

        private static bool CheckAccessionNumber(string criterionAccessionNumber, string studyAccessionNumber)
        {
            return !string.IsNullOrEmpty(criterionAccessionNumber)
                && string.Equals(studyAccessionNumber, criterionAccessionNumber,
                    StringComparison.CurrentCultureIgnoreCase);
        }

        private IDicomStudy AddMatchingSeriesToStudy(IDicomStudy study,
            IEnumerable<ISeriesSelectionCriteria> criteria, IDicomNode localNode, IDicomNode sourceNode)
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