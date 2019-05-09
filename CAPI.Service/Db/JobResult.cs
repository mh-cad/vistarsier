using CAPI.Service.Db;

namespace CAPI.Service.Db
{
    public class JobResult : IJobResult
    {
        public string NiftiFilePath { get; set; }
        public string DicomFolderPath { get; set; }
        public string ImagesFolderPath { get; set; }
        public string LutFilePath { get; set; }
    }
}
