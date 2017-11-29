using System.Collections.Generic;

namespace CAPI.Domain.Model
{
    public class MultiFileSeries : SeriesBase
    {
        public string FolderPath { get; set; }
        public IList<string> ImagesFileList { get; set; }
        public int GetNumberOfImages()
        {
            return ImagesFileList.Count;
        }

        public MultiFileSeries()
        {
            
        }
    }
}
