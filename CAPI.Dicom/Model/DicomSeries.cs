using CAPI.Domain.Model;

namespace CAPI.Dicom.Model
{
    public class DicomSeries : MultiFileSeries
    {
        public string SeriesUid { get; set;}

        public DicomSeries(string description, string folderPath)
        {
            SeriesUid = string.Empty;
        }

        //public void ToNii(string outFileFullPath)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
