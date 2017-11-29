using System.Collections.Generic;

namespace CAPI.Domain.Model
{
    public class Study
    {
        public string Description { get; set; }
        public List<SeriesBase> SeriesList { get; set; }

        public Study()
        {
            Description = string.Empty;
            SeriesList = new List<SeriesBase>();
        }
        public Study(string description) : this()
        {
            Description = description;
        }
    }
}
