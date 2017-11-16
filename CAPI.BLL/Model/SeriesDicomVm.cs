namespace CAPI.BLL.Model
{
    public class SeriesDicomVm : ISeriesVm
    {
        public string Name { get; set; }
        public int NumberOfImages { get; set; }
        public string FilesFormat { get; set; }
        public string[] FilesList { get; set; }

        public SeriesDicomVm()
        {
            Name = string.Empty;
            NumberOfImages = 0;
            FilesFormat = "dicom";
            FilesList = new string[] { };
        }
    }
}
