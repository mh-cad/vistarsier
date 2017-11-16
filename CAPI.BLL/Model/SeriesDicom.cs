using System;
using System.IO;

namespace CAPI.BLL.Model
{
    public class SeriesDicom : ISeries
    {
        public string Name { get; set; }
        public string FileFullPath { get; set;}
        public string FolderPath { get; set; }
        public int NumberOfImages { get; set; }

        public SeriesDicom(string name, string folderPath)
        {
            Name = name;
            FolderPath = folderPath;
            // TODO3: Handle empty directory
            NumberOfImages = Directory.GetFiles(folderPath).Length;
        }

        public void ToNii(string outputFileFullPath)
        {
            throw new NotImplementedException();
        }

        // ReSharper disable once InconsistentNaming
        public SeriesHdr ToHdr(string outputPath, string outputFileNameNE)
        {
            if (string.IsNullOrEmpty(FolderPath)) throw new ArgumentNullException();
            if (!Directory.Exists(FolderPath)) throw new DirectoryNotFoundException("Dicom folder path does not exist in file system: " + FolderPath);
            if (Directory.GetFiles(FolderPath).Length == 0) throw new FileNotFoundException("Dicom path contains no files: " + FolderPath);
    
            var imageFormatConvertor = new ImageProcessor();
            imageFormatConvertor.ConvertDicomToHdr(FolderPath, outputPath, outputFileNameNE);

            return new SeriesHdr(Name, outputPath + '\\' + outputFileNameNE, NumberOfImages);
        }
    }
}
