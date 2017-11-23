using System.Collections.Generic;

namespace CAPI.BLL.Model
{
    public class MultiSliceSeries : SeriesBase
    {
        public string FolderPath { get; set; }
        public IList<string> Images { get; set; }
        public int GetNumberOfImages()
        {
            return Images.Count;
        }

        public MultiSliceSeries()
        {
            
        }
    }
}
