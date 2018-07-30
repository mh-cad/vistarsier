namespace CAPI.Common.Abstractions.Config
{
    public interface IImgProcConfig
    {
        string ImgProcBinPath { get; set; }
        string JavaExeBin { get; set; }
        string JavaClassPath { get; set; }
        string ProcessesLogPath { get; set; }
        string ImageRepositoryPath { get; set; }
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
