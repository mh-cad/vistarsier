using System.Collections.Generic;

namespace VisTarsier.Config
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Recipe
    {
        public enum RegisterToOption { PRIOR, CURRENT }
        public Recipe()
        {
            CurrentSeriesCriteria = new List<SeriesSelectionCriteria>();
            PriorSeriesCriteria = new List<SeriesSelectionCriteria>();
        }

        public static Recipe Default()
        {
            CapiConfig conf = CapiConfig.GetConfig();

            string sourceAet = "";
            string destAet = "";
            if (conf.DicomConfig?.RemoteNodes != null && conf.DicomConfig.RemoteNodes.Count > 0)
            {
                sourceAet = conf.DicomConfig.RemoteNodes[0].AeTitle;
                if (conf.DicomConfig.RemoteNodes.Count > 1)
                {
                    destAet = conf.DicomConfig.RemoteNodes[1].AeTitle;
                }
                else
                {
                    destAet = sourceAet;
                }
            }

            // By default we will match any of the following series.
            const string SERIES_DESC = "flair sag;flair sag 3d;t2 sag flair;flair sag fs;flair sag 3d - t2 space da-fl;3d sag flair fs;t2 sag flair 3d;flair 3d t2_spc_irprep_ns_sag_p2_da-fl_iso;3d flair_sag_p3_iso;3d t2 flair sag;t2_spc_irprep_ns_sag_p2_da-fl_iso;flair 3d;t2_3d_sag flair iso;sag 3d flair;t2 sag flair 3d;sag 3d flair";

            return new Recipe()
            {
                SourceAet = sourceAet,
                PatientId = "",
                PatientFullName = "",
                PatientBirthDate = "",
                CurrentAccession = "",
                PriorAccession = "",
                ExtractBrain = true,
                RegisterTo = RegisterToOption.CURRENT,
                BiasFieldCorrection = true,
                CurrentSeriesCriteria = new List<SeriesSelectionCriteria>(new[] { new SeriesSelectionCriteria()
                {
                    PriorStudyIndex = 0, // 0-based index i.e. 0=most recent, 1=second most recent
                    MostRecentPriorStudy = false, // overrides PriorStudyIndex if true; true equals PriorStudyIndex of 0
                    OldestPriorStudy = false, // overrides PriorStudyIndex if true
                    CutOffPeriodValueInMonths = "", // leave blank to include all dates
                    StudyDescription = "demyelin",
                    StudyDescriptionOperand = Common.StringOperand.Contains,
                    StudyDate = "",
                    StudyDateOperand = Common.DateOperand.Before,
                    SeriesDescription = SERIES_DESC,
                    SeriesDescriptionOperand = Common.StringOperand.OccursIn,
                    SeriesDescriptionDelimiter = ";"
                } }),
                PriorSeriesCriteria = new List<SeriesSelectionCriteria>(new[] { new SeriesSelectionCriteria()
                {
                    PriorStudyIndex = 0, // 0-based index i.e. 0=most recent, 1=second most recent
                    MostRecentPriorStudy = false, // overrides PriorStudyIndex if true; true equals PriorStudyIndex of 0
                    OldestPriorStudy = false, // overrides PriorStudyIndex if true
                    CutOffPeriodValueInMonths = "", // leave blank to include all dates
                    StudyDescription = "",
                    StudyDescriptionOperand = Common.StringOperand.Contains,
                    StudyDate = "",
                    StudyDateOperand = Common.DateOperand.Before,
                    SeriesDescription = SERIES_DESC,
                    SeriesDescriptionOperand = Common.StringOperand.OccursIn,
                    SeriesDescriptionDelimiter = ";"
                } }),
                CompareSettings = new CompareSettings()
                {
                    CompareIncrease = true,
                    CompareDecrease = true,
                    BackgroundThreshold = 10,
                    MinRelevantStd = -1,
                    MaxRelevantStd = 5,
                    MinChange = 0.8f,
                    MaxChange = 5,
                    GenerateHistogram = true
                },
                OutputSettings = new OutputSettings()
                {
                    ResultsDicomSeriesDescription = "VisTarsier Results",
                    ReslicedDicomSeriesDescription = "VisTarsier Resliced",
                    FilesystemDestinations = new List<string>(),
                    OnlyCopyResults = false,
                    DicomDestinations = new List<string> (new[] {destAet})
                }

            };
        }


        public string SourceAet { get; set; }

        public string PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientBirthDate { get; set; }

        public string CurrentSeriesDicomFolder { get; set; }
        public string CurrentAccession { get; set; }
        public List<SeriesSelectionCriteria> CurrentSeriesCriteria { get; set; }

        public string PriorSeriesDicomFolder { get; set; }
        public string PriorAccession { get; set; }
        public List<SeriesSelectionCriteria> PriorSeriesCriteria { get; set; }

        public bool ExtractBrain { get; set; }
        public RegisterToOption RegisterTo { get; set; }
        public bool BiasFieldCorrection { get; set; }
        public CompareSettings CompareSettings { get; set; }
        public OutputSettings OutputSettings { get; set; }
    }
}
