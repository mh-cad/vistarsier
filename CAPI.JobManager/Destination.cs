using CAPI.Dicom.Abstraction;
using CAPI.JobManager.Abstraction;

namespace CAPI.JobManager
{
    public class Destination : IDestination
    {
        public Destination(string id, string folderPath, string aeTitle)
        {
            Id = id;
            FolderPath = folderPath;
            AeTitle = aeTitle;
        }

        public string Id { get; set; }
        public string FolderPath { get; set; }
        public string AeTitle { get; set; }
        public IDicomNode DicomNode { get; set; }
    }
}