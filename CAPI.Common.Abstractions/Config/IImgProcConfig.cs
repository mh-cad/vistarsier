namespace CAPI.Common.Abstractions.Config
{
    public interface IImgProcConfig
    {
        string ImgProcBinFolderPath { get; set; }
        string JavaExeFilePath { get; set; }
        string JavaClassPath { get; set; }
        string ProcessesLogPath { get; set; }
        string ImageRepositoryPath { get; set; }
        string ManualProcessPath { get; set; }
        string Hl7ProcessPath { get; set; }
        string Dcm2NiiExeRelFilePath { get; set; }
        string BseExeRelFilePath { get; set; }
        string BseParams { get; set; }
        string BfcExeRelFilePath { get; set; }
        string BfcParams { get; set; }
        string RegistrationRelFilePath { get; set; }
        string RegistrationParams { get; set; }
        string CmtkFolderName { get; set; }
        string CmtkRawxformFile { get; set; }
        string CmtkResultxformFile { get; set; }
        string ReformatXRelFilePath { get; set; }
        string ResultsDicomSeriesDescription { get; set; }
        string PriorReslicedDicomSeriesDescription { get; set; }
    }
}
