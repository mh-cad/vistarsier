using System;
using System.IO;

namespace CAPI.BLL.Model
{
    public class SeriesDicom : ISeries
    {
        public string Name { get; set; }
        public string FileFullPath { get; set;}
        public string FolderPath { get; set; }
        public int NumberOfImages => { get } (Directory.Exists(folderPath)) ? Directory.GetFiles(folderPath).Length : 0;

        public SeriesDicom(string name, string folderPath)
        {
            Name = name;
            FolderPath = folderPath;
            //NumberOfImages = (Directory.Exists(folderPath))? Directory.GetFiles(folderPath).Length:0;
        }

        public void ToNii(string outputFileFullPath)
        {
            throw new NotImplementedException();
        }

        //public SeriesHdr ToHdr(string outputPath, string outputFileNameNoExt)
        //{
        //    if (string.IsNullOrEmpty(FolderPath)) throw new ArgumentNullException();
        //    if (!Directory.Exists(FolderPath)) throw new DirectoryNotFoundException("Dicom folder path does not exist in file system: " + FolderPath);
        //    if (Directory.GetFiles(FolderPath).Length == 0) throw new FileNotFoundException("Dicom path contains no files: " + FolderPath);
    
        //    var imageFormatConvertor = new ImageProcessor();
        //    imageFormatConvertor.ConvertDicomToHdr(FolderPath, outputPath, outputFileNameNoExt);

        //    return new SeriesHdr(Name, outputPath + '\\' + outputFileNameNoExt, NumberOfImages);
        //}
    }
}
