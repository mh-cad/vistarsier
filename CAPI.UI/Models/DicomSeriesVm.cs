using CAPI.Dicom.Model;

namespace CAPI.UI.Models
{
    public class DicomSeriesVm
    {
        public string Name { get; set; }
        public int NumberOfImages { get; set; }
        public string FilesFormat { get; set; }
        public string[] FilesList { get; set; }

        public DicomSeriesVm()
        {
            Name = string.Empty;
            NumberOfImages = 0;
            FilesFormat = "dicom";
            FilesList = new string[] { };
        }

        public DicomSeriesVm MapDicomSeriesToVm(DicomSeries dicomSeries)
        {
            //return new DicomSeriesVm()
            //{
            //    Name = dicomSeries.Description,
            //    NumberOfImages = NumberOfImages,
            //    FilesList = Directory.GetFiles(dicomSeries.FolderPath)
            //        .Select(f =>
            //        {
            //            var imgRepPathLength = Config.GetImageRepositoryPath().Length + 1;
            //            var filePathLength = f.Length;
            //            return f.Substring(imgRepPathLength, filePathLength - imgRepPathLength);
            //        }).ToArray() // TODO1: Modify to reflect relative path to files in the website
            //};
            return null;
        }
    }
}