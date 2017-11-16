using System.IO;

namespace CAPI.BLL.Model
{
    public class SeriesHdr : ISeries
    {
        public string Name { get; set; }
        public string FileFullPath { get; set; }
        public string FolderPath { get; set; }
        public int NumberOfImages { get; set; }

        public SeriesHdr(string name, string fileFullPath, int numberOfImages)
        {
            Name = name;
            FileFullPath = fileFullPath;
            NumberOfImages = numberOfImages;
            GetFolderPath();
        }

        private void GetFolderPath()
        {
            FolderPath = Path.GetDirectoryName(FileFullPath);
        }
    }
}
