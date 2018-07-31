namespace CAPI.Common.Abstractions.Config
{
    public interface IImgProcConfig
    {
        string ImgProcBinFolderPath { get; set; }
        string ImgProcConfigFolderPath { get; set; }
        string JavaExeFilePath { get; set; }
        string JavaClassPath { get; set; }
        string ProcessesLogPath { get; set; }
        string ImageRepositoryPath { get; set; }
        string ManualProcessPath { get; set; }
        string Hl7ProcessPath { get; set; }
        bool ProcessCasesAddedManually { get; set; }
        bool ProcessCasesAddedByHl7 { get; set; }


        string Dcm2NiiExeFilePath { get; set; }
        string BseExeFilePath { get; set; }
        string BseParams { get; set; }
        string BfcExeFilePath { get; set; }
        string BfcParams { get; set; }
        string RegistrationFile { get; set; }
        string RegistrationParams { get; set; }
        string CmtkFolderName { get; set; }
        string CmtkRawxformFile { get; set; }
        string CmtkResultxformFile { get; set; }
        string ReformatXFilePath { get; set; }
    }
}
