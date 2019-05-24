using VisTarsier.Service.Db;

namespace VisTarsier.Service.Db
{
    public class JobResult : IJobResult
    {
        public string NiftiFilePath { get; set; }
        public string DicomFolderPath { get; set; }
        public string ImagesFolderPath { get; set; }
        public string LutFilePath { get; set; }
    }
}
