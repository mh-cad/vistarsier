namespace CAPI.Common.Abstractions.Config
{
    public interface ICapiConfig
    {
        IDicomConfig DicomConfig { get; set; }
        IImgProcConfig ImgProcConfig { get; set; }
        ITestsConfig TestsConfig { get; set; }

        string RunInterval { get; set; }
        string AgentDbConnectionString { get; set; }
        string ManualProcessPath { get; set; }
        string Hl7ProcessPath { get; set; }
        string DefaultRecipePath { get; set; }

        ICapiConfig GetConfig(string[] args = null);
    }
}
