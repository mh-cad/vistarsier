namespace CAPI.Domain.Model
{
    public class SingleFileSeries : SeriesBase
    {
        public string FileFullPath { get; set; }
        public int NumberOfImages { get; set; }
    }
}
