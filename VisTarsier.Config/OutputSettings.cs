using System.Collections.Generic;

namespace VisTarsier.Config
{
    public class OutputSettings
    {
        public string ResultsDicomSeriesDescription { get; set; }
        public string ReslicedDicomSeriesDescription { get; set; }

        public List<string> FilesystemDestinations { get; set; }
        public bool OnlyCopyResults { get; set; }
        public List<string> DicomDestinations { get; set; }

        public OutputSettings()
        {
            FilesystemDestinations = new List<string>();
            DicomDestinations = new List<string>();
        }
    }
}
