using VisTarsier.Config;
using VisTarsier.Common;
using log4net;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VisTarsier.Dicom;
using System.ComponentModel.DataAnnotations;
using VisTarsier.NiftiLib.Processing;
using Newtonsoft.Json;

namespace VisTarsier.Service
{
    public class Job
    {
        public Recipe Recipe;
        public long Id { get; set; }
        public string Status { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string RecipeString { get { return JsonConvert.SerializeObject(Recipe); } set { Recipe = JsonConvert.DeserializeObject<Recipe>(value); } } //TODO check this works :/
        public string DbExt { get; set; }
        [ForeignKey("AttemptId")]
        public long? AttemptId { get; set; }
        public long? RecipeId { get; set; }

        [NotMapped]
        public Attempt Attempt { get; set; }
        [NotMapped]
        public string CurrentSeriesDicomFolder { get; set; }
        [NotMapped]
        public string PriorSeriesDicomFolder { get; set; }
        [NotMapped]
        public string ReferenceSeriesDicomFolder { get; set; }
       // [NotMapped]
        //public IJobResult[] Results { get; set; }
        [NotMapped]
        public string ResultSeriesDicomFolder { get; set; }
        [NotMapped]
        public string PriorReslicedSeriesDicomFolder { get; set; }
        [NotMapped]
        public string ProcessingFolder { get; set; }
        [NotMapped]
        public string DefaultDestination { get; set; }

        // Needed for EntityFramework
        public Job() { }

        public Job(Recipe recipe, Attempt attempt)
        {
            Recipe = recipe;
            Attempt = attempt;

            Attempt.SourceAet = recipe.SourceAet;
            Attempt.PatientId = recipe.PatientId;
            Attempt.PatientFullName = recipe.PatientFullName;
            Attempt.PatientBirthDate = recipe.PatientBirthDate;
            Attempt.CurrentAccession = recipe.CurrentAccession;
            Attempt.PriorAccession = recipe.PriorAccession;

            DefaultDestination =
                recipe.OutputSettings.DicomDestinations != null && !string.IsNullOrEmpty(recipe.OutputSettings.DicomDestinations.FirstOrDefault()) ?
                recipe.OutputSettings.DicomDestinations.FirstOrDefault() :
                recipe.OutputSettings.FilesystemDestinations.FirstOrDefault();
        }

        public string GetStudyIdFromReferenceSeries()
        {
            return Attempt.ReferenceSeries.Split('|')[0];
        }

        public string GetSeriesIdFromReferenceSeries()
        {
            return Attempt.ReferenceSeries.Split('|').Length < 2 ? string.Empty :
                                                           Attempt.ReferenceSeries.Split('|')[1];
        }

        public void WriteStudyAndSeriesIdsToReferenceSeries(string studyId, string seriesId)
        {
            Attempt.ReferenceSeries = string.Join("|", studyId, seriesId);
        }
    }
}
