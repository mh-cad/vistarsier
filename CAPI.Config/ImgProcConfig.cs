namespace CAPI.Config
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImgProcConfig : IImgProcConfig
    {
        public string ImgProcBinFolderPath { get; set; }
        public string JavaExeFilePath { get; set; }
        public string JavaClassPath { get; set; }
        public string ProcessesLogPath { get; set; }
        public string ImageRepositoryPath { get; set; }
        public string ManualProcessPath { get; set; }
        public string Hl7ProcessPath { get; set; }
        public string Dcm2NiiExeRelFilePath { get; set; }
        public string BseExeRelFilePath { get; set; }
        public string BseParams { get; set; }
        public string BfcExeRelFilePath { get; set; }
        public string BfcParams { get; set; }
        public string RegistrationRelFilePath { get; set; }
        public string RegistrationParams { get; set; }
        public string CmtkFolderName { get; set; }
        public string CmtkRawxformFile { get; set; }
        public string CmtkResultxformFile { get; set; }
        public string ReformatXRelFilePath { get; set; }
        public string ResultsDicomSeriesDescription { get; set; }
        public string PriorReslicedDicomSeriesDescription { get; set; }
    }
}
