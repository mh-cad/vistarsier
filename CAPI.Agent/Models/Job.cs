using CAPI.Agent.Abstractions.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CAPI.Agent.Models
{
    public class Job : IJob
    {
        //private readonly IDicomFactory _dicomFactory;
        //private readonly IDicomServices _dicomServices;
        //private readonly ICapiConfig _capiConfig;
        //private readonly IFileSystem _filesystem;
        //private readonly IProcessBuilder _processBuilder;

        public int Id { get; set; }
        public string SourceAet { get; set; }
        public string PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientBirthDate { get; set; }
        public string CurrentAccession { get; set; }
        public string PriorAccession { get; set; }
        public string ResultDestination { get; set; }
        public bool ExtractBrain { get; set; }
        public string ExtractBrainParams { get; set; }
        public bool Register { get; set; }
        public bool BiasFieldCorrection { get; set; }
        public string BiasFieldCorrectionParams { get; set; }

        public string Status { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public Job(Recipe recipe)
        {
            SourceAet = recipe.SourceAet;
            PatientId = recipe.PatientId;
            PatientFullName = recipe.PatientFullName;
            PatientBirthDate = recipe.PatientBirthDate;
            CurrentAccession = recipe.CurrentAccession;
            PriorAccession = recipe.PriorAccession;
            ResultDestination = recipe.Destinations.FirstOrDefault()?.DisplayName;
            ExtractBrain = recipe.ExtractBrain;
            ExtractBrainParams = recipe.ExtractBrainParams;
            Register = recipe.Register;
            BiasFieldCorrection = recipe.BiasFieldCorrection;
            BiasFieldCorrectionParams = recipe.BiasFieldCorrectionParams;
        }

        [NotMapped]
        public string CurrentSeriesDicomFolder { get; set; }
        [NotMapped]
        public string PriorSeriesDicomFolder { get; set; }

        //public Job(IDicomFactory dicomFactory, ICapiConfig capiConfig,
        //           IFileSystem filesystem, IProcessBuilder processBuilder)
        //{
        //    //_dicomFactory = dicomFactory;
        //    //_capiConfig = capiConfig;
        //    //_filesystem = filesystem;
        //    //_processBuilder = processBuilder;

        //    //var dicomConfig = GetDicomConfigFromCapiConfig(_capiConfig);
        //    //_dicomServices = _dicomFactory.CreateDicomServices(dicomConfig, _filesystem, _processBuilder);
        //}

        //public void Build(Recipe recipe)
        //{
        //    GetCurrentSeriesDicomFiles(recipe);
        //    GetPriorSeriesDicomFiles(recipe);

        //    ExtractBrain = recipe.ExtractBrain;
        //    ExtractBrainParams = recipe.ExtractBrainParams;
        //    Register = recipe.Register;
        //    BiasFieldCorrection = recipe.BiasFieldCorrection;
        //    BiasFieldCorrectionParams = recipe.BiasFieldCorrectionParams;
        //}

        public void Process()
        {
            Start = DateTime.Now;
            Status = "Processing";
            // TODO1: Update database

            throw new NotImplementedException();

#pragma warning disable 162
            End = DateTime.Now;
#pragma warning restore 162
        }

        //private void GetCurrentSeriesDicomFiles(Recipe recipe)
        //{
        //    // Recipe has path to dicom folder
        //    if (!string.IsNullOrEmpty(recipe.CurrentSeriesDicomFolder))
        //        CurrentSeriesDicomFolder = recipe.CurrentSeriesDicomFolder;

        //    if (!DicomNodeIsUp(recipe.CurrentSourceAet))
        //        throw new Exception($"Dicom node with following AET not accessible to retrieve study from: {recipe.CurrentSourceAet}");

        //    // Recipe doesn't has path to dicom folder, but has SourceAet and Accession
        //    if (!string.IsNullOrEmpty(recipe.CurrentAccession))
        //        CurrentSeriesDicomFolder =
        //            GetSeriesByAccession(recipe.CurrentSourceAet, recipe.CurrentAccession, recipe.CurrentSeriesCriteria);

        //    // Recipe doesn't has path to dicom folder or accession, but has SourceAet and selection criteria
        //    CurrentSeriesDicomFolder = GetSeriesBySelectionCriteria(recipe);
        //}
        //private void GetPriorSeriesDicomFiles(Recipe recipe)
        //{
        //    // Recipe has path to dicom folder
        //    if (!string.IsNullOrEmpty(recipe.PriorSeriesDicomFolder))
        //        PriorSeriesDicomFolder = recipe.PriorSeriesDicomFolder;

        //    if (!DicomNodeIsUp(recipe.PriorSourceAet))
        //        throw new Exception($"Dicom node with following AET not accessible to retrieve study from: {recipe.CurrentSourceAet}");

        //    // Recipe doesn't has path to dicom folder, but has SourceAet and Accession
        //    if (!string.IsNullOrEmpty(recipe.PriorAccession))
        //        PriorSeriesDicomFolder =
        //            GetSeriesByAccession(recipe.PriorSourceAet, recipe.PriorAccession, recipe.PriorSeriesCriteria);

        //    // Recipe doesn't has path to dicom folder or accession, but has SourceAet and selection criteria
        //    PriorSeriesDicomFolder = GetSeriesBySelectionCriteria(recipe);
        //}

        //private bool DicomNodeIsUp(string aet)
        //{
        //    var localDicomNode = _capiConfig.DicomConfig.LocalNode;
        //    var remoteDicomNode = _capiConfig.DicomConfig.RemoteNodes.Single(n => n.AeTitle == aet);

        //    _dicomServices.CheckRemoteNodeAvailability(localDicomNode, remoteDicomNode);

        //    return true;
        //}

        //private IDicomConfig GetDicomConfigFromCapiConfig(ICapiConfig capiConfig)
        //{
        //    var dicomConfig = _dicomFactory.CreateDicomConfig();
        //    dicomConfig.ExecutablesPath = capiConfig.DicomConfig.DicomServicesExecutablesPath;
        //    return dicomConfig;
        //}

        //private string GetSeriesByAccession(string aet, string accession, List<SeriesSelectionCriteria> criteria)
        //{
        //    var localDicomNode = _capiConfig.DicomConfig.LocalNode;
        //    var remoteDicomNode = _capiConfig.DicomConfig.RemoteNodes.Single(n => n.AeTitle == aet);

        //    var study = _dicomServices.GetStudyForAccession(accession, localDicomNode, remoteDicomNode);
        //    var studySeries = _dicomServices.GetSeriesForStudy(study.StudyInstanceUid, localDicomNode, remoteDicomNode);
        //    var series = GetMatchingSeries(studySeries, criteria);

        //    var imgRepoFolderPath = _capiConfig.ImgProcConfig.ImageRepositoryPath;
        //    var dicomFolderPath = Path.Combine(imgRepoFolderPath, ""); // TODO1: Folder name
        //    _dicomServices.SaveSeriesToLocalDisk(series, dicomFolderPath, localDicomNode, remoteDicomNode);

        //    return dicomFolderPath;
        //}

        //private IDicomSeries GetMatchingSeries(IEnumerable<IDicomSeries> studySeries, List<SeriesSelectionCriteria> criteria)
        //{
        //    return null;
        //}

        //private string GetSeriesBySelectionCriteria(Recipe recipe)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
