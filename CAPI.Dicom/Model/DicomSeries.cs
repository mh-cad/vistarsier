using System;
using System.IO;
using System.Linq;
using CAPI.BLL.Model;
using CAPI.Common;

namespace CAPI.Dicom.Model
{
    public class DicomSeries : ISeries
    {
        public string Name { get; set; }
        public string FileFullPath { get; set;}
        public string FolderPath { get; set; }
        public int NumberOfImages { get; set; }

        public DicomSeries(string name, string folderPath)
        {
            Name = name;
            FolderPath = folderPath;
            NumberOfImages = Directory.GetFiles(folderPath).Length; // TODO3: Handle empty directory
        }

        public void ToNii(string outFileFullPath)
        {
            throw new NotImplementedException();
        }

        public SeriesDicomVm GetViewModel()
        {
            return new SeriesDicomVm()
            {
                Name = Name,
                NumberOfImages = NumberOfImages,
                FilesList = Directory.GetFiles(FolderPath)
                    .Select(f =>
                    {
                        var imgRepPathLength = Config.GetImageRepositoryPath().Length + 1;
                        var filePathLength = f.Length;
                        return f.Substring(imgRepPathLength, filePathLength - imgRepPathLength);
                    }).ToArray() // TODO1: Modify to reflect relative path to files in the website
            };
        }
    }
}
